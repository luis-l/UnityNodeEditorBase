
using UnityEditor;
using LibNoise.Generator;

public class VoronoiNode : EditorNode
{
    private Voronoi noise = new Voronoi();

    public VoronoiNode()
    {
        name = "Voronoi";

        var noiseIn = AddInput();
        noiseIn.name = "Input";

        var mask = AddInput();
        mask.name = "Mask";

        var noiseOut = AddOutput();
        noiseOut.name = "Output";

        FitKnobs();

        bodyRect.height += 65f;
        bodyRect.width = 150f;
    }

    public override void OnBodyGUI()
    {
        noise.Frequency = EditorGUILayout.DoubleField("Frequency", noise.Frequency);
        noise.Displacement = EditorGUILayout.DoubleField("Displacement", noise.Displacement);
        noise.UseDistance = EditorGUILayout.Toggle("Use Distance", noise.UseDistance);
    }
}