
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using LibNoise.Generator;

public class PerlinNode : EditorNode
{
    private Perlin noise = new Perlin();

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

        bodyRect.height += 65f;
        bodyRect.width = 150f;
    }

    public override void OnBodyGUI()
    {
        noise.OctaveCount = EditorGUILayout.IntField("Octaves", noise.OctaveCount);
        noise.Persistence = EditorGUILayout.DoubleField("Persistence", noise.Persistence);
        noise.Lacunarity = EditorGUILayout.DoubleField("Lacunarity", noise.Lacunarity);
    }
}