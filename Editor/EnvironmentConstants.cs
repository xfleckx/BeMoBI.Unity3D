using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class EditorEnvironmentConstants : ScriptableObject
{
    public const string ASSET_PACKAGE_NAME = "BeMoBI.Unity3D";

    public const string ASSET_DIR = "Assets";

    public const string PREFABS_DIR = "Prefabs";
    public const string PREFAB_EXTENSION = "prefab";
    public const string MODELS_DIR = "Models";

    public static string Get_BASE_ASSET_PATH(){
        return ASSET_DIR + Path.AltDirectorySeparatorChar + ASSET_PACKAGE_NAME;
    }

    public static string Get_PREFAB_DIR_PATH()
    {
        return Get_BASE_ASSET_PATH() + Path.AltDirectorySeparatorChar + PREFABS_DIR;
    }

    public static string Get_MODEL_DIR_PATH()
    {
        return Get_BASE_ASSET_PATH() + Path.AltDirectorySeparatorChar + MODELS_DIR;
    }
}