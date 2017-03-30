using UnityEditor;
using UnityEngine;
using System;
using System.Diagnostics;

public class SimpleBuilder {
    static public string[] scenes = new string[] { "Assets/s1.unity" };
    static public string path = "./";
    public static void Log(string message){
        System.Console.WriteLine(message);
    }
    public static void BasicBuild(BuildTarget buildTarget, string subFolder, string extension)
    {
        SimpleBuilder.Log("Starting");
        if(BuildPipeline.isBuildingPlayer){
            SimpleBuilder.Log("Unity already running, aborting build process");
            return;
        }
        string buildResult = BuildPipeline.BuildPlayer(scenes, path + "/" + subFolder + "/ZGame" + extension, buildTarget, BuildOptions.None);
        if (buildResult.Length > 0) {
            throw new Exception("BuildPlayer failure: " + buildResult);
        }
        SimpleBuilder.Finish();
    }
    public static void BuildAndroid(){
        SimpleBuilder.BasicBuild(BuildTarget.Android, "/.build/Android", ".apk");
    }
    public static void BuildWindows(){
        SimpleBuilder.BasicBuild(BuildTarget.StandaloneWindows, "/.build/Windows", ".exe");
    }
    public static void BuildLinux(){
        SimpleBuilder.BasicBuild(BuildTarget.StandaloneLinuxUniversal, "/.build/Linux", "");
    }
    public static void BuildWebGL(){
        SimpleBuilder.BasicBuild(BuildTarget.WebGL, "/.build/WebGL", "");
    }
    public static void BuildOSX(){
        SimpleBuilder.BasicBuild(BuildTarget.StandaloneOSXUniversal, "/.build/OSX", ".app");
    }
    public static void BuildiOS(){
        SimpleBuilder.BasicBuild(BuildTarget.iOS, "/.build/iOS", "");
    }
    public static void Finish(){
        SimpleBuilder.Log("Finished, game created");
        EditorApplication.Exit(0);
    }
}
