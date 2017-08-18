
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    public class NodeEditorState
    {

        public Node selectedNode;
        public Vector2 lastClickedPosition;

        public NodeOutput selectedOutput;
        public NodeInput selectedInput;

        public System.Type typeToCreate;
    }
}