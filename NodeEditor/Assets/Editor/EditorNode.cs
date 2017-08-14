
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// The visual representation of a logic unit such as an object or function.
/// </summary>
public class EditorNode
{
    public static readonly Vector2 kDefaultSize = new Vector2(140f, 110f);

    /// <summary>
    /// The space reserved between knobs.
    /// </summary>
    public const float kKnobOffset = 4f;

    /// <summary>
    /// The space reserved for the header (title) of the node.
    /// </summary>
    public const float kHeaderHeight = 15f;

    /// <summary>
    /// The rect of the node in canvas space.
    /// </summary>
    public Rect bodyRect;

    public string name = "Node";

    /// <summary>
    /// How much additional offset to apply when resizing.
    /// </summary>
    public const float resizePaddingX = 20f;

    private List<EditorOutputKnob> _outputs = new List<EditorOutputKnob>();
    private List<EditorInputKnob> _inputs = new List<EditorInputKnob>();

    public EditorNode()
    {
        bodyRect.size = kDefaultSize;
    }

    public EditorNode(string nodeName, Texture2D icon, Vector2 size)
    {
        name = nodeName;
        bodyRect.size = size;
    }

    public EditorInputKnob AddInput()
    {
        var input = new EditorInputKnob(this);
        _inputs.Add(input);

        return input;
    }

    public EditorOutputKnob AddOutput()
    {
        var output = new EditorOutputKnob(this);
        _outputs.Add(output);

        return output;
    }

    /// <summary>
    /// Called when the output knob had an input connection removed.
    /// </summary>
    /// <param name="removedInput"></param>
    public virtual void OnInputConnectionRemoved(EditorInputKnob removedInput) { }

    /// <summary>
    /// Called when the output knob made a connection to an input knob.
    /// </summary>
    /// <param name="addedInput"></param>
    public virtual void OnNewInputConnection(EditorInputKnob addedInput) { }

    public IEnumerable<EditorOutputKnob> Outputs
    {
        get { return _outputs; }
    }

    public IEnumerable<EditorInputKnob> Inputs
    {
        get { return _inputs; }
    }

    public int InputCount
    {
        get { return _inputs.Count; }
    }

    public int OutputCount
    {
        get { return _outputs.Count; }
    }

    public EditorInputKnob GetInput(int index)
    {
        return _inputs[index];
    }

    public EditorOutputKnob GetOutput(int index)
    {
        return _outputs[index];
    }

    /// <summary>
    /// Get the Y value of the top header.
    /// </summary>
    public float HeaderTop
    {
        get { return bodyRect.yMin + kHeaderHeight; }
    }

    #region Styles and Contents

    public GUIStyle HeaderStyle
    {
        get
        {
            var style = new GUIStyle();
            
            style.stretchWidth = true;
            style.alignment = TextAnchor.MiddleLeft;
            style.padding.left = 5;
            style.normal.textColor = Color.white * 0.9f;
            style.normal.background = TextureLib.GetTintTex("Square", ColorExtensions.From255(79, 82, 94));
            style.fixedHeight = kHeaderHeight;

            return style;
        }
    }

    /// <summary>
    /// Resize the node to fit the knobs.
    /// </summary>
    public void FitKnobs()
    {
        int maxCount = (int)Mathf.Max(_inputs.Count, _outputs.Count);

        float totalKnobsHeight = maxCount * EditorKnob.kMinSize.y;
        float totalOffsetHeight = (maxCount - 1) * kKnobOffset;

        float heightRequired = totalKnobsHeight + totalOffsetHeight + kHeaderHeight;

        bodyRect.height = heightRequired;
    }

    #endregion
}
