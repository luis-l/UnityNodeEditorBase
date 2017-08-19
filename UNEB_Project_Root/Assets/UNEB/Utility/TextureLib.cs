
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace UNEB.Utility
{
    /// <summary>
    /// A static library that loads and stores references to textures via name.
    /// </summary>
    public class TextureLib
    {
        public const string kStandardTexturesFolder = "UNEB Textures";
        public enum TexType { PNG, JPEG, BMP };

        private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private static Dictionary<TintedTextureKey, Texture2D> _tintedTextures = new Dictionary<TintedTextureKey, Texture2D>();

        public static void LoadStandardTextures()
        {
            _textures.Clear();
            _tintedTextures.Clear();

            LoadTexture("Grid");
            LoadTexture("Circle");
            LoadTexture("Square");
        }

        public static void LoadTexture(string name, TexType type = TexType.PNG)
        {
            string ext = GetTexTypeExtension(type);
            string filename = name.Ext(ext);
            string path = GetTextureFolderPath().Dir(filename);

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
                case TexType.PNG: return "png";
                case TexType.JPEG: return "jpg";
                case TexType.BMP: return "bmp";
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

        /// <summary>
        /// Gets the texture with a tint.
        /// It must be already loaded in the library.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Texture2D GetTintTex(string name, Color color)
        {
            var key = new TintedTextureKey(color, name);

            // Check if it already exsists in the tint dictionary.
            if (_tintedTextures.ContainsKey(key)) {

                if (_tintedTextures[key]) {
                    return _tintedTextures[key];
                }

                // Rebuild texture.
                else {
                    _tintedTextures[key] = tintCopy(GetTexture(name), color);
                    return _tintedTextures[key];
                }
            }

            // Make a new tint from the pre-loaded texture.
            Texture2D tex = GetTexture(name);

            // Tint the tex and add to tinted tex dictionary.
            if (tex) {

                var tintedTex = tintCopy(tex, color);
                _tintedTextures.Add(key, tintedTex);

                return tintedTex;
            }

            return null;
        }

        private static Texture2D tintCopy(Texture2D tex, Color color)
        {
            int pixCount = tex.width * tex.height;

            var tintedTex = new Texture2D(tex.width, tex.height);
            tintedTex.alphaIsTransparency = true;

            var newPixels = new Color[pixCount];
            var pixels = tex.GetPixels();

            for (int i = 0; i < pixCount; ++i) {
                newPixels[i] = color;
                newPixels[i].a = pixels[i].a;
            }

            tintedTex.SetPixels(newPixels);
            tintedTex.Apply();

            return tintedTex;
        }

        public static string GetTextureFolderPath()
        {
            string fullpath = getFullPath(Application.dataPath, kStandardTexturesFolder);

            if (!string.IsNullOrEmpty(fullpath)) {

                // Return the texture folder path relative to Unity's Asset folder.
                int index = fullpath.IndexOf("Assets");
                string localPath = fullpath.Substring(index);

                return localPath;
            }

            Debug.LogError("Could not find folder: " + kStandardTexturesFolder);
            return "";
        }

        private static string getFullPath(string root, string targetFolderName)
        {
            string[] dirs = Directory.GetDirectories(root, targetFolderName, SearchOption.AllDirectories);

            // Return first occurance containing targetFolderName.
            if (dirs.Length != 0) {
                return dirs[0];
            }

            // Could not find anything.
            return "";
        }

        private struct TintedTextureKey
        {
            public readonly Color color;
            public readonly string texName;

            public TintedTextureKey(Color c, string name)
            {
                color = c;
                texName = name;
            }

            public override int GetHashCode()
            {
                return color.GetHashCode() * texName.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is TintedTextureKey) {

                    var key = (TintedTextureKey)obj;
                    return key.color == color && key.texName == texName;
                }

                else {
                    return false;
                }
            }
        }
    }
}