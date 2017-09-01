
using UnityEngine;

namespace UNEB
{
    public class RemoveConnection : UndoableAction
    {

        private NodeOutput _output;
        private NodeInput _input;

        public override void Do()
        {
            _input = manager.window.state.selectedInput;
            _output = _input.Outputs[0];

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