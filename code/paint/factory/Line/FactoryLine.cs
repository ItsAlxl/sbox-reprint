namespace Reprint;

public sealed class FactoryLine : FactoryStepStroke
{
	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		var inkCost = 0;
		for ( var x = 0; x < p.width; x++ )
			inkCost += InkPixel(p, x, p.cursorY);
		return (-1, 1, inkCost);
	}
}
