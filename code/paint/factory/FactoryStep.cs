namespace Reprint;

public abstract class FactoryStep
{
	public FactoryPanel panel;
	protected Workspace Workspace { get => panel.workspace; }
	abstract public (int next, int timeCost, int inkCost) ApplyTo( Painting p );
	public virtual void Placed( int atIdx ) { }
	public virtual void Removed() { }
	public virtual void ResetInternal() { }
}

