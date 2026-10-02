#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Asadito.Editor
{
    /// <summary>Repeatable, non-publishing Android validation build configuration.</summary>
    public static class AndroidReleaseBuild
    {
        private const string Company = "Cuervation";
        private const string Product = "Asadito";
        private const string BundleId = "com.cuervation.asadito";
        private const string BundleVersion = "1.5.0";
        private const int VersionCode = 7;

        [MenuItem("Asadito/Configure Android Release Settings")]
        public static void ConfigureAndroidSettings()
        {
            PlayerSettings.companyName = Company;
            PlayerSettings.productName = Product;
            PlayerSettings.bundleVersion = BundleVersion;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            AssetDatabase.SaveAssets();
            Debug.Log("Configured Asadito Android: " + BundleId + " " + BundleVersion + " (" + VersionCode + "), ARM64 IL2CPP, API 26–36, portrait.");
        }

        /// <summary>CLI: -executeMethod Asadito.Editor.AndroidReleaseBuild.BuildAndroidValidation</summary>
        public static void BuildAndroidValidation()
        {
            ConfigureAndroidSettings();
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0) throw new BuildFailedException("No enabled scenes in Build Settings.");

            string output = Environment.GetEnvironmentVariable("ASADITO_APK_PATH");
            if (string.IsNullOrWhiteSpace(output)) output = "/tmp/Asadito-expanded-android.apk";
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("Asadito Android build result: " + report.summary.result + "; output=" + output + "; bytes=" + report.summary.totalSize);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Asadito Android build failed: " + report.summary.result);
        }
    }
}
#endif
