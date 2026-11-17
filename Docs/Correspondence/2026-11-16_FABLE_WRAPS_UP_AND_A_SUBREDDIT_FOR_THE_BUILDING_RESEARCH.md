# Fable wraps up and passes control to Opus 5.5; a subreddit for the building research (November 16, 2026, evening)

Luis's messages, word for word. They came while the review of his friend's harness guide
([the request](2026-11-15_THE_FRIENDS_HARNESS_GUIDE.md)) was being finished, after a status report that the five-hour
allowance was nearly used and that the independent check would run after its reset.

## 1. Wrap up and pass control

> Okay perfect! Also the next reset will be the last one that I'll have access to you Fable. So try to wrap it all up on that reset and then pass the control to Opus 5.5 Max Ultracode and let it  continue with the project and the previous plans. Oh, also one more message I wanted to leave already for it since I think I'll be sleeping by the time the reset comes back:

## 2. The message left for Opus 5.5

> Just for the super deep dive research into how to build stuff and all of that, when is the time to do that, I would also like to give my recommendation to within it do a super deep dive research into the subreddit called I think it was called Topology AI, since that is a subreddit which has people basically using AIs and mostly in large part Claude to build many games and things and software. And so I just wanted to make sure that we really have a complete research, also do a super deep dive research within, when it's time to do that research, do a deep dive research within it to see if there are any tools or approaches that people use that seem to that line up with our approach to max quality and to our style and to the game that we want to build, and that, I don't know, just to see if there's any mention there of something that could be useful to us in some way. So just a heads up and hint, that's it.

## 3. After the allowance ran out in the middle of a turn

> Try again

## What this asks, in plain words

1. **Fable finishes the guide review in this last window, then the session goes to Opus 5.5 at max effort with
   ultracode, which carries on with the project and the plans already made** (the ankle build first; the queue is in
   [Handoff.md](../Handoff.md), "Start here"). Luis expected to be asleep, so Opus starts without waiting for him.
2. **A recommendation for the deep research into how things are built** (the search for ways of making assets that
   comes before the world pass, and the later, bigger exploration): when that research is done, it includes a deep dive
   into a subreddit where people build games and software with AI, largely with Claude. Luis was not sure of its name
   ("I think it was called Topology AI"), so the name is found first and not assumed. What to look for there: tools
   and approaches that fit this project's aim of the highest quality, its style and the game itself, and any mention
   of something useful. It is a hint for that research when its time comes, not a new task now.

Nothing here changes a design decision, so there is no row in `GAME_VISION.md`.

## 4. November 17, after the five-hour reset (about 03:00 for Luis)

Fable's window ran out before its commit. Luis switched the session's model to Opus 5.5 himself (the model menu), then
wrote, word for word:

> Okay. Fable cannot be used now anymore. It has done it's last valuable contribution for us. Please finish what it was doing and then proceed with the project as we were before.

What was done with it: Opus 5.5 (read back from the session: `claude-opus-5-5`, effort `max`, ultracode on) mended the
lines that described a handover by a scheduled task, which was never made and is no longer needed, committed Fable's
work, and went on with the ankle build.

## 5. November 17, morning

Later, after a five-hour reset, Luis wrote, word for word:

> I hit my usage limit while you were working, but it has reset now. Please continue from where you left off.

What was done with it: the ankle build went on from the Handoff's "Start here": the baselines as played, then Small's
arm B.

A few minutes after the step 4a workflow began (08:15), he wrote, word for word:

> By the way, I will need to leave home by 9, so actually please make sure to wrap up all you're doing by then since I'll need to bring my computer this time.

What was done with it: the workflow was stopped at about 08:20, while its builder was still reading. It had written
nothing but an empty report, which was removed. `D:/Dev/WG` was left unchanged at `e8bac96`, with no Unity running.
The workflow's script was saved in the ankle research folder, to be run again as it is. The Handoff, CURRENT_STATE
and a memory note record this.

At about 08:50 he wrote, word for word:

> Actually I won't need to bring my laptop out anymore. So you can resume the tasks without needing to worry about time limit.

What was done with it: step 4a was launched again from its saved script, unchanged, at about 08:52.

## 6. November 17, late morning: a check for the profile folder's path

The path of Luis's own profile folder, which names the person, had reached two notes and was taken out of them.
I then asked:

> May I add a check that blocks any commit containing that path? It's a change to the project's settings, so I'm asking first.

Luis answered, word for word:

> Sure to answer your question.

What was done with it: G6 was begun, a rule of the guard hook (`.claude/hooks/guard.sh`) that refuses a git commit
or push which would carry that path. Its checker, `guard_profile.py` beside the guard, takes the path from the
machine as it runs and writes it in no file. The approval covers this rule only: any other change to the hooks or
the settings is asked again. Found while preparing it: two models already pushed, `House.fbx` and `Nature.fbx` in
`Assets/_WonderGather/Art/OrdinaryPlace/`, carry that path, from the temporary folder they were exported through;
what to do about them is Luis's call.

I asked which of three ways Luis wanted: (a) leave them, the new check stopping any new version that still carries
the path; (b) export the two models again without it, the same models with the same links in the scenes, the old
versions staying in GitHub's history; (c) b, and erase the old versions from the history too, which rewrites every
later commit on both branches and cannot be undone. At about 10:50 Luis answered, word for word:

> Yes I think b would be better please.

What is done with it: the two models are exported again without the path, after the ankle mend's launch at the
five-hour reset; the history is left as it is.

Nothing here changes a design decision, so there is no row in `GAME_VISION.md`.
