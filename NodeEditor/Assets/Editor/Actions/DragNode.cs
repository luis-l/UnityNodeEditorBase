
using UnityEngine;

public class DragNode : ActionBase
{
    private EditorNode _draggingNode;

    private Vector2 _startDragPos, _endDragPos;

    public const float dragSpeed = 1f;

    public override void Do()
    {
        _draggingNode = input.selectedNode;
        _startDragPos = _draggingNode.bodyRect.position;
    }

    public override void Undo()
    {
        _draggingNode.bodyRect.position = _startDragPos;
    }

    public override void Redo()
    {
        _draggingNode.bodyRect.position = _endDragPos;
    }

    public override void ActionUpdate()
    {
        NodeCanvas canvas = input.window.canvas;
        _draggingNode.bodyRect.position += Event.current.delta * canvas.ZoomScale * dragSpeed;
    }

    public override void OnActionDone()
    {
        _endDragPos = _draggingNode.bodyRect.position;
    }
}
