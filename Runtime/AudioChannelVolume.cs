// ArchNet · Settings Module — audio channel volume
// Put it next to an AudioSource and pick its channel (Music, Effects, Interface): the source's
// volume follows that slider of the options menu. Its own volume below stays the mix level.
// Docs: https://archnet-plateform.fr/#ModuleSettings

using ArchNet.Core;
using UnityEngine;

namespace ArchNet.Module
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("ArchNet/Audio Channel Volume")]
    public class AudioChannelVolume : MonoBehaviour
    {
        [SerializeField] AudioChannel _channel = AudioChannel.Effects;
        [Tooltip("This source's own level before the player's volume is applied.")]
        [SerializeField, Range(0f, 1f)] float _volume = 1f;

        AudioSource _source;
        SettingsService _settings;

        void OnEnable()
        {
            _source = GetComponent<AudioSource>();
            if (Services.TryGet(out _settings))
            {
                _settings.Changed += OnChanged;
                OnChanged(_settings.Data);
            }
            else _source.volume = _volume;
        }

        void OnDisable()
        {
            if (_settings != null) _settings.Changed -= OnChanged;
            _settings = null;
        }

        // The master volume is already applied by AudioListener.volume.
        void OnChanged(SettingsData data) =>
            _source.volume = _volume * (_channel == AudioChannel.Master ? 1f : data.Volume(_channel));
    }
}
