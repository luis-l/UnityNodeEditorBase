
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

/// <summary>
/// A static library that loads and stores references to textures via name.
/// </summary>
public class TextureLib
{
    public const string kStandardTexturesFolder = "NodeEditorTextures";
    public enum TexType { PNG, JPEG, BMP };

    private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    public static void LoadStandardTextures()
    {
        _textures.Clear();

        LoadTexture("UnityLogo");
        LoadTexture("GrayGradient");
        LoadTexture("Grid");
        LoadTexture("Circle");
    }

    public static void LoadTexture(string name, TexType type = TexType.PNG)
    {
        string path = GetTextureFolderPath() + name + GetTexTypeExtension(type);

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        if (tex != null) {
            _textures.Add(name, tex);
        }

        else {
            Debug.LogError("The texture: " + path + " could not be found.");
        }
    }

    public static string GetTexTypeExtension(TexType type)
    {
        switch (type) {
            case TexType.PNG: return ".png";
            case TexType.JPEG: return ".jpg";
            case TexType.BMP: return ".bmp";
        }

        return "";
    }

    public static Texture2D GetTexture(string name)
    {
        if (_textures.ContainsKey(name)) {
            return _textures[name];
        }

        Debug.LogError(name + " is not loaded in the texture library.");
        return null;
    }

    public static string GetTextureFolderPath()
    {
        string fullpath = GetFullPath(Application.dataPath, kStandardTexturesFolder);

        if (fullpath != null) {

            // Return the texture folder path relative to Unity's Asset folder.
            int index = fullpath.IndexOf("Assets");
            string localPath = fullpath.Substring(index);

            return localPath + '/';
        }

        Debug.LogError("Could not find folder: " + kStandardTexturesFolder);
        return "";
    }

    static string GetFullPath(string root, string targetFolderName)
    {
        string[] dirs = Directory.GetDirectories(root, targetFolderName, SearchOption.AllDirectories);

        // Return first occurance containing targetFolderName.
        if (dirs.Length != 0) {
            return dirs[0];
        }
        
        // Could not find anything.
        return null;
    }
}
