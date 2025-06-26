using System;

namespace Reprint;

public abstract class FactoryStep
{
	public FactoryPanel panel;
	public Workspace Workspace { get => panel?.workspace; }
	public static Painting TargetPaint { get; set; } = null;

	abstract public (int next, int timeCost, int inkCost) ApplyTo( Painting p );
	abstract public void Randomize( Random rng );

	public virtual void Placed( int atIdx ) { }
	public virtual void Removed() { }
	public virtual void ResetInternal() { }

	public virtual void OnStart() { }
	protected void ConfigUpdated()
	{
		Workspace?.UpdateResult();
	}
}

