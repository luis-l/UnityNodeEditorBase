
using System.Collections.Generic;
using UnityEngine;

public abstract class UndoableAction : ActionBase {

    public abstract void Undo();
    public abstract void Redo();
}
