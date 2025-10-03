using UnityEngine;
using UnityEditor;
using System.IO;

public class FluxImagePreviewWindow : EditorWindow
{
    private Texture2D[] previewTextures;
    private string[] imagePaths;
    private Vector2 scrollPosition;

    public static void ShowWindow(string[] filePaths)
    {
        var window = GetWindow<FluxImagePreviewWindow>("Generated Images");
        window.minSize = new Vector2(600, 400);
        window.LoadImages(filePaths);
        window.Show();
    }

    private void LoadImages(string[] filePaths)
    {
        imagePaths = filePaths;
        previewTextures = new Texture2D[filePaths.Length];

        for (int i = 0; i < filePaths.Length; i++)
        {
            if (File.Exists(filePaths[i]))
            {
                byte[] fileData = File.ReadAllBytes(filePaths[i]);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                previewTextures[i] = texture;
            }
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Generated Images", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        if (previewTextures != null && previewTextures.Length > 0)
        {
            // Display images in a grid
            if (previewTextures.Length == 1)
            {
                // Single image - display large
                DisplayImage(0, position.width - 40);
            }
            else if (previewTextures.Length == 4)
            {
                // 2x2 grid for 4 images
                float imageSize = Mathf.Min((position.width - 60) / 2, 400);
                
                EditorGUILayout.BeginHorizontal();
                DisplayImage(0, imageSize);
                GUILayout.Space(20);
                DisplayImage(1, imageSize);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(20);

                EditorGUILayout.BeginHorizontal();
                DisplayImage(2, imageSize);
                GUILayout.Space(20);
                DisplayImage(3, imageSize);
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No images to display", MessageType.Info);
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.EndScrollView();
    }

    private void DisplayImage(int index, float maxWidth)
    {
        if (index >= previewTextures.Length || previewTextures[index] == null)
            return;

        EditorGUILayout.BeginVertical(GUILayout.Width(maxWidth));

        Texture2D texture = previewTextures[index];
        float aspectRatio = (float)texture.height / texture.width;
        float displayHeight = maxWidth * aspectRatio;

        Rect imageRect = GUILayoutUtility.GetRect(maxWidth, displayHeight);
        GUI.DrawTexture(imageRect, texture, ScaleMode.ScaleToFit);

        // Display filename
        string filename = Path.GetFileName(imagePaths[index]);
        EditorGUILayout.LabelField(filename, EditorStyles.miniLabel);

        // Button to select in project
        if (GUILayout.Button("Select in Project", GUILayout.Width(maxWidth)))
        {
            // Convert to asset path
            string assetPath = "Assets" + imagePaths[index].Substring(Application.dataPath.Length);
            AssetDatabase.Refresh();
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void OnDestroy()
    {
        // Clean up textures
        if (previewTextures != null)
        {
            foreach (var texture in previewTextures)
            {
                if (texture != null)
                {
                    DestroyImmediate(texture);
                }
            }
        }
    }
}

