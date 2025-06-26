using System;

namespace Reprint;

public sealed class Painting
{
	const char SERIAL_DELIMITER = ':';
	public enum CursorMoveMode { Set, Clamp, Wrap, Return }

	public int cursorX = 0, cursorY = 0;
	public readonly int width, height;
	public readonly Pixel[] pixels;

	private readonly int[] transform2D = [1, 0, 0, 1];

	public string CursorReadout { get => CreateReadout( cursorX, cursorY ); }

	public Painting( int w, int h )
	{
		width = w;
		height = h;
		pixels = new Pixel[w * h];
		for ( var i = 0; i < width * height; i++ )
			pixels[i] = new Pixel( Pixel.ColorLookup.Red );
	}
	public Painting( int s ) : this( s, s ) { }

	public Painting( Painting p )
	{
		width = p.width;
		height = p.height;
		pixels = new Pixel[width * height];
		Copy( p );
	}

	public Painting( string serial )
	{
		var comps = serial.Split( SERIAL_DELIMITER );
		width = int.Parse( comps[0] );
		height = int.Parse( comps[1] );
		pixels = new Pixel[width * height];
		for ( var i = 0; i < width * height; i++ )
			pixels[i] = new Pixel( Pixel.ColorLookup.Red );
		DeserializePixels( comps[2] );
	}

	public void Reset()
	{
		cursorX = 0;
		cursorY = 0;
		for ( var i = 0; i < width * height; i++ )
			pixels[i].Reset();
	}

	public void Copy( Painting p )
	{
		cursorX = p.cursorX;
		cursorY = p.cursorY;
		for ( var i = 0; i < transform2D.Length; i++ )
			transform2D[i] = p.transform2D[i];
		for ( var i = 0; i < width * height; i++ )
			pixels[i] = new Pixel( p.pixels[i] );
	}

	private void MoveCursor( bool isX, int amt, CursorMoveMode mode )
	{
		var curs = isX ? cursorX : cursorY;
		var limit = isX ? width : height;
		if ( mode == CursorMoveMode.Set )
		{
			curs = amt.Clamp( 0, limit - 1 );
		}
		else
		{
			curs += amt;
			switch ( mode )
			{
				case CursorMoveMode.Clamp:
					curs = curs.Clamp( 0, limit - 1 );
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

	private static int[] MatrixMultiply2x2( int[] a, int[] b )
	{
		var ans = new int[b.Length];
		for ( var i = 0; i <= 2; i += 2 )
			for ( var j = 0; j < 2; j++ )
				ans[i + j] = a[i] * b[j] + a[i + 1] * b[j + 2];
		return ans;
	}

	private static int[] MatrixMultiply2x1( int[] a, int[] b )
	{
		var ans = new int[b.Length];
		for ( var i = 0; i < 2; i++ )
			ans[i] = a[2 * i] * b[0] + a[2 * i + 1] * b[1];
		return ans;
	}

	private static int[] MatrixMultiply( int[] a, int[] b ) => b.Length == 2 ? MatrixMultiply2x1( a, b ) : MatrixMultiply2x2( a, b );

	private void SetTransform2D( int[] matrix )
	{
		for ( var i = 0; i < transform2D.Length; i++ )
			transform2D[i] = matrix[i];
	}

	public void RotateCW()
	{
		SetTransform2D( MatrixMultiply( transform2D, [0, 1, -1, 0] ) );
	}

	public void RotateCCW()
	{
		SetTransform2D( MatrixMultiply( transform2D, [0, -1, 1, 0] ) );
	}

	public void FlipH()
	{
		transform2D[0] = -transform2D[0];
		transform2D[2] = -transform2D[2];
	}

	public void FlipV()
	{
		transform2D[1] = -transform2D[1];
		transform2D[3] = -transform2D[3];
	}

	private int[] TransformCoords( int x, int y )
	{
		var t = MatrixMultiply( transform2D, [x + 1, y + 1] );
		t[0] = t[0] < 0 ? t[0] + width : (t[0] - 1);
		t[1] = t[1] < 0 ? t[1] + height : (t[1] - 1);
		return t;
	}

	public Pixel PixelAt( int x, int y )
	{
		var coords = TransformCoords( x, y );
		return (x >= 0 && x < width && y >= 0 && y < height) ? pixels[coords[1] * width + coords[0]] : null;
	}

	public void Randomize()
	{
		Random rng = new();
		for ( var x = 0; x < width; x++ )
			for ( var y = 0; y < height; y++ )
				PixelAt( x, y ).Randomize( rng );
	}

	public string CreateReadout( int x, int y ) => $"({x},{y}) = {PixelAt( x, y ).Readout}";
	public string CreateReadoutVerbose( int x, int y ) => $"({x}, {y})\n{PixelAt( x, y ).ReadoutVerbose}";

	public float ScoreAgainst( Painting p )
	{
		var scores = new float[8];
		var half = scores.Length / 2;
		for ( var x = 0; x < width; x++ )
		{
			for ( var y = 0; y < height; y++ )
			{
				var negX = width - x - 1;
				var negY = width - y - 1;
				var testPxl = p.PixelAt( x, y );
				for ( var i = 0; i < half; i++ )
				{
					var stepX = i / 2 == 0 ? x : negX;
					var stepY = i % 2 == 0 ? y : negY;
					scores[i] += PixelAt( stepX, stepY ).ScoreAgainst( testPxl );
					scores[i + half] += PixelAt( stepY, stepX ).ScoreAgainst( testPxl );
				}
			}
		}
		return scores.Max() / pixels.Length;
	}

	public string Serialize()
	{
		string serial = "" + width + SERIAL_DELIMITER + height + SERIAL_DELIMITER;
		for ( var i = 0; i < pixels.Length; i++ )
			serial += pixels[i].Serialize();
		return serial;
	}

	public int BuildHash()
	{
		return HashCode.Combine( Serialize(), cursorX, cursorY, transform2D[0], transform2D[1], transform2D[2], transform2D[3] );
	}

	private void DeserializePixels( string pxSerial )
	{
		for ( var i = 0; i < pxSerial.Length; i += 2 )
		{
			pixels[i / 2].Deserialize( pxSerial.Substring( i, 2 ) );
		}
	}

	public void Deserialize( string serial )
	{
		var comps = serial.Split( SERIAL_DELIMITER );
		if ( int.Parse( comps[0] ) == width && int.Parse( comps[1] ) == height )
		{
			DeserializePixels( comps[2] );
		}
	}

	public static bool ValidSerial( string serial )
	{
		var comps = serial.Split( SERIAL_DELIMITER );
		if ( comps.Length == 3 &&
			int.Parse( comps[0] ).ToString() == comps[0] &&
			int.Parse( comps[1] ).ToString() == comps[1] &&
			comps[0] == comps[1] && comps[2].Length % 2 == 0 )
		{
			var pxSerial = comps[2];
			for ( var i = 0; i < pxSerial.Length; i += 2 )
			{
				if ( !Pixel.ValidSerial( pxSerial.Substring( i, 2 ) ) )
					return false;
			}
			return true;
		}
		return false;
	}
}
