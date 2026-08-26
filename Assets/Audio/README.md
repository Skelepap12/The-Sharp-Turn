# The Sharp Turn - Audio Asset Plan

Phase 7 audio stays simple and course-level: `System.Media.SoundPlayer` with WAV files only. Only the manually controlled entity produces audio.

## Selected source pages

- `car_engine_loop.wav` - OpenGameArt, **racing car engine sound loops** by domasx2, CC0. Use one of the supplied WAV loop variants as the passenger-car engine. Source: https://opengameart.org/content/racing-car-engine-sound-loops
- `bus_engine_loop.wav` - Freesound, **busSnd_002.wav** by tec_studio, CC0. It is a short recording of a USA transit-bus engine and is already a WAV file, so it fits the bus sound well without conversion. Source: https://freesound.org/people/tec_studio/sounds/107273/
- `motorcycle_engine_loop.wav` - Freesound, **motorcycle_20** by Lauri_Lehtonen, CC0. It is a five-second mono WAV of a motorcycle idling and is compact enough for the project. Source: https://freesound.org/people/Lauri_Lehtonen/sounds/714062/
- `emergency_engine_loop.wav` - Freesound, **Fire Truck - Idle** by Filmscore, CC0. It is a WAV recording of a parked fire-truck engine and gives the emergency class a heavier sound than normal cars when the siren is off. Source: https://freesound.org/people/Filmscore/sounds/268526/
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

## Download note

The OpenGameArt files can be downloaded directly from their pages. Freesound requires signing in before downloading the original file. After download, keep the chosen sound as WAV and rename it to the runtime filename above. If a source does not loop cleanly in testing, trim a steady middle section in a basic audio editor rather than adding any new audio framework to the project.

The project copies WAV files from `Assets/Audio/` into the build output when they are present.
