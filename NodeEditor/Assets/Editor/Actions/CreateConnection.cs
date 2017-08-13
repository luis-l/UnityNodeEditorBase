
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class CreateConnection : MultiStageAction
{
    private EditorInputKnob _input;
    private EditorOutputKnob _output;

    // The output of the old node it was connected to.
    private EditorOutputKnob _oldConnectedOutput;

    // Old inputs of the node.
    private List<EditorInputKnob> _oldConnectedInputs;

    public override void Do()
    {
        manager.window.state.selectedOutput = _output;
    }

    public override void Undo()
    {
        _output.Remove(_input);
        reconnectOldConnections();
    }

    public override void Redo()
    {
        disconnectOldConnections();
        _output.Add(_input);
    }

    private void reconnectOldConnections()
    {
        //  Re-connect old connections
        if (_oldConnectedOutput != null) {
            _oldConnectedOutput.Add(_input);
        }

        if (_oldConnectedInputs != null) {
            foreach (var input in _oldConnectedInputs) {
                _output.Add(input);
            }
        }
    }

    private void disconnectOldConnections()
    {
        // Remove old connections
        if (_oldConnectedOutput != null) {
            _oldConnectedOutput.Remove(_input);
        }

        if (_oldConnectedInputs != null) {
            _output.RemoveAll();
        }
    }

    public override void OnActionStart()
    {
        _output = manager.window.state.selectedOutput;
    }

    public override bool OnActionEnd()
    {
        manager.window.state.selectedOutput = null;
        manager.window.editor.OnMouseOverInput((input) => { _input = input; });

        // Make the connection.
        if (_input != null && _output.CanConnectInput(_input)) {

            cacheOldConnections();
            disconnectOldConnections();
            return _output.Add(_input);
        }

        return false;
    }

    private void cacheOldConnections()
    {
        // Check if the receiving node was already connected.
        if (_input != null && _input.HasOutputConnected()) {
            _oldConnectedOutput = _input.OutputConnection;
        }

        // Check if the origin node already had inputs
        if (!_output.ParentNode.bCanHaveMultipleOutputs && _output.InputCount > 0) {
            _oldConnectedInputs = _output.Inputs.ToList();
        }
    }
}
