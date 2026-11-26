<p align="center">
  <a href="https://archnet-plateform.fr"><img src="https://archnet-plateform.fr/ArchNetLogoWithBck.png" alt="ArchNet" width="240"></a>
</p>

# ArchNet · Settings Module

A complete options menu in three pages, applied at once and saved. Replaces the Audio, Video and Shadow modules for new projects.

- Unity **6000.0** or newer · Needs `com.unity.ugui` and [ArchNet Core](https://github.com/ArchNet-1/ArchNetCore)
- Part of [ArchNet](https://archnet-plateform.fr), free and open-source Unity packages

## Add it to your game (one click)

*ArchNet > Hub > Services > Settings > Add to game* creates `Assets/ArchNet/Settings/Scripts/GameSettings.cs` (your class) and `Prefabs/Settings.prefab` (the menu) under the ArchNet Bootstrap.

| Page | Settings |
|---|---|
| General | Frame rate (30 / 60 / 120 / unlimited), V-Sync |
| Audio | Master, Music, Effects, Interface volumes |
| Video | Display (fullscreen / windowed) and resolution (computers only), quality level |

```csharp
Services.Get<GameSettings>().Open();
```

The Main menu and the pause menu of the Hub's scenes get a Settings button by themselves.

**Volumes**: the master volume drives `AudioListener.volume`. Add **ArchNet/Audio Channel Volume** next to each `AudioSource` and pick its channel; it follows the slider.

**Your own settings**: override `Apply()` in `GameSettings`; `Changed` is raised after every change.

Settings are saved to `persistentDataPath/settings.json` when the menu closes. **Open tool**: restyle the menu, open it in Play Mode, forget the saved settings.

## License

MIT, see [LICENSE.md](LICENSE.md).
