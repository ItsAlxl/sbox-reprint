using System;

namespace Reprint;

public sealed class FactoryFlip : FactoryStep
{
	private bool _vertical = true;
	public bool Vertical
	{
		get => _vertical;
		set
		{
			_vertical = value;
			ConfigUpdated();
		}
	}

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		if ( Vertical )
			p.FlipV();
		else
			p.FlipH();

		return (-1, 1, 0);
	}

	public override void Randomize( Random rng )
	{
		Vertical = rng.NextDouble() < 0.5;
	}
}
