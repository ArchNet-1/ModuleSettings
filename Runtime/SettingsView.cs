// ArchNet · Settings Module — options menu screen
// Three pages (General, Audio, Video) selected by tab buttons. Every control applies at once;
// the settings are saved when the menu closes. Rows listed in "Desktop Only" are hidden on
// phones and tablets (display mode, resolution).
// Docs: https://archnet-plateform.fr/#ModuleSettings

using System;
using System.Collections.Generic;
using System.Linq;
using ArchNet.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArchNet.Module
{
    [RequireComponent(typeof(UIPanel))]
    public class SettingsView : MonoBehaviour
    {
        static readonly int[] FrameRates = { 30, 60, 120, 0 };
        static readonly string[] FrameRateNames = { "30 FPS", "60 FPS", "120 FPS", "Unlimited" };
        static readonly FullScreenMode[] DisplayModes = { FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
        static readonly string[] DisplayModeNames = { "Fullscreen", "Windowed" };

        [Header("Pages")]
        [SerializeField] Button[] _tabs;
        [SerializeField] GameObject[] _pages;
        [Header("General")]
        [SerializeField] TMP_Dropdown _frameRate;
        [SerializeField] Toggle _vSync;
        [Header("Audio")]
        [SerializeField] Slider _master;
        [SerializeField] Slider _music;
        [SerializeField] Slider _effects;
        [SerializeField] Slider _interface;
        [Header("Video")]
        [SerializeField] TMP_Dropdown _displayMode;
        [SerializeField] TMP_Dropdown _resolution;
        [SerializeField] TMP_Dropdown _quality;
        [SerializeField] GameObject[] _desktopOnly;
        [Header("Buttons")]
        [SerializeField] Button _back;
        [SerializeField] Button _reset;

        readonly List<Vector2Int> _resolutions = new List<Vector2Int>();
        SettingsService _service;
        UIPanel _panel;

        public bool IsOpen => Panel.IsVisible;

        UIPanel Panel => _panel != null ? _panel : _panel = GetComponent<UIPanel>();

        void Awake()
        {
            for (int i = 0; i < (_tabs?.Length ?? 0); i++)
            {
                int page = i;
                if (_tabs[i] != null) _tabs[i].onClick.AddListener(() => ShowPage(page));
            }
            if (_back != null) _back.onClick.AddListener(Close);
            Panel.Hidden.AddListener(() => _service?.Save()); // also when the Back key closes the menu
            if (_reset != null) _reset.onClick.AddListener(() =>
            {
                _service?.ResetToDefaults();
                Refresh();
            });

            Bind(_master, AudioChannel.Master);
            Bind(_music, AudioChannel.Music);
            Bind(_effects, AudioChannel.Effects);
            Bind(_interface, AudioChannel.Interface);
            Bind(_vSync, (data, on) => data.vSync = on);
            Bind(_frameRate, (data, index) => data.frameRate = FrameRates[index]);
            Bind(_quality, (data, index) => data.qualityLevel = index);
            Bind(_displayMode, (data, index) => data.displayMode = DisplayModes[index]);
            Bind(_resolution, (data, index) =>
            {
                data.width = _resolutions[index].x;
                data.height = _resolutions[index].y;
            });

            if (Application.isMobilePlatform)
                foreach (GameObject row in _desktopOnly ?? Array.Empty<GameObject>())
                    if (row != null) row.SetActive(false);
        }

        public void Open(SettingsService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            UISupport.EnsureEventSystem();
            FillOptions();
            Refresh();
            ShowPage(0);
            Panel.Show();
        }

        public void Close() => Panel.Hide(); // saving happens on Hidden

        public void ShowPage(int index)
        {
            for (int i = 0; i < (_pages?.Length ?? 0); i++)
                if (_pages[i] != null) _pages[i].SetActive(i == index);
            for (int i = 0; i < (_tabs?.Length ?? 0); i++)
                if (_tabs[i] != null) _tabs[i].interactable = i != index; // the current tab looks pressed
        }

        void FillOptions()
        {
            Fill(_frameRate, FrameRateNames);
            Fill(_quality, QualitySettings.names);
            Fill(_displayMode, DisplayModeNames);
            _resolutions.Clear();
            _resolutions.AddRange(Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)).Distinct().OrderByDescending(r => r.x * r.y));
            if (_resolutions.Count == 0) _resolutions.Add(new Vector2Int(Screen.width, Screen.height));
            Fill(_resolution, _resolutions.Select(r => $"{r.x} × {r.y}"));
        }

        // Shows the current settings without triggering the controls' callbacks.
        void Refresh()
        {
            SettingsData data = _service.Data;
            _master?.SetValueWithoutNotify(data.masterVolume);
            _music?.SetValueWithoutNotify(data.musicVolume);
            _effects?.SetValueWithoutNotify(data.effectsVolume);
            _interface?.SetValueWithoutNotify(data.interfaceVolume);
            _vSync?.SetIsOnWithoutNotify(data.vSync);
            _frameRate?.SetValueWithoutNotify(Mathf.Max(0, Array.IndexOf(FrameRates, data.frameRate)));
            _quality?.SetValueWithoutNotify(data.qualityLevel >= 0 ? data.qualityLevel : QualitySettings.GetQualityLevel());
            _displayMode?.SetValueWithoutNotify(Mathf.Max(0, Array.IndexOf(DisplayModes, data.displayMode)));
            Vector2Int size = data.width > 0 ? new Vector2Int(data.width, data.height) : new Vector2Int(Screen.width, Screen.height);
            _resolution?.SetValueWithoutNotify(Mathf.Max(0, _resolutions.IndexOf(size)));
        }

        void Changed()
        {
            if (_service != null) _service.Apply();
        }

        void Bind(Slider slider, AudioChannel channel)
        {
            if (slider == null) return;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.onValueChanged.AddListener(value =>
            {
                _service?.Data.SetVolume(channel, value);
                Changed();
            });
        }

        void Bind(Toggle toggle, Action<SettingsData, bool> set)
        {
            if (toggle == null) return;
            toggle.onValueChanged.AddListener(on =>
            {
                if (_service != null) set(_service.Data, on);
                Changed();
            });
        }

        void Bind(TMP_Dropdown dropdown, Action<SettingsData, int> set)
        {
            if (dropdown == null) return;
            dropdown.onValueChanged.AddListener(index =>
            {
                if (_service != null) set(_service.Data, index);
                Changed();
            });
        }

        static void Fill(TMP_Dropdown dropdown, IEnumerable<string> options)
        {
            if (dropdown == null) return;
            dropdown.ClearOptions();
            dropdown.AddOptions(options.ToList());
        }
    }
}
