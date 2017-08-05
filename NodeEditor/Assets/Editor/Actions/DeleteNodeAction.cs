
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeleteNodeAction : UndoableAction
{
    private NodeCanvas _canvas;
    private EditorNode _nodeRemoved = null;

    private EditorOutputKnob _outputKnobToInput;
    private List<EditorInputKnob> _inputKnobsToOutput;

    public override bool Init()
    {
        return manager.window.state.selectedNode != null;
    }

    public override void Do()
    {
        _canvas = manager.window.canvas;
        _nodeRemoved = manager.window.state.selectedNode;
        _canvas.Remove(_nodeRemoved);

        _outputKnobToInput = _nodeRemoved.input.OutputConnection;
        _inputKnobsToOutput = _nodeRemoved.output.Inputs.ToList();

        _outputKnobToInput.Remove(_nodeRemoved.input);
        _nodeRemoved.output.RemoveAll();
    }

    public override void Undo()
    {
        _canvas.nodes.Add(_nodeRemoved);
        _outputKnobToInput.Add(_nodeRemoved.input);

        foreach (EditorInputKnob input in _inputKnobsToOutput) {
            _nodeRemoved.output.Add(input);
        }
    }

    public override void Redo()
    {
        _canvas.Remove(_nodeRemoved);

        _outputKnobToInput.Remove(_nodeRemoved.input);
        _nodeRemoved.output.RemoveAll();
    }
}
