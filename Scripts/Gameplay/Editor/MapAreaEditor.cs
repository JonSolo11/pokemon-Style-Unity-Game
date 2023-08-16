using System.Net.WebSockets;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]

public class NewBehaviourScript : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int totalChance = serializedObject.FindProperty("totalChance").intValue;

        GUILayout.Label($"Total Chance  = {totalChance}");

        if(totalChance != 100)
            EditorGUILayout.HelpBox("Total Chance NOT 100", MessageType.Error);
    }
}
