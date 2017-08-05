
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class CreateConnection : MultiStageAction
{

    private EditorInputKnob _input;
    private EditorOutputKnob _output;

    // The output of the old node it was connected to.
    private EditorOutputKnob _oldNodeOutput;

    // Old inputs of the origin node.
    private List<EditorInputKnob> _oldOriginInputs;

    public override void Do()
    {
        manager.window.state.selectedOutput = _output;
    }

    public override void Undo()
    {
        _output.Remove(_input);

        //  Re-connect old connections
        if (_oldNodeOutput != null) {
            _oldNodeOutput.Add(_input);
        }

        if (_oldOriginInputs != null) {
            foreach (var input in _oldOriginInputs) {
                _output.Add(input);
            }
        }
    }

    public override void Redo()
    {
        // Remove old connections
        if (_oldNodeOutput != null) {
            _oldNodeOutput.Remove(_input);
        }

        if (_oldOriginInputs != null) {
            _output.RemoveAll();
        }

        _output.Add(_input);
    }

    public override void OnActionStart()
    {
        _output = manager.window.state.selectedOutput;
    }

    public override void OnActionEnd()
    {
        manager.window.state.selectedOutput = null;
        manager.window.editor.OnMouseOverNode_OrInput((node) => { _input = node.input; });
    }

    public override bool ValidateActionEnd()
    {
        // Make the connection.
        if (_input != null) {

            cacheOldConnections();
            Redo();
            return true;
        }

        return false;
    }

    private void cacheOldConnections()
    {
        // Check if the receiving node was already connected.
        if (_input != null && _input.HasOutputConnected()) {
            _oldNodeOutput = _input.OutputConnection;
        }

        // Check if the origin node already had inputs
        if (!_output.ParentNode.bCanHaveMultipleOutputs && _output.InputCount > 0) {
            _oldOriginInputs = _output.Inputs.ToList();
        }
    }
}
