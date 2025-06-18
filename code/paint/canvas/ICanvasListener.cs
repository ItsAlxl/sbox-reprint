namespace Reprint;

public interface ICanvasListener
{
	public void OnPxlHover( Pixel p, int x, int y ) { }
	public void OnPxlLClick( Pixel p, int x, int y ) { }
	public void OnPxlRClick( Pixel p, int x, int y ) { }
}
