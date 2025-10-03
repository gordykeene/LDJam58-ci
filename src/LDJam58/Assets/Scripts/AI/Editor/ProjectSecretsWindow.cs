using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ProjectSecretsWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<string, string> secrets = new Dictionary<string, string>();
    private string newKey = "";
    private string newValue = "";
    private string keyToDelete = "";
    
    private const string SECRETS_LIST_KEY = "ProjectSecrets_KeyList";
    private const string SECRETS_PREFIX = "ProjectSecrets_";

    [MenuItem("Tools/AI/Project Secrets")]
    public static void ShowWindow()
    {
        var window = GetWindow<ProjectSecretsWindow>("Project Secrets");
        window.minSize = new Vector2(450, 300);
    }

    private void OnEnable()
    {
        LoadSecrets();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Project Secrets Manager", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Store API keys and other secrets for your project. These are saved locally and will not be committed to version control.", MessageType.Info);
        EditorGUILayout.Space(10);

        // Display existing secrets
        EditorGUILayout.LabelField("Current Secrets:", EditorStyles.miniBoldLabel);
        EditorGUILayout.Space(5);

        if (secrets.Count == 0)
        {
            EditorGUILayout.HelpBox("No secrets stored yet.", MessageType.None);
        }
        else
        {
            var secretsList = secrets.ToList();
            foreach (var kvp in secretsList)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(200));
                
                // Show masked value
                string maskedValue = string.IsNullOrEmpty(kvp.Value) ? "(empty)" : new string('*', Mathf.Min(kvp.Value.Length, 20));
                EditorGUILayout.LabelField(maskedValue, GUILayout.Width(150));
                
                // Edit button
                if (GUILayout.Button("Edit", GUILayout.Width(50)))
                {
                    newKey = kvp.Key;
                    newValue = kvp.Value;
                }
                
                // Delete button
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    keyToDelete = kvp.Key;
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        // Handle deletion
        if (!string.IsNullOrEmpty(keyToDelete))
        {
            DeleteSecret(keyToDelete);
            keyToDelete = "";
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Add/Edit Secret:", EditorStyles.miniBoldLabel);
        EditorGUILayout.Space(5);

        // Add/Edit form
        EditorGUILayout.BeginVertical("box");
        
        newKey = EditorGUILayout.TextField("Key", newKey);
        newValue = EditorGUILayout.TextField("Value", newValue);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = !string.IsNullOrWhiteSpace(newKey);
        if (GUILayout.Button(secrets.ContainsKey(newKey) ? "Update Secret" : "Add Secret", GUILayout.Height(25)))
        {
            SaveSecret(newKey, newValue);
            newKey = "";
            newValue = "";
            GUI.FocusControl(null);
        }
        GUI.enabled = true;
        
        if (GUILayout.Button("Clear Form", GUILayout.Height(25), GUILayout.Width(100)))
        {
            newKey = "";
            newValue = "";
            GUI.FocusControl(null);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Quick setup for common secrets
        EditorGUILayout.LabelField("Quick Setup:", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Add Replicate API Key"))
        {
            newKey = "REPLICATE_API_TOKEN";
            newValue = secrets.ContainsKey(newKey) ? secrets[newKey] : "";
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void LoadSecrets()
    {
        secrets.Clear();
        string keyList = EditorPrefs.GetString(SECRETS_LIST_KEY, "");
        
        if (!string.IsNullOrEmpty(keyList))
        {
            string[] keys = keyList.Split('|');
            foreach (string key in keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    string value = EditorPrefs.GetString(SECRETS_PREFIX + key, "");
                    secrets[key] = value;
                }
            }
        }
    }

    private void SaveSecret(string key, string value)
    {
        secrets[key] = value;
        EditorPrefs.SetString(SECRETS_PREFIX + key, value);
        
        // Update key list
        string keyList = string.Join("|", secrets.Keys);
        EditorPrefs.SetString(SECRETS_LIST_KEY, keyList);
        
        Debug.Log($"Secret '{key}' saved successfully.");
    }

    private void DeleteSecret(string key)
    {
        if (secrets.ContainsKey(key))
        {
            secrets.Remove(key);
            EditorPrefs.DeleteKey(SECRETS_PREFIX + key);
            
            // Update key list
            string keyList = string.Join("|", secrets.Keys);
            EditorPrefs.SetString(SECRETS_LIST_KEY, keyList);
            
            Debug.Log($"Secret '{key}' deleted.");
        }
    }

    public static string GetSecret(string key)
    {
        return EditorPrefs.GetString(SECRETS_PREFIX + key, "");
    }

    public static bool HasSecret(string key)
    {
        return EditorPrefs.HasKey(SECRETS_PREFIX + key);
    }
}

