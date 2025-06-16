namespace Reprint;

public sealed class FactoryLine : FactoryStep
{
	public Pixel.ColorLookup color = Pixel.ColorLookup.Red;

	override public string ApplyTo( Painting p )
	{
		for ( var x = 0; x < p.width; x++ )
			p.PixelAt( x, p.cursorY ).PaintOver( color );
		return "_cont";
	}
}
