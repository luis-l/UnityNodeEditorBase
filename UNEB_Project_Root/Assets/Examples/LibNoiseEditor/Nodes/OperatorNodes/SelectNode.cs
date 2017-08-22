using LibNoise.Operator;

using UnityEngine;
using UnityEditor;
using UNEB;

public class SelectNode : Node
{
    private Select _op = new Select();

    public override void Init()
    {
        var inputA = AddInput();
        inputA.name = "Input A";

        var inputB = AddInput();
        inputB.name = "Input B";

        var output = AddOutput();
        output.name = "Output";

        FitKnobs();

        bodyRect.height += 60f;
        bodyRect.width = 150f;
    }

    public override void OnBodyGUI()
    {
        _op.FallOff = EditorGUILayout.DoubleField("Fall Off", _op.FallOff);
        _op.Minimum = EditorGUILayout.DoubleField("Min Bound", _op.Minimum);
        _op.Maximum = EditorGUILayout.DoubleField("Max Bound", _op.Maximum);
    }
}
