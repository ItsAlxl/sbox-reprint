namespace Reprint;

public sealed class FactoryFig : FactoryStepStroke
{
	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		StrokeFig.Copy( strokeMain );
		return (-1, 0, 0);
	}
}
