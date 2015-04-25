using UnityEngine;
using System.Collections;
using UnityEditor;

public static class AssetHelper {


    public static string CreateCompanionFolderForPrefab(string prefabPath)
    {
        int indexOfLastSlash = prefabPath.LastIndexOf('/');
        int indexOfFileExtension = prefabPath.LastIndexOf('.');
        string folderForMazeContents = prefabPath.Substring(0, indexOfLastSlash);

        int fileNameLength = prefabPath.Length - indexOfFileExtension - 1;

        string prefabName = prefabPath.Substring(indexOfLastSlash, fileNameLength);

        string guid = AssetDatabase.CreateFolder(folderForMazeContents, prefabName);

        return AssetDatabase.GUIDToAssetPath(guid);
    }

}
