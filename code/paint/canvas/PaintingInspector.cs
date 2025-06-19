namespace Reprint;

public sealed class PaintingInspector : ICanvasListener
{
	public int obvX = 0;
	public int obvY = 0;
	private bool inside = false;

	public void OnPxlHover( Pixel p, int x, int y )
	{
		obvX = x;
		obvY = y;
	}

	public void OnMouseCross(bool ins)
	{
		inside = ins;
	}

	public string CreateHoverReadout( Painting p ) => p is not null && inside ? p.CreateReadout( obvX, obvY ) : "Hover for info";
	public string CreateHoverReadoutVerbose( Painting p ) => p is not null && inside ? p.CreateReadoutVerbose( obvX, obvY ) : "Hover over a tile for details";
}
