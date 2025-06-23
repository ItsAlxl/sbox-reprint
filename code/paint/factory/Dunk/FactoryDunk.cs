namespace Reprint;

public sealed class FactoryDunk : FactoryStepStroke
{
	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		var inkCost = 0;
		for (var x = 0; x < p.width; x++)
			for (var y = 0; y < p.height; y++)
				inkCost += InkPixel( p, x, y );
		return (-1, 1, inkCost);
	}
}
