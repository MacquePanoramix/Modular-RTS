"""Recovery, integrity, and compare-and-swap tests using temporary local files."""

from concurrent.futures import ThreadPoolExecutor
import hashlib
from pathlib import Path
import sqlite3
import subprocess
import sys
import tempfile
import threading
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "controller"))
from journal import Conflict, Journal, JournalError


class JournalTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.path = Path(self.temporary.name) / "local" / "journal.sqlite3"
        self.journal = Journal(self.path)

    def initialize(self):
        return self.journal.initialize({"status": "idle", "count": 0})

    def apply(self, event_id="event-1", expected_revision=0, count=1):
        return self.journal.apply(expected_revision, event_id, "increment", {"amount": 1},
                                  {"status": "idle", "count": count})

    def sql(self, *statements):
        connection = sqlite3.connect(self.path)
        try:
            for statement in statements:
                connection.execute(statement)
            connection.commit()
        finally:
            connection.close()

    def test_missing_reads_and_apply_do_not_create_anything(self):
        for operation in (self.journal.read, self.journal.events, self.apply):
            with self.assertRaises(JournalError):
                operation()
            self.assertFalse(self.path.parent.exists())

    def test_initialize_and_canonical_hash(self):
        snapshot = self.initialize()
        self.assertEqual(snapshot["revision"], 0)
        self.assertEqual(snapshot["state_hash"],
                         hashlib.sha256(b'{"count":0,"status":"idle"}').hexdigest())
        self.assertEqual(self.journal.events(), [])
        self.assertEqual(self.journal.read(), snapshot)
        self.assertEqual(self.initialize(), snapshot)

    def test_existing_initialize_compares_initial_state_after_updates(self):
        self.initialize()
        latest = self.apply()
        self.assertEqual(self.initialize(), latest)
        with self.assertRaises(Conflict):
            self.journal.initialize(latest["state"])

    def test_duplicate_is_idempotent_and_returns_original_snapshot(self):
        self.initialize()
        first = self.apply()
        second = self.apply("event-2", 1, 2)
        self.assertEqual(self.apply(), first)
        self.assertEqual(self.journal.read(), second)
        self.assertEqual(len(self.journal.events()), 2)

    def test_duplicate_rejects_each_changed_content_field(self):
        self.initialize()
        self.apply()
        for kind, payload, state in (
            ("different", {"amount": 1}, {"count": 1, "status": "idle"}),
            ("increment", {"amount": 2}, {"count": 1, "status": "idle"}),
            ("increment", {"amount": 1}, {"count": 2, "status": "idle"}),
        ):
            with self.subTest(kind=kind, payload=payload, state=state):
                with self.assertRaises(Conflict):
                    self.journal.apply(0, "event-1", kind, payload, state)
        self.assertEqual(self.journal.read()["revision"], 1)

    def test_stale_revision_rejected(self):
        self.initialize()
        self.apply()
        with self.assertRaises(Conflict):
            self.apply("event-2", 0, 2)
        self.assertEqual(self.journal.read()["revision"], 1)

    def test_two_writers_same_revision_only_one_commits(self):
        self.initialize()
        barrier = threading.Barrier(2)

        def writer(event_id):
            barrier.wait(timeout=5)
            try:
                return Journal(self.path).apply(0, event_id, "increment", {}, {"count": 1})
            except Conflict:
                return None

        with ThreadPoolExecutor(max_workers=2) as executor:
            results = list(executor.map(writer, ("one", "two")))
        self.assertEqual(sum(result is not None for result in results), 1)
        self.assertEqual(self.journal.read()["revision"], 1)
        self.assertEqual(len(self.journal.events()), 1)

    def test_transaction_failure_rolls_back_inserted_snapshot(self):
        before = self.initialize()
        self.sql("CREATE TRIGGER simulate_crash BEFORE INSERT ON events "
                 "BEGIN SELECT RAISE(ABORT, 'simulated failure'); END")
        with self.assertRaises(JournalError):
            self.apply()
        self.assertEqual(Journal(self.path).read(), before)
        self.assertEqual(self.journal.events(), [])
        self.sql("DROP TRIGGER simulate_crash")
        self.assertEqual(self.apply()["revision"], 1)

    def test_uncommitted_crash_transaction_is_rolled_back_on_close(self):
        before = self.initialize()
        connection = sqlite3.connect(self.path, isolation_level=None)
        connection.execute("BEGIN IMMEDIATE")
        connection.execute("INSERT INTO snapshots VALUES (1, '{}', ?)",
                           (hashlib.sha256(b'{}').hexdigest(),))
        connection.close()
        self.assertEqual(Journal(self.path).read(), before)

    def test_abrupt_process_crash_recovers_hot_rollback_journal(self):
        before = self.initialize()
        # Force dirty pages to disk, then exit without sqlite close/rollback.
        script = (
            "import os, sqlite3, sys; "
            "connection = sqlite3.connect(sys.argv[1], isolation_level=None); "
            "connection.execute('PRAGMA cache_size = 2'); "
            "connection.execute('BEGIN IMMEDIATE'); "
            "connection.execute('INSERT INTO snapshots VALUES (1, ?, ?)', "
            "('x' * 131072, 'uncommitted')); "
            "os._exit(23)"
        )
        result = subprocess.run([sys.executable, "-c", script, str(self.path)],
                                capture_output=True, timeout=10)
        self.assertEqual(result.returncode, 23, result.stderr.decode())
        self.assertTrue(Path(str(self.path) + "-journal").exists())
        self.assertEqual(Journal(self.path).read(), before)
        self.assertEqual(self.journal.events(), [])

    def test_replacement_journal_recovers_latest_state_and_events(self):
        self.initialize()
        expected = self.apply()
        replacement = Journal(self.path)
        self.assertEqual(replacement.read(), expected)
        event = replacement.events()[0]
        self.assertEqual(event, {"revision": 1, "event_id": "event-1", "kind": "increment",
                                 "payload": {"amount": 1}, "state_hash": expected["state_hash"]})
        self.assertEqual(replacement.apply(1, "event-2", "increment", {}, {"count": 2})["revision"], 2)

    def test_state_hash_corruption_is_rejected(self):
        self.initialize()
        self.sql("DROP TRIGGER snapshots_no_update",
                 "UPDATE snapshots SET state_json = '{\"count\":99,\"status\":\"idle\"}'")
        with self.assertRaisesRegex(JournalError, "hash mismatch"):
            self.journal.read()

    def test_missing_event_and_snapshot_are_rejected(self):
        self.initialize()
        self.apply()
        self.apply("event-2", 1, 2)
        self.sql("DROP TRIGGER events_no_delete", "DELETE FROM events WHERE revision = 1",
                 "DROP TRIGGER snapshots_no_delete", "DELETE FROM snapshots WHERE revision = 1")
        with self.assertRaisesRegex(JournalError, "contiguous"):
            self.journal.read()

    def test_orphan_snapshot_and_event_hash_corruption_are_rejected(self):
        self.initialize()
        self.apply()
        self.sql("DROP TRIGGER events_no_update", "UPDATE events SET state_hash = 'invalid'")
        with self.assertRaisesRegex(JournalError, "hashes disagree"):
            self.journal.events()
        self.sql("DROP TRIGGER events_no_delete", "DELETE FROM events")
        with self.assertRaisesRegex(JournalError, "incomplete"):
            self.journal.read()

    def test_records_are_immutable_at_sql_layer(self):
        self.initialize()
        self.apply()
        for statement in ("DELETE FROM events", "UPDATE events SET kind = 'other'",
                          "DELETE FROM snapshots", "UPDATE snapshots SET state_hash = 'bad'",
                          "DELETE FROM metadata"):
            with self.subTest(statement=statement), self.assertRaises(sqlite3.IntegrityError):
                self.sql(statement)
        self.assertEqual(self.journal.read()["revision"], 1)

    def test_invalid_json_is_rejected_before_creation(self):
        for invalid in ({"value": float("nan")}, {"value": float("inf")}, {1: "value"},
                        {"value": (1, 2)}, {"value": "\ud800"}):
            with self.subTest(invalid=repr(invalid)), self.assertRaises(JournalError):
                self.journal.initialize(invalid)
            self.assertFalse(self.path.parent.exists())

    def test_corrupt_file_is_rejected(self):
        self.path.parent.mkdir()
        self.path.write_bytes(b"not a SQLite database")
        with self.assertRaises(JournalError):
            self.journal.read()

    def test_symlink_path_is_rejected_if_supported(self):
        self.initialize()
        link = self.path.parent / "linked.sqlite3"
        try:
            link.symlink_to(self.path)
        except OSError:
            self.skipTest("OS does not permit creating symbolic links")
        with self.assertRaisesRegex(JournalError, "links or reparse"):
            Journal(link).read()


if __name__ == "__main__":
    unittest.main()
