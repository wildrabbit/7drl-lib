using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;

[CustomEditor(typeof(MapEditorGUI))]
public class MapEditorGUI : MonoBehaviour
{
    public bool Shown = true;
    Tilemap _tileMap;
    TextAsset _mapFile;

    void OnSceneGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 250, 40));

        var rect = EditorGUILayout.BeginVertical();
        GUI.color = Color.yellow;
        GUI.Box(rect, GUIContent.none);

        GUI.color = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("File:");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("New"))
        {
            NewMap();
        }

        if (GUILayout.Button("Load"))
        {
            OpenMap();
        }

        if (GUILayout.Button("Save"))
        {
            if(_mapFile != null)
            {
                WriteMapContents();
            }
            else
            {
                SaveMapAs();
            }
        }

        if (GUILayout.Button("SaveAs"))
        {
            SaveMapAs();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        GUILayout.EndArea();
    }

    private void SaveMapAs()
    {
        throw new NotImplementedException();
    }

    private void OpenMap()
    {
        throw new NotImplementedException();
    }

    void NewMap()
    {
        
    }

    void WriteMapContents()
    {
        if (_mapFile == null)
        {
        }


    }
}
