using System;
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
            CreateOrResetPlayableScene(true);
        }

        [MenuItem("Project Echo/Build Windows Demo")]
        public static void BuildWindowsDemo()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Project Echo", "Create the playable scene first using Project Echo > Create or Reset Playable Scene.", "OK");
                return;
            }

            BuildWindowsPlayer("Build/ProjectEcho.exe", true);
        }

        // GameCI calls this static method in batch mode. It creates the generated scene before building.
        public static void BuildWindowsPlayerCI()
        {
            CreateOrResetPlayableScene(false);
            BuildWindowsPlayer("Build/ProjectEcho.exe", false);
        }

        private static void CreateOrResetPlayableScene(bool showDialog)
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
            Debug.Log("Project Echo playable scene created at " + ScenePath + ".");

            if (showDialog)
                EditorUtility.DisplayDialog("Project Echo", "Playable scene created. Press Play to enter the Sanctuary.", "Begin");
        }

        private static void BuildWindowsPlayer(string output, bool showDialog)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? "Build");
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                string message = "Windows build failed: " + report.summary.result + ". Check the Unity build log and confirm Windows Build Support is installed.";
                Debug.LogError(message);
                if (showDialog) EditorUtility.DisplayDialog("Project Echo", message, "OK");
                throw new Exception(message);
            }

            Debug.Log("Project Echo Windows build succeeded: " + output);
            if (showDialog)
                EditorUtility.DisplayDialog("Project Echo", "Windows demo built to " + output + ". This is a local demo build, not a Steam release.", "OK");
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
