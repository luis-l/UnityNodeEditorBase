
namespace UNEB
{
    public abstract class ActionBase
    {

        public ActionManager manager;

        /// <summary>
        /// Can be used to check if the action is a valid state for furthur execution.
        /// For example, we only want to run delete node if a node is selected for deletion.
        /// </summary>
        /// <returns></returns>
        public virtual bool Init() { return true; }

        public abstract void Do();
    }
}