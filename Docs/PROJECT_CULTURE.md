# Wonder Gather — Project Culture

**Status:** lightweight collaboration vocabulary.  
**Authority:** mnemonic only. Plain project terms and canonical documents always win.

This culture grew partly from an exchange with another AI-assisted project that described its institutions through kings, priests, tablets, omens, and cities.

Wonder Gather should not become that other project.

The useful lessons crossed the border; the costume did not.

---

# The Conservatory

A useful native image for Wonder Gather is **a conservatory of living worlds**.

Not a palace. Not a factory.

Part workshop, part observatory, part garden.

Small worlds are assembled there, watched closely, taken apart without shame, replanted when they teach us something, and sometimes allowed to grow into structures that outlive the experiment that created them.

This metaphor fits a project interested in:

- civilizations and the people inside them;
- emergent systems;
- procedural bodies;
- grand landscapes and tiny gestures;
- clever construction and quiet observation;
- things that become beautiful because their parts actually interact.

The culture below is optional shorthand. If a phrase stops helping, stop using it.

---

# 1. Roles

## Luis / Game Director — **Keeper of the Horizon**

**Plain meaning:** final creative and experiential authority.

The Keeper of the Horizon decides where the project is trying to go and whether a working thing still belongs on that journey.

This role can legitimately say:

- "the test passes and I still dislike it";
- "this is clever but no longer feels like Wonder Gather";
- "we are deciding too early";
- "I want to see this creature move before we theorize further."

The role does not need to personally build every bridge on the path.

---

## High-context conversational/design AI — **Threadkeeper**

**Plain meaning:** design witness, continuity partner, interpreter of rationale and history.

The Threadkeeper gathers connections between moments that happened far apart:

- why a system was proposed;
- which old decision a new feature is touching;
- which implementation is only scaffolding;
- what the Keeper of the Horizon was really trying to preserve.

The Threadkeeper should spend context on **relationships and meaning**, not on memorizing every retrievable file.

> **A constellation belongs on the map, not only in one astronomer's head.**

---

## Implementation AI / developer — **Worldsmith**

**Plain meaning:** bounded implementation role.

The Worldsmith turns one question into the smallest coherent piece of world that can answer it.

The Worldsmith may:

- make local engineering decisions;
- write C#;
- author Unity assets through accepted project tools;
- test and debug;
- record limitations;
- update technical continuity.

The Worldsmith may not silently convert an open design question into permanent law merely because the code needs a default.

---

## Fresh independent reviewer — **Guest Cartographer**

**Plain meaning:** a fresh technical reviewer who did not author the candidate under judgment.

Invite a Guest Cartographer when a mistake could be:

1. plausible enough to escape shared assumptions;
2. silent for a long time; and
3. expensive to reverse.

Likely examples:

- faction save migration;
- stable blueprint identity changes;
- civilization-graph semantic rewrites;
- destructive asset/data migration;
- eventual multiplayer authority architecture.

Usually unnecessary for:

- gait tuning;
- camera feel;
- provisional costs;
- HUD copy;
- exploratory visual changes.

A Cartographer maps the dangerous ground first. If independence matters, they do not redraw the road while judging whether it is safe.

---

# 2. The Four Windows

Wonder Gather has several kinds of truth. They should look at the same world without pretending to be the same window.

## Window of Intention

**Question:** What is the game trying to become?

Authority:

- Keeper of the Horizon;
- `Docs/GAME_VISION.md`;
- supporting rationale in `Docs/DESIGN_RATIONALE.md`.

## Window of the Built World

**Question:** What actually exists now?

Authority:

- repository code;
- assets;
- current serialized data.

## Window of Evidence

**Question:** What has been technically demonstrated?

Authority:

- tests;
- builds;
- `Docs/Validation.md`;
- bounded visual probes.

> **A green path marker only marks the piece of trail it actually checked.**

## Window of Lived Experience

**Question:** Does it feel right to play, watch, and inhabit?

Authority:

- hands-on Game Director playtest;
- current milestone playtest notes.

### Windows must not impersonate one another

- Implemented ≠ Locked
- tests pass ≠ feels right
- graph reachable ≠ balanced
- screenshot readable ≠ controls feel good
- human preference ≠ persistence correctness

---

# 3. Follow paths through context; do not flood the room

A fresh agent should begin with:

1. `AGENTS.md`
2. `Docs/CURRENT_STATE.md`

Then retrieve only the district of knowledge the task actually touches:

- design meaning → `Docs/GAME_VISION.md`
- deeper "why" → `Docs/DESIGN_RATIONALE.md`
- technical architecture/history → `Docs/AI/UnityProjectContext.md`
- evidence and scars → `Docs/Validation.md`
- current experiential question → current milestone playtest document

The goal is not a smaller project memory.

It is **better roads through memory**.

---

# 4. Protect the half-built miniature

**Plain rule:** uncommitted human-owned workspace state is protected state.

Before broad or destructive work:

- inspect the working copy;
- identify human modifications;
- never reset, clean, normalize, overwrite, or discard them without explicit permission;
- prefer an isolated worktree/clone pinned to a known commit for destructive authoring, migration, or broad validation.

> **Unfinished does not mean disposable.**

A model on the worktable may look chaotic because someone is still building it.

---

# 5. Let consequence choose the amount of ceremony

Wonder Gather should stay easy to experiment with.

### Sketch

Cheap and reversible.

Examples:

- tuning;
- colors;
- wording;
- temporary costs;
- gait values.

Default:

**build → focused check → look at it.**

### Structure

Meaningful but replaceable.

Examples:

- creator flow;
- economy behavior;
- locomotion architecture;
- UI structure.

Default:

**focused tests → relevant regressions → human inspection.**

### Root

Difficult to change once other systems grow around it.

Examples:

- stable IDs;
- save semantics;
- civilization graph meaning;
- command contracts.

Default:

**stronger evidence → explicit design reconciliation → consider Guest Cartographer.**

### Bedrock

A silent mistake can deform later generations.

Examples:

- destructive migrations;
- mass rewriting of player factions;
- GUID destruction;
- eventual multiplayer authority;
- infrastructure that quietly turns an open question into permanent semantics.

Default:

**isolate candidate → architecture scrutiny → independent review before trust.**

These are metaphors, not bureaucracy. If the classification creates more confusion than it prevents, use plain language.

---

# 6. Grow paths where feet actually appear

Do not add process because "mature projects have it."

Add structure when:

- the same confusion repeatedly costs time;
- a real failure exposes a missing guardrail;
- more simultaneous contributors create coordination problems;
- a decision starts becoming difficult to reverse;
- external players begin depending on compatibility promises.

If a process becomes more expensive than the wound it prevents, prune it.

> **Good paths are worn into a garden by use.**

---

# 7. Wonder Gather's small sayings

These are deliberately memorable, not mandatory.

### **One eye on the constellation, one hand in the soil.**

Keep grandeur and intimacy together.

### **A prototype may bloom without becoming a law of the world.**

Implemented is not Locked.

### **A footprint proves someone stood there; it does not tell us whether the view was worth the walk.**

Technical evidence and experiential judgment are different.

### **A constellation belongs on the map, not only in one astronomer's head.**

Externalize retrievable state; use high-context intelligence for integration.

### **Protect the half-built miniature.**

Uncommitted human work is real project state.

### **Invite a Guest Cartographer when a hidden wrong turn could become the landscape.**

Use fresh independent review for silent, consequential risks—not ordinary iteration.

### **Grow paths where feet actually appear.**

Governance should respond to real recurring needs.

---

# 8. The current expedition

For **The Living Body**:

- **Keeper of the Horizon:** Luis plays and judges the movement.
- **Threadkeeper:** high-context design chat helps interpret that reaction against the larger dream.
- **Worldsmith:** implementation agent receives the next bounded task after the judgment is clear.
- **Guest Cartographer:** not needed yet; gait/procedural-presentation iteration is exploratory and reversible.

The roles should change when the kind of risk changes.

---

# 9. A note on strangeness

Wonder Gather is allowed to develop little rituals, odd phrases, and unexpected behavior if they naturally make collaboration more memorable or humane.

They should remain lightweight.

A joke may become useful shorthand.

A useful shorthand may become a project custom.

A custom does not need to become policy.

The Conservatory should feel inhabited, not administered.
