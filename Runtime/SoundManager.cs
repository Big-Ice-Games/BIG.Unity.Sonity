using Sonity;
using UnityEngine;
using UnityEngine.Audio;

namespace BIG.Unity.Sonity
{
    public class SoundUserDataKeysProvider : IUserDataKeysProvider
    {
        public string Name => "Sound";
        public static UserDataKey MusicVolume => new UserDataKey("MusicVolume");
        public static UserDataKey EffectsVolume => new UserDataKey("EffectsVolume");
        public static UserDataKey UIVolume => new UserDataKey("UIVolume");
        public static UserDataKey MusicEnabled => new UserDataKey("MusicEnabled");
        public static UserDataKey EffectsEnabled => new UserDataKey("EffectsEnabled");
        public static UserDataKey UIVolumeEnabled => new UserDataKey("UIVolumeEnabled");
        public static UserDataKey VoiceEnabled => new UserDataKey("VoiceEnabled");
        public static UserDataKey VoiceVolume => new UserDataKey("VoiceVolume");
        public static UserDataKey MicrophoneState => new UserDataKey("MicrophoneState");
    }

    public class SoundManager : BaseBehaviour
    {
        /// <summary> Seems to be a right value for converting linear volume to logarithmic scale. </summary>
        private const float SOUND_LOG_MULTIPLIER = 50f;
        private static SoundManager _instance;

        [Inject] private IUserData _userData;
        [SerializeField] protected global::Sonity.SoundManager _sonitySoundManager;
        [SerializeField] protected AudioMixer _audioMixer;
        [SerializeField] protected AudioMixerSnapshot _enableSnapshot;
        [SerializeField] protected AudioMixerSnapshot _disableSnapshot;

        private bool _musicEnabled;
        private bool _effectsEnabled;
        private bool _voiceEnabled;
        private bool _uiEnabled;
        private float _musicVolume;
        private float _effectsVolume;
        private float _voiceVolume;
        private float _uiVolume;
        public static float MusicVolume => _instance._musicEnabled ? _instance._musicVolume : 0;
        public static float EffectsVolume => _instance._effectsEnabled ? _instance._effectsVolume : 0;
        public static float VoiceVolume => _instance._voiceEnabled ? _instance._voiceVolume : 0;
        public static float UIVolume => _instance._uiEnabled ? _instance._uiVolume : 0;
        public static bool MusicActive => _instance._musicEnabled;
        public static bool EffectsActive => _instance._effectsEnabled;
        public static bool VoiceActive => _instance._voiceEnabled;
        public static bool UIActive => _instance._uiEnabled;

        protected override void Awake()
        {
            base.Awake();

            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _instance._musicVolume = _userData.GetFloat(SoundUserDataKeysProvider.MusicVolume, 1f);
            _instance._effectsVolume = _userData.GetFloat(SoundUserDataKeysProvider.EffectsVolume, 1f);
            _instance._uiVolume = _userData.GetFloat(SoundUserDataKeysProvider.UIVolume, 1f);
            _instance._voiceVolume = _userData.GetFloat(SoundUserDataKeysProvider.VoiceVolume, 1f);

            _instance._musicEnabled = _userData.GetBool(SoundUserDataKeysProvider.MusicEnabled, true);
            _instance._effectsEnabled = _userData.GetBool(SoundUserDataKeysProvider.EffectsEnabled, true);
            _instance._voiceEnabled = _userData.GetBool(SoundUserDataKeysProvider.VoiceEnabled, true);
            _instance._uiEnabled = _userData.GetBool(SoundUserDataKeysProvider.UIVolumeEnabled, true);
            Audio.MicrophoneState = _userData.GetEnum(SoundUserDataKeysProvider.MicrophoneState, MicrophoneSettingsState.Enable);

            // Gated properties, not raw fields — a disabled channel must start muted.
            RefreshMusic(MusicVolume, false);
            RefreshEffects(EffectsVolume, false);
            RefreshVoice(VoiceVolume, false);
            RefreshUI(UIVolume, false);
        }

        public static void PlayMusic(SoundEvent soundEvent)
        {
            _instance?._sonitySoundManager.MusicPlay(soundEvent);
        }

        public static void StopMusic()
        {
            _instance?._sonitySoundManager.MusicStopAll();
        }

        public static void Play(SoundEvent soundEvent)
        {
            _instance?._sonitySoundManager.Play(soundEvent, _instance.transform);
        }

        public static void MusicSetActive(bool value)
        {
            _instance._musicEnabled = value;
            _instance._userData.Set(SoundUserDataKeysProvider.MusicEnabled, value);
            RefreshMusic(MusicVolume, false);
        }

        public static void EffectsSetActive(bool value)
        {
            _instance._effectsEnabled = value;
            _instance._userData.Set(SoundUserDataKeysProvider.EffectsEnabled, value);
            RefreshEffects(EffectsVolume, false);
        }

        public static void UISetActive(bool value)
        {
            _instance._uiEnabled = value;
            _instance._userData.Set(SoundUserDataKeysProvider.UIVolumeEnabled, value);
            RefreshUI(UIVolume, false);
        }

        public static void VoiceSetActive(bool value)
        {
            _instance._voiceEnabled = value;
            _instance._userData.Set(SoundUserDataKeysProvider.VoiceEnabled, value);
            RefreshVoice(VoiceVolume, false);
        }

        public static void SetMusicVolume(float volume)
        {
            RefreshMusic(volume, true);
            _instance._userData.Set(SoundUserDataKeysProvider.MusicVolume, volume);
        }

        public static void SetUIVolume(float volume)
        {
            RefreshUI(volume, true);
            _instance._userData.Set(SoundUserDataKeysProvider.UIVolume, volume);
        }

        public static void SetEffectsVolume(float volume)
        {
            RefreshEffects(volume, true);
            _instance._userData.Set(SoundUserDataKeysProvider.EffectsVolume, volume);
        }

        public static void SetVoiceVolume(float volume)
        {
            RefreshVoice(volume, true);
            _instance._userData.Set(SoundUserDataKeysProvider.VoiceVolume, volume);
        }

        private static void RefreshMusic(float value, bool saveValue)
        {
            var mixerValue = value > 0 ? (Mathf.Log10(value) * SOUND_LOG_MULTIPLIER) : -80;
            _instance._audioMixer.SetFloat("music", mixerValue);
            if (saveValue)
                _instance._musicVolume = value;

        }

        private static void RefreshUI(float value, bool saveValue)
        {
            var mixerValue = value > 0 ? (Mathf.Log10(value) * SOUND_LOG_MULTIPLIER) : -80;
            _instance._audioMixer.SetFloat("ui", mixerValue);
            if (saveValue)
                _instance._uiVolume = value;
        }

        private static void RefreshEffects(float value, bool saveValue)
        {
            var mixerValue = value > 0 ? (Mathf.Log10(value) * SOUND_LOG_MULTIPLIER) : -80;
            _instance._audioMixer.SetFloat("effects", mixerValue);
            if (saveValue)
                _instance._effectsVolume = value;

        }

        private static void RefreshVoice(float value, bool saveValue)
        {
            var mixerValue = value > 0 ? (Mathf.Log10(value) * SOUND_LOG_MULTIPLIER) : -80;
            _instance._audioMixer.SetFloat("voice", mixerValue);
            if (saveValue)
                _instance._voiceVolume = value;
        }

        private void Reset()
        {
            if(_sonitySoundManager == null)
            {
                if((_sonitySoundManager = GetComponent<global::Sonity.SoundManager>()) == null)
                    _sonitySoundManager = gameObject.AddComponent<global::Sonity.SoundManager>();
            }

            if (_sonitySoundManager == null)
            {
                Debug.LogError("Failed to add Sonity.SoundManager.");
            }
        }
    }

    public enum MicrophoneSettingsState
    {
        Enable,
        EnableWithButton,
        Disable
    }

    public static class Audio
    {
        public static MicrophoneSettingsState MicrophoneState;
        /// <summary> Set this value from input if microphone is in <see cref="MicrophoneSettingsState.EnableWithButton"/> and player input for voice chat is active.</summary>
        public static bool IsMicrophoneCurrentlyActive;

        private const int SAMPLE_WINDOW = 64;
        private const float LOUDNESS_SENSIBILITY = 100;
        private const float MIN_THRESHOLD = 1;
        private const float MAX_THRESHOLD = 1;

        private static readonly float[] SAMPLE_WAVE = new float[SAMPLE_WINDOW];

        public static float GetLoudnessFrom0To1(this float audioSourceLoudness)
        {
            var multipliedLoudness = audioSourceLoudness * LOUDNESS_SENSIBILITY;
            if (multipliedLoudness < MIN_THRESHOLD) return 0;
            if (multipliedLoudness > MAX_THRESHOLD) return 1;
            return multipliedLoudness / MAX_THRESHOLD;
        }

        public static float Loudness(this AudioSource source)
        {
            var startSample = source.timeSamples - SAMPLE_WINDOW;
            if (startSample < 0) return 0f;

            source.clip.GetData(SAMPLE_WAVE, startSample);
            float totalLoudness = 0f;
            for (int i = 0; i < SAMPLE_WINDOW; i++)
            {
                totalLoudness += Mathf.Abs(SAMPLE_WAVE[i]);
            }

            return totalLoudness / SAMPLE_WINDOW;
        }

        public static float Loudness(this AudioClip clip, string microphoneName)
        {
            var startSample = Microphone.GetPosition(microphoneName) - SAMPLE_WINDOW;
            if (startSample < 0) return 0f;

            clip.GetData(SAMPLE_WAVE, startSample);
            float totalLoudness = 0f;
            for (int i = 0; i < SAMPLE_WINDOW; i++)
            {
                totalLoudness += Mathf.Abs(SAMPLE_WAVE[i]);
            }

            return totalLoudness / SAMPLE_WINDOW;
        }
    }
}