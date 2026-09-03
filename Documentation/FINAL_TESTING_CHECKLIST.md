# The Sharp Turn - Final Testing Checklist

The project is feature-complete. From this point onward, change functionality only when a real bug or requirement gap is found.

## Core assignment requirements

- Verify the custom inheritance hierarchy is present and used polymorphically.
- Verify TrafficObjectList is the collection object and supports the simulator's Add/Delete workflow.
- Verify Manual Control changes persistent object state and therefore serves as Modify.
- Verify the WinForms interface is event-driven.
- Verify every concrete entity is represented graphically.
- Verify a mixed scene saves and loads through the .tst serialization format.

## Entity and movement regression tests

- Test every Car model: SportsCar, Sedan and Hatchback.
- Test every Motorcycle model: SportsBike and Chopper.
- Test both Bus models: CityBus and IntercityBus.
- Test all EmergencyVehicle models: Ambulance, FireTruck and PoliceCar.
- Test every Bicycle model: Cruiser, BMX and MountainBike.
- Test both Pedestrian models: Male and Female.
- Verify road vehicles follow slower traffic, overtake safely and return to their original lane.
- Verify road vehicles do not enter the opposite carriageway.
- Verify emergency vehicles behave normally with the siren off and request safe yielding with the siren on.
- Verify bicycles pass only when the opposing bicycle lane is safe and return to the correct directional lane.
- Verify pedestrians overtake only within their own sidewalk pair and return to their correct directional lane.
- Verify off-screen spawning and removal work for both travel directions.

## Manual Control

- Verify W/Up increases DesiredSpeed only up to MaximumSpeed.
- Verify S/Down decreases DesiredSpeed.
- Verify A/Left and D/Right are relative to the object's travel direction.
- Verify Space brakes/stops the controlled object.
- Verify H performs the correct action for RoadUser, EmergencyVehicle, Bicycle and Pedestrian.
- Verify Escape, map click and leaving the map all exit Manual Control cleanly.
- Verify normal UI returns correctly after leaving Focus Mode.

## Save and load

- Save a mixed scene containing every entity family.
- Load it and verify type, model, position, lane, direction, DesiredSpeed, ActualSpeed and SirenOn state.
- Verify loading replaces the current scene.
- Verify selection and Manual Control runtime state are cleared after loading.
- Verify the simulation continues after loading.

## Presentation

- Verify the MetroCity map appears in both windowed and full-screen modes.
- Verify the red two-way bicycle path and road/pedestrian direction markings match the logical lane directions.
- Verify entity sprites face the correct direction.
- Verify selected-object highlighting and telemetry remain aligned with logical Bounds.
- Verify F11 can enter and leave full-screen repeatedly without breaking layout or input mapping.

## Audio

- Test car, bus and motorcycle idle/low/mid/high movement sounds in Manual Control.
- Test the normal road horn.
- Test emergency siren on/off behavior and FireTruck idle sound.
- Test bicycle movement sounds and horn.
- Test Male and Female pedestrian walking/shout sounds.
- Verify AI-controlled objects remain silent.
- Verify a missing audio file does not crash the program.

## Assets and submission

- Keep Assets/CREDITS.txt with the submitted project.
- Verify all runtime assets referenced by the program exist under Assets/.
- Verify no bin/, obj/, IDE cache or temporary generated files are included in the submission.
- Keep Lecturer Instructions/ and References/ out of the final hand-in package unless the lecturer specifically asks for them.
- Build the final project on Windows with Visual Studio/MSBuild.
- Run one final clean smoke test from the built output.
- Confirm git status is clean before creating the submission package.
