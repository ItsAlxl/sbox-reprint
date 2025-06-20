namespace Reprint;

public sealed class FactoryFlip : FactoryStep
{
	public bool vertical = true;

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		if ( vertical )
			p.FlipV();
		else
			p.FlipH();

		return (-1, 1, 0);
	}
}
