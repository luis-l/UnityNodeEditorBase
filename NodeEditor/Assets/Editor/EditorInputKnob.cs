
using UnityEngine;

public class EditorInputKnob : EditorKnob {

    private EditorOutputKnob _connectedOutput;

    public EditorInputKnob(EditorNode parent) : base(parent)
    {

    }

    public void Connect(EditorOutputKnob output)
    {
        if (!HasOutputConnected()) {
            _connectedOutput = output;
        }
    }

    public void Disconnect()
    {
        if (HasOutputConnected()) {
            _connectedOutput = null;
        }
    }

    public bool HasOutputConnected()
    {
        return _connectedOutput != null;
    }

    public EditorOutputKnob OutputConnection
    {
        get { return _connectedOutput; }
    }
}
