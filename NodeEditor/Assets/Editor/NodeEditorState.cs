
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    public class NodeEditorState
    {

        public EditorNode selectedNode;
        public Vector2 lastClickedPosition;

        public EditorOutputKnob selectedOutput;
        public EditorInputKnob selectedInput;

        public System.Type typeToCreate;
    }
}