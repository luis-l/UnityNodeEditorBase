
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorNode {

    public static readonly Vector2 kDefaultSize = new Vector2(100f, 80f);

    /// <summary>
    /// The rect of the node in canvas space.
    /// </summary>
    public Rect bodyRect;

    /// <summary>
    /// A texture associated with the node.
    /// </summary>
    public Texture2D iconTex;

    public string name = "Node";

    /// <summary>
    /// How much additional offset to apply when resizing.
    /// </summary>
    public const float resizePaddingX = 20f;

    private GUIStyle _iconNameStyle;
    private GUIContent _iconNameContent;

    public EditorNode(string nodeName, Texture2D icon, Vector2 size)
    {
        name = nodeName;
        iconTex = icon;
        bodyRect.size = size;
    }

    #region Styles and Contents

    public string NiceName
    {
        get { return ObjectNames.NicifyVariableName(name); }
    }

    public GUIContent IconNameContent
    {
        get
        {
            if (_iconNameContent == null) {
                _iconNameContent = new GUIContent(NiceName, iconTex);
            }
            return _iconNameContent;
        }
    }

    public GUIStyle IconNameStyle
    {
        get
        {
            if (_iconNameStyle == null) {
                SetupStyle();
            }

            return _iconNameStyle;
        }
    }

    /// <summary>
    /// Sets up the style to render the node.
    /// </summary>
    public void SetupStyle()
    {
        _iconNameStyle = new GUIStyle();
        _iconNameStyle.normal.textColor = Color.white;
        _iconNameStyle.alignment = TextAnchor.LowerCenter;

        _iconNameStyle.imagePosition = ImagePosition.ImageAbove;

        // Test if the name fits
        Vector2 contentSize = _iconNameStyle.CalcSize(new GUIContent(NiceName));

        // Resize width of the node body.
        if (contentSize.x > bodyRect.width - resizePaddingX) {
            bodyRect.width = contentSize.x + resizePaddingX;
        }

        _iconNameStyle.fixedHeight = bodyRect.height - 5f;
        _iconNameStyle.fixedWidth = bodyRect.width;
    }

    #endregion
}
