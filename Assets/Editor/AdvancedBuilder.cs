using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;

[InitializeOnLoad]
public static class BatchmodeBuilder
{
    public const string FLAG_BATCH_MODE = "-batchmode";
    public const string FLAG_BATCH_MODE_BUILDER = "-batchmodebuilder";
    public const string FLAG_DEV_BUILD = "-development";
    public const string FLAG_DEBUG_BUILD = "-debug";
    public const string ARG_BUILD_TARGET = "-buildtarget";
    public const string ARG_BUILD_OPTIONS = "-buildopts";
    public const string ARG_BUILD_PATH = "-buildpath";
    private static List<String> _args = null;
    private class BuildConfiguration
    {
        public readonly BuildOptions buildOptions;
        public readonly BuildTarget buildTarget;
        public readonly bool development;
        public readonly bool allowDebugging;
        public readonly string buildPath;
        public readonly string[] scenes;
        public BuildConfiguration(BuildTarget buildTarget, string buildPath, BuildOptions buildOptions, bool development, bool allowDebugging)
        {
            this.buildTarget = buildTarget;
            this.buildPath = buildPath;
            this.buildOptions = buildOptions;
            this.development = development;
            this.allowDebugging = allowDebugging;
            this.scenes = this._getScenes();
        }
        private string[] _getScenes()
        {
            return EditorBuildSettings.scenes.Select(l => l.path).ToArray();
        }
    }
    static BatchmodeBuilder()
    {
        EditorApplication.update += Init;
    }
    private static void Init()
    {
        EditorApplication.update -= Init;
        Build();
    }
    private static void Build()
    {
        if (!InitArgs())
        {
            return;
        }
        if(BuildPipeline.isBuildingPlayer)
        {
            return;
        }
        Debug.LogError("BatchmodeBuilder : Batchmode detected, builder trying to parse command line");
        BuildConfiguration configuration = ParseBuildConfiguration();
        if (configuration == null)
        {
            Debug.LogWarning("BatchmodeBuilder : failed to parse the command line, check your logs and try again");
            return;
        }
        Build(configuration);
    }
    private static bool InitArgs()
    {
        BatchmodeBuilder._args = System.Environment.GetCommandLineArgs().ToList();
        if (!BatchmodeBuilder.GetFlag(FLAG_BATCH_MODE, false) || !BatchmodeBuilder.GetFlag(FLAG_BATCH_MODE_BUILDER, false))
        {
            BatchmodeBuilder._args = null;
            return false;
        }
        return true;
    }
    private static void Build(BuildConfiguration configuration)
    {
        Debug.Log("BatchmodeBuilder : Starting build");
        Debug.Log(
            string.Format("BatchmodeBuilder Configuration : \n target path : {0}\n build target : {1}\n build options : {2}\n isDebug : {3}\n isDev : {4}\n",
            configuration.buildPath,
            configuration.buildTarget.ToString(),
            configuration.buildOptions.ToString(),
            configuration.allowDebugging,
            configuration.development));
        EditorUserBuildSettings.development = configuration.development;
        EditorUserBuildSettings.allowDebugging = configuration.allowDebugging;
        BuildPipeline.BuildPlayer(configuration.scenes, configuration.buildPath, configuration.buildTarget, configuration.buildOptions);
        EditorApplication.Exit(0);
    }
    private static BuildConfiguration ParseBuildConfiguration()
    {
        String buildTargetArg = BatchmodeBuilder.GetArg(ARG_BUILD_TARGET, null);
        if (buildTargetArg == null)
        {
            Debug.LogError(string.Format("BatchmodeBuilder : No {0} argument set BatchmodeBuilder aborting", ARG_BUILD_TARGET));
            return null;
        }
        if (!Enum.GetNames(typeof(BuildTarget)).Any(n => n.Equals(buildTargetArg)))
        {
            Debug.LogError(string.Format("BatchmodeBuilder : {0} argument unrecognized value {1}; BatchmodeBuilder aborting", ARG_BUILD_TARGET, buildTargetArg));
            return null;
        }
        String buildPath = BatchmodeBuilder.GetArg(ARG_BUILD_PATH, null);
        if (buildPath == null)
        {
            Debug.LogError(string.Format("BatchmodeBuilder : No {0} argument set BatchmodeBuilder aborting", ARG_BUILD_TARGET));
            return null;
        }
        BuildTarget buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), buildTargetArg);
        bool isDev = BatchmodeBuilder.GetFlag(FLAG_DEV_BUILD, false);
        bool isDebug = BatchmodeBuilder.GetFlag(FLAG_DEV_BUILD, false);
        String buildOptionsArgs = BatchmodeBuilder.GetArg(ARG_BUILD_OPTIONS, null);
        BuildOptions buildOptions = BuildOptions.None;
        if (buildOptionsArgs != null)
        {
            buildOptions = (BuildOptions)Enum.Parse(typeof(BuildOptions), buildOptionsArgs);
        }
        if (isDev)
        {
            buildOptions |= BuildOptions.Development;
        }
        if (isDebug)
        {
            buildOptions |= BuildOptions.AllowDebugging;
        }
        return new BuildConfiguration(buildTarget, buildPath, buildOptions, isDev, isDebug);
    }
    private static bool GetFlag(string name, bool defaultValue)
    {
        int index = BatchmodeBuilder._args.IndexOf(name);
        if (index >= 0)
        {
            return true;
        }
        return defaultValue;
    }
    private static string GetArg(string name, string defaultValue)
    {
        int index = BatchmodeBuilder._args.IndexOf(name);
        if (index >= 0 && index < BatchmodeBuilder._args.Count - 1)
        {
            return BatchmodeBuilder._args[index + 1];
        }
        return defaultValue;
    }
}
