from pathlib import Path
import shutil
import time
import urllib.request

root = Path('.')
project = root / 'Code' / 'TheSharpTurn'
ui = project / 'UI'
gui = project / 'GUI'
audio_code = project / 'Audio'

# Folder cleanup requested for the final source layout.
if ui.exists():
    if gui.exists():
        raise SystemExit('GUI folder already exists while UI still exists.')
    ui.rename(gui)

audio_code.mkdir(exist_ok=True)
old_audio = gui / 'Form1.Audio.cs'
new_audio = audio_code / 'Form1.Audio.cs'
if old_audio.exists():
    if new_audio.exists():
        raise SystemExit('Audio/Form1.Audio.cs already exists.')
    shutil.move(str(old_audio), str(new_audio))

form_path = gui / 'Form1.cs'
form = form_path.read_text(encoding='utf-8')

old_update = '''        private void UpdateRoadUser(RoadUser roadUser)
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
                    {
                        if (!TryRegularRoadOvertake(roadUser))
                        {
                            MoveAtBestSpeed(roadUser, blocker.ActualSpeed);
                            return;
                        }
                    }
                }
            }

            MoveAtBestSpeed(roadUser);
        }
'''

old_overtake = '''        private bool TryRegularRoadOvertake(RoadUser roadUser)
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
'''

new_overtake = '''        private bool TryRegularRoadOvertake(RoadUser roadUser)
        {
            int leftLane;
            int rightLane;

            if (roadUser.Direction == TravelDirection.Right)
            {
                leftLane = roadUser.Lane - 1;
                rightLane = roadUser.Lane + 1;
            }
            else
            {
                leftLane = roadUser.Lane + 1;
                rightLane = roadUser.Lane - 1;
            }

            int returnLane = roadUser.Lane;

            if (!TryMoveRoadUserToLane(roadUser, leftLane) &&
                !TryMoveRoadUserToLane(roadUser, rightLane))
                return false;

            roadUser.OvertakeReturnLane = returnLane;
            roadUser.IsOvertaking = true;
            return true;
        }
'''

old_move = '''        private void MoveAtBestSpeed(TrafficObject obj)
        {
            int allowedSpeed = obj.DesiredSpeed;

            while (allowedSpeed > 0 &&
                !trafficObjects.IsAreaFree(GetMovedBounds(obj, allowedSpeed), obj))
            {
                allowedSpeed--;
            }

            obj.ActualSpeed = allowedSpeed;

            if (obj.ActualSpeed > 0)
                obj.Move();
        }
'''

new_move = '''        private void MoveAtBestSpeed(TrafficObject obj)
        {
            MoveAtBestSpeed(obj, obj.DesiredSpeed);
        }

        private void MoveAtBestSpeed(TrafficObject obj, int maximumSpeed)
        {
            if (maximumSpeed < 0)
                maximumSpeed = 0;

            int allowedSpeed = obj.DesiredSpeed;

            if (allowedSpeed > maximumSpeed)
                allowedSpeed = maximumSpeed;

            while (allowedSpeed > 0 &&
                !trafficObjects.IsAreaFree(GetMovedBounds(obj, allowedSpeed), obj))
            {
                allowedSpeed--;
            }

            obj.ActualSpeed = allowedSpeed;

            if (obj.ActualSpeed > 0)
                obj.Move();
        }
'''

old_keys = '''            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
            {
                TryManualLaneChange(manualObject, -1);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
            {
                TryManualLaneChange(manualObject, 1);
                e.Handled = true;
            }
'''

new_keys = '''            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
            {
                int laneDirection = -1;

                if (manualObject.Direction == TravelDirection.Left)
                    laneDirection = 1;

                TryManualLaneChange(manualObject, laneDirection);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
            {
                int laneDirection = 1;

                if (manualObject.Direction == TravelDirection.Left)
                    laneDirection = -1;

                TryManualLaneChange(manualObject, laneDirection);
                e.Handled = true;
            }
'''

for old, new, name in [
    (old_update, new_update, 'UpdateRoadUser'),
    (old_overtake, new_overtake, 'TryRegularRoadOvertake'),
    (old_move, new_move, 'MoveAtBestSpeed'),
    (old_keys, new_keys, 'manual A/D handling')
]:
    if old not in form:
        raise SystemExit('Could not find expected Form1.cs block: ' + name)
    form = form.replace(old, new, 1)

form_path.write_text(form, encoding='utf-8')

csproj_path = project / 'TheSharpTurn.csproj'
csproj = csproj_path.read_text(encoding='utf-8')
csproj = csproj.replace('UI\\Form1.cs', 'GUI\\Form1.cs')
csproj = csproj.replace('UI\\Form1.Designer.cs', 'GUI\\Form1.Designer.cs')
csproj = csproj.replace('''    <Compile Include="UI\\Form1.Audio.cs">
      <DependentUpon>Form1.cs</DependentUpon>
    </Compile>''', '    <Compile Include="Audio\\Form1.Audio.cs" />')
if 'UI\\' in csproj:
    raise SystemExit('An old UI path remains in TheSharpTurn.csproj.')
csproj_path.write_text(csproj, encoding='utf-8')

# Keep the Phase 7 regression notes aligned with the corrected passing rules.
next_tasks = root / 'Documentation' / 'NEXT_PHASE_TASKS.md'
text = next_tasks.read_text(encoding='utf-8')
old_regression = '''- Regular RoadUsers overtake only on the left relative to travel direction.
- A RoadUser already in the leftmost lane does not pass on the right; it slows/follows if blocked.
- After a regular overtake, the RoadUser returns to the lane it came from once that lane is safe.
'''
new_regression = '''- Regular RoadUsers try the left lane first when overtaking.
- If the left lane is unavailable or the RoadUser is already leftmost, it may overtake on the right.
- If neither adjacent lane is available, it matches the slower RoadUser ahead instead of forcing a lane change.
- After a regular overtake, the RoadUser returns to the lane it came from once that lane is safe.
'''
if old_regression in text:
    text = text.replace(old_regression, new_regression, 1)
next_tasks.write_text(text, encoding='utf-8')

# Download login-free CC0 WAV files so the project is immediately testable.
audio_assets = root / 'Assets' / 'Audio'
audio_assets.mkdir(parents=True, exist_ok=True)
base = 'https://opengameart.org/sites/default/files/'
downloads = {
    'car_engine_loop.wav': base + 'loop_2.wav',
    'bus_engine_loop.wav': base + 'loop_0.wav',
    'motorcycle_engine_loop.wav': base + 'scooter_p.wav',
    'emergency_engine_loop.wav': base + 'loop_1.wav',
    'car_horn.wav': base + 'car2.wav',
    'siren.wav': base + 'alarm_0.wav',
    'bike_horn.wav': base + 'bicycle-horn-1.wav',
    'bike_loop.wav': base + 'bones-2.wav',
    'footsteps.wav': base + 'snd_footsteps1.wav',
    'pedestrian_shout.wav': base + 'hey_aggressive_mujtaba.wav',
}

for target_name, url in downloads.items():
    target = audio_assets / target_name
    request = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0 TheSharpTurnCourseProject'})
    last_error = None
    for attempt in range(3):
        try:
            with urllib.request.urlopen(request, timeout=45) as response:
                data = response.read()
            if len(data) < 12 or data[0:4] != b'RIFF' or data[8:12] != b'WAVE':
                raise RuntimeError('Downloaded file is not a RIFF/WAVE file: ' + url)
            target.write_bytes(data)
            print(target_name, len(data), 'bytes')
            last_error = None
            break
        except Exception as exc:
            last_error = exc
            time.sleep(2)
    if last_error is not None:
        raise last_error

readme = '''# The Sharp Turn - Audio Assets

The project uses only standard `System.Media.SoundPlayer` with WAV files. Only the manually controlled object produces audio.

All WAV files currently included here are login-free **CC0** assets from OpenGameArt. They were renamed to stable runtime filenames so the C# code does not depend on the original download names.

## Runtime files and sources

- `car_engine_loop.wav` <- `loop_2.wav`, **racing car engine sound loops** by domasx2, CC0: https://opengameart.org/content/racing-car-engine-sound-loops
- `bus_engine_loop.wav` <- `loop_0.wav`, **racing car engine sound loops** by domasx2, CC0: https://opengameart.org/content/racing-car-engine-sound-loops
- `motorcycle_engine_loop.wav` <- `scooter_p.wav`, **Various Sound Effects** by Spring Spring, CC0: https://opengameart.org/content/various-sound-effects-0
- `emergency_engine_loop.wav` <- `loop_1.wav`, **racing car engine sound loops** by domasx2, CC0: https://opengameart.org/content/racing-car-engine-sound-loops
- `car_horn.wav` <- `car2.wav`, **Car signal** by Yaroslav_Novikov, CC0: https://opengameart.org/content/car-signal
- `siren.wav` <- `alarm.wav`, **Alarm** by Frenchyboy, CC0: https://opengameart.org/content/alarm-2
- `bike_horn.wav` <- `bicycle-horn-1.wav`, **Bicycle Horn** by AntumDeluge, CC0: https://opengameart.org/content/bicycle-horn
- `bike_loop.wav` <- `bones-2.wav`, **Bones 2** by AntumDeluge, CC0: https://opengameart.org/content/bones-2
- `footsteps.wav` <- `snd_footsteps1.wav`, **Various Sound Effects** by Spring Spring, CC0: https://opengameart.org/content/various-sound-effects-0
- `pedestrian_shout.wav` <- `hey_aggressive_mujtaba.wav`, **Aggressive NPC sounds** by mujtaba-io, CC0: https://opengameart.org/content/aggressive-npc-sounds-hey-i-will-kill-you

## Runtime behavior

- Car, Bus, Motorcycle and EmergencyVehicle use separate movement sounds.
- EmergencyVehicle uses the siren loop instead of its engine loop while `SirenOn` is true.
- RoadUsers use the car horn for the `H` action for now.
- Bicycle uses a movement/spoke sound and bicycle horn.
- Pedestrian uses footsteps and a short shout.
- AI-controlled objects are silent.
- Missing/broken sound files never stop the simulator from running.

These sounds are deliberately simple placeholders for the course project. They can be swapped later without changing any simulation or OOP logic because the runtime filenames stay the same.
'''
(audio_assets / 'README.md').write_text(readme, encoding='utf-8')

print('Phase 7 cleanup applied successfully.')
