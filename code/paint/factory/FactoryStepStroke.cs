using System;

namespace Reprint;

public abstract class FactoryStepStroke : FactoryStep
{
	public Pixel.ColorLookup color = Pixel.ColorLookup.Red;

	protected void StrokePaint( Pixel pxl )
	{
		pxl.PaintOver( color );
	}

	protected static int InkPixel( Painting p, int x, int y, Action<Pixel> stroke )
	{
		var pxl = p.PixelAt( x, y );
		if ( pxl is null )
			return 0;

		stroke( pxl );
		return 1;
	}
}

