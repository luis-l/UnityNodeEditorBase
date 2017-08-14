
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

        bodyRect.height += 95f;
        bodyRect.width = 150f;
    }

    public override void OnBodyGUI()
    {
        EditorGUI.BeginChangeCheck();

        _noise.Seed = EditorGUILayout.IntField("Seed", _noise.Seed);
        _noise.OctaveCount = EditorGUILayout.IntField("Octaves", _noise.OctaveCount);
        _noise.Persistence = EditorGUILayout.DoubleField("Persistence", _noise.Persistence);
        _noise.Frequency = EditorGUILayout.DoubleField("Frequency", _noise.Frequency);
        _noise.Lacunarity = EditorGUILayout.DoubleField("Lacunarity", _noise.Lacunarity);

        if (EditorGUI.EndChangeCheck()) {
            updateOutputNodes();
        }
    }

    private void updateOutputNodes()
    {
        // Temp solution.
        var dfs = new Stack<EditorNode>();

        dfs.Push(this);

        while (dfs.Count != 0) {

            var node = dfs.Pop();

            // Search neighbors
            foreach (var output in node.Outputs) {
                foreach (var input in output.Inputs) {
                    dfs.Push(input.ParentNode);
                }
            }

            var outputNode = node as OutputTexture2D;
            if (outputNode != null) {
                outputNode.UpdateTexture();
            }
        }
    }
}