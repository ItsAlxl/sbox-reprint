namespace Reprint;

public sealed class FactoryLine : FactoryStep
{
	public Pixel.ColorLookup color = Pixel.ColorLookup.Red;

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		var inkCost = 0;
		for ( var x = 0; x < p.width; x++ )
		{
			p.PixelAt( x, p.cursorY ).PaintOver( color );
			inkCost++;
		}
		return (-1, 1, inkCost);
	}
}
