
using System.Collections.Generic;
using UnityEngine;

public class DeleteNodeAction : ActionBase
{
    private NodeCanvas _canvas;
    private EditorNode _nodeRemoved = null;

    public override bool Init()
    {
        return input.selectedNode != null;
    }

    public override void Do()
    {
        _canvas = input.window.canvas;
        _nodeRemoved = input.selectedNode;
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
