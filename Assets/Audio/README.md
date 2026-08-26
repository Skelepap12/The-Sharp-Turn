# The Sharp Turn - Audio Asset Plan

Phase 7 audio stays simple and course-level: `System.Media.SoundPlayer` with WAV files only. Only the manually controlled entity produces audio.

## Selected source pages

- `car_engine_loop.wav` - passenger-car engine loop. We can use one of the CC0 loops from OpenGameArt, **racing car engine sound loops** by domasx2, as the car candidate. Source: https://opengameart.org/content/racing-car-engine-sound-loops
- `bus_engine_loop.wav` - heavier/lower bus engine loop. Select a fitting CC0 WAV before final integration.
- `motorcycle_engine_loop.wav` - motorcycle engine loop. Select a fitting CC0 WAV before final integration.
- `emergency_engine_loop.wav` - emergency-vehicle engine loop when the siren is off. Select a fitting CC0 WAV before final integration.
- `car_horn.wav` - OpenGameArt, **Car signal** by Yaroslav_Novikov, CC0 (`car2.wav`). Source: https://opengameart.org/content/car-signal
- `siren.wav` - Freesound, **police siren.wav** by vlammenos, CC0, loopable WAV. Source: https://freesound.org/people/vlammenos/sounds/52906/
- `bike_horn.wav` - OpenGameArt, **Bicycle Horn** by AntumDeluge, CC0 (`bicycle-horn-1.wav`). Source: https://opengameart.org/content/bicycle-horn
- `bike_loop.wav` - OpenGameArt, **Bicycle Sounds** by AntumDeluge, CC0. Source: https://opengameart.org/content/bicycle-sounds
- `footsteps.wav` - OpenGameArt, **Different steps on wood, stone, leaves, gravel and mud** by TinyWorlds, CC0. Source: https://opengameart.org/content/different-steps-on-wood-stone-leaves-gravel-and-mud
- `pedestrian_shout.wav` - Freesound, **HEY VERY LOW 4.WAV** by metrostock99, CC0. Source: https://freesound.org/people/metrostock99/sounds/345083/

## Runtime file names

The chosen WAV files should be renamed to the exact names below before they are added to `Assets/Audio/`:

- `car_engine_loop.wav`
- `bus_engine_loop.wav`
- `motorcycle_engine_loop.wav`
- `emergency_engine_loop.wav`
- `car_horn.wav`
- `siren.wav`
- `bike_horn.wav`
- `bike_loop.wav`
- `footsteps.wav`
- `pedestrian_shout.wav`

## Integration behavior

- `SoundPlayer` objects and sound resources are runtime-only and are not serialized.
- AI-controlled entities remain silent.
- Manual Cars use `car_engine_loop.wav`.
- Manual Buses use `bus_engine_loop.wav`.
- Manual Motorcycles use `motorcycle_engine_loop.wav`.
- Manual EmergencyVehicles use `emergency_engine_loop.wav` with the siren off and `siren.wav` with the siren on.
- Normal road users still use `car_horn.wav` for the `H` action for now; separate horns can be added later if wanted.
- Manual bicycles use a bicycle movement loop; `H` temporarily plays the bicycle horn.
- Manual pedestrians use a walking loop; `H` temporarily plays the shout.
- If an audio file is missing, the simulator keeps running normally without that sound.
- No pitch shifting, spatial audio, audio engine, mixer framework, or multithreading is used.

The project copies WAV files from `Assets/Audio/` into the build output when they are present.
