// ArchNet · Settings Module — settings data
// What the player chose, saved as persistentDataPath/settings.json. Add your own fields
// (mouse sensitivity, subtitles…) by deriving from SettingsService and saving them yourself,
// or simply add them here if you keep a copy of this package.
// Docs: https://archnet-plateform.fr/#ModuleSettings

using System;
using UnityEngine;

namespace ArchNet.Module
{
    public enum AudioChannel { Master, Music, Effects, Interface }

    [Serializable]
    public class SettingsData
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.8f;
        [Range(0f, 1f)] public float effectsVolume = 1f;
        [Range(0f, 1f)] public float interfaceVolume = 1f;

        [Tooltip("Frames per second when V-Sync is off; 0 = unlimited.")]
        public int frameRate = 60;
        public bool vSync = true;

        [Tooltip("Index in Project Settings > Quality; -1 keeps the project's default.")]
        public int qualityLevel = -1;
        public FullScreenMode displayMode = FullScreenMode.FullScreenWindow;
        [Tooltip("0 keeps the current size.")]
        public int width;
        public int height;

        public float Volume(AudioChannel channel) => channel switch
        {
            AudioChannel.Master => masterVolume,
            AudioChannel.Music => musicVolume,
            AudioChannel.Effects => effectsVolume,
            _ => interfaceVolume,
        };

        public void SetVolume(AudioChannel channel, float volume)
        {
            volume = Mathf.Clamp01(volume);
            switch (channel)
            {
                case AudioChannel.Master: masterVolume = volume; break;
                case AudioChannel.Music: musicVolume = volume; break;
                case AudioChannel.Effects: effectsVolume = volume; break;
                default: interfaceVolume = volume; break;
            }
        }
    }
}
