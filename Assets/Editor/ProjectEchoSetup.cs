using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectEcho.EditorTools
{
    public static class ProjectEchoSetup
    {
        private const string ScenePath = "Assets/Scenes/ProjectEcho.unity";
        private const string TuningPath = "Assets/Resources/EchoTuning.asset";

        [MenuItem("Project Echo/Create or Reset Playable Scene")]
        public static void CreatePlayableScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Resources");
            EchoTuning tuning = AssetDatabase.LoadAssetAtPath<EchoTuning>(TuningPath);
            if (tuning == null)
            {
                tuning = ScriptableObject.CreateInstance<EchoTuning>();
                tuning.EnsureDefaults();
                AssetDatabase.CreateAsset(tuning, TuningPath);
            }
            else
            {
                tuning.EnsureDefaults();
                EditorUtility.SetDirty(tuning);
                AssetDatabase.SaveAssets();
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var systems = new GameObject("Project Echo - Runtime Systems");
            systems.AddComponent<EchoSceneBootstrap>();
            systems.AddComponent<EchoHUD>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.productName = "Project Echo";
            PlayerSettings.companyName = "Project Echo";
            AssetDatabase.SaveAssets();
            Debug.Log("Project Echo playable scene created at " + ScenePath + ". Press Play to enter the Sanctuary.");
            EditorUtility.DisplayDialog("Project Echo", "Playable scene created. Press Play to enter the Sanctuary.", "Begin");
        }

        [MenuItem("Project Echo/Build Windows Demo")]
        public static void BuildWindowsDemo()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Project Echo", "Create the playable scene first using Project Echo > Create or Reset Playable Scene.", "OK");
                return;
            }
            const string output = "Build/ProjectEcho.exe";
            Directory.CreateDirectory("Build");
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                EditorUtility.DisplayDialog("Project Echo", "Windows demo built to " + output + ". This is a local demo build, not a Steam release.", "OK");
            else
                EditorUtility.DisplayDialog("Project Echo", "Build did not complete. Check the Unity Console and install the Windows Build Support module if needed.", "OK");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, name);
        }
    }
}
