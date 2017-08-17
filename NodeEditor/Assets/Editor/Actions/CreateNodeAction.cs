
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    public class CreateNodeAction : UndoableAction
    {
        private NodeCanvas _canvas;
        private EditorNode _nodeCreated;

        public override bool Init()
        {
            System.Type t = manager.window.state.typeToCreate;
            return t != null && typeof(EditorNode).IsAssignableFrom(t);
        }

        public override void Do()
        {
            _canvas = manager.window.canvas;

            var state = manager.window.state;

            _nodeCreated = _canvas.CreateNode(state.typeToCreate);
            _nodeCreated.bodyRect.position = manager.window.state.lastClickedPosition;

            // Done with this type creation.
            state.typeToCreate = null;
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
}