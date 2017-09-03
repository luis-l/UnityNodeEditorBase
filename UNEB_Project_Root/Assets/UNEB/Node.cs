
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UNEB.Utility;

namespace UNEB
{
    /// <summary>
    /// The visual representation of a logic unit such as an object or function.
    /// </summary>
    public abstract class Node : ScriptableObject
    {
        public static readonly Vector2 kDefaultSize = new Vector2(140f, 110f);

        /// <summary>
        /// The space reserved between knobs.
        /// </summary>
        public const float kKnobOffset = 4f;

        /// <summary>
        /// The space reserved for the header (title) of the node.
        /// </summary>
        public const float kHeaderHeight = 15f;

        /// <summary>
        /// The max label width for a field in the body.
        /// </summary>
        public const float kBodyLabelWidth = 100f;

        /// <summary>
        /// The rect of the node in canvas space.
        /// </summary>
        [HideInInspector]
        public Rect bodyRect;

        /// <summary>
        /// How much additional offset to apply when resizing.
        /// </summary>
        public const float resizePaddingX = 20f;

        [SerializeField, HideInInspector]
        private List<NodeOutput> _outputs = new List<NodeOutput>();

        [SerializeField, HideInInspector]
        private List<NodeInput> _inputs = new List<NodeInput>();

        // Hides the node asset.
        // Sets up the name via type information.
        void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;
            name = GetType().Name;

#if UNITY_EDITOR
            name = ObjectNames.NicifyVariableName(name);
#endif

        }

        /// <summary>
        /// Always call the base OnDisable() to cleanup the connection objects.
        /// </summary>
        protected virtual void OnDestroy()
        {
            _inputs.RemoveAll(
                (input) =>
                {
                    ScriptableObject.DestroyImmediate(input, true);
                    return true;
                });

            _outputs.RemoveAll(
                (output) =>
                {
                    ScriptableObject.DestroyImmediate(output, true);
                    return true;
                });
        }

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        public virtual void Init() {
            bodyRect.size = kDefaultSize;
        }

        public virtual void OnNodeGUI()
        {
            OnNodeHeaderGUI();
            OnConnectionsGUI();
            onBodyGuiInternal();
        }

        /// <summary>
        /// Renders the node connections. By default, after the header.
        /// </summary>
        public virtual void OnConnectionsGUI()
        {
            int inputCount = _inputs.Count;
            int outputCount = _outputs.Count;

            int maxCount = (int)Mathf.Max(inputCount, outputCount);

            // The entire knob section is stacked rows of inputs and outputs.
            for (int i = 0; i < maxCount; ++i) {

                GUILayout.BeginHorizontal();

                // Render the knob layout horizontally.
                if (i < inputCount) _inputs[i].OnConnectionGUI(i);
                if (i < outputCount) _outputs[i].OnConnectionGUI(i);

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Render the title/header of the node. By default, renders on top of the node.
        /// </summary>
        public virtual void OnNodeHeaderGUI()
        {
            // Draw header
            GUILayout.Box(name, HeaderStyle);
        }

        /// <summary>
        /// Draws the body of the node. By default, after the connections.
        /// </summary>
        public virtual void OnBodyGUI() { }

        // Handles the coloring and layout of the body.
        // This is for convenience so the user does not need to worry about this boiler plate code.
        protected virtual void onBodyGuiInternal()
        {
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = kBodyLabelWidth;

            // Cache the old label style.
            // Do this first before changing the EditorStyles.label style.
            // So the original values are kept.
            var oldLabelStyle = UnityLabelStyle;

            // Setup new values for the label style.
            EditorStyles.label.normal = DefaultStyle.normal;
            EditorStyles.label.active = DefaultStyle.active;
            EditorStyles.label.focused = DefaultStyle.focused;

            EditorGUILayout.BeginVertical();

            GUILayout.Space(kKnobOffset);
            OnBodyGUI();

            // Revert back to old label style.
            EditorStyles.label.normal = oldLabelStyle.normal;
            EditorStyles.label.active = oldLabelStyle.active;
            EditorStyles.label.focused = oldLabelStyle.focused;

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndVertical();
        }

        public NodeInput AddInput(string name = "input")
        {
            var input = NodeInput.Create(this);
            input.name = name;
            _inputs.Add(input);

            return input;
        }

        public NodeOutput AddOutput(string name = "output")
        {
            var output = NodeOutput.Create(this);
            output.name = name;
            _outputs.Add(output);

            return output;
        }

        /// <summary>
        /// Called when the output knob had an input connection removed.
        /// </summary>
        /// <param name="removedInput"></param>
        public virtual void OnInputConnectionRemoved(NodeInput removedInput) { }

        /// <summary>
        /// Called when the output knob made a connection to an input knob.
        /// </summary>
        /// <param name="addedInput"></param>
        public virtual void OnNewInputConnection(NodeInput addedInput) { }

        public IEnumerable<NodeOutput> Outputs
        {
            get { return _outputs; }
        }

        public IEnumerable<NodeInput> Inputs
        {
            get { return _inputs; }
        }

        public int InputCount
        {
            get { return _inputs.Count; }
        }

        public int OutputCount
        {
            get { return _outputs.Count; }
        }

        public NodeInput GetInput(int index)
        {
            return _inputs[index];
        }

        public NodeOutput GetOutput(int index)
        {
            return _outputs[index];
        }

        /// <summary>
        /// Get the Y value of the top header.
        /// </summary>
        public float HeaderTop
        {
            get { return bodyRect.yMin + kHeaderHeight; }
        }

        /// <summary>
        /// Resize the node to fit the knobs.
        /// </summary>
        public void FitKnobs()
        {
            int maxCount = (int)Mathf.Max(_inputs.Count, _outputs.Count);

            float totalKnobsHeight = maxCount * NodeConnection.kMinSize.y;
            float totalOffsetHeight = (maxCount - 1) * kKnobOffset;

            float heightRequired = totalKnobsHeight + totalOffsetHeight + kHeaderHeight;

            // Add some extra height at the end.
            bodyRect.height = heightRequired + kHeaderHeight / 2f;
        }

        #region Styles and Contents

        private static GUIStyle _unityLabelStyle;

        /// <summary>
        /// Caches the default EditorStyle.
        /// There is a strange bug with it being overriden when opening an Animation window.
        /// </summary>
        public static GUIStyle UnityLabelStyle
        {
            get
            {
                if (_unityLabelStyle == null) {
                    _unityLabelStyle = new GUIStyle(EditorStyles.label);
                }

                return _unityLabelStyle;
            }
        }

        private static GUIStyle _defStyle;
        public static GUIStyle DefaultStyle
        {
            get
            {
                if (_defStyle == null) {
                    _defStyle = new GUIStyle(EditorStyles.label);
                    _defStyle.normal.textColor = Color.white * 0.9f;
                    _defStyle.active.textColor = ColorExtensions.From255(126, 186, 255) * 0.9f;
                    _defStyle.focused.textColor = ColorExtensions.From255(126, 186, 255);
                }

                return _defStyle;
            }
        }

        private static GUIStyle _headerStyle;
        public GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null) {

                    _headerStyle = new GUIStyle();

                    _headerStyle.stretchWidth = true;
                    _headerStyle.alignment = TextAnchor.MiddleLeft;
                    _headerStyle.padding.left = 5;
                    _headerStyle.normal.textColor = Color.white * 0.9f;
                    _headerStyle.fixedHeight = kHeaderHeight;
                }

                return _headerStyle;
            }
        }

        #endregion
    }
}