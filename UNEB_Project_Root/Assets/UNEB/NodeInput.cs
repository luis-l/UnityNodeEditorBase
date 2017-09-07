
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UNEB
{
    public class NodeInput : NodeConnection
    {
        [SerializeField]
        private bool _bCanHaveMultipleConnections;

        [SerializeField]
        private List<NodeOutput> _outputs = new List<NodeOutput>();

        private GUIStyle _style;

        public virtual void Init(Node parent, bool multipleConnections = true)
        {
            base.Init(parent);
            name = "input";
            _bCanHaveMultipleConnections = multipleConnections;
        }

        /// <summary>
        /// Should only be called by NodeOutput
        /// </summary>
        /// <param name="output"></param>
        internal void Connect(NodeOutput output)
        {
            if (!_outputs.Contains(output))
                _outputs.Add(output);
        }

        /// <summary>
        /// Should only be called by NodeOutput.
        /// </summary>
        internal void Disconnect(NodeOutput output)
        {
            if (_outputs.Contains(output))
                _outputs.Remove(output);
        }

        public bool HasOutputConnected()
        {
            return _outputs.Count > 0;
        }

        public void RemoveAll() {
            // Cache the outputs since in order to disconnect them,
            // they must be removed from _outputs List.
            var outputs = new List<NodeOutput>(_outputs);

            _outputs.Clear();

            foreach (NodeOutput output in outputs) {
                output.Remove(this);
            }
        }

        public List<NodeOutput> Outputs
        {
            get { return _outputs; }
        }

        public int OutputCount
        {
            get { return _outputs.Count; }
        }

        public NodeOutput GetOutput(int index)
        {
            return _outputs[index];
        }

        public override GUIStyle GetStyle()
        {
            if (_style == null) {

                _style = new GUIStyle();

                _style.fixedHeight = kMinSize.y + Node.kKnobOffset;
                _style.alignment = TextAnchor.MiddleLeft;
                _style.normal.textColor = Color.white * 0.9f;
                _style.padding.left = (int)kMinHalfSize.x + 5;
            }

            return _style;
        }

        /// <summary>
        /// The input is anchored at the left side of the node.
        /// </summary>
        /// <returns></returns>
        public override float GetNodeAnchor()
        {
            return parentNode.bodyRect.xMin;
        }

        public bool bCanHaveMultipleConnections
        {
            get { return _bCanHaveMultipleConnections; }
        }

        public static NodeInput Create(Node parent, bool multipleConnections = false)
        {
            var input = ScriptableObject.CreateInstance<NodeInput>();
            input.Init(parent, multipleConnections);

            NodeConnection.OnConnectionCreated(input);
            return input;
        }
    }
}