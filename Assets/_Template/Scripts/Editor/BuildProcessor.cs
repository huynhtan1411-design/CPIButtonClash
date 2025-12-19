using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

#if UNITY_IOS && UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System;

public class BuildProcessor : Editor
{
    [MenuItem("BuildProcessor/Build APK", false, 0)]
    static void CustomBuildOnly()
    {
        CustomBuild(false);
    }

    [MenuItem("BuildProcessor/Build AAB", false, 0)]
    static void CustomBuildAndRun()
    {
        CustomBuild(true);
    }

    static BuildOptions customBuildOptions = BuildOptions.None;
    static string savepath;
    static RemoveRequest Request;
    static Queue<string> packages = new Queue<string>();

    const string TEMP_FOLDER = ".imba_tmp";
    static List<string> excludeResourceFolders = new List<string>(new string[] {
        #if UNITY_IOS
            "StreamingAssets/Android"
        #endif
        #if UNITY_ANDROID
            "StreamingAssets/iOS"
        #endif
    });

    static void CustomBuild(bool buildAppBundle)
    {

        customBuildOptions = BuildOptions.None;

        EditorUserBuildSettings.buildAppBundle = buildAppBundle;




        if ((customBuildOptions & BuildOptions.ShowBuiltPlayer) != BuildOptions.ShowBuiltPlayer)
        {
            customBuildOptions = customBuildOptions | BuildOptions.ShowBuiltPlayer;
        }

        string extn = buildAppBundle ? "aab" : "apk";
        string version = "";

#if UNITY_ANDROID || UNITY_IOS
        UpdateVersionBuildNumber();
        version = PlayerSettings.bundleVersion;
#endif

        string app_name = PlayerSettings.productName + "_"
            + version
            + "_prod";

        Debug.LogError(app_name);
        savepath = EditorUtility.SaveFilePanel("Build " + EditorUserBuildSettings.activeBuildTarget,
                              EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget), app_name, extn);
        if (savepath.Length == 0)
            return;
        if (savepath.Length != 0)
        {
            string dir = System.IO.Path.GetDirectoryName(savepath); //get build directory
            string[] scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path.ToString();
            }
            PreExport_Local();
            BuildPipeline.BuildPlayer(scenes, savepath, EditorUserBuildSettings.activeBuildTarget, customBuildOptions);
            EditorUserBuildSettings.SetBuildLocation(EditorUserBuildSettings.activeBuildTarget, dir);
            PostExport();
        }
    }


#if UNITY_CLOUD_BUILD
    public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
    {
        UpdateVersionBuildNumber();
       
        PreExport_Local();
    }
#endif

    static void UpdateVersionBuildNumber()
    {
        //int buildNumber = 0;
        //increase build number
#if UNITY_ANDROID
        //buildNumber = UnityEditor.PlayerSettings.Android.bundleVersionCode;
        //buildNumber++;
        //UnityEditor.PlayerSettings.Android.bundleVersionCode = buildNumber;
#elif UNITY_IOS
        //buildNumber = int.Parse(UnityEditor.PlayerSettings.iOS.buildNumber);
        //buildNumber++;
        //UnityEditor.PlayerSettings.iOS.buildNumber = buildNumber.ToString();
#endif

        //Version oldVersion = Version.Parse(PlayerSettings.bundleVersion);
        //Version newVersion = new Version(oldVersion.Major, oldVersion.Minor, buildNumber);
        //Debug.Log($"UpdateVersionBuildNumber: Version changed from {oldVersion} to {newVersion}.");
        //PlayerSettings.bundleVersion = newVersion.ToString();
    }

    public static void PreExport_Local()
    {

        foreach (string ex in excludeResourceFolders)
        {
            MoveAssetFolder(ex, string.Format("{0}/{1}", TEMP_FOLDER, ex));
        }
        AssetDatabase.Refresh();
    }

    public static void PostExport()
    {
        foreach (string ex in excludeResourceFolders)
        {
            MoveAssetFolder(string.Format("{0}/{1}", TEMP_FOLDER, ex), ex);
        }
        AssetDatabase.Refresh();
    }

    static void Progress()
    {
        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
            {
                Debug.LogError("Removed: " + Request.PackageIdOrName);

            }
            else if (Request.Status >= StatusCode.Failure)
            {
                Debug.LogError(Request.Error.message);
            }
            if (packages.Count > 0)
            {
                Request = Client.Remove(packages.Dequeue());
            }
            else
            {
                EditorApplication.update -= Progress;
            }
        }
    }

    static void MoveAssetFolder(string fromDir, string toDir)
    {
        string fromFullPath = Application.dataPath + "/" + fromDir;

        if (!System.IO.Directory.Exists(fromFullPath))
        {
            Debug.LogWarning(fromDir + " not found!. SKIP");
            return;
        }

        string fromRoot = Path.GetFullPath(fromFullPath + "/../");

        string toFullPath = Application.dataPath + "/" + toDir;
        string toRoot = Path.GetFullPath(toFullPath + "/../");

        string folderName = Path.GetFileName(fromFullPath);
        string folderMetadata = folderName + ".meta";

        Debug.LogFormat("Move: {0} to {1}", fromDir, toDir);

        string root = Application.dataPath.Replace("/Assets", string.Empty);
        string parent = "/Assets";
        string[] folders = toDir.Split('/');
        for (int i = 0; i < folders.Length; i++)
        {
            string folder = "/" + folders[i];
            string dir = root + parent + folder;
            // Debug.Log (dir);
            if (i < folders.Length - 1)
            {
                if (!Directory.Exists(dir))
                {
                    Debug.Log("Create folder: " + dir);
                    // AssetDatabase.CreateFolder (parent, folders [i]);
                    Directory.CreateDirectory(dir);
                }
            }
            parent += folder;
        }

        FileUtil.DeleteFileOrDirectory(toRoot + folderMetadata);
        FileUtil.DeleteFileOrDirectory(toFullPath);
        FileUtil.MoveFileOrDirectory(fromRoot + folderMetadata, toRoot + folderMetadata);
        FileUtil.MoveFileOrDirectory(fromFullPath, toFullPath);
    }
}
