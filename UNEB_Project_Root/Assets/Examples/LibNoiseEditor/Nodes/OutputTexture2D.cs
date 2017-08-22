
using UnityEngine;
using UnityEditor;
using UNEB;

public class OutputTexture2D : Node
{
    public override string name { get { return "Output Texture2D"; } }

    private Texture2D texPreview;

    NodeInput inputNoise;

    private int _texRes = 100;
    public int Resolution
    {
        get { return _texRes; }
        set { _texRes = Mathf.Clamp(value, 10, 300); }
    }

    public override void Init()
    {
        inputNoise = AddInput();
        inputNoise.name = "Input Noise";

        texPreview = new Texture2D(200, 200);

        FitKnobs();
        bodyRect.height += 245;
        bodyRect.width = 210f;
    }

    public override void OnBodyGUI()
    {
        EditorGUI.BeginChangeCheck();
        Resolution = EditorGUILayout.IntField("Resolution", Resolution);

        GUILayout.Box(texPreview, GUILayout.Width(texPreview.width), GUILayout.Height(texPreview.height));

        if (GUILayout.Button("Update")) {
            UpdateTexture();
        }

        if (EditorGUI.EndChangeCheck()) {
            UpdateTexture();
        }
    }

    public void UpdateTexture()
    {
        if (!inputNoise.HasOutputConnected()) {
            return;
        }

        var noise = inputNoise.OutputConnection.GetValue<LibNoise.Generator.Perlin>();

        for (int x = 0; x < texPreview.width; ++x) {
            for (int y = 0; y < texPreview.height; ++y) {

                var point = new Vector3(x, y, 0f) / _texRes;
                float value = (float)noise.GetValue(point);

                value = Mathf.Clamp01((value + 1) / 2f);
                Color color = Color.HSVToRGB(value, 1f, 1f);

                texPreview.SetPixel(x, y, color);
            }
        }

        texPreview.Apply();
    }
}
