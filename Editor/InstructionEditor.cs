using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(HUD_Instruction))]
public class InstructionEditor : Editor {

    private bool showHUDElementReferences = false;
    private int selectedSetIndex = 0;
    public override void OnInspectorGUI()
    {
        HUD_Instruction instructions = (HUD_Instruction)target;

        EditorGUILayout.BeginVertical();

        if(instructions.KnownSets.Any()){
            selectedSetIndex = instructions.KnownSets.Values.RenderAsSelectionBox(selectedSetIndex);
            instructions.currentInstructionSet = instructions.KnownSets.ElementAt(selectedSetIndex).Value;
        }
        else
            EditorGUILayout.HelpBox("No instructions available! Start with loading a set! - (Edit)", MessageType.Info);

        if (GUILayout.Button("Play Instructions"))
        {
            if (instructions.currentInstructionSet == null)
                instructions.currentInstructionSet = InstructionEditorWindow.CreateExampleInstructionSet();

            if(instructions.currentInstructionSet != null && !instructions.currentInstructionSet.instructions.Any())
                instructions.currentInstructionSet = InstructionEditorWindow.CreateExampleInstructionSet();

            instructions.StartDisplaying();
        }

        if (GUILayout.Button("Stop Instructions"))
        {
            instructions.StopDisplaying();
        }

        if (GUILayout.Button("Edit Instructions")) 
        {
            var window = (InstructionEditorWindow) InstructionEditorWindow.GetWindow(typeof(InstructionEditorWindow), true, "Instructions Editor");
            window.HudController = instructions;
            window.Show();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Skip Instruction"))
        {
            instructions.StopDisplaying();
        }

        EditorGUILayout.EndVertical();

        showHUDElementReferences = EditorGUILayout.Foldout(showHUDElementReferences, "HUD Elements");

        if (showHUDElementReferences)
        {
            instructions.InstructionTextBesideImage = (Text)EditorGUILayout.ObjectField(new GUIContent("Text renderer"), instructions.InstructionTextBesideImage, typeof(Text), true);
            instructions.InstructionTextWide = (Text)EditorGUILayout.ObjectField(new GUIContent("Text w/o Image"), instructions.InstructionTextWide, typeof(Text), true);
            instructions.InstructionImage = (RawImage)EditorGUILayout.ObjectField(new GUIContent("Image renderer"), instructions.InstructionImage, typeof(RawImage), true);

        }
        
    }

}
