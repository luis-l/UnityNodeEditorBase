
using System.Collections.Generic;
using UnityEngine;

public class CreateNodeAction : UndoableAction
{
    private NodeCanvas _canvas;
    private EditorNode _nodeCreated;

    public override void Do()
    {
        _canvas = manager.window.canvas;
        _nodeCreated = _canvas.CreateBaseNode();

        _nodeCreated.bodyRect.position = manager.window.state.lastClickedPosition;

        _nodeCreated.AddInput();
        _nodeCreated.AddInput();

        _nodeCreated.AddOutput();
        _nodeCreated.AddOutput();
        _nodeCreated.AddOutput();
    }

    public override void Undo()
    {
        _canvas.Remove(_nodeCreated);
    }

    public override void Redo()
    {
        _canvas.nodes.Add(_nodeCreated);
    }
}
