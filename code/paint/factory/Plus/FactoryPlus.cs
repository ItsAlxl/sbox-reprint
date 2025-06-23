namespace Reprint;

public sealed class FactoryPlus : FactoryStepStroke
{
	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		var inkCost = 0;
		inkCost += InkPixel( p, p.cursorX, p.cursorY );
		inkCost += InkPixel( p, p.cursorX, p.cursorY + 1 );
		inkCost += InkPixel( p, p.cursorX + 1, p.cursorY );
		inkCost += InkPixel( p, p.cursorX, p.cursorY - 1 );
		inkCost += InkPixel( p, p.cursorX - 1, p.cursorY );
		return (-1, 1, inkCost);
	}
}
