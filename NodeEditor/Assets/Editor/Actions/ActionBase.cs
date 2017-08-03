
public abstract class ActionBase {

    public InputManager input;

    public virtual bool Init() { return true; }

    public abstract void Do();
    public abstract void Undo();
    public abstract void Redo();

    public virtual void ActionUpdate() { }
    public virtual void OnActionDone() { }
}
