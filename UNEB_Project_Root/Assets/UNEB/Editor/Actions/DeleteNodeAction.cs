
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    // Each input is paired with 1 output.
    using InputToOutputPair = Pair<EditorInputKnob, EditorOutputKnob>;

    // Each output can have many inputs
    using OutputToInputsPair = Pair<EditorOutputKnob, System.Collections.Generic.List<EditorInputKnob>>;

    public class DeleteNodeAction : UndoableAction
    {
        private NodeCanvas _canvas;
        private EditorNode _nodeRemoved = null;

        private List<InputToOutputPair> _oldConnectedOutputs;
        private List<OutputToInputsPair> _oldConnectedInputs;

        public DeleteNodeAction()
        {
            _oldConnectedOutputs = new List<InputToOutputPair>();
            _oldConnectedInputs = new List<OutputToInputsPair>();
        }

        public override bool Init()
        {
            return manager.window.state.selectedNode != null;
        }

        public override void Do()
        {
            _canvas = manager.window.canvas;
            _nodeRemoved = manager.window.state.selectedNode;
            _canvas.Remove(_nodeRemoved);

            // Remember all the old outputs the inputs were connected to.
            foreach (var input in _nodeRemoved.Inputs) {

                if (input.HasOutputConnected()) {
                    _oldConnectedOutputs.Add(new InputToOutputPair(input, input.OutputConnection));
                }
            }

            // Remember all the old input connections that the outputs were connected to.
            foreach (var output in _nodeRemoved.Outputs) {

                if (output.InputCount != 0) {
                    _oldConnectedInputs.Add(new OutputToInputsPair(output, output.Inputs.ToList()));
                }
            }

            disconnectOldConnections();
        }

        public override void Undo()
        {
            _canvas.nodes.Add(_nodeRemoved);
            reconnectOldConnections();
        }

        public override void Redo()
        {
            _canvas.Remove(_nodeRemoved);
            disconnectOldConnections();
        }

        private void disconnectOldConnections()
        {
            // For all the outputs for this node, remove all the connected inputs.
            foreach (var output in _nodeRemoved.Outputs) {
                output.RemoveAll();
            }

            // For all the inputs for this node, have their connected outputs disconnect.
            foreach (var input in _nodeRemoved.Inputs) {

                if (input.HasOutputConnected()) {
                    input.OutputConnection.Remove(input);
                }
            }
        }

        private void reconnectOldConnections()
        {
            // For all the remembered inputs (of this node) to output pairs, reconnect.
            foreach (InputToOutputPair inOutPair in _oldConnectedOutputs) {

                EditorInputKnob input = inOutPair.item1;
                EditorOutputKnob output = inOutPair.item2;

                output.Add(input);
            }

            // For all the remembered outputs (of this node) to inputs, reconnect.
            foreach (OutputToInputsPair outInsPair in _oldConnectedInputs) {

                EditorOutputKnob output = outInsPair.item1;
                List<EditorInputKnob> inputs = outInsPair.item2;

                foreach (var input in inputs) {
                    output.Add(input);
                }
            }
        }
    }
}