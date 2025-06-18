namespace Reprint;

public sealed class PaintingDesigner : ICanvasListener
{
	public Painting paint = new( 4, 4 );
	public Pixel.ColorLookup brushColor = Pixel.ColorLookup.Red;
	public int brushDarken = 0;
	public int brushDesat = 0;

	public void Resize( int w, int h )
	{
		paint = new( w, h );
	}

	public void OnPxlLClick( Pixel p, int x, int y )
	{
		p.PaintOver( brushColor );
		p.darkenLevel = brushDarken;
		p.desatLevel = brushDesat;
	}

	public void OnPxlRClick( Pixel p, int x, int y )
	{
		brushColor = p.GetColorLookup();
		brushDarken = p.darkenLevel;
		brushDesat = p.desatLevel;
	}

	public Color CalculateColor()
	{
		return Pixel.CalculateColor(brushColor, brushDarken, brushDesat);
	}
}
