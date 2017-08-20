
using System;
using System.Collections.Generic;
using UnityEngine;
using UNEB.Utility;

namespace UNEB
{
    /// <summary>
    /// Handles execution of actions, undo, and redo.
    /// </summary>
    public class ActionManager
    {
        private NodeEditorWindow _window;

        private FiniteStack<UndoableAction> _undoStack;
        private Stack<UndoableAction> _redoStack;

        // Caches the current multi-stage action that is currently executing.
        private MultiStageAction _activeMultiAction = null;

        public event Action OnUndo;
        public event Action OnRedo;

        public ActionManager(NodeEditorWindow w)
        {
            _undoStack = new FiniteStack<UndoableAction>(100);
            _redoStack = new Stack<UndoableAction>();

            _window = w;

            // Makes sure that the action cleans up after itself
            // when it is removed from the undo buffer.
            _undoStack.OnRemoveBottomItem += (action) =>
            {
                action.OnDestroy();
            };
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

                clearRedoStack();
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

                clearRedoStack();
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

        public void Reset()
        {
            _activeMultiAction = null;
            clearUndoStack();
            clearRedoStack();
        }

        private void clearRedoStack()
        {
            foreach (var action in _redoStack) {
                action.OnDestroy();
            }

            _redoStack.Clear();
        }

        private void clearUndoStack()
        {
            foreach (var action in _undoStack) {
                action.OnDestroy();
            }

            _undoStack.Clear();
        }

        public NodeEditorWindow window
        {
            get { return _window; }
        }
    }
}