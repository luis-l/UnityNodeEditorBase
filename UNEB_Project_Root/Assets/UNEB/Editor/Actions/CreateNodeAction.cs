
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    public class CreateNodeAction : UndoableAction
    {
        private NodeGraph _graph;
        private Node _nodeCreated;

        // The node referenced can only be destroyed if the 
        // create action has been undone.
        private bool _bCanDeleteNode = false;

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
            _bCanDeleteNode = true;
        }

        public override void Redo()
        {
            _graph.Add(_nodeCreated);
            _bCanDeleteNode = false;
        }

        public override void OnDestroy()
        {
            if (_bCanDeleteNode && _nodeCreated) {
                ScriptableObject.DestroyImmediate(_nodeCreated, true);
            }
        }
    }
}