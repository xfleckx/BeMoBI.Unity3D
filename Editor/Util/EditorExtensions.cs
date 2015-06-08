using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
 
public static class EditorExtensions
{

    public static int RenderAsSelectionBox<T>(this IEnumerable<T> list, int selectionIndex)
    {
        int optionCount = list.Count();
        string[] options = new string[optionCount];
        list.Select(i => i.ToString()).ToArray().CopyTo(options, 0);
        selectionIndex = EditorGUILayout.Popup(selectionIndex, options);
        return selectionIndex;
    } 
} 