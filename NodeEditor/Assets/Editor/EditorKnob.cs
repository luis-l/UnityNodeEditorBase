using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EditorKnob {

    public static readonly Vector2 kMinSize = new Vector2(10f, 30f);

    public Rect bodyRect = new Rect(Vector2.zero, kMinSize);

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
}
