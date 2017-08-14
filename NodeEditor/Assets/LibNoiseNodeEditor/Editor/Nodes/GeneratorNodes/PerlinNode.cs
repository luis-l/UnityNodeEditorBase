
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
        noiseOut.getValue = () => { return _noise; };

        FitKnobs();

        bodyRect.height += 75f;
        bodyRect.width = 150f;
    }

    public override void OnBodyGUI()
    {
        _noise.OctaveCount = EditorGUILayout.IntField("Octaves", _noise.OctaveCount);
        _noise.Persistence = EditorGUILayout.DoubleField("Persistence", _noise.Persistence);
        _noise.Frequency = EditorGUILayout.DoubleField("Frequency", _noise.Frequency);
        _noise.Lacunarity = EditorGUILayout.DoubleField("Lacunarity", _noise.Lacunarity);
    }
}