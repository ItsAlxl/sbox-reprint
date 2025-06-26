using System;

namespace Reprint;

public sealed class FactoryAnchor : FactoryStep
{
	public int id = 0;
	public string Label { get => "" + (char)('A' + (id / 26)) + (char)('A' + (id % 26)); }
	public int idx = 0;
	public FactoryStep source;
	public GameObject SourceGo { get => source.panel.GameObject; }

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		return (-1, 0, 0);
	}

	public override void Randomize( Random rng ) { }
}
