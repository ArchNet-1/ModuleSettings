using System;
using ArchNet.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArchNet.Module.Tests
{
    public class SettingsServiceTests
    {
        GameObject _object;
        SettingsService _settings;
        string _file;
        float _listenerVolume;

        [SetUp]
        public void SetUp()
        {
            _listenerVolume = AudioListener.volume;
            _file = $"archnet-tests/settings-{Guid.NewGuid():N}.json";
            _object = new GameObject("Settings");
            _settings = _object.AddComponent<SettingsService>();
            _settings.UseFile(_file);
        }

        [TearDown]
        public void TearDown()
        {
            AudioListener.volume = _listenerVolume;
            JsonStorage.Delete(_file);
            UnityEngine.Object.DestroyImmediate(_object);
        }

        [Test]
        public void SavedSettingsComeBackOnTheNextLaunch()
        {
            _settings.Data.SetVolume(AudioChannel.Music, 0.25f);
            _settings.Data.vSync = false;
            _settings.Save();

            _settings.ResetToDefaultsWithoutSaving();
            _settings.Load();

            Assert.AreEqual(0.25f, _settings.Data.musicVolume, 0.0001f);
            Assert.IsFalse(_settings.Data.vSync);
        }

        [Test]
        public void ApplyDrivesTheMasterVolumeAndTellsListeners()
        {
            SettingsData received = null;
            _settings.Changed += data => received = data;
            _settings.Data.SetVolume(AudioChannel.Master, 0.4f);

            _settings.Apply();

            Assert.AreEqual(0.4f, AudioListener.volume, 0.0001f);
            Assert.AreSame(_settings.Data, received);
        }

        [Test]
        public void VolumesStayBetweenZeroAndOne()
        {
            var data = new SettingsData();
            data.SetVolume(AudioChannel.Effects, 3f);
            data.SetVolume(AudioChannel.Interface, -1f);

            Assert.AreEqual(1f, data.Volume(AudioChannel.Effects));
            Assert.AreEqual(0f, data.Volume(AudioChannel.Interface));
        }
    }
}
