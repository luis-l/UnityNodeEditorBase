
using UnityEngine;

namespace UNEB
{
    public class DragNode : MultiStageAction
    {
        private Node _draggingNode;

        private Vector2 _startDragPos, _endDragPos;

        public const float dragSpeed = 1f;

        public override void Undo()
        {
            _draggingNode.bodyRect.position = _startDragPos;
        }

        public override void Redo()
        {
            _draggingNode.bodyRect.position = _endDragPos;
        }

        public override void Do()
        {
            NodeEditor editor = manager.window.editor;
            _draggingNode.bodyRect.position += Event.current.delta * editor.ZoomScale * dragSpeed;
        }

        public override void OnActionStart()
        {
            _draggingNode = manager.window.state.selectedNode;
            _startDragPos = _draggingNode.bodyRect.position;
        }

        public override bool OnActionEnd()
        {
            _endDragPos = _draggingNode.bodyRect.position;
            return true;
        }
    }
}