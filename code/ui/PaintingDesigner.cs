namespace Reprint;

public sealed class PaintingDesigner : ICanvasListener
{
	public Painting paint = new( 4, 4 );
	public Pixel.ColorLookup brushColor = Pixel.ColorLookup.Red;
	public int brushDarken = 0;
	public int brushDesat = 0;
	public Color CalculatedColor => Pixel.CalculateColor( brushColor, brushDarken, brushDesat );

	private bool mousedown = false;
	public bool IsMouseDown => mousedown;

	public void Resize( int w, int h )
	{
		if ( paint.width != w || paint.height != h )
			paint = new( w, h );
	}

	public void OnPxlHover( Pixel p, int x, int y )
	{
		if ( mousedown )
			OnPxlLClick( p, x, y );
	}

	public void OnPxlLClick( Pixel p, int x, int y )
	{
		mousedown = true;

		// due to shenanigans, `p` may be from a different Painting object
		var pxl = paint.PixelAt( x, y );
		pxl.PaintOver( brushColor );
		pxl.darkenLevel = brushDarken;
		pxl.desatLevel = brushDesat;
	}

	public void OnPxlRClick( Pixel p, int x, int y )
	{
		brushColor = p.BaseColor;
		brushDarken = p.darkenLevel;
		brushDesat = p.desatLevel;
	}

	public string PaintSource()
	{
		return paint.Serialize();
	}

	public void ReleaseMouse()
	{
		mousedown = false;
	}
}
