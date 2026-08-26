# Next Phase Tasks

## Phase 7 - Current Regression Checks

- Verify regular RoadUsers only overtake on the left side of their direction of travel.
- Verify a RoadUser returns to the lane it left after safely completing a regular overtake.
- Verify a RoadUser in the already-leftmost road lane slows behind traffic instead of passing on the right.
- Verify emergency-vehicle siren yielding still allows the blocking RoadUser to move to either safe adjacent lane as before.
- Verify manual lane changes cancel any pending automatic return-to-lane state.

## Phase 8 - Final UI and Graphics

- Add a proper full-screen option while keeping normal windowed mode available.
- Make the map and UI scale cleanly when switching between windowed and full-screen.
- Replace temporary entity drawings with the selected real sprite assets.
- Integrate MetroCity-style environment graphics without changing simulation lane geometry.
- Finish the manual-control overlay and context-sensitive instructions.
- Polish selected-object information, creation controls, spacing, fonts and layout.
- Improve road, sidewalk and bicycle-path presentation.
- Keep graphics/presentation separate from simulation Bounds and lane logic.

## Phase 9 - Final Testing and Submission

- Full behavior regression test.
- Save/load verification.
- Course-requirement and lecturer-style audit.
- Final Windows build and submission package.
