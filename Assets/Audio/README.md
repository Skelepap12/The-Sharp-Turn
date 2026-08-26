# The Sharp Turn - Audio Asset Plan

Phase 7 audio will stay simple and course-level: `System.Media.SoundPlayer` with WAV files only. Only the manually controlled entity should produce audio.

## Planned roles

- `engine_loop.wav` - road vehicle engine loop. Candidate: OpenGameArt, "racing car engine sound loops" by domasx2, CC0.
- `car_horn.wav` - normal road vehicle horn. Candidate: OpenGameArt, "Car signal" by Yaroslav_Novikov, CC0 (`car2.wav`).
- `siren.wav` - emergency vehicle siren. Candidate: Freesound, "police siren.wav" by vlammenos, CC0, loopable WAV.
- `bike_horn.wav` - bicycle horn. Candidate: OpenGameArt, "Bicycle Horn" by AntumDeluge, CC0 (`bicycle-horn-1.wav`).
- `bike_loop.wav` - bicycle movement / spoke / chain sound. Candidate: OpenGameArt, "Bicycle Sounds" by AntumDeluge, CC0.
- `footsteps.wav` - pedestrian walking sound. Candidate: OpenGameArt CC0 footsteps packs; select a short WAV loop before integration.
- `pedestrian_shout.wav` - pedestrian H-action. Candidate: Freesound, "HEY VERY LOW 4.WAV" by metrostock99, CC0.

## Integration rules

- Do not serialize `SoundPlayer` objects or sound resources.
- AI-controlled entities remain silent.
- Manual road users: engine loop + horn.
- Manual emergency vehicles: engine loop + siren instead of horn.
- Manual bicycles: bike movement loop + bike horn.
- Manual pedestrians: footsteps + shout.
- Keep sound behavior deliberately simple; no pitch shifting, spatial audio, audio engine, or mixer framework.

The final selected files and exact source links should be recorded here before submission.
