
using UnityEngine;

namespace UNEB
{
    public class NodeInput : NodeConnection
    {

        private NodeOutput _connectedOutput;

        public NodeInput(Node parent)
            : base(parent)
        {
            name = "input";
        }

        public void Connect(NodeOutput output)
        {
            if (!HasOutputConnected()) {
                _connectedOutput = output;
            }
        }

        public void Disconnect()
        {
            _connectedOutput = null;
        }

        public bool HasOutputConnected()
        {
            return _connectedOutput != null;
        }

        public NodeOutput OutputConnection
        {
            get { return _connectedOutput; }
        }

        public override GUIStyle GetStyle()
        {
            var style = new GUIStyle();

            style.fixedHeight = kMinSize.y + Node.kKnobOffset;
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = Color.white * 0.9f;
            style.padding.left = (int)kMinHalfSize.x + 5;

            return style;
        }

        /// <summary>
        /// The input is anchored at the left side of the node.
        /// </summary>
        /// <returns></returns>
        public override float GetNodeAnchor()
        {
            return parentNode.bodyRect.xMin;
        }
    }
}