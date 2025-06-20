namespace Reprint;

public sealed class FactoryRot : FactoryStep
{
	public bool clockwise = true;

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		if ( clockwise )
			p.RotateCW();
		else
			p.RotateCCW();

		return (-1, 1, 0);
	}
}
