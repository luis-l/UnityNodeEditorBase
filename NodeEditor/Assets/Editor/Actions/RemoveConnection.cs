
using UnityEngine;

namespace UNEB
{
    public class RemoveConnection : UndoableAction
    {

        private EditorOutputKnob _output;
        private EditorInputKnob _input;

        public override void Do()
        {
            _input = manager.window.state.selectedInput;
            _output = _input.OutputConnection;

            _output.Remove(_input);

            manager.window.state.selectedInput = null;
        }

        public override void Undo()
        {
            _output.Add(_input);
        }

        public override void Redo()
        {
            _output.Remove(_input);
        }
    }
}