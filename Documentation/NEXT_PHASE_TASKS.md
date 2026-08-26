# Next Phase Tasks

## Phase 7 - Current Regression Checks

- Verify regular RoadUsers try to overtake on the left side of their direction of travel first.
- Verify they use the right lane only when the left lane is unavailable or they are already in the leftmost lane.
- Verify a RoadUser matches the slower vehicle ahead when neither adjacent passing lane is available.
- Verify a RoadUser returns to the lane it left after safely completing a regular overtake.
- Verify emergency-vehicle siren yielding still allows the blocking RoadUser to move to either safe adjacent lane as before.
- Verify manual lane changes cancel any pending automatic return-to-lane state.
- Verify A/Left and D/Right are relative to travel direction: controls are reversed for objects travelling Left.
- Verify bicycle passing, oncoming-bike safety and return-to-correct-lane behavior.
- Verify pedestrian overtaking stays within the same sidewalk pair and head-on pedestrians keep right.
- Verify a mixed scene survives Save/Load with type, model, lane, direction, position, speed and SirenOn state intact.
- Add the actual WAV assets listed in `Assets/Audio/README.md` and test every manual-control sound mapping.
- Fix only bugs found during these tests, then freeze Phase 7 functionality.

## Phase 8 - Final GUI and Graphics

- Add a proper full-screen option while keeping normal windowed mode available.
- Make the map and GUI scale cleanly when switching between windowed and full-screen.
- Add Manual Control Focus Mode: while manually controlling an object, hide the normal GUI and show only a clean control/instructions overlay over the simulation.
- Give Focus Mode a context title based on the controlled entity: `DRIVE MODE`, `EMERGENCY DRIVE MODE`, `CYCLE MODE` or `WALK MODE`.
- Keep Escape, simulation-area mouse click and leaving the map as ways to exit Manual Control Focus Mode.
- Fix the selected-object speed display so it shows `ActualSpeed / MaximumSpeed` rather than `ActualSpeed / DesiredSpeed` (for example, a pedestrian at speed 1 should show `1 / 2`).
- If useful, display DesiredSpeed separately so the GUI does not confuse the requested speed with the entity's maximum speed.
- Replace temporary entity drawings with the selected real sprite assets.
- Integrate MetroCity-style environment graphics without changing simulation lane geometry.
- Polish selected-object information, creation controls, button sizing, spacing, fonts and layout.
- Improve road, sidewalk and bicycle-path presentation.
- Keep graphics/presentation separate from simulation Bounds and lane logic.

## Phase 9 - Final Testing and Submission

- Full behavior regression test.
- Save/load verification.
- Test Manual Control Focus Mode and full-screen/windowed switching.
- Test all final audio and sprite mappings.
- Course-requirement and lecturer-style audit.
- Remove genuinely unused/dead code only after functionality is frozen.
- Refresh the Project Bible/documentation.
- Final Windows build and submission package.
