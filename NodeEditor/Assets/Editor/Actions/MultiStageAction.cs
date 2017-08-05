
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiStageAction : UndoableAction {

    public abstract void OnActionStart();
    public abstract void OnActionEnd();

    /// <summary>
    /// Can be used to guard against actions that did not complete properly
    /// and should be recorded for undo.
    /// For example, when the user cancels creating a connection between nodes, this
    /// would return false.
    /// </summary>
    /// <returns></returns>
    public virtual bool ValidateActionEnd() { return true; }
}
