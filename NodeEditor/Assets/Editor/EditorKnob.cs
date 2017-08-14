using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EditorKnob {

    public static readonly Vector2 kMinSize = new Vector2(15f, 15f);
    public static readonly Vector2 kMinHalfSize = kMinSize / 2f;

    public Rect bodyRect = new Rect(Vector2.zero, kMinSize);
    public string name = "knob";

    protected EditorNode parentNode;
    public EditorKnob(EditorNode parentNode)
    {
        this.parentNode = parentNode;
    }

    /// <summary>
    /// The node associated with this knob.
    /// </summary>
    public EditorNode ParentNode
    {
        get { return parentNode; }
    }

    public abstract GUIStyle GetStyle();

    public virtual void OnGUI(int order)
    {
        OnNameGUI();

        // Position the knobs properly for the draw pass of the knobs in the editor.
        float yPos = parentNode.HeaderTop + order * GetStyle().fixedHeight + EditorNode.kKnobOffset;
        bodyRect.center = new Vector2(GetNodeAnchor(), 0f);
        bodyRect.y = yPos;
    }

    public virtual void OnNameGUI()
    {
        GUILayout.Label(name, GetStyle());
    }

    /// <summary>
    /// What side of the node should the knob anchor to.
    /// </summary>
    /// <returns></returns>
    public abstract float GetNodeAnchor();
}
