
using System.Collections.Generic;
using UnityEngine;

public class EditorOutputKnob : EditorKnob {

    private List<EditorInputKnob> _inputs;

    public EditorOutputKnob(EditorNode parent) : base(parent)
    {

    }

    public IEnumerable<EditorInputKnob> Inputs
    {
        get { return _inputs; }
    }

    public void Add(EditorInputKnob input)
    {
        // Avoid connecting when it is already connected.
        if (input.OutputConnection == this) {
            Debug.LogWarning("Already Connected");
            return;
        }

        // Unparent
        if (input.HasOutputConnected()) {
            input.OutputConnection.Remove(input);
        }

        input.Connect(this);

        if (!parentNode.bCanHaveMultipleOutputs) {
            RemoveAll();
        }

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
