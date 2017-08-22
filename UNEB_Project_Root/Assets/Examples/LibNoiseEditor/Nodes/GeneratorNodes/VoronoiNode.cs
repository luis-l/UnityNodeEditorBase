
using UnityEditor;
using LibNoise.Generator;
using UNEB;

public class VoronoiNode : Node
{
    private Voronoi _noise = new Voronoi();

    public override void Init()
    {
        var noiseIn = AddInput();
        noiseIn.name = "Input";

        var mask = AddInput();
        mask.name = "Mask";

        var noiseOut = AddOutput();
        noiseOut.name = "Output";
        noiseOut.getValue = () => { return _noise; };

        FitKnobs();

        bodyRect.height += 80f;
        bodyRect.width = 150f;
    }

    public override void OnBodyGUI()
    {
        _noise.Seed = EditorGUILayout.IntField("Seed", _noise.Seed);
        _noise.Frequency = EditorGUILayout.DoubleField("Frequency", _noise.Frequency);
        _noise.Displacement = EditorGUILayout.DoubleField("Displacement", _noise.Displacement);
        _noise.UseDistance = EditorGUILayout.Toggle("Use Distance", _noise.UseDistance);
    }
}