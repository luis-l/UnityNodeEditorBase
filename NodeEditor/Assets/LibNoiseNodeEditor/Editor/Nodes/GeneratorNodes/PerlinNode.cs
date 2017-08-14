
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using LibNoise.Generator;

public class PerlinNode : EditorNode
{
    private Perlin _noise = new Perlin();

    public PerlinNode()
    {
        name = "Perlin";

        var noiseIn = AddInput();
        noiseIn.name = "Input";

        var mask = AddInput();
        mask.name = "Mask";

        var noiseOut = AddOutput();
        noiseOut.name = "Output";

        FitKnobs();

        bodyRect.height += 60f;
        bodyRect.width = 150f;
    }

    public override void OnBodyGUI()
    {
        _noise.OctaveCount = EditorGUILayout.IntField("Octaves", _noise.OctaveCount);
        _noise.Persistence = EditorGUILayout.DoubleField("Persistence", _noise.Persistence);
        _noise.Lacunarity = EditorGUILayout.DoubleField("Lacunarity", _noise.Lacunarity);
    }
}