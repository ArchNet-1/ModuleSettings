// ArchNet · Settings Module — settings service
// Loads the player's settings when the game starts, applies them (volume, frame rate, V-Sync,
// quality, display) and saves them when the menu closes. The Hub creates GameSettings (your
// class) deriving from it:
//
//   Services.Get<GameSettings>().Open();     // the options menu
//
// Music and sound volumes: put an AudioChannelVolume next to each AudioSource and pick its
// channel; it follows the sliders. The master volume drives AudioListener.volume.
// Docs: https://archnet-plateform.fr/#ModuleSettings

using System;
using System.Runtime.CompilerServices;
using ArchNet.Core;
using UnityEngine;

[assembly: InternalsVisibleTo("ArchNet.Module.Settings.Tests")]

namespace ArchNet.Module
{
    public class SettingsService : GameService
    {
        [SerializeField] SettingsView _view;
        [Tooltip("File under Application.persistentDataPath.")]
        [SerializeField] string _fileName = "settings.json";

        public SettingsData Data { get; private set; } = new SettingsData();

        /// <summary>Raised after the settings were applied (each change in the menu).</summary>
        public event Action<SettingsData> Changed;

        public bool IsOpen => _view != null && _view.IsOpen;

        internal void UseFile(string fileName) => _fileName = fileName;
        internal void ResetToDefaultsWithoutSaving() => Data = new SettingsData();

        protected override void Awake()
        {
            base.Awake();
            Load();
            Apply();
        }

        internal void Load() => Data = JsonStorage.Load(_fileName, new SettingsData());

        public void Open()
        {
            if (_view == null)
            {
                Debug.LogError("[ArchNet] The settings service has no SettingsView assigned.", this);
                return;
            }
            _view.Open(this);
        }

        public void Close()
        {
            if (_view != null) _view.Close();
        }

        /// <summary>Applies Data to the game. Override it to apply settings of your own too.</summary>
        public virtual void Apply()
        {
            AudioListener.volume = Data.masterVolume;
            if (Application.isPlaying) // outside Play Mode these would change the project's own Quality settings
            {
                QualitySettings.vSyncCount = Data.vSync ? 1 : 0;
                Application.targetFrameRate = Data.frameRate > 0 ? Data.frameRate : -1;
                if (Data.qualityLevel >= 0 && Data.qualityLevel < QualitySettings.names.Length && QualitySettings.GetQualityLevel() != Data.qualityLevel)
                    QualitySettings.SetQualityLevel(Data.qualityLevel, true);
                ApplyDisplay();
            }
            Changed?.Invoke(Data);
        }

        // Phones and tablets have one screen size and are always full screen.
        void ApplyDisplay()
        {
            if (Application.isMobilePlatform || Application.isEditor) return;
            bool keepSize = Data.width <= 0 || Data.height <= 0;
            int width = keepSize ? Screen.width : Data.width;
            int height = keepSize ? Screen.height : Data.height;
            if (Screen.width != width || Screen.height != height || Screen.fullScreenMode != Data.displayMode)
                Screen.SetResolution(width, height, Data.displayMode);
        }

        public void Save() => JsonStorage.Save(_fileName, Data);

        public void ResetToDefaults()
        {
            Data = new SettingsData();
            Apply();
            Save();
        }
    }
}
