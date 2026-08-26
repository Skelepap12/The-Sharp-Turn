from pathlib import Path

form_path = Path('Code/TheSharpTurn/UI/Form1.cs')
road_path = Path('Code/TheSharpTurn/Motor Vehicle/RoadUser.cs')
checklist_path = Path('Documentation/NEXT_PHASE_TASKS.md')
workflow_path = Path('.github/workflows/phase7-road-rules-patch.yml')
script_path = Path('Tools/phase7_patch.py')

form = form_path.read_text(encoding='utf-8')

old_update = '''        private void UpdateRoadUser(RoadUser roadUser)
        {
            RoadUser blocker = FindRoadUserAhead(roadUser);

            if (blocker != null)
            {
                int gap = GetForwardGap(roadUser, blocker);
                bool blockerIsSlower = blocker.ActualSpeed < roadUser.DesiredSpeed ||
                    blocker.DesiredSpeed < roadUser.DesiredSpeed;

                if (blockerIsSlower && gap <= roadUser.DesiredSpeed + 12)
                    TryAutomaticRoadLaneChange(roadUser);
            }

            MoveAtBestSpeed(roadUser);
        }
'''
new_update = '''        private void UpdateRoadUser(RoadUser roadUser)
        {
            if (roadUser.IsOvertaking)
            {
                if (roadUser.Lane == roadUser.OvertakeReturnLane)
                {
                    roadUser.IsOvertaking = false;
                    roadUser.OvertakeReturnLane = -1;
                }
                else if (TryMoveRoadUserToLane(roadUser,
                    roadUser.OvertakeReturnLane))
                {
                    roadUser.IsOvertaking = false;
                    roadUser.OvertakeReturnLane = -1;
                }
            }

            if (!roadUser.IsOvertaking)
            {
                RoadUser blocker = FindRoadUserAhead(roadUser);

                if (blocker != null)
                {
                    int gap = GetForwardGap(roadUser, blocker);
                    bool blockerIsSlower = blocker.ActualSpeed < roadUser.DesiredSpeed ||
                        blocker.DesiredSpeed < roadUser.DesiredSpeed;

                    if (blockerIsSlower && gap <= roadUser.DesiredSpeed + 12)
                        TryRegularRoadOvertake(roadUser);
                }
            }

            MoveAtBestSpeed(roadUser);
        }
'''

old_emergency = '''        private void UpdateEmergencyVehicle(EmergencyVehicle emergency)
        {
            if (!emergency.SirenOn)
            {
                UpdateRoadUser(emergency);
                return;
            }

            RoadUser blocker = FindRoadUserAhead(emergency);

            if (blocker != null)
            {
                int gap = GetForwardGap(emergency, blocker);

                if (gap <= emergency.DesiredSpeed + 14 && blocker != manualObject)
                    TryAutomaticRoadLaneChange(blocker);
            }

            MoveAtBestSpeed(emergency);
        }
'''
new_emergency = '''        private void UpdateEmergencyVehicle(EmergencyVehicle emergency)
        {
            if (!emergency.SirenOn)
            {
                UpdateRoadUser(emergency);
                return;
            }

            RoadUser blocker = FindRoadUserAhead(emergency);

            if (blocker != null)
            {
                int gap = GetForwardGap(emergency, blocker);

                if (gap <= emergency.DesiredSpeed + 14 && blocker != manualObject)
                {
                    if (TryEmergencyRoadLaneChange(blocker))
                    {
                        blocker.IsOvertaking = false;
                        blocker.OvertakeReturnLane = -1;
                    }
                }
            }

            MoveAtBestSpeed(emergency);
        }
'''

old_auto = '''        private bool TryAutomaticRoadLaneChange(RoadUser roadUser)
        {
            switch (roadUser.Lane)
            {
                case 0:
                    return TryMoveRoadUserToLane(roadUser, 1);

                case 1:
                    if (TryMoveRoadUserToLane(roadUser, 2))
                        return true;
                    return TryMoveRoadUserToLane(roadUser, 0);

                case 2:
                    return TryMoveRoadUserToLane(roadUser, 1);

                case 3:
                    return TryMoveRoadUserToLane(roadUser, 4);

                case 4:
                    if (TryMoveRoadUserToLane(roadUser, 3))
                        return true;
                    return TryMoveRoadUserToLane(roadUser, 5);

                case 5:
                    return TryMoveRoadUserToLane(roadUser, 4);
            }

            return false;
        }
'''
new_auto = '''        private bool TryRegularRoadOvertake(RoadUser roadUser)
        {
            int passingLane = -1;

            if (roadUser.Direction == TravelDirection.Left)
            {
                if (roadUser.Lane == 0)
                    passingLane = 1;
                else if (roadUser.Lane == 1)
                    passingLane = 2;
            }
            else
            {
                if (roadUser.Lane == 5)
                    passingLane = 4;
                else if (roadUser.Lane == 4)
                    passingLane = 3;
            }

            if (passingLane < 0)
                return false;

            int returnLane = roadUser.Lane;

            if (!TryMoveRoadUserToLane(roadUser, passingLane))
                return false;

            roadUser.OvertakeReturnLane = returnLane;
            roadUser.IsOvertaking = true;
            return true;
        }

        private bool TryEmergencyRoadLaneChange(RoadUser roadUser)
        {
            switch (roadUser.Lane)
            {
                case 0:
                    return TryMoveRoadUserToLane(roadUser, 1);

                case 1:
                    if (TryMoveRoadUserToLane(roadUser, 2))
                        return true;
                    return TryMoveRoadUserToLane(roadUser, 0);

                case 2:
                    return TryMoveRoadUserToLane(roadUser, 1);

                case 3:
                    return TryMoveRoadUserToLane(roadUser, 4);

                case 4:
                    if (TryMoveRoadUserToLane(roadUser, 3))
                        return true;
                    return TryMoveRoadUserToLane(roadUser, 5);

                case 5:
                    return TryMoveRoadUserToLane(roadUser, 4);
            }

            return false;
        }
'''

old_manual = '''            if (obj is RoadUser)
            {
                RoadUser roadUser = (RoadUser)obj;
                changed = TryMoveRoadUserToLane(roadUser,
                    roadUser.Lane + verticalDirection);
            }
'''
new_manual = '''            if (obj is RoadUser)
            {
                RoadUser roadUser = (RoadUser)obj;
                changed = TryMoveRoadUserToLane(roadUser,
                    roadUser.Lane + verticalDirection);

                if (changed)
                {
                    roadUser.IsOvertaking = false;
                    roadUser.OvertakeReturnLane = -1;
                }
            }
'''

for old, new, label in [
    (old_update, new_update, 'UpdateRoadUser'),
    (old_emergency, new_emergency, 'UpdateEmergencyVehicle'),
    (old_auto, new_auto, 'automatic lane change'),
    (old_manual, new_manual, 'manual road lane change')
]:
    if old not in form:
        raise SystemExit('Could not find expected block: ' + label)
    form = form.replace(old, new, 1)

form_path.write_text(form, encoding='utf-8')

road = road_path.read_text(encoding='utf-8')
old_road = '''    public abstract class RoadUser : TrafficObject
    {
        public RoadUser()
            : base()
        { }

        public RoadUser(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  actualSpeedVal, widthVal, heightVal)
        { }
    }
'''
new_road = '''    public abstract class RoadUser : TrafficObject
    {
        int overtakeReturnLane;
        bool isOvertaking;

        public RoadUser()
            : base()
        {
            OvertakeReturnLane = -1;
            IsOvertaking = false;
        }

        public RoadUser(int xVal, int yVal, int laneVal,
            TravelDirection directionVal, int desiredSpeedVal,
            int actualSpeedVal, int widthVal, int heightVal)
            : base(xVal, yVal, laneVal, directionVal, desiredSpeedVal,
                  actualSpeedVal, widthVal, heightVal)
        {
            OvertakeReturnLane = -1;
            IsOvertaking = false;
        }

        public int OvertakeReturnLane
        {
            get
            {
                return overtakeReturnLane;
            }
            set
            {
                overtakeReturnLane = value;
            }
        }

        public bool IsOvertaking
        {
            get
            {
                return isOvertaking;
            }
            set
            {
                isOvertaking = value;
            }
        }
    }
'''
if old_road not in road:
    raise SystemExit('Could not find expected RoadUser.cs body')
road_path.write_text(road.replace(old_road, new_road, 1), encoding='utf-8')

checklist_path.write_text('''# Next Phase Tasks

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
''', encoding='utf-8')

if workflow_path.exists():
    workflow_path.unlink()
if script_path.exists():
    script_path.unlink()
