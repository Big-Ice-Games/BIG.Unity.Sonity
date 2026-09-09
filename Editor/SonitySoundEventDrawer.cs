using System;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sonity;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BIG.Unity.Sonity.Editor
{
    public class SonitySoundEventDrawer : OdinValueDrawer<SoundEvent>
    {
        private const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private const float BAR_HEIGHT = 4f;

        private const double STARTUP_GRACE = 2.5;
        private const double GAP_GRACE = 0.4;
        private const double MIN_PLAY_TIME = 0.15;
        private static readonly Color BAR_BACKGROUND = new Color(0f, 0f, 0f, 0.3f);
        private static readonly Color BAR_FILL = new Color(0.85f, 0.7f, 0.15f);

        private static MethodInfo _addAndPlay;
        private static MethodInfo _stop;
        private static bool _init;
        private static SoundEvent _lastPlayed;
        private static double _playRequestTime;
        private static double _lastAudibleTime;
        private static bool _started;
        private static float _lastRatio;
        private static bool _driving;
        private static Type _inspectorWindowType;

        private bool _barVisible;
        private float _barRatio;


        protected override void DrawPropertyLayout(GUIContent label)
        {
            GUILayout.BeginHorizontal();

            ValueEntry.SmartValue = (SoundEvent)SirenixEditorFields.UnityObjectField(label, ValueEntry.SmartValue, typeof(SoundEvent), false);

            SoundEvent se = this.ValueEntry.SmartValue;

            bool prevEnabled = GUI.enabled;
            GUI.enabled = se != null;
            if (GUILayout.Button("▶", GUILayout.Width(22), GUILayout.Height(18)))
                Play(se);
            GUI.enabled = prevEnabled;

            if (GUILayout.Button("■", GUILayout.Width(22), GUILayout.Height(18)))
                Stop();

            GUILayout.EndHorizontal();

            if (Event.current.type == EventType.Layout)
                _barVisible = IsPlaying(se, out _barRatio);

            Rect r = EditorGUILayout.GetControlRect(false, BAR_HEIGHT);
            if (Event.current.type == EventType.Repaint && _barVisible)
            {
                EditorGUI.DrawRect(r, BAR_BACKGROUND);
                EditorGUI.DrawRect(new Rect(r.x, r.y, r.width * _barRatio, r.height), BAR_FILL);
            }
        }

        private static void Play(SoundEvent soundEvent)
        {
            if (soundEvent == null) return;
            EnsureInit();
            if (_addAndPlay == null) { Debug.LogWarning("[SonityPreview] No AddAndPlaySoundEvent."); return; }
            Stop();
            _lastPlayed = soundEvent;
            _playRequestTime = EditorApplication.timeSinceStartup;
            _lastAudibleTime = _playRequestTime;
            _started = false;
            _lastRatio = 0f;
            StartDriver();

            EditorApplication.delayCall += () =>
            {
                if (soundEvent == null) return;
                try { _addAndPlay.Invoke(null, new object[] { soundEvent, true }); }
                catch (Exception e) { Debug.LogError("[SonityPreview] " + e); }
            };
        }

        private static void Stop()
        {
            EnsureInit();

            _lastPlayed = null;
            _started = false;
            _lastRatio = 0f;

            try { _stop?.Invoke(null, null); _stop?.Invoke(null, null); }
            catch (Exception e) { Debug.LogError("[SonityPreview] " + e); }

            foreach (var a in Resources.FindObjectsOfTypeAll<AudioSource>())
                if (a != null && a.isPlaying) a.Stop();

            StopDriver();
        }

        private static bool IsPlaying(SoundEvent soundEvent, out float timeRatio)
        {
            timeRatio = 0f;
            if (soundEvent == null || soundEvent != _lastPlayed) return false;

            double now = EditorApplication.timeSinceStartup;

            bool audible = false;
            float r = 0f;
            foreach (var a in Resources.FindObjectsOfTypeAll<AudioSource>())
            {
                if (a == null || !a.isPlaying) continue;
                audible = true;
                if (a.clip != null && a.clip.length > 0f)
                    r = Mathf.Clamp01(a.time / a.clip.length);
                break;
            }

            if (audible)
            {
                _lastAudibleTime = now;
                _lastRatio = r;
                timeRatio = r;
                if (!_started && now - _playRequestTime >= MIN_PLAY_TIME)
                    _started = true;
                return true;
            }

            if (!_started)
            {
                if (now - _playRequestTime < STARTUP_GRACE) { timeRatio = _lastRatio; return true; }
            }
            else
            {
                if (now - _lastAudibleTime < GAP_GRACE) { timeRatio = _lastRatio; return true; }
            }

            _lastPlayed = null;
            return false;
        }

        private static void StartDriver()
        {
            if (_driving) return;
            _driving = true;
            EditorApplication.update += DriverTick;
        }

        private static void StopDriver()
        {
            if (!_driving) return;
            _driving = false;
            EditorApplication.update -= DriverTick;
        }

        private static void DriverTick()
        {
            if (_lastPlayed == null) { StopDriver(); return; }
            RepaintInspectors();
        }

        private static void RepaintInspectors()
        {
            if (_inspectorWindowType == null)
                _inspectorWindowType = typeof(UnityEditor.Editor).Assembly
                    .GetType("UnityEditor.InspectorWindow");

            if (_inspectorWindowType == null)
            {
                InternalEditorUtility.RepaintAllViews();
                return;
            }
            foreach (var w in Resources.FindObjectsOfTypeAll(_inspectorWindowType))
                ((EditorWindow)w).Repaint();
        }

        private static void EnsureInit()
        {
            if (_init) return;
            _init = true;

            Type eps = FindType("Sonity.Internal.EditorPreviewSound");
            _addAndPlay = eps?.GetMethod("AddAndPlaySoundEvent", F);

            _stop = FindType("Sonity.Internal.EditorShortcutsPreview")?.GetMethod("SoundPreviewStop", F);

            if (_addAndPlay == null)
                Debug.LogWarning("[SonityPreview] EditorPreviewSound.AddAndPlaySoundEvent not found.");
        }

        private static Type FindType(string fullName) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName, false))
                .FirstOrDefault(x => x != null);
    }
}