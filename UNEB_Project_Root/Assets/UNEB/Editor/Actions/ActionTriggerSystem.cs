
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UNEB.Utility;
using System.Reflection;
using System.Linq;

namespace UNEB
{
    public class ActionTriggerSystem
    {
        public List<TriggerMapping> triggers;

        /// <summary>
        /// Passive triggers do not interrupt the next possible trigger.
        /// </summary>
        public List<TriggerMapping> passiveTriggers;

        private ActionManager _manager;
        private TriggerMapping _focus;

        public ActionTriggerSystem(ActionManager m)
        {
            _manager = m;

            triggers = new List<TriggerMapping>();
            passiveTriggers = new List<TriggerMapping>();

            setupStandardTriggers();
        }

        public void Update()
        {
            foreach (TriggerMapping tm in triggers) {
                if (tm.AllTriggersSatisfied()) {
                    tm.action();
                    return;
                }
            }

            foreach (TriggerMapping tm in passiveTriggers) {
                if (tm.AllTriggersSatisfied()) {
                    tm.action();
                }
            }

            // Block all key inputs from passing through the Unity Editor
            if (Event.current.isKey)
                Event.current.Use();
        }

        private void setupStandardTriggers()
        {
            setupImmediateTriggers();
            setupContextTriggers();
            setupMultiStageTriggers();
        }

        private void setupImmediateTriggers()
        {
            var panInput = Create<InputTrigger>().Mouse(EventType.MouseDrag, InputTrigger.Button.Wheel);
            panInput.action = () =>
            {
                window.editor.Pan(Event.current.delta);
                window.Repaint();
            };

            var zoomInput = Create<InputTrigger>().Key(EventType.ScrollWheel, KeyCode.None, false, false);
            zoomInput.action = () =>
            {
                window.editor.Zoom(Event.current.delta.y);
                window.Repaint();
            };

            var selectSingle = Create<InputTrigger>().Mouse(EventType.MouseDown, InputTrigger.Button.Left);
            selectSingle.action = () =>
            {
                bool bResult = window.editor.OnMouseOverNode(onSingleSelected);

                // If the canvas is clicked then remove focus of GUI elements.
                if (!bResult) {
                    GUI.FocusControl(null);
                    window.Repaint();
                }
            };

            var undoInput = Create<InputTrigger>().Key(EventType.KeyDown, KeyCode.Z, true, false);
            undoInput.action = _manager.UndoAction;

            var redoInput = Create<InputTrigger>().Key(EventType.KeyDown, KeyCode.Y, true, false);
            redoInput.action = _manager.RedoAction;

            var recordClick = Create<InputTrigger>().EventOnly(EventType.MouseDown);
            recordClick.action = () => { window.state.lastClickedPosition = window.editor.MousePosition(); };

            var homeView = Create<InputTrigger>().Key(EventType.KeyDown, KeyCode.F, false, false);
            homeView.action = window.editor.HomeView;
        }

        private void setupContextTriggers()
        {
            //Get all classes deriving from Node via reflection
            Type derivedType = typeof(Node);
            Assembly assembly = Assembly.GetAssembly(derivedType);
            List<Type> nodeTypes = assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                    ).ToList();
            //Populate canvasContext with entries for all node types
            Pair<string, Action>[] canvasContext = new Pair<string, Action>[nodeTypes.Count];
            for (int i = 0; i < nodeTypes.Count; i++) {
                Type nodeType = nodeTypes[i];
                Action createNode = () =>
                {
                    _manager.window.state.typeToCreate = nodeType;
                    _manager.RunUndoableAction<CreateNodeAction>();
                };
                //We need to create an instance to get the name.
                Node node = (Node)ScriptableObject.CreateInstance(nodeType);
                canvasContext[i] = ContextItem(node.name, createNode);
            }

            var canvasTrigger = Create<ContextTrigger>().Build(canvasContext).EventOnly(EventType.ContextClick);
            canvasTrigger.triggers.Add(isMouseOverCanvas);
            canvasTrigger.triggers.Add(isGraphValid);

            Pair<string, Action>[] nodeContext = 
            {
                ContextItem("Copy Node", () => { Debug.Log("Not Implemented"); }),
                ContextItem("Delete Node", _manager.RunUndoableAction<DeleteNodeAction>)
            };

            var nodeTrigger = Create<ContextTrigger>().Build(nodeContext).EventOnly(EventType.ContextClick);
            nodeTrigger.triggers.Add(isMouseOverNode);
            nodeTrigger.triggers.Add(isGraphValid);
        }

        private void setupMultiStageTriggers()
        {
            setupNodeDrag();
            setupNodeConnection();
        }

        private void setupNodeDrag()
        {
            var endDragInput = Create<InputTrigger>(false, true).Mouse(EventType.MouseUp, InputTrigger.Button.Left);
            var runningDragInput = Create<InputTrigger>(false, true).EventOnly(EventType.MouseDrag);
            var startDragInput = Create<InputTrigger>(false, true).Mouse(EventType.MouseDown, InputTrigger.Button.Left);

            startDragInput.triggers.Add(isMouseOverNode);
            startDragInput.action = _manager.StartMultiStageAction<DragNode>;

            endDragInput.action = _manager.FinishMultiStageAction;

            runningDragInput.action = _manager.Update;
            runningDragInput.action += window.Repaint;

            new MultiStageInputTrigger(startDragInput, endDragInput, runningDragInput);
        }

        private void setupNodeConnection()
        {
            var endConnInput = Create<InputTrigger>(false, true).Mouse(EventType.MouseUp, InputTrigger.Button.Left);
            var runningConnInput = Create<InputTrigger>(false, true).EventOnly(EventType.MouseDrag);
            var startConnInput = Create<InputTrigger>(false, true).Mouse(EventType.MouseDown, InputTrigger.Button.Left);

            Func<bool> knobCondition = () => { return isMouseOverOutput() || isMouseOverInputStartConn(); };

            startConnInput.triggers.Add(knobCondition);
            startConnInput.triggers.Add(isOutputSelected);

            startConnInput.action = _manager.StartMultiStageAction<CreateConnection>;

            endConnInput.action = _manager.FinishMultiStageAction;
            endConnInput.action += window.Repaint;

            runningConnInput.action = _manager.Update;
            runningConnInput.action += window.Repaint;

            new MultiStageInputTrigger(startConnInput, endConnInput, runningConnInput);
        }

        /// <summary>
        /// Create a trigger mapping and store it in the triggers list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Create<T>(bool isPassive = false, bool pushToFront = false) where T : TriggerMapping, new()
        {
            T mapping = new T();

            if (isPassive) {

                if (pushToFront && passiveTriggers.Count > 0) passiveTriggers.Insert(0, mapping);
                else passiveTriggers.Add(mapping);
            }

            else {

                if (pushToFront && triggers.Count > 0) triggers.Insert(0, mapping);
                else triggers.Add(mapping);
            }

            return mapping;
        }

        private void onSingleSelected(Node node)
        {
            _manager.window.state.selectedNode = node;
            _manager.window.graph.PushToEnd(node);

            Selection.activeObject = node;
        }

        private void onOutputKnobSelected(NodeOutput output)
        {
            _manager.window.state.selectedOutput = output;
        }

        private bool isMouseOverNode()
        {
            return window.editor.OnMouseOverNode(onSingleSelected);
        }

        private bool isMouseOverCanvas()
        {
            return !isMouseOverNode();
        }

        private bool isMouseOverOutput()
        {
            return window.editor.OnMouseOverOutput(onOutputKnobSelected);
        }

        private bool isMouseOverInputStartConn()
        {
            Action<NodeInput> startConnFromInput = (NodeInput input) =>
            {
                window.state.selectedOutput = input.OutputConnection;

                // Detach this input if we are starting a connection action from the input.
                if (window.state.selectedOutput != null) {

                    window.state.selectedInput = input;
                    _manager.RunUndoableAction<RemoveConnection>();
                }
            };

            return window.editor.OnMouseOverInput(startConnFromInput);
        }

        private bool isOutputSelected()
        {
            return window.state.selectedOutput != null;
        }

        private NodeEditorWindow window
        {
            get { return _manager.window; }
        }

        private bool isGraphValid()
        {
            return window.graph != null;
        }

        public Pair<string, Action> ContextItem(string label, Action a)
        {
            return new Pair<string, Action>(label, a);
        }

        /// <summary>
        /// Maps a conditional trigger with an action.
        /// </summary>
        public class TriggerMapping
        {
            public List<Func<bool>> triggers = new List<Func<bool>>();
            public Action action;

            protected TriggerMapping() { }

            public TriggerMapping(Func<bool> trigger, Action action)
            {
                ;
                triggers.Add(trigger);
                this.action = action;
            }

            public bool AllTriggersSatisfied()
            {
                foreach (var t in triggers) {
                    if (!t()) {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Special trigger that uses input as a conditional.
        /// </summary>
        public class InputTrigger : TriggerMapping
        {
            public enum Button
            {
                Left = 0,
                Right = 1,
                Wheel = 2
            }

            private EventType t;
            private KeyCode k;
            private int button;
            private bool bIsShift, bIsCtrl;

            /// <summary>
            /// Initialize the input mapping with a key trigger.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="key"></param>
            /// <param name="bShift"></param>
            /// <param name="bCtrl"></param>
            /// <returns></returns>
            public InputTrigger Key(EventType type, KeyCode key, bool bShift, bool bCtrl)
            {
                t = type;
                k = key;

                bIsShift = bShift;
                bIsCtrl = bCtrl;

                Func<bool> trigger = () =>
                {
                    var e = Event.current;

                    return
                        e.type == t &&
                        e.keyCode == k &&
                        e.shift == bIsShift &&
                        e.control == bIsCtrl;
                };

                triggers.Add(trigger);
                return this;
            }

            /// <summary>
            /// Initialize the input mapping with a mouse button tirgger.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="mButton"></param>
            /// <returns></returns>
            public InputTrigger Mouse(EventType type, Button mButton)
            {
                t = type;
                button = (int)mButton;

                Func<bool> trigger = () =>
                {
                    var e = Event.current;

                    return
                        e.type == t &&
                        e.button == button;
                };

                triggers.Add(trigger);
                return this;
            }

            /// <summary>
            /// Initializes the input mapping with the event.
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public InputTrigger EventOnly(EventType type)
            {
                t = type;
                Func<bool> trigger = () => { return Event.current.type == t; };

                triggers.Add(trigger);
                return this;
            }
        }

        /// <summary>
        /// Special trigger that uses context menus to execute other actions.
        /// </summary>
        public class ContextTrigger : InputTrigger
        {
            private GenericMenu menu;

            public ContextTrigger()
            {
                menu = new GenericMenu();
                action = menu.ShowAsContext;
            }

            public ContextTrigger Build(params Pair<string, Action>[] contents)
            {
                foreach (var content in contents) {

                    string label = content.item1;
                    Action action = content.item2;

                    menu.AddItem(new GUIContent(label), false, () => { action(); });
                }

                return this;
            }
        }

        public class MultiStageInputTrigger
        {
            private InputTrigger _startTrigger, _endTrigger, _runningTrigger;

            private bool _bStarted = false;

            public MultiStageInputTrigger(InputTrigger start, InputTrigger end, InputTrigger running)
            {
                start.action += () => { _bStarted = true; };
                start.triggers.Add(hasNotStarted);

                end.triggers.Add(HasStarted);
                end.action += () => { _bStarted = false; };

                running.triggers.Add(HasStarted);
            }

            public bool HasStarted()
            {
                return _bStarted;
            }

            private bool hasNotStarted()
            {
                return !_bStarted;
            }
        }
    }
}