# Unity Node Editor Base

Basic editor extension functionality to get a node editor up and running. (Click on image to see video preview)

Main Features
* Editor view with panning, zooming, and grid background
* Save System using Scriptable Objects
* Customizable GUI
* Create, delete, drag nodes
* Create, change, delete connections between nodes
* Undo/Redo system
* Action binding system (bind an action to a key input, context menu, etc..)
* Reactive nodes. Output nodes are updated automatically if a change occurs.
* More to come...!

[![Editor Preview](http://i.imgur.com/Xe87a3R.png)](https://www.youtube.com/watch?v=Ei93d362uYE)


Custom Node Rendering. This is a LibNoise port (WIP). Click on image to see a video preview.
[![Libnoise Port Sample](http://i.imgur.com/HyVRkHV.png)](https://twitter.com/Unit_978/status/897544106670383104)

Custom node class example

```csharp

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
            // ... do stuff...
        }
    }
    
```
The [Discord](https://discord.gg/ph2p7qC) is just an archive now.
Please visit https://github.com/Siccity/xNode as a more complete Node plugin solution.
