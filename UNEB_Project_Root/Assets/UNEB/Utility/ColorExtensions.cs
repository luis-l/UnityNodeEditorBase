
using UnityEngine;

namespace UNEB.Utility
{
    public static class ColorExtensions
    {
        public static Color From255(byte r, byte g, byte b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }
    }
}