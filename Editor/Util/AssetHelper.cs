using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
public static class AssetHelper {

    public static string CreateCompanionFolderForPrefab(string prefabPath)
    {
        int indexOfLastSlash = prefabPath.LastIndexOf('/');

        string folderForMazeContents = prefabPath.Substring(0, indexOfLastSlash);

        var prefabName = Path.GetFileNameWithoutExtension(prefabPath);

        string guid = AssetDatabase.CreateFolder(folderForMazeContents, prefabName);

        return AssetDatabase.GUIDToAssetPath(guid);
    }

}
