
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    public abstract class UndoableAction : ActionBase
    {
        public abstract void Undo();
        public abstract void Redo();
    }
}