namespace Reprint;

public sealed class FactoryDiag : FactoryStepStroke
{
	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		var inkCost = 0;
		var startY = p.cursorX + p.cursorY;
		for ( var x = 0; x < p.width; x++ )
			inkCost += InkPixel(p, x, startY - x);
		return (-1, 1, inkCost);
	}
}
