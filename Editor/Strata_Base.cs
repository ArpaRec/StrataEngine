using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Strata_Base : EditorWindow
{
    public string editorWindowText = "Project Name";
    public string folderPath;

    void OnGUI()
    {
        string inputText = EditorGUILayout.TextField(editorWindowText, "MyProject");

        if (GUILayout.Button("Create"))
            // do create folder
            folderPath = inputText + "Game";
            AssetDatabase.CreateFolder("Assets", folderPath);
            AssetDatabase.CreateFolder("Assets/" + folderPath, "Content");
            AssetDatabase.CreateFolder("Assets/" + folderPath, "Plugins");
            AssetDatabase.CreateFolder("Assets/" + folderPath, "Platform");
            AssetDatabase.CreateFolder("Assets/" + folderPath, "Config");

        if (GUILayout.Button("Cancel"))
            Close();
    }

    [MenuItem("ArpaRec/Create Project [DEPRECATED]")]
    static void CreateProject()
    {
        Strata_Base window = new Strata_Base();
        window.ShowUtility();
    }
}
