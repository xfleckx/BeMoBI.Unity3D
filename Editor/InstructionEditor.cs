using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(HUD_Instruction))]
public class InstructionEditor : Editor {
    
    public override void OnInspectorGUI()
    {
        HUD_Instruction instructions = (HUD_Instruction)target;
        base.OnInspectorGUI();
     }

}
