public abstract class UnitAction
{
    public abstract string ActionName { get; }
    public abstract bool CanExecute(Unit unit);
    public abstract void Execute(Unit unit);
}