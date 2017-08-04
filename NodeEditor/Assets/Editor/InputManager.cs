
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class InputManager
{
    private Stack<ActionBase> _undoStack, _redoStack;
    private List<Action<Event>> _inputActions;

    public NodeEditorWindow window;

    public EditorNode selectedNode;

    private ActionBase _currentMultiStageAction = null;

    public Vector2 lastClickedPosition;

    public InputManager()
    {
        _inputActions = new List<Action<Event>>();
        _undoStack = new Stack<ActionBase>();
        _redoStack = new Stack<ActionBase>();

        _inputActions.Add(recordLastMouseClick);
        _inputActions.Add(handleZoom);
        _inputActions.Add(handlePan);
        _inputActions.Add(handleUndo);
        _inputActions.Add(handleRedo);
        _inputActions.Add(activateContextMenus);
        _inputActions.Add(handleSingleSelection);
        _inputActions.Add(handleDrag);
    }

    public void ProcessInput(Event e)
    {
        foreach (var simpleAction in _inputActions) {
            simpleAction(e);
        }
    }

    private void handleZoom(Event e)
    {
        if (e.type == EventType.ScrollWheel) {
            e.Use();
            window.editor.Zoom(e.delta.y);
        }
    }

    private void handlePan(Event e)
    {
        if (e.type == EventType.MouseDrag) {
            if (e.button == 2) {
                e.Use();
                window.editor.Pan(e.delta);
            }
        }
    }

    private void handleSingleSelection(Event e)
    {
        if (e.type == EventType.MouseDown && e.button == 0) {
            window.editor.OnMouseOverNode(onSingleSelected);
        }
    }

    private void activateContextMenus(Event e)
    {
        if (e.type == EventType.ContextClick) {

            if (window.editor.OnMouseOverNode(onSingleSelected)) {
                showNodeContextMenu();
            }

            else {
                showContextMenu();
            }

            e.Use();
        }
    }

    private void handleUndo(Event e)
    {
        bool bKeysDown = e.control && e.shift && e.keyCode == KeyCode.Z && e.type == EventType.KeyDown;

        if (bKeysDown) {
            UndoAction();
        }
    }

    private void handleRedo(Event e)
    {
        bool bKeysDown = e.control && e.shift && e.keyCode == KeyCode.Y && e.type == EventType.KeyDown;

        if (bKeysDown) {
            RedoAction();
        }
    }

    private void showContextMenu()
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Basic Node"), false, RunAction<CreateNodeAction>);
        menu.AddItem(new GUIContent("Create Add Node"), false, () => { Debug.Log("Not implemented"); });

        menu.ShowAsContext();
    }

    private void showNodeContextMenu()
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("Copy Node"), false, () => { Debug.Log("Not implemented"); });
        menu.AddItem(new GUIContent("Delete Node"), false, RunAction<DeleteNodeAction>);

        menu.ShowAsContext();
    }

    private void handleDrag(Event e)
    {
        // End of drag.
        if (e.type == EventType.MouseUp && e.button == 0 && _currentMultiStageAction != null) {
            _currentMultiStageAction.OnActionDone();
            _currentMultiStageAction = null;
        }

        // While draging.
        if (e.type == EventType.MouseDrag) {

            // Start drag
            if (selectedNode != null && _currentMultiStageAction == null && window.editor.OnMouseOverNode(onSingleSelected)) {
                _currentMultiStageAction = RunAndGetAction<DragNode>();
            }

            else if (_currentMultiStageAction != null) {

                // Update drag.
                _currentMultiStageAction.ActionUpdate();
                e.Use();
            }
        }
    }

    private void onSingleSelected(EditorNode node)
    {
        selectedNode = node;
        window.canvas.PushToEnd(node);
    }

    private void recordLastMouseClick(Event e)
    {
        if (e.type == EventType.MouseDown) {
            lastClickedPosition = window.editor.MousePosition();
        }
    }

    private void RunAction<T>() where T : ActionBase, new()
    {
        T action = new T();
        action.input = this;

        if (action.Init()) {

            _redoStack.Clear();
            _undoStack.Push(action);

            action.Do();
        }
    }

    private T RunAndGetAction<T>() where T : ActionBase, new()
    {
        T action = new T();
        action.input = this;

        if (action.Init()) {

            _redoStack.Clear();
            _undoStack.Push(action);

            action.Do();
            return action;
        }

        return null;
    }

    public void UndoAction()
    {
        if (_undoStack.Count != 0) {

            ActionBase action = _undoStack.Pop();
            _redoStack.Push(action);

            action.Undo();
            window.Repaint();
        }
    }

    public void RedoAction()
    {
        if (_redoStack.Count != 0) {

            ActionBase action = _redoStack.Pop();
            _undoStack.Push(action);

            action.Redo();
            window.Repaint();
        }
    }
}
