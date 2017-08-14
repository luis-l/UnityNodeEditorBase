
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EditorOutputKnob : EditorKnob {

    public readonly bool bCanHaveMultipleConnections;

    private List<EditorInputKnob> _inputs;

    public EditorOutputKnob(EditorNode parent, bool canHaveMultipleConnections = true) : base(parent)
    {
        name = "output";
        bCanHaveMultipleConnections = canHaveMultipleConnections;
        _inputs = new List<EditorInputKnob>();
    }

    public IEnumerable<EditorInputKnob> Inputs
    {
        get { return _inputs; }
    }

    public int InputCount
    {
        get { return _inputs.Count; }
    }

    /// <summary>
    /// Returns true if the connection was added successfully.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool Add(EditorInputKnob input)
    {
        if (!CanConnectInput(input)) {
            return false;
        }

        // The input cannot be connected to anything.
        // This test is not inside the CanConnectInput() because
        // an action can remove the old connections automatically to make it
        // easier for the user to change connections between nodes.
        if (input.HasOutputConnected()) {

            // Changes to the inputs need to be properly handled by the Action system,
            // so it works with undo.
            Debug.LogWarning("Cannot add an input that is already connected");
            return false;
        }

        input.Connect(this);
        _inputs.Add(input);

        parentNode.OnNewInputConnection(input);

        return true;
    }

    /// <summary>
    /// Tests to see if the output can be connected to the input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool CanConnectInput(EditorInputKnob input)
    {
        if (input == null) {
            Debug.LogError("Attempted to add a null input.");
            return false;
        }

        // Avoid self-connecting
        if (parentNode.Inputs.Contains(input)) {
            Debug.LogWarning("Cannot self connect.");
            return false;
        }

        // Avoid connecting when it is already connected.
        if (input.OutputConnection == this) {
            Debug.LogWarning("Already Connected");
            return false;
        }

        return true;
    }

    public void Remove(EditorInputKnob input)
    {
        if (_inputs.Remove(input)) {
            parentNode.OnInputConnectionRemoved(input);
            input.Disconnect();
        }
    }

    public void RemoveAll()
    {
        foreach (EditorInputKnob input in _inputs) {
            parentNode.OnInputConnectionRemoved(input);
            input.Disconnect();
        }

        _inputs.Clear();
    }

    public override GUIStyle GetStyle()
    {
        var style = new GUIStyle();

        style.fixedHeight = kMinSize.y + EditorNode.kKnobOffset;
        style.alignment = TextAnchor.MiddleRight;
        style.normal.textColor = Color.white * 0.9f;
        style.padding.right = (int)kMinHalfSize.x + 5;

        return style;
    }

    /// <summary>
    /// The output is anchored at the right side of the node.
    /// </summary>
    /// <returns></returns>
    public override float GetNodeAnchor()
    {
        return parentNode.bodyRect.xMax;
    }
}
