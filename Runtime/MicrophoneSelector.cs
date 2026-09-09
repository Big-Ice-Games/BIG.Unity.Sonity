using System.Linq;
using Sirenix.OdinInspector;
using Sonity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BIG.Unity.Sonity
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class MicrophoneDropdown : BaseBehaviour
    {
        [SerializeField, Required] private TMP_Dropdown _dropdown;
        [ShowInInspector, ReadOnly] private string _selectedMicrophone;
        [ShowInInspector, ReadOnly] private AudioClip _microphoneAudioClip;

        #region Microphone state
        [SerializeField, BoxGroup("Microphone state")] private Sprite _microphoneEnabled;
        [SerializeField, BoxGroup("Microphone state")] private Sprite _microphoneDisabled;
        [SerializeField, BoxGroup("Microphone state"), Header("Button to toggle microphone state between Enable/EnableWithButton/Disable")] private Button _microphoneStateButton;
        #endregion

        #region Loudness
        [SerializeField] private bool _showLoudness;
        [SerializeField, ShowIf("_showLoudness"), BoxGroup("Loudness"), Header("Assign fill-type image to show microphone loudness.")]
        private Image _loudnessBar;
        [SerializeField, ShowIf("_showLoudness"), BoxGroup("Loudness"), Header("Speed at which the loudness bar updates.")]
        private float _loudnessBarSpeed = 5f;
        [ShowInInspector, ShowIf("_showLoudness"), ReadOnly, BoxGroup("Loudness")]
        private float _currentLoudness;
        #endregion

        private void OnMicrophoneStateButton()
        {
            Audio.MicrophoneState = Audio.MicrophoneState.GetNextEnum();
            OnMicrophoneStateChanged();
        }

        private void OnMicrophoneStateChanged()
        {
            switch (Audio.MicrophoneState)
            {
                case MicrophoneSettingsState.Disable: _microphoneStateButton.image.sprite = _microphoneDisabled; break;
                default: _microphoneStateButton.image.sprite = _microphoneEnabled; break;
            }
        }

        private void PopulateMicrophoneDropdown()
        {
            _dropdown.ClearOptions();
            string[] microphones = Microphone.devices;
            if (microphones.Length == 0) return;
            _dropdown.AddOptions(microphones.ToList());
            _selectedMicrophone = microphones[0];
            _dropdown.onValueChanged.AddListener(OnMicrophoneSelected);
            OnMicrophoneSelected(0);
        }

        private void OnMicrophoneSelected(int index)
        {
            if (index >= Microphone.devices.Length)
            {
                this.LogEditor("Microphone not found.");
                return;
            }
            _selectedMicrophone = Microphone.devices[index];
            _microphoneAudioClip = Microphone.Start(_selectedMicrophone, true, 20, AudioSettings.outputSampleRate);
        }

        private void Update()
        {
            if (!_showLoudness || _loudnessBar == null) return;
            if ((Audio.MicrophoneState == MicrophoneSettingsState.Disable) ||
                (Audio.MicrophoneState == MicrophoneSettingsState.EnableWithButton &&
                 !Audio.IsMicrophoneCurrentlyActive))
            {
                _currentLoudness = 0;
                _loudnessBar.fillAmount = _currentLoudness;
                return;
            }

            _currentLoudness = Mathf.Lerp(_currentLoudness, GetLoudnessFromMicrophone(),
                Time.deltaTime * _loudnessBarSpeed);
            _loudnessBar.fillAmount = _currentLoudness;
        }

        private float GetLoudnessFromMicrophone()
        {
            var loudness = _microphoneAudioClip.Loudness(_selectedMicrophone);
            return loudness.GetLoudnessFrom0To1();
        }

        public void Reset()
        {
            if(_dropdown == null)
                _dropdown = GetComponent<TMP_Dropdown>();
        }

        protected override void OnEnable()
        {
            PopulateMicrophoneDropdown();
            OnMicrophoneStateChanged();
        }
    }
}
