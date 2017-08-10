
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeleteNodeAction : UndoableAction
{
    private NodeCanvas _canvas;
    private EditorNode _nodeRemoved = null;

    private EditorOutputKnob _oldConnectedOutput;
    private List<EditorInputKnob> _oldConnectedInputs;

    public override bool Init()
    {
        return manager.window.state.selectedNode != null;
    }

    public override void Do()
    {
        _canvas = manager.window.canvas;
        _nodeRemoved = manager.window.state.selectedNode;
        _canvas.Remove(_nodeRemoved);

        _oldConnectedOutput = _nodeRemoved.input.OutputConnection;
        _oldConnectedInputs = _nodeRemoved.output.Inputs.ToList();

        disconnectOldOutput();
        _nodeRemoved.output.RemoveAll();
    }

    public override void Undo()
    {
        _canvas.nodes.Add(_nodeRemoved);
        reconnectOldConnections();
    }

    public override void Redo()
    {
        _canvas.Remove(_nodeRemoved);
        disconnectOldConnections();
    }

    private void disconnectOldConnections()
    {
        disconnectOldOutput();
        _nodeRemoved.output.RemoveAll();
    }

    private void reconnectOldConnections()
    {
        reconnectOldOutput();
        
        foreach (EditorInputKnob input in _oldConnectedInputs) {
            _nodeRemoved.output.Add(input);
        }
    }

    private void disconnectOldOutput()
    {
        if (_oldConnectedOutput != null) {
            _oldConnectedOutput.Remove(_nodeRemoved.input);
        }
    }

    private void reconnectOldOutput()
    {
        if (_oldConnectedOutput != null) {
            _oldConnectedOutput.Add(_nodeRemoved.input);
        }
    }
}
