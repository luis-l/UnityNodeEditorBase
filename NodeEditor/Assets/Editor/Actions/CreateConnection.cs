
using UnityEngine;
using UnityEditor;

public class CreateConnection : MultiStageAction {

    private EditorInputKnob _input;
    private EditorOutputKnob _output;

    public override void Do()
    {
        manager.window.state.selectedOutput = _output;
    }

    public override void Undo()
    {
        _output.Remove(_input);
    }

    public override void Redo()
    {
        _output.Add(_input);
    }

    public override void OnActionStart()
    {
        _output = manager.window.state.selectedOutput;
    }

    public override void OnActionEnd()
    {
        manager.window.state.selectedOutput = null;
        manager.window.editor.OnMouseOverNode_OrInput((node) => { _input = node.input; });
    }

    public override bool ValidateActionEnd()
    {
        // Make the connection.
        if (_input != null) {
            _output.Add(_input);
            return true;
        }

        return false;
    }
}
