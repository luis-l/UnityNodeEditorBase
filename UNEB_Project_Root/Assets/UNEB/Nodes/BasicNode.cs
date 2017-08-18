
using UnityEngine;
using UnityEditor;

namespace UNEB
{
    public class BasicNode : Node
    {

        private int _someInt = 0;

        public BasicNode()
        {
            name = "Basic Node";

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