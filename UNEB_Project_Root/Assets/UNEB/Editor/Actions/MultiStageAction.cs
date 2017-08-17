
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    public abstract class MultiStageAction : UndoableAction
    {

        public abstract void OnActionStart();

        /// <summary>
        /// Returns true if the action completed succesfully.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnActionEnd();
    }
}