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

	public string CreateHoverReadout( Painting p ) =>  p.CreateReadout( obvX, obvY );
	public string CreateHoverReadoutVerbose( Painting p ) =>  p.CreateReadoutVerbose( obvX, obvY );
	
}
