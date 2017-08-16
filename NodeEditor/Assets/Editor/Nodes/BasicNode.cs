
using UnityEngine;
using UnityEditor;

public class BasicNode : EditorNode {

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

    public override void OnNewInputConnection(EditorInputKnob addedInput)
    {
        Debug.Log("Added Input: " + addedInput.name);
    }

    public override void OnInputConnectionRemoved(EditorInputKnob removedInput)
    {
        Debug.Log("Removed Input: " + removedInput.name);
    }
}
