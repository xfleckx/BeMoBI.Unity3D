using UnityEngine;
using UnityEditor;

namespace Assets.SNEED
{
    public class SNEEDPreferences
    {
        // Have we loaded the prefs yet
        private static bool prefsLoaded = false;
        
        public static bool debugVerbosity = false;

        public static string DefaultExportDirectory = "";
        
        [PreferenceItem("SNEED")]

        public static void PreferencesGUI()
        {
            // Load the preferences
            if (!prefsLoaded)
            {
                debugVerbosity = EditorPrefs.GetBool("DebugVerbosity", false);
                prefsLoaded = true;
            }

            // Preferences GUI
            debugVerbosity = EditorGUILayout.Toggle("Show DEBUG messages", debugVerbosity);

            EditorGUILayout.LabelField("Choose a default directory for the maze export function.");

            EditorGUILayout.BeginHorizontal();

            DefaultExportDirectory = EditorGUILayout.TextField(DefaultExportDirectory);

            if (GUILayout.Button("Select"))
            {
                DefaultExportDirectory = EditorUtility.OpenFolderPanel("Select a directory", Application.dataPath, "");
            }

            EditorGUILayout.EndHorizontal();

            // Save the preferences
            if (GUI.changed)
                EditorPrefs.SetBool("DebugVerbosity", debugVerbosity);
        }
    }
}