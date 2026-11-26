// ArchNet · Settings Module — Settings recipe (ArchNet Hub > Services > Settings)
// Creates, under Assets/ArchNet/Settings/: Scripts/GameSettings.cs (your class) and
// Prefabs/Settings.prefab (the options menu: General, Audio and Video pages), placed under the
// ArchNet Bootstrap so the menu opens from any scene.
// This assembly only compiles when the ArchNet Hub (com.archnet.hub 2.0.0+) is installed.
// Docs: https://archnet-plateform.fr/#ModuleSettings

using System;
using ArchNet.Core.Editor;
using ArchNet.Installer.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ArchNet.Module.Editor
{
    public sealed class SettingsRecipe : ServiceRecipe
    {
        public const string RecipeId = "settings";
        const string TemplatePath = "Packages/com.archnet.module.settings/Editor/Recipe/Templates/GameSettings.cs.txt";
        const string ServiceScript = "Scripts/GameSettings.cs";
        const string PrefabFile = "Prefabs/Settings.prefab";
        const int SortingOrder = 800; // above the game and the credits, below popups

        public override string Id => RecipeId;
        public override string DisplayName => "Settings";
        public override string Description => "A complete options menu (general, audio, video) applied at once and saved: frame rate, V-Sync, volumes per channel, display, resolution, quality. Services.Get<GameSettings>().Open().";
        public override string Category => "Game";
        public override string FolderName => "Settings";
        public override string[] RequiredServices => new[] { BootstrapRecipe.RecipeId };
        public override bool HasTool => true;

        public static string PrefabPath => $"{RecipeRunner.RootFolder}/Settings/{PrefabFile}";

        public override void Create(RecipeContext context)
        {
            UIBuilder.EnsureTextMeshPro();
            context.WriteTemplate(ServiceScript, RecipeContext.LoadTemplate(TemplatePath));
        }

        public override void AfterScriptsCompiled(RecipeContext context)
        {
            Type serviceType = RecipeTools.GeneratedType<SettingsService>(context, "GameSettings", ServiceScript);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(context.PathOf(PrefabFile))
                                ?? context.SavePrefab(PrefabFile, BuildScreen(serviceType));
            BootstrapRecipe.AddUnderBootstrap(prefab);
        }

        public override void OpenTool() => SettingsWindow.Open();

        static GameObject BuildScreen(Type serviceType)
        {
            GameObject root = UIBuilder.ScreenCanvas("Settings", SortingOrder);
            UIBuilder.Stretch(UIBuilder.Image("Shade", root.transform, UIBuilder.Shade).rectTransform);

            Image window = UIBuilder.Image("Window", root.transform, UIBuilder.Window);
            UIBuilder.Anchor(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1160f, 820f), Vector2.zero);
            UIBuilder.Column(window.rectTransform, 24, 44);

            TextMeshProUGUI title = UIBuilder.Text("Title", window.transform, "Settings", 56, style: FontStyles.Bold);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 70;

            RectTransform tabRow = Row("Tabs", window.transform, 84);
            Button general = UIBuilder.Button("General", tabRow, "General", UIBuilder.Neutral, out _);
            Button audio = UIBuilder.Button("Audio", tabRow, "Audio", UIBuilder.Neutral, out _);
            Button video = UIBuilder.Button("Video", tabRow, "Video", UIBuilder.Neutral, out _);

            RectTransform pages = UIBuilder.Rect("Pages", window.transform);
            pages.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;

            RectTransform generalPage = Page("General Page", pages);
            UIBuilder.OptionRow("Frame rate", generalPage, out RectTransform slot);
            TMP_Dropdown frameRate = UIBuilder.Dropdown("Frame Rate", slot);
            UIBuilder.OptionRow("V-Sync", generalPage, out slot);
            Toggle vSync = UIBuilder.Toggle("V-Sync", slot);

            RectTransform audioPage = Page("Audio Page", pages);
            Slider master = VolumeRow("Master", audioPage);
            Slider music = VolumeRow("Music", audioPage);
            Slider effects = VolumeRow("Effects", audioPage);
            Slider ui = VolumeRow("Interface", audioPage);

            RectTransform videoPage = Page("Video Page", pages);
            RectTransform displayRow = UIBuilder.OptionRow("Display", videoPage, out slot);
            TMP_Dropdown displayMode = UIBuilder.Dropdown("Display Mode", slot);
            RectTransform resolutionRow = UIBuilder.OptionRow("Resolution", videoPage, out slot);
            TMP_Dropdown resolution = UIBuilder.Dropdown("Resolution", slot);
            UIBuilder.OptionRow("Quality", videoPage, out slot);
            TMP_Dropdown quality = UIBuilder.Dropdown("Quality", slot);

            RectTransform footer = Row("Footer", window.transform, 84);
            Button reset = UIBuilder.Button("Reset", footer, "Reset", UIBuilder.Neutral, out _);
            Button back = UIBuilder.Button("Back", footer, "Back", UIBuilder.Accent, out _);

            var view = root.AddComponent<SettingsView>();
            RecipeTools.AssignArray(view, "_tabs", general, audio, video);
            RecipeTools.AssignArray(view, "_pages", generalPage.gameObject, audioPage.gameObject, videoPage.gameObject);
            RecipeTools.Assign(view, "_frameRate", frameRate);
            RecipeTools.Assign(view, "_vSync", vSync);
            RecipeTools.Assign(view, "_master", master);
            RecipeTools.Assign(view, "_music", music);
            RecipeTools.Assign(view, "_effects", effects);
            RecipeTools.Assign(view, "_interface", ui);
            RecipeTools.Assign(view, "_displayMode", displayMode);
            RecipeTools.Assign(view, "_resolution", resolution);
            RecipeTools.Assign(view, "_quality", quality);
            RecipeTools.AssignArray(view, "_desktopOnly", displayRow.gameObject, resolutionRow.gameObject);
            RecipeTools.Assign(view, "_back", back);
            RecipeTools.Assign(view, "_reset", reset);

            Component service = root.AddComponent(serviceType);
            RecipeTools.Assign(service, "_view", view);
            return root;
        }

        static RectTransform Row(string name, Transform parent, float height)
        {
            RectTransform row = UIBuilder.Rect(name, parent);
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true; // buttons take their preferred size (UIBuilder.Button: 300 × 84)
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return row;
        }

        static RectTransform Page(string name, Transform parent)
        {
            RectTransform page = UIBuilder.Stretch(UIBuilder.Rect(name, parent));
            UIBuilder.Column(page, 18);
            return page;
        }

        static Slider VolumeRow(string label, Transform page)
        {
            UIBuilder.OptionRow(label, page, out RectTransform slot);
            return UIBuilder.Slider(label, slot);
        }
    }
}
