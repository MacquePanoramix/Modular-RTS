# Proposed implementation plan — S1: The worker model and art direction

**Proposed:** October 1, 2026.
**Baseline:** Strength and Burden checkpoint A, plus the arrival fix. Unity
6000.6.0f1 / URP; Blender 4.4/4.5 on Luis's computer.
**Status:** Approved by Luis on October 1: model first (O4), with the art
direction to be chosen from rendered style studies (O1). Step 1 is done: three
directions rendered in [StyleStudies.md](StyleStudies.md). Waiting for Luis's
choice before modeling. The overall plan is in
[ShowcaseRoadmap.md](ShowcaseRoadmap.md); the previous plan is archived in
[Plans/StrengthAndBurden.md](Plans/StrengthAndBurden.md).

## The question

Can a high-quality worker model made in Blender be driven by the procedural
body without losing the grounded motion of checkpoint A? And what should
Wonder Gather's showcase look like?

## Why this comes first

Every later stage poses this body:

- the creator's sliders;
- the swing;
- the hands that hold tools and loads;
- the hauling gear.

Doing those on the primitive biped would mean redoing them once a real model
arrives. The primitive rig also has unusual proportions: the pelvis sits about
1.43 m high and the figure is about 2.2 m tall. A human-proportioned worker
needs the rig's dimensions to come from the body, not from constants. Fixing
that now is what later makes height and build sliders possible.

## Scope

1. **Style studies.** Render the same small vignette (a worker figure, a
   pickaxe, an ore boulder and some ground) in two or three directions, for
   example soft stylized, semi-realistic and faceted low-poly. Luis chooses
   one, mixes them, or provides references. Nothing is modeled in final
   quality until this choice is made.
2. **The worker model.**
   - Built in Blender in the chosen direction, as reproducible source: the
     `.blend` file and Python generation/export scripts live in the repository
     under `Art/`, outside Unity's `Assets`. The exported model goes under
     `Assets/_WonderGather/Art/`.
   - A skeleton matching the procedural rig: pelvis, spine, chest, neck, head,
     upper arms, forearms, hands, thighs, shins, feet and toes.
   - Weight-painted skin, plus shape keys for build (lean, heavy, muscular)
     prepared for the S2 creator.
   - One complete, neutral work outfit and head.
3. **A better standard pickaxe.** Modeled to match the chosen style. Its grip
   and striking-head points are exported so the tool data matches the visible
   handle and head.
4. **A body that comes from the model.**
   - Replace the rig's fixed constants with dimensions read from the skeleton:
     hip height and width, thigh, shin and foot lengths, torso, shoulders,
     and arm lengths.
   - The gait already scales with hip height. Stride, cadence and the walk/jog
     threshold follow automatically.
   - Navigation radius and height, the selection ring and camera framing
     follow the new body.
5. **Driving the bones.** A rig adapter applies the procedural solution (feet
   and roll, legs, pelvis, chest twist, head, arms, grips) to the model's bones
   instead of stretching primitive segments. The primitive rig stays available
   for tests and side-by-side comparison.
6. **Where it appears.** The model is used in TheLivingBody and in the
   equipment map. The faction creator stays as it is until the S2 creator
   replaces it in the showcase flow.

### Outside S1

- The creator interface and its sliders (S2).
- Clothing and hair variety beyond one outfit (S2).
- The swing redesign (S3).
- Hauling gear (S4).
- The landscape and sound (S5).

The swing is unchanged in S1. It will look wrong on the new body until S3.

## Contracts to keep

- **Planted feet.** They keep fixed world-space support frames. A walk always
  has a support foot; only a jog has brief flight. Arrival finishes the stride
  without shuffling.
- **Grips.** Hands reach the tool's actual grips. Reach is judged from the
  actual shoulders.
- **Body–navigation split.** Navigation owns the root and the body only
  presents it. There is still no active ragdoll (D1).
- **Saves and IDs.** No change to faction saves or stable IDs.

## Required evidence

- **Tests.** The gait, support and grip tests run on the model rig as well as
  the primitive rig. New checks: bone lengths are preserved, and feet and
  hands land where the solver puts them.
- **Regression.** The full PlayMode suite passes.
- **Visual review.** Rendered stills of walking, jogging, standing and
  carrying on the model, plus a short recorded sequence if possible.
- **Build.** A Windows build.
- **Licensing.** Any third-party asset or add-on is downloaded only with Luis's
  approval, and its license is recorded.

## Decisions needed from Luis

- **O1 — Art direction.** Choose from the rendered style studies, or point to
  references you already have.
- **O4 — Stage order.** Model first (this plan, recommended) or creator first.
- **O6 — Sourcing.** A semi-realistic human may be best built with the free
  MakeHuman/MPFB Blender add-on, whose generated bodies are CC0. Downloading it
  needs your approval. A stylized worker can be modeled from scratch.
