namespace Reprint;

public sealed class PaintingInspector : ICanvasListener
{
	public int obvX = 0;
	public int obvY = 0;

	public void OnPxlHover( Pixel p, int x, int y )
	{
		obvX = x;
		obvY = y;
	}

	public string CreateHoverReadout( Painting p )
	{
		return p.CreateReadout( obvX, obvY );
	}
}
