
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    /// <summary>
    /// Handles execution of actions, undo, and redo.
    /// </summary>
    public class ActionManager
    {
        private NodeEditorWindow _window;

        private Stack<UndoableAction> _undoStack, _redoStack;
        private List<Action<Event>> _inputActions;

        // Caches the current multi-stage action that is currently executing.
        private MultiStageAction _activeMultiAction = null;

        public event Action OnUndo;
        public event Action OnRedo;

        public ActionManager(NodeEditorWindow w)
        {
            _undoStack = new Stack<UndoableAction>();
            _redoStack = new Stack<UndoableAction>();

            _window = w;
        }

        public void Update()
        {
            if (IsRunningAction) {
                _activeMultiAction.Do();
            }
        }

        public bool IsRunningAction
        {
            get { return _activeMultiAction != null; }
        }

        /// <summary>
        /// Runs an action and stores it in the undo stack.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RunUndoableAction<T>() where T : UndoableAction, new()
        {
            T action = new T();
            action.manager = this;

            if (action.Init()) {

                _redoStack.Clear();
                _undoStack.Push(action);
                action.Do();
            }
        }

        /// <summary>
        /// Starts a multi stage action but does not record it in the undo stack.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void StartMultiStageAction<T>() where T : MultiStageAction, new()
        {
            // Only run 1 multi-action at a time.
            if (_activeMultiAction != null) {
                return;
            }

            T action = new T();
            action.manager = this;

            if (action.Init()) {
                _activeMultiAction = action;
                _activeMultiAction.OnActionStart();
            }
        }

        /// <summary>
        /// Records the multi-stage action in the undo stack.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void FinishMultiStageAction()
        {
            if (_activeMultiAction == null) {
                return;
            }

            // We check if the action ended properly so it can be stored in undo.
            if (_activeMultiAction.OnActionEnd()) {

                _redoStack.Clear();
                _undoStack.Push(_activeMultiAction);
            }

            // There is no longer an active multi-stage action.
            _activeMultiAction = null;
        }

        public void UndoAction()
        {
            if (_undoStack.Count != 0) {

                var action = _undoStack.Pop();
                _redoStack.Push(action);

                action.Undo();

                if (OnUndo != null)
                    OnUndo();
            }
        }

        public void RedoAction()
        {
            if (_redoStack.Count != 0) {

                var action = _redoStack.Pop();
                _undoStack.Push(action);

                action.Redo();

                if (OnRedo != null)
                    OnRedo();
            }
        }

        public NodeEditorWindow window
        {
            get { return _window; }
        }
    }
}