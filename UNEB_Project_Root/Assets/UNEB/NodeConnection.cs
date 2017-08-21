using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    public abstract class NodeConnection : ScriptableObject
    {
        public static readonly Vector2 kMinSize = new Vector2(12f, 12f);
        public static readonly Vector2 kMinHalfSize = kMinSize / 2f;

        [HideInInspector]
        public Rect bodyRect = new Rect(Vector2.zero, kMinSize);
        
        public System.Func<object> getValue;

        public static System.Action<NodeConnection> OnConnectionCreated;

        [SerializeField, HideInInspector]
        protected Node parentNode;

        void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }

        public virtual void Init(Node parent)
        {
            name = "connection";
            parentNode = parent;
        }

        /// <summary>
        /// The node associated with this knob.
        /// </summary>
        public Node ParentNode
        {
            get { return parentNode; }
        }

        public abstract GUIStyle GetStyle();

        public virtual void OnConnectionGUI(int order)
        {
            OnNameGUI();

            // Position the knobs properly for the draw pass of the knobs in the editor.
            float yPos = parentNode.HeaderTop + order * GetStyle().fixedHeight + Node.kKnobOffset;
            bodyRect.center = new Vector2(GetNodeAnchor(), 0f);
            bodyRect.y = yPos;
        }

        public virtual void OnNameGUI()
        {
            GUILayout.Label(Content, GetStyle());
        }

        private GUIContent _content;
        private GUIContent Content
        {
            get
            {
                if (_content == null) {
                    _content = new GUIContent(name);
                }

                return _content;
            }
        }

        /// <summary>
        /// What side of the node should the knob anchor to.
        /// </summary>
        /// <returns></returns>
        public abstract float GetNodeAnchor();

        /// <summary>
        /// Attempts to get the value of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>()
        {
            if (getValue != null) {
                return (T)getValue();
            }

            return default(T);
        }
    }
}