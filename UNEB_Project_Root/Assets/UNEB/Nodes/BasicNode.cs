
using UnityEngine;
using UnityEditor;

namespace UNEB
{
    public class BasicNode : Node
    {
        public override string name { get { return "Basic Node"; } }

        private int _someInt = 0;

        public override void Init()
        {
            base.Init();

            AddInput("Input");
            AddOutput("Ouput");

            FitKnobs();

            // Fit the int field, need to automate this.
            bodyRect.height += 20f;
        }

        public override void OnBodyGUI()
        {
            _someInt = EditorGUILayout.IntField("Int Value", _someInt);
        }

        public override void OnNewInputConnection(NodeInput addedInput)
        {
            Debug.Log("Added Input: " + addedInput.name);
        }

        public override void OnInputConnectionRemoved(NodeInput removedInput)
        {
            Debug.Log("Removed Input: " + removedInput.name);
        }
    }
}