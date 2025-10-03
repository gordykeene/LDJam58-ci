using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class FluxImageGeneratorWindow : EditorWindow
{
    private FluxPromptSettings promptSettings;
    private string specificPrompt = "";
    private int imageCount = 1;
    private bool isGenerating = false;
    private string statusMessage = "";
    private Vector2 scrollPosition;

    private const string PREF_IMAGE_COUNT = "FluxGenerator_ImageCount";
    private const string PREF_SPECIFIC_PROMPT = "FluxGenerator_SpecificPrompt";
    
    private static readonly HttpClient httpClient = new HttpClient();

    [MenuItem("Tools/AI/Flux Image Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<FluxImageGeneratorWindow>("Flux Image Generator");
        window.minSize = new Vector2(400, 300);
    }

    private void OnEnable()
    {
        imageCount = EditorPrefs.GetInt(PREF_IMAGE_COUNT, 1);
        specificPrompt = EditorPrefs.GetString(PREF_SPECIFIC_PROMPT, "");
        
        // Load the previously assigned prompt settings
        string promptSettingsPath = EditorPrefs.GetString("FluxGenerator_PromptSettingsPath", "");
        if (!string.IsNullOrEmpty(promptSettingsPath))
        {
            promptSettings = AssetDatabase.LoadAssetAtPath<FluxPromptSettings>(promptSettingsPath);
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Flux Pro 1.1 Image Generator", EditorStyles.boldLabel);
        if (GUILayout.Button("Manage API Keys", GUILayout.Width(120)))
        {
            ProjectSecretsWindow.ShowWindow();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        // Prompt Settings ScriptableObject
        EditorGUI.BeginChangeCheck();
        promptSettings = (FluxPromptSettings)EditorGUILayout.ObjectField(
            "Prompt Settings", 
            promptSettings, 
            typeof(FluxPromptSettings), 
            false
        );
        if (EditorGUI.EndChangeCheck() && promptSettings != null)
        {
            EditorPrefs.SetString("FluxGenerator_PromptSettingsPath", AssetDatabase.GetAssetPath(promptSettings));
        }

        // Display base prompt if settings are assigned
        if (promptSettings != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Base Prompt:", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(promptSettings.BasePrompt, MessageType.None);
        }

        EditorGUILayout.Space(10);

        // Specific prompt input
        EditorGUILayout.LabelField("Specific Prompt:", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        specificPrompt = EditorGUILayout.TextArea(specificPrompt, GUILayout.Height(60));
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(PREF_SPECIFIC_PROMPT, specificPrompt);
        }

        EditorGUILayout.Space(10);

        // Image count dropdown
        EditorGUI.BeginChangeCheck();
        imageCount = EditorGUILayout.IntPopup("Number of Images", imageCount, new[] { "1", "4" }, new[] { 1, 4 });
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetInt(PREF_IMAGE_COUNT, imageCount);
        }

        EditorGUILayout.Space(10);

        // Generate button
        GUI.enabled = !isGenerating;
        if (GUILayout.Button(isGenerating ? "Generating..." : "Generate Images", GUILayout.Height(30)))
        {
            GenerateImages();
        }
        GUI.enabled = true;

        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private async void GenerateImages()
    {
        if (string.IsNullOrWhiteSpace(specificPrompt))
        {
            statusMessage = "ERROR: Specific prompt is required!";
            EditorUtility.DisplayDialog("Error", "Please enter a specific prompt before generating images.", "OK");
            return;
        }

        string apiKey = ProjectSecretsWindow.GetSecret("REPLICATE_API_TOKEN");
        
        // Fallback to environment variable if not in secrets
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("REPLICATE_API_TOKEN");
        }
        
        if (string.IsNullOrEmpty(apiKey))
        {
            statusMessage = "ERROR: REPLICATE_API_TOKEN not found!";
            bool openSecrets = EditorUtility.DisplayDialog(
                "API Key Required", 
                "REPLICATE_API_TOKEN not found in Project Secrets or environment variables.\n\nWould you like to open Project Secrets to add it?", 
                "Open Secrets", 
                "Cancel"
            );
            
            if (openSecrets)
            {
                ProjectSecretsWindow.ShowWindow();
            }
            return;
        }

        isGenerating = true;
        statusMessage = "Starting generation...";
        Repaint();

        try
        {
            string fullPrompt = specificPrompt;
            if (promptSettings != null && !string.IsNullOrWhiteSpace(promptSettings.BasePrompt))
            {
                fullPrompt = $"{specificPrompt}, {promptSettings.BasePrompt}";
            }

            await GenerateAndSaveImages(apiKey, fullPrompt, imageCount);
            
            statusMessage = $"Successfully generated {imageCount} image(s)!";
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            statusMessage = $"ERROR: {ex.Message}";
            Debug.LogError($"Flux generation error: {ex}");
            EditorUtility.DisplayDialog("Error", $"Failed to generate images:\n{ex.Message}", "OK");
        }
        finally
        {
            isGenerating = false;
            Repaint();
        }
    }

    private async Task GenerateAndSaveImages(string apiKey, string prompt, int count)
    {
        // Create output directory
        string outputDir = Path.Combine(Application.dataPath, "Generated");
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string promptPrefix = CreatePromptPrefix(specificPrompt);

        // Create multiple predictions in parallel
        statusMessage = $"Creating {count} prediction(s)...";
        Repaint();

        List<Task<string>> predictionTasks = new List<Task<string>>();
        for (int i = 0; i < count; i++)
        {
            predictionTasks.Add(CreatePredictionAndGetUrl(apiKey, prompt));
        }

        // Wait for all predictions to complete
        string[] imageUrls = await Task.WhenAll(predictionTasks);

        List<string> generatedFilePaths = new List<string>();

        // Download and save images
        for (int i = 0; i < imageUrls.Length; i++)
        {
            if (string.IsNullOrEmpty(imageUrls[i]))
            {
                Debug.LogWarning($"Image {i + 1} failed to generate");
                continue;
            }

            statusMessage = $"Downloading image {i + 1}/{imageUrls.Length}...";
            Repaint();

            byte[] imageData = await DownloadImage(imageUrls[i]);
            
            // Convert to JPG with quality 80
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            byte[] jpgData = texture.EncodeToJPG(80);
            DestroyImmediate(texture);

            // Save to file
            string filename = $"{promptPrefix}_{timestamp}_{i + 1}.jpg";
            string filepath = Path.Combine(outputDir, filename);
            File.WriteAllBytes(filepath, jpgData);

            Debug.Log($"Saved image to: {filepath}");
            generatedFilePaths.Add(filepath);
        }

        // Show the generated images in a preview window
        if (generatedFilePaths.Count > 0)
        {
            FluxImagePreviewWindow.ShowWindow(generatedFilePaths.ToArray());
        }
    }

    private async Task<string> CreatePredictionAndGetUrl(string apiKey, string prompt)
    {
        string requestBody = $@"{{
            ""input"": {{
                ""prompt"": ""{EscapeJsonString(prompt)}"",
                ""aspect_ratio"": ""1:1"",
                ""output_format"": ""jpg"",
                ""output_quality"": 80,
                ""prompt_upsampling"": true
            }}
        }}";

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.replicate.com/v1/models/black-forest-labs/flux-1.1-pro/predictions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Headers.Add("Prefer", "wait");
        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        
        // Parse output URL from response
        string outputUrl = ParseJsonValue(responseBody, "output");
        return outputUrl;
    }


    private async Task<byte[]> DownloadImage(string url)
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    private string ParseJsonValue(string json, string key)
    {
        string searchKey = $"\"{key}\":";
        int startIndex = json.IndexOf(searchKey);
        if (startIndex == -1) return null;

        startIndex += searchKey.Length;
        
        // Skip whitespace
        while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
            startIndex++;

        if (startIndex >= json.Length) return null;

        // Check if value is a string
        if (json[startIndex] == '"')
        {
            startIndex++;
            int endIndex = json.IndexOf('"', startIndex);
            return json.Substring(startIndex, endIndex - startIndex);
        }
        else
        {
            // Non-string value
            int endIndex = startIndex;
            while (endIndex < json.Length && json[endIndex] != ',' && json[endIndex] != '}' && json[endIndex] != ']')
                endIndex++;
            return json.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }


    private string EscapeJsonString(string str)
    {
        return str
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private string CreatePromptPrefix(string prompt)
    {
        // Take first 24 characters
        string prefix = prompt.Length > 24 ? prompt.Substring(0, 24) : prompt;
        
        // Convert to lowercase
        prefix = prefix.ToLower();
        
        // Replace non-alphanumeric characters with underscores
        StringBuilder sb = new StringBuilder();
        foreach (char c in prefix)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else if (char.IsWhiteSpace(c) || c == '-')
            {
                // Replace spaces and hyphens with underscores
                if (sb.Length > 0 && sb[sb.Length - 1] != '_')
                {
                    sb.Append('_');
                }
            }
        }
        
        // Remove trailing underscores
        string result = sb.ToString().TrimEnd('_');
        
        // Fallback if empty
        return string.IsNullOrEmpty(result) ? "image" : result;
    }
}


