
using System.Collections.Generic;
using UnityEngine;

public class EditorOutputKnob : EditorKnob {

    private List<EditorInputKnob> _inputs;

    public EditorOutputKnob(EditorNode parent) : base(parent)
    {
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

    public void Add(EditorInputKnob input)
    {
        // the input cannot be connected to anything.
        if (input.HasOutputConnected()) {

            // Inputs need to be properly handled by the Action system.
            // So it works with undo.
            Debug.LogWarning("Cannot add an input that is already connected");
            return;
        }

        // Avoid connecting when it is already connected.
        if (input.OutputConnection == this) {
            Debug.LogWarning("Already Connected");
            return;
        }

        input.Connect(this);
        _inputs.Add(input);

        parentNode.OnNewInputConnection(input);
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
}
