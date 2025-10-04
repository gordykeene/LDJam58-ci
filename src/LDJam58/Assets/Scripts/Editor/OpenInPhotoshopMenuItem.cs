using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

public static class OpenInPhotoshopMenuItem
{
    private const string MENU_PATH = "Assets/Open in Photoshop";
    private const int MENU_PRIORITY = 20;

    [MenuItem(MENU_PATH, false, MENU_PRIORITY)]
    private static void OpenInPhotoshop()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null)
            return;

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        if (string.IsNullOrEmpty(assetPath))
            return;

        string fullPath = Path.GetFullPath(assetPath);
        
        if (!File.Exists(fullPath))
        {
            UnityEngine.Debug.LogError($"File not found: {fullPath}");
            return;
        }

        // Try to find Photoshop executable
        string photoshopPath = FindPhotoshopPath();
        
		if (string.IsNullOrEmpty(photoshopPath))
		{
			// Try macOS bundle id first
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				try
				{
					Process.Start("open", $"-b com.adobe.Photoshop \"{fullPath}\"");
					UnityEngine.Debug.Log($"Opening {fullPath} in Photoshop via bundle id");
					return;
				}
				catch (System.Exception)
				{
					// ignore and fall back below
				}
			}

			// Fallback: try to open with default application
			UnityEngine.Debug.LogWarning("Photoshop not found. Opening with default application...");
			if (Application.platform == RuntimePlatform.OSXEditor)
				Process.Start("open", $"\"{fullPath}\"");
			else
				Process.Start(fullPath);
			return;
		}

		try
		{
			if (Application.platform == RuntimePlatform.OSXEditor && Directory.Exists(photoshopPath))
			{
				// photoshopPath points to an .app bundle directory
				Process.Start("open", $"-a \"{photoshopPath}\" \"{fullPath}\"");
			}
			else
			{
				Process.Start(photoshopPath, $"\"{fullPath}\"");
			}
			UnityEngine.Debug.Log($"Opening {fullPath} in Photoshop");
		}
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to open Photoshop: {ex.Message}");
            // Fallback to default application
			if (Application.platform == RuntimePlatform.OSXEditor)
				Process.Start("open", $"\"{fullPath}\"");
			else
				Process.Start(fullPath);
        }
    }

    [MenuItem(MENU_PATH, true)]
    private static bool ValidateOpenInPhotoshop()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null)
            return false;

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        if (string.IsNullOrEmpty(assetPath))
            return false;

        // Check if the selected asset is an image
        string extension = Path.GetExtension(assetPath).ToLower();
        return extension == ".png" || 
               extension == ".jpg" || 
               extension == ".jpeg" || 
               extension == ".psd" || 
               extension == ".tga" || 
               extension == ".bmp" || 
               extension == ".tiff" || 
               extension == ".tif" || 
               extension == ".gif";
    }

    private static string FindPhotoshopPath()
    {
        // Common Photoshop installation paths on Windows
        string[] possiblePaths = new string[]
        {
            // Adobe Creative Cloud versions
            @"C:\Program Files\Adobe\Adobe Photoshop 2025\Photoshop.exe",
            @"C:\Program Files\Adobe\Adobe Photoshop 2024\Photoshop.exe",
            @"C:\Program Files\Adobe\Adobe Photoshop 2023\Photoshop.exe",
            @"C:\Program Files\Adobe\Adobe Photoshop 2022\Photoshop.exe",
            @"C:\Program Files\Adobe\Adobe Photoshop 2021\Photoshop.exe",
            @"C:\Program Files\Adobe\Adobe Photoshop 2020\Photoshop.exe",
            @"C:\Program Files\Adobe\Adobe Photoshop CC 2019\Photoshop.exe",
            @"C:\Program Files\Adobe\Adobe Photoshop CC 2018\Photoshop.exe",
            
            // 32-bit installations
            @"C:\Program Files (x86)\Adobe\Adobe Photoshop 2025\Photoshop.exe",
            @"C:\Program Files (x86)\Adobe\Adobe Photoshop 2024\Photoshop.exe",
            @"C:\Program Files (x86)\Adobe\Adobe Photoshop 2023\Photoshop.exe",
            @"C:\Program Files (x86)\Adobe\Adobe Photoshop 2022\Photoshop.exe",
            @"C:\Program Files (x86)\Adobe\Adobe Photoshop 2021\Photoshop.exe",
            @"C:\Program Files (x86)\Adobe\Adobe Photoshop 2020\Photoshop.exe",
            @"C:\Program Files (x86)\Adobe\Adobe Photoshop CC 2019\Photoshop.exe",
            @"C:\Program Files (x86)\Adobe\Adobe Photoshop CC 2018\Photoshop.exe",
        };

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

		// Check EditorPrefs for custom path
		string customPath = EditorPrefs.GetString("CustomPhotoshopPath", "");
		if (!string.IsNullOrEmpty(customPath) && (File.Exists(customPath) || Directory.Exists(customPath)))
			return customPath;

		// macOS: attempt to autodetect Adobe Photoshop .app bundles
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			try
			{
				List<string> candidates = new List<string>();
				string userHome = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
				string[] roots = new string[]
				{
					"/Applications",
					Path.Combine(userHome, "Applications")
				};

				foreach (string root in roots)
				{
					if (!Directory.Exists(root))
						continue;

					// Direct .app bundles
					string[] directApps = Directory.GetDirectories(root, "Adobe Photoshop*.app", SearchOption.TopDirectoryOnly);
					if (directApps != null && directApps.Length > 0)
						candidates.AddRange(directApps);

					// Version directories that may contain the .app bundle
					string[] versionFolders = Directory.GetDirectories(root, "Adobe Photoshop*", SearchOption.TopDirectoryOnly);
					foreach (string folder in versionFolders)
					{
						try
						{
							string[] innerApps = Directory.GetDirectories(folder, "Adobe Photoshop*.app", SearchOption.TopDirectoryOnly);
							if (innerApps != null && innerApps.Length > 0)
								candidates.AddRange(innerApps);
						}
						catch { }
					}
				}

				if (candidates.Count > 0)
				{
					candidates.Sort();
					return candidates[candidates.Count - 1];
				}
			}
			catch { }
		}

		return null;
    }

    // Allow users to set a custom Photoshop path
    [MenuItem("Tools/Set Custom Photoshop Path")]
    private static void SetCustomPhotoshopPath()
    {
        string currentPath = EditorPrefs.GetString("CustomPhotoshopPath", "");
        
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            string startDir = string.IsNullOrEmpty(currentPath) ? "/Applications" : currentPath;
            string path = EditorUtility.OpenFolderPanel("Select Photoshop .app bundle", startDir, "");
            
            if (!string.IsNullOrEmpty(path))
            {
                EditorPrefs.SetString("CustomPhotoshopPath", path);
                UnityEngine.Debug.Log($"Custom Photoshop path set to: {path}");
            }
        }
        else
        {
            string path = EditorUtility.OpenFilePanel("Select Photoshop Executable", 
                string.IsNullOrEmpty(currentPath) ? @"C:\\Program Files\\Adobe" : Path.GetDirectoryName(currentPath), 
                "exe");

            if (!string.IsNullOrEmpty(path))
            {
                EditorPrefs.SetString("CustomPhotoshopPath", path);
                UnityEngine.Debug.Log($"Custom Photoshop path set to: {path}");
            }
        }
    }
}

