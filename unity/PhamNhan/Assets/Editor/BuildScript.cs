using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhamNhan.EditorTools
{
    /// <summary>Tạo cảnh chính, đặt cấu hình điện thoại và build APK. Dùng được từ menu Unity hoặc GitHub Actions.</summary>
    [InitializeOnLoad]
    public static class BuildScript
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string ApkPath = "build/Android/PhamNhan.apk";

        static BuildScript() { EditorApplication.delayCall += () => { if (!Application.isBatchMode) EnsureScene(); }; }

        [MenuItem("Phàm Nhân/Tạo cảnh chính")]
        public static void EnsureScene()
        {
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void ApplySettings()
        {
            PlayerSettings.companyName = "Tan";
            PlayerSettings.productName = "Phàm Nhân";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.tttan.phamnhan");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            EditorUserBuildSettings.buildAppBundle = false;
        }

        const string WebPath = "build/WebGL/PhamNhan";

        [MenuItem("Phàm Nhân/Build WebGL")]
        public static void BuildWebGL()
        {
            EnsureScene();
            ApplySettings();
            // nén Brotli và tự giải nén bằng JS để chạy trên mọi máy chủ tĩnh (GitHub Pages)
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.template = "PROJECT:PhamNhan";
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.nameFilesAsHashes = false;
            PlayerSettings.runInBackground = true;
            Directory.CreateDirectory(WebPath);
            var opts = new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = WebPath, target = BuildTarget.WebGL, targetGroup = BuildTargetGroup.WebGL, options = BuildOptions.None };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log("Build WebGL: " + report.summary.result + " · " + report.summary.totalSize + " byte · " + WebPath);
            if (report.summary.result != BuildResult.Succeeded && Application.isBatchMode) EditorApplication.Exit(1);
        }

        [MenuItem("Phàm Nhân/Build Android APK")]
        public static void BuildAndroid()
        {
            EnsureScene();
            ApplySettings();
            Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
            var opts = new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = ApkPath, target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android, options = BuildOptions.None };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log("Build Android: " + report.summary.result + " · " + report.summary.totalSize + " byte · " + ApkPath);
            if (report.summary.result != BuildResult.Succeeded && Application.isBatchMode) EditorApplication.Exit(1);
        }
    }
}
