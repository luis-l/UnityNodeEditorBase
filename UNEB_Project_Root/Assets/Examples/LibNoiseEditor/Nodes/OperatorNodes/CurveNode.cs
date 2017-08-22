
using UnityEngine;
using UnityEditor;
using UNEB;

public class CurveNode : Node
{
    public override string name { get { return "Curve"; } }

    private AnimationCurve _curve = new AnimationCurve();
    private readonly Rect kCurveRange = new Rect(-1, -1, 2, 2);

    private const float kBodyHeight = 100f;

    public override void Init()
    {
        var input = AddInput();
        input.name = "Input";

        var output = AddOutput();
        output.name = "Output";

        FitKnobs();

        bodyRect.height += kBodyHeight;
        bodyRect.width = 150f;
    }

    public override void OnBodyGUI()
    {
        float boxHeight = kBodyHeight - kHeaderHeight;
        _curve = EditorGUILayout.CurveField(_curve, Color.cyan, kCurveRange, GUILayout.Height(boxHeight), GUILayout.ExpandWidth(true));
    }
}
