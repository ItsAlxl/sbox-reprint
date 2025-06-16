using System;

namespace Reprint;

public sealed class Painting
{
	public enum CursorMoveMode { Set, Clamp, Wrap, Return }

	public int cursorX, cursorY;
	public readonly int width, height;
	public readonly Pixel[] pixels;

	public Painting( int w, int h )
	{
		width = w;
		height = h;
		pixels = new Pixel[h * w];
		for ( var i = 0; i < width * height; i++ )
		{
			pixels[i] = new Pixel();
		}
	}

	public Painting( Painting p )
	{
		width = p.width;
		height = p.height;
		pixels = new Pixel[width * height];
		for ( var i = 0; i < width * height; i++ )
			pixels[i] = new Pixel( p.pixels[i] );
	}

	private void MoveCursor( bool isX, int amt, CursorMoveMode mode )
	{
		var curs = isX ? cursorX : cursorY;
		var limit = isX ? width : height;
		if ( mode == CursorMoveMode.Set )
		{
			curs = Math.Clamp( amt, 0, limit - 1 );
		}
		else
		{
			curs += amt;
			switch ( mode )
			{
				case CursorMoveMode.Clamp:
					curs = Math.Clamp( curs, 0, limit - 1 );
					break;
				case CursorMoveMode.Wrap:
					curs %= limit;
					curs = curs < 0 ? curs + limit : curs;
					break;
				case CursorMoveMode.Return:
					if ( curs < 0 || curs >= limit )
					{
						MoveCursor( !isX, curs < 0 ? -1 : 1, CursorMoveMode.Wrap );
						curs = 0;
					}
					break;
			}
		}

		if ( isX )
			cursorX = curs;
		else
			cursorY = curs;
	}

	public void MoveCursorX( int x, CursorMoveMode mode )
	{
		MoveCursor( true, x, mode );
	}

	public void MoveCursorY( int y, CursorMoveMode mode )
	{
		MoveCursor( false, y, mode );
	}

	public Pixel PixelAt( int x, int y )
	{
		return pixels[y * width + x];
	}

	public void Randomize()
	{
		Random rng = new();
		for ( var x = 0; x < width; x++ )
			for ( var y = 0; y < height; y++ )
				PixelAt( x, y ).Randomize(rng);
	}
}
