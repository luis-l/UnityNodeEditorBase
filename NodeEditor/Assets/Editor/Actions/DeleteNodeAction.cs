
using System.Collections.Generic;
using UnityEngine;

public class DeleteNodeAction : UndoableAction
{
    private NodeCanvas _canvas;
    private EditorNode _nodeRemoved = null;

    public override bool Init()
    {
        return manager.window.state.selectedNode != null;
    }

    public override void Do()
    {
        _canvas = manager.window.canvas;
        _nodeRemoved = manager.window.state.selectedNode;
        _canvas.Remove(_nodeRemoved);
    }

    public override void Undo()
    {
        _canvas.nodes.Add(_nodeRemoved);
    }

    public override void Redo()
    {
        _canvas.Remove(_nodeRemoved);
    }
}
