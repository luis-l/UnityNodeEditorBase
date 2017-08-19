
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    public class CreateNodeAction : UndoableAction, IDisposable
    {
        private NodeGraph _graph;
        private Node _nodeCreated;

        public override bool Init()
        {
            System.Type t = manager.window.state.typeToCreate;
            return t != null && typeof(Node).IsAssignableFrom(t);
        }

        public override void Do()
        {
            _graph = manager.window.graph;

            var state = manager.window.state;

            _nodeCreated = SaveManager.CreateNode(state.typeToCreate, _graph);
            _nodeCreated.bodyRect.position = manager.window.state.lastClickedPosition;

            // Done with this type creation.
            state.typeToCreate = null;
        }

        public override void Undo()
        {
            _graph.Remove(_nodeCreated);
        }

        public override void Redo()
        {
            _graph.nodes.Add(_nodeCreated);
        }

        public void Dispose()
        {
            if (_nodeCreated) {
                ScriptableObject.DestroyImmediate(_nodeCreated, true);
            }
        }
    }
}