// ArchNet · Settings Module — Settings tool (ArchNet Hub > Services > Settings > Open tool)
// Restyle the options menu, open it in Play Mode, or forget the saved settings to test a first launch.
// Docs: https://archnet-plateform.fr/#ModuleSettings

using System.IO;
using ArchNet.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArchNet.Module.Editor
{
    public class SettingsWindow : EditorWindow
    {
        const string StylePath = "Packages/com.archnet.hub/Editor/UI/Hub.uss";

        public static void Open()
        {
            var window = GetWindow<SettingsWindow>();
            window.titleContent = new GUIContent("Settings");
            window.minSize = new Vector2(380, 200);
        }

        void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeChanged;
        void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        void OnPlayModeChanged(PlayModeStateChange _) => Rebuild();
        void CreateGUI() => Rebuild();

        void Rebuild()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            root.AddToClassList("archnet-hub");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
            if (style != null) root.styleSheets.Add(style);
            root.style.paddingLeft = root.style.paddingRight = root.style.paddingTop = 10;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsRecipe.PrefabPath);
            if (prefab == null)
            {
                root.Add(new HelpBox($"{SettingsRecipe.PrefabPath} was not found. Add the Settings service from ArchNet > Hub > Services.", HelpBoxMessageType.Warning));
                return;
            }
            var restyle = new Button(() => AssetDatabase.OpenAsset(prefab)) { text = "Restyle the menu" };
            restyle.AddToClassList("primary");
            root.Add(restyle);

            var open = new Button(() => Services.Get<SettingsService>().Open()) { text = "Open the menu (Play Mode)" };
            open.SetEnabled(EditorApplication.isPlaying);
            root.Add(open);
            if (!EditorApplication.isPlaying) root.Add(new Button(EditorApplication.EnterPlaymode) { text = "Enter Play Mode" });

            root.Add(new Button(() =>
            {
                string file = Path.Combine(Application.persistentDataPath, "settings.json");
                File.Delete(file);
                File.Delete(file + ".bak");
                Debug.Log("[ArchNet] Saved settings deleted: the next launch starts with the defaults.");
            }) { text = "Forget the saved settings" });
        }
    }
}
