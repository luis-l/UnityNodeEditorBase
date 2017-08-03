
using System.Collections.Generic;
using UnityEngine;

public class CreateNodeAction : ActionBase
{
    private NodeCanvas _canvas;
    private EditorNode _nodeCreated;

    public override void Do()
    {
        _canvas = input.window.canvas;
        _nodeCreated = _canvas.CreateBaseNode();

        _nodeCreated.bodyRect.position = input.lastClickedPosition;
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
