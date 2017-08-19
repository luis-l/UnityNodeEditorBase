
using System.Linq;
using UnityEngine;

namespace UNEB
{
    public class NodeInput : NodeConnection
    {
        [SerializeField]
        private NodeOutput _connectedOutput;

        public override void Init(Node parent)
        {
            base.Init(parent);
            name = "input";
        }

        /// <summary>
        /// Should only be called by NodeOutput
        /// </summary>
        /// <param name="output"></param>
        internal void Connect(NodeOutput output)
        {
            if (!HasOutputConnected()) {
                _connectedOutput = output;
            }

            else {

                const string msg = 
                    "Connot Connect." +
                    "The Input has an output connected and should be disconnected first, " +
                    "before trying to connect to some output.";

                Debug.LogWarning(msg);
            }
        }

        /// <summary>
        /// Should only be called by NodeOutput.
        /// </summary>
        internal void Disconnect()
        {
            if (_connectedOutput.Inputs.Contains(this)) {
                
                const string msg = 
                    "Cannot disconnect. " +
                    "The Output should remove this Input from its input list first, " +
                    "before calling Disconnect().";

                Debug.LogWarning(msg);
            }

            else {
                _connectedOutput = null;
            }
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

        public static NodeInput Create(Node parent)
        {
            var input = ScriptableObject.CreateInstance<NodeInput>();
            input.Init(parent);

            return input;
        }
    }
}