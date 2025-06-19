using System;

namespace Reprint;

public sealed class Painting
{
	const char SERIAL_DELIMITER = ':';
	public enum CursorMoveMode { Set, Clamp, Wrap, Return }

	public int cursorX = 0, cursorY = 0;
	public readonly int width, height;
	public readonly Pixel[] pixels;

	private enum TransKey { NegY = -2, NegX = -1, PosX = 1, PosY = 2 }
	private TransKey transX = TransKey.PosX;
	private TransKey transY = TransKey.PosY;

	public string CursorReadout { get => CreateReadout( cursorX, cursorY ); }

	public Painting( int w, int h )
	{
		width = w;
		height = h;
		pixels = new Pixel[w * h];
		for ( var i = 0; i < width * height; i++ )
			pixels[i] = new Pixel( Pixel.ColorLookup.Red );
	}

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
		transX = p.transX;
		transY = p.transY;
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

	private int TransformCoord( int x, int y, TransKey trans )
	{
		return trans switch
		{
			TransKey.PosX => x,
			TransKey.NegX => width - x - 1,
			TransKey.PosY => y,
			TransKey.NegY => height - y - 1,
			_ => -1,
		};
	}

	public void RotateCW()
	{
		(transX, transY) = (transY, (TransKey)(-(int)transX));
	}

	public void RotateCCW()
	{
		(transX, transY) = ((TransKey)(-(int)transY), transX);
	}

	private void Flip( TransKey posAxis, TransKey negAxis )
	{
		if ( transX == posAxis || transX == negAxis )
			transX = (TransKey)(-(int)transX);
		else
			transY = (TransKey)(-(int)transY);
	}

	public void FlipH()
	{
		Flip( TransKey.PosX, TransKey.NegX );
	}

	public void FlipV()
	{
		Flip( TransKey.PosY, TransKey.NegY );
	}

	public Pixel PixelAt( int x, int y )
	{
		return pixels[TransformCoord( x, y, transY ) * width + TransformCoord( x, y, transX )];
	}

	public void Randomize()
	{
		Random rng = new();
		for ( var x = 0; x < width; x++ )
			for ( var y = 0; y < height; y++ )
				PixelAt( x, y ).Randomize( rng );
	}

	public string CreateReadout( int x, int y )
	{
		return $"({x},{y}) = {PixelAt( x, y ).Readout}";
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
		return HashCode.Combine( Serialize(), cursorX, cursorY, transX, transY );
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
}
