# BIG.Unity.Sonity

Audio plugin of the BIG ecosystem — a ready-to-use `SoundManager` with volume control and microphone
handling, built on top of [BIG.Unity](https://github.com/Big-Ice-Games/BIG.Unity) and the commercial
[Sonity](https://assetstore.unity.com/packages/tools/audio/sonity-audio-middleware-229857) audio middleware.

## Requirements

Installed BEFORE this package (none of them is redistributed here):

1. [BIG.Unity](https://github.com/Big-Ice-Games/BIG.Unity) — `https://github.com/Big-Ice-Games/BIG.Unity.git`
2. **Sonity** (paid, Asset Store) — you must own and import it.
3. **Odin Inspector** (paid) — you must own and import it.

## Installation

Window > Package Manager > `+` > `Install package from git URL`:

```
https://github.com/Big-Ice-Games/BIG.Unity.Sonity.git
```

## What's inside

### SoundManager
Wraps Sonity's SoundManager with three mixer channels — **music**, **effects**, **voice** — exposed as a simple static API:

```csharp
SoundManager.PlayMusic(soundEvent);
SoundManager.Play(soundEvent);
SoundManager.SetMusicVolume(0.8f);
SoundManager.EffectsSetActive(false);
```

Volumes and on/off states are persisted automatically through BIG's `IUserData` (PlayerPrefs by default)
and restored on start. Setup: drop the ready `Prefabs/SoundManager` prefab into your scene — it comes
wired with Sonity's SoundManager and `BigAudioMixer` (exposed `music`, `effects` and `voice` parameters).

Persisted keys are provided by `SoundUserDataKeysProvider` — run **BIG > Generate User Keys** to access
them as `Keys.Sound.MusicVolume` etc.

### ButtonSound
Drop-on component for UI buttons — plays Sonity `SoundEvent`s on hover enter/exit and click.

### MicrophoneDropdown
TMP dropdown listing available microphones with:
- microphone state toggle (`Enable` / `EnableWithButton` / `Disable`, persisted),
- live loudness bar (fill image) driven by the selected microphone,
- `Audio` utility class for loudness sampling from AudioSource/AudioClip.

### SonitySoundEventDrawer (Editor)
Odin drawer for `SoundEvent` fields — preview playback with a progress bar directly in the inspector.

## License

MIT — see [LICENSE.md](LICENSE.md). This package redistributes no third-party code —
see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for required commercial assets.
