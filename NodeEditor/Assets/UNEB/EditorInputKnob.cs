
using UnityEngine;

namespace UNEB
{
    public class EditorInputKnob : EditorKnob
    {

        private EditorOutputKnob _connectedOutput;

        public EditorInputKnob(EditorNode parent)
            : base(parent)
        {
            name = "input";
        }

        public void Connect(EditorOutputKnob output)
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

        public EditorOutputKnob OutputConnection
        {
            get { return _connectedOutput; }
        }

        public override GUIStyle GetStyle()
        {
            var style = new GUIStyle();

            style.fixedHeight = kMinSize.y + EditorNode.kKnobOffset;
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