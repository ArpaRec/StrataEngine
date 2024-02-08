using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System;
using System.Security.Cryptography;

public class JsonDownloaderWindow : EditorWindow
{
    public static string nsUrl = "https://ns-rrs.arparec.dev";
    string destUrlFormat;
    string destFolderName = "Assets/_DownloadedAssets";
    private List<ItemData> itemList = new List<ItemData>();
    private Vector2 scrollPosition;

    [MenuItem("ArpaRec/Strata Asset Store")]
    static void OpenWindow()
    {
        JsonDownloaderWindow window = (JsonDownloaderWindow)EditorWindow.GetWindow(typeof(JsonDownloaderWindow));
        window.titleContent = new GUIContent("RRS Asset Store");
        window.Show();
        IncrementCounter("WindowOpened");

        string assetUrl = GetNestedSubstring(SendGetRequest(nsUrl), "assets- ", " -assets");



    }

    void OnGUI()
    {
        GUILayout.Label("RRS Asset Store", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        GUILayout.Label("Welcome to the ArpaRec RRS Asset Store!", EditorStyles.label);
        GUILayout.Label("Here, you can download assets", EditorStyles.label);
        GUILayout.Label("that are made specifically for RRS,", EditorStyles.label);
        GUILayout.Label("right here from the editor.", EditorStyles.label);
        GUILayout.Label("This is still in BETA, and might not work as intented.", EditorStyles.label);

        EditorGUILayout.Space();

        GUILayout.Label("Assets are downloaded to 'Assets/_DownloadedAssets'", EditorStyles.label);

        EditorGUILayout.Space();

        if (GUILayout.Button("REFRESH"))
        {
            LoadJsonFromApi();
            IncrementCounter("RefreshedByUser");
        }

        EditorGUILayout.Space();

        GUILayout.Label("Prefabs", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (ItemData item in itemList)
        {
            if (GUILayout.Button(item.name))
            {
                DownloadFile(item.url);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    void LoadJsonFromApi()
    {
        string apiUrl = "https://arparec-rrs-assets.firebaseio.com/.json"; // Asset Listing Repo URL
        destUrlFormat = "https://rrs.arparec.dev/assetstore/prefabs/"; // Formatting for destination file name

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        // Send the request and wait for a response
        request.SendWebRequest();

        while (!request.isDone)
        {
            // Waiting for the request to complete
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Wrap the JSON array in an object
            string jsonContent = "{\"items\":" + request.downloadHandler.text + "}";
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(jsonContent);

            itemList = wrapper.items;
        }
        else
        {
            // Debug.LogError("Failed to load JSON from API: " + request.error); // DEBUGGING
        }
    }

    void DownloadFile(string url)
    {
        string destinationPath = String.Format("{0}/{1}", destFolderName, url.Replace(destUrlFormat, ""));

        // Create the destination folder if it doesn't exist
        if (!Directory.Exists(destFolderName))
        {
            Directory.CreateDirectory(destFolderName);
        }

        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, destinationPath);
        AssetDatabase.Refresh();
        string noURL = url.Replace(destUrlFormat, "");
        string noExt = noURL.Replace(".prefab", "");
        IncrementCounter(noExt);
        IncrementCounter("TotalDownload");
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<ItemData> items;
    }

    [System.Serializable]
    public class ItemData
    {
        public string name;
        public string url;
    }
    private const string databaseUrl = "https://arparec-rrs-default-rtdb.firebaseio.com/";

    public static void IncrementCounter(string counterKey)
    {
        string apiUrl = $"{databaseUrl}{counterKey}.json";

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.method = "GET";

        var operation = request.SendWebRequest();

        while (!operation.isDone) { }

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError($"Error: {request.error}");
        }
        else
        {
            string currentData = request.downloadHandler.text;

            // Check if the currentData is null or empty
            if (currentData == "null" || string.IsNullOrEmpty(currentData.Trim()))
            {
                // If null or empty, set the current value to 0
                currentData = "0";
            }

            // Attempt to parse the current data as an integer
            if (int.TryParse(currentData, out int currentValue))
            {
                // Increment the value
                int newValue = currentValue + 1;

                UnityWebRequest updateRequest = UnityWebRequest.Put(apiUrl, newValue.ToString());
                updateRequest.method = "PUT";

                var updateOperation = updateRequest.SendWebRequest();

                while (!updateOperation.isDone) { }

                if (updateRequest.isNetworkError || updateRequest.isHttpError)
                {
                    Debug.LogError($"Error: {updateRequest.error}");
                }
            }
            else
            {
                Debug.LogError($"Error: Failed to parse current data as an integer: {currentData}");
            }
        }
    }

    public static string SendGetRequest(string url)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(url);

        // Send the request and wait for a response
        webRequest.SendWebRequest();

        // Check for errors
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + webRequest.error);
            return "Error: " + webRequest.error;
        }
        else
        {
            // Request successful
            Debug.Log("Response: " + webRequest.downloadHandler.text);
            return webRequest.downloadHandler.text;
        }
    }

    public static string GetNestedSubstring(string input, string index1, string index2)
    {
        string someString = input;
        int index1i = someString.IndexOf(index1);
        int index2i = someString.IndexOf(index2);
        someString = someString.Substring(index1i + 1, index2i - index1i - 1);
        return someString;
    }
}
