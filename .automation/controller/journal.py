"""Transactional recovery journal for a single, trusted local shadow workspace.

This SQLite file is local recovery evidence, not remote canonical project state.
Existing symlinks and Windows reparse points are rejected, including SQLite's
sidecars. The containing directory must be trusted: these checks cannot prevent
another local process from replacing a path between checking and opening it.
"""

from contextlib import contextmanager
import hashlib
import json
import math
from pathlib import Path
import sqlite3
import stat


class JournalError(Exception):
    """The journal is unavailable, invalid, or corrupt."""


class Conflict(JournalError):
    """A revision or idempotency precondition did not match."""


def _json_value(value):
    if value is None or type(value) in (str, bool, int):
        return
    if type(value) is float and math.isfinite(value):
        return
    if type(value) is list:
        for item in value:
            _json_value(item)
        return
    if type(value) is dict and all(type(key) is str for key in value):
        for item in value.values():
            _json_value(item)
        return
    raise JournalError("journal values must contain only finite JSON values")


def _encode(value):
    if type(value) is not dict:
        raise JournalError("journal state and payload must be JSON objects")
    try:
        _json_value(value)
        return json.dumps(value, sort_keys=True, separators=(",", ":"),
                          ensure_ascii=False, allow_nan=False)
    except (ValueError, TypeError, RecursionError) as exc:
        raise JournalError("journal value cannot be encoded as JSON") from exc


def _decode(value):
    try:
        decoded = json.loads(value)
        if _encode(decoded) != value:
            raise JournalError("journal JSON is not canonical")
        return decoded
    except (ValueError, TypeError, RecursionError) as exc:
        raise JournalError("journal contains invalid JSON") from exc


def _hash(value):
    try:
        return hashlib.sha256(value.encode("utf-8")).hexdigest()
    except UnicodeError as exc:
        raise JournalError("journal JSON must be valid UTF-8") from exc


class Journal:
    """Every operation opens and closes its own SQLite connection.

    initialize() is the only operation that creates a database or directories.
    Reads open a must-exist database with query_only enabled. SQLite may repair
    its own hot rollback journal after a crash; ordinary reads do not mutate it.
    Duplicate event IDs return their original snapshot when kind, payload, and
    state match, even after later events; otherwise they raise Conflict.
    """

    def __init__(self, path: Path):
        self.path = Path(path).absolute()

    def _check_paths(self):
        candidates = (self.path, *self.path.parents,
                      *(Path(str(self.path) + suffix)
                        for suffix in ("-journal", "-wal", "-shm")))
        try:
            for path in candidates:
                try:
                    info = path.lstat()
                except FileNotFoundError:
                    continue
                if stat.S_ISLNK(info.st_mode) or (
                    getattr(info, "st_file_attributes", 0)
                    & getattr(stat, "FILE_ATTRIBUTE_REPARSE_POINT", 0x400)
                ):
                    raise JournalError("journal paths must not contain links or reparse points")
            if self.path.exists() and not self.path.is_file():
                raise JournalError("journal path must be a regular file")
        except OSError as exc:
            raise JournalError("journal path cannot be inspected") from exc

    @contextmanager
    def _connection(self, mode, query_only=False):
        self._check_paths()
        if mode != "rwc" and not self.path.is_file():
            raise JournalError("journal does not exist; initialize it explicitly")
        connection = None
        try:
            connection = sqlite3.connect(self.path.as_uri() + "?mode=" + mode,
                                         uri=True, timeout=1.0,
                                         isolation_level=None)
            connection.row_factory = sqlite3.Row
            connection.execute("PRAGMA foreign_keys = ON")
            if mode != "ro":
                connection.execute("PRAGMA synchronous = FULL")
            if query_only:
                connection.execute("PRAGMA query_only = ON")
            yield connection
        except sqlite3.Error as exc:
            raise JournalError("journal storage error: " + str(exc)) from exc
        finally:
            if connection is not None:
                connection.close()

    @staticmethod
    @contextmanager
    def _transaction(connection, immediate=False):
        connection.execute("BEGIN IMMEDIATE" if immediate else "BEGIN")
        try:
            yield
            connection.execute("COMMIT")
        except BaseException:
            if connection.in_transaction:
                connection.execute("ROLLBACK")
            raise

    @staticmethod
    def _create(connection, state_json):
        # Individual statements preserve the encompassing transaction;
        # executescript() would commit it implicitly.
        statements = (
            "CREATE TABLE metadata (key TEXT PRIMARY KEY, value TEXT NOT NULL)",
            "INSERT INTO metadata VALUES ('format_version', '1')",
            "CREATE TABLE snapshots (revision INTEGER PRIMARY KEY CHECK (revision >= 0), "
            "state_json TEXT NOT NULL, state_hash TEXT NOT NULL)",
            "CREATE TABLE events (revision INTEGER PRIMARY KEY CHECK (revision > 0), "
            "event_id TEXT UNIQUE NOT NULL, kind TEXT NOT NULL, payload_json TEXT NOT NULL, "
            "state_hash TEXT NOT NULL, FOREIGN KEY (revision) REFERENCES snapshots(revision))",
        )
        for statement in statements:
            connection.execute(statement)
        for table in ("metadata", "snapshots", "events"):
            for operation in ("UPDATE", "DELETE"):
                connection.execute(
                    f"CREATE TRIGGER {table}_no_{operation.lower()} BEFORE {operation} ON {table} "
                    "BEGIN SELECT RAISE(ABORT, 'journal records are immutable'); END"
                )
        connection.execute("INSERT INTO snapshots VALUES (0, ?, ?)",
                           (state_json, _hash(state_json)))

    @staticmethod
    def _verified(connection):
        if [row[0] for row in connection.execute("PRAGMA quick_check")] != ["ok"]:
            raise JournalError("journal SQLite integrity check failed")
        metadata = [tuple(row) for row in connection.execute("SELECT key, value FROM metadata")]
        if metadata != [("format_version", "1")]:
            raise JournalError("unsupported or corrupt journal format")
        snapshot_rows = list(connection.execute("SELECT * FROM snapshots ORDER BY revision"))
        event_rows = list(connection.execute("SELECT * FROM events ORDER BY revision"))
        if not snapshot_rows or len(snapshot_rows) != len(event_rows) + 1:
            raise JournalError("journal snapshots and events are incomplete")
        snapshots = []
        for revision, row in enumerate(snapshot_rows):
            if type(row["revision"]) is not int or row["revision"] != revision:
                raise JournalError("journal snapshot revisions are not contiguous")
            state = _decode(row["state_json"])
            if _hash(row["state_json"]) != row["state_hash"]:
                raise JournalError("journal state hash mismatch")
            snapshots.append({"revision": revision, "state": state, "state_hash": row["state_hash"]})
        events = []
        seen = set()
        for revision, row in enumerate(event_rows, start=1):
            if type(row["revision"]) is not int or row["revision"] != revision:
                raise JournalError("journal event revisions are not contiguous")
            if row["state_hash"] != snapshots[revision]["state_hash"]:
                raise JournalError("journal event and snapshot hashes disagree")
            if (type(row["event_id"]) is not str or not row["event_id"] or
                    type(row["kind"]) is not str or not row["kind"] or row["event_id"] in seen):
                raise JournalError("journal event identity is invalid")
            seen.add(row["event_id"])
            events.append({"event_id": row["event_id"], "revision": revision,
                           "kind": row["kind"], "payload": _decode(row["payload_json"]),
                           "state_hash": row["state_hash"]})
        return snapshots, events

    def initialize(self, state: dict) -> dict:
        state_json = _encode(state)
        _hash(state_json)
        self._check_paths()
        try:
            self.path.parent.mkdir(parents=True, exist_ok=True)
        except OSError as exc:
            raise JournalError("journal directory cannot be created") from exc
        with self._connection("rwc") as connection:
            with self._transaction(connection, immediate=True):
                tables = connection.execute(
                    "SELECT name FROM sqlite_master WHERE type = 'table'"
                ).fetchall()
                if not tables:
                    self._create(connection, state_json)
                snapshots, _ = self._verified(connection)
                if _encode(snapshots[0]["state"]) != state_json:
                    raise Conflict("journal was initialized with a different state")
                return snapshots[-1]

    def read(self) -> dict:
        with self._connection("rw", query_only=True) as connection:
            with self._transaction(connection):
                snapshots, _ = self._verified(connection)
                return snapshots[-1]

    def events(self) -> list:
        with self._connection("rw", query_only=True) as connection:
            with self._transaction(connection):
                _, events = self._verified(connection)
                return events

    def apply(self, expected_revision: int, event_id: str, kind: str,
              payload: dict, state: dict) -> dict:
        if type(expected_revision) is not int or expected_revision < 0:
            raise JournalError("expected revision must be a nonnegative integer")
        if type(event_id) is not str or not event_id or type(kind) is not str or not kind:
            raise JournalError("event ID and kind must be nonempty strings")
        payload_json, state_json = _encode(payload), _encode(state)
        state_hash = _hash(state_json)
        _hash(payload_json)
        with self._connection("rw") as connection:
            with self._transaction(connection, immediate=True):
                snapshots, events = self._verified(connection)
                for event in events:
                    if event["event_id"] == event_id:
                        snapshot = snapshots[event["revision"]]
                        if (event["kind"] != kind or _encode(event["payload"]) != payload_json or
                                _encode(snapshot["state"]) != state_json):
                            raise Conflict("event ID already exists with different content")
                        return snapshot
                if snapshots[-1]["revision"] != expected_revision:
                    raise Conflict("stale journal revision")
                revision = expected_revision + 1
                connection.execute("INSERT INTO snapshots VALUES (?, ?, ?)",
                                   (revision, state_json, state_hash))
                connection.execute("INSERT INTO events VALUES (?, ?, ?, ?, ?)",
                                   (revision, event_id, kind, payload_json, state_hash))
                return {"revision": revision, "state": json.loads(state_json), "state_hash": state_hash}
