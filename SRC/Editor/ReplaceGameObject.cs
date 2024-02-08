using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ReplaceGameObject : EditorWindow
{
    GameObject[] selectedObjects;
    GameObject replacementObject;
    bool copyRotation;
    bool copyScale;

    [MenuItem("ArpaRec/Optimiser/Object Replacer")]
    public static void ShowWindow() {
        ReplaceGameObject window = GetWindow<ReplaceGameObject>("Object Replacer");
        window.maxSize = new Vector2(window.maxSize.x, 150);
        window.minSize = new Vector2(250, 130);
    }

    private void OnGUI() {
        GUILayout.Space(15);
        replacementObject = (GameObject)EditorGUILayout.ObjectField("Replacement object", replacementObject, typeof(GameObject),true);
        GUILayout.Space(3);
        copyRotation = EditorGUILayout.Toggle("Copy Rotation", copyRotation);
        copyScale = EditorGUILayout.Toggle("Copy Scale", copyScale);

        GUILayout.Space(30);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Replace", GUILayout.Width(75))) {
            Replace();
        }
        GUILayout.EndHorizontal();
    }

    private void Replace()
    {
        selectedObjects = Selection.gameObjects;

        string prefabType = PrefabUtility.GetPrefabAssetType(replacementObject).ToString();
        string instanceStatus = null;
        if(prefabType == "Regular" || prefabType == "Variant")
            instanceStatus = PrefabUtility.GetPrefabInstanceStatus(replacementObject).ToString();
        
        List<GameObject> newSelected = new List<GameObject>();

        foreach (GameObject gameObject in selectedObjects)
        {
            if(gameObject == replacementObject) {
                newSelected.Add(gameObject);
                continue;
            }

            bool hasParent = false;
            Transform parent = null;
            if(gameObject.transform.parent != null) {
                hasParent = true;
                parent = gameObject.transform.parent;
            }

            GameObject newGameObject = null;
            
            if(prefabType == "Regular" || prefabType == "Variant") {
                if(instanceStatus == "Connected") {
                    Object newPrefab = PrefabUtility.GetCorrespondingObjectFromSource(replacementObject);
                    newGameObject = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
                    PrefabUtility.SetPropertyModifications(newGameObject, PrefabUtility.GetPropertyModifications(replacementObject));
                    if(hasParent)
                        newGameObject.transform.parent = parent;
                }
                else {
                    newGameObject = (GameObject)PrefabUtility.InstantiatePrefab(replacementObject);
                    if(hasParent)
                        newGameObject.transform.parent = parent;
                }
            }
            else {
                newGameObject = (GameObject)GameObject.Instantiate(replacementObject);
                newGameObject.name = gameObject.name;
                if(hasParent)
                    newGameObject.transform.parent = parent;
            }

            Undo.RegisterCreatedObjectUndo(newGameObject, "created object");

            newGameObject.transform.position = gameObject.transform.position;
            if(copyRotation)
                newGameObject.transform.rotation = gameObject.transform.rotation;
            if(copyScale)
                newGameObject.transform.localScale = gameObject.transform.localScale;

            Undo.DestroyObjectImmediate(gameObject);
            newSelected.Add(newGameObject);
        }
        Selection.objects = newSelected.ToArray();

        string goString = (newSelected.Count > 1) ? " GameObjects have " : " GameObject has ";
        if(prefabType == "Regular" || prefabType == "Variant") {
            if(instanceStatus == "Connected")
                prefabType = "Prefab Instance";
            else
                prefabType = "Prefab";
        }
        else
            prefabType = "None";
    }
}