using System;

namespace Reprint;

public class FactoryStroke
{
	public enum Mode { Paint, Burn, Sponge };
	public Mode mode = Mode.Paint;
	public Pixel.ColorLookup color = Pixel.ColorLookup.Red;

	private int _darkenAmt = 1;
	public int DarkenAmt
	{
		get => _darkenAmt;
		set
		{
			_darkenAmt = ClampLevelValue( _darkenAmt, value );
		}
	}

	private int _desatAmt = 1;
	public int DesatAmt
	{
		get => _desatAmt;
		set
		{
			_desatAmt = ClampLevelValue( _desatAmt, value );
		}
	}

	private Action<Pixel> StrokeCb
	{
		get => mode switch
		{
			Mode.Burn => StrokeBurn,
			Mode.Sponge => StrokeDesat,
			_ => StrokePaint,
		};
	}

	public FactoryStroke()
	{
		Reset();
	}

	private static int ClampLevelValue( int from, int to )
	{
		return to == 0 ? (from > 0 ? -1 : 1) : to.Clamp( -Pixel.MAX_LEVEL, Pixel.MAX_LEVEL );
	}

	private void StrokePaint( Pixel pxl )
	{
		pxl.PaintOver( color );
	}

	private void StrokeBurn( Pixel pxl )
	{
		pxl.DarkenLevel += DarkenAmt;
	}

	private void StrokeDesat( Pixel pxl )
	{
		pxl.DesatLevel += DesatAmt;
	}

	public void InkPixel( Pixel pxl )
	{
		StrokeCb( pxl );
	}

	public void Copy( FactoryStroke other )
	{
		mode = other.mode;
		color = other.color;
		DarkenAmt = other.DarkenAmt;
		DesatAmt = other.DesatAmt;
	}

	public void Reset()
	{
		mode = Mode.Paint;
		color = Pixel.ColorLookup.Red;
		DarkenAmt = 1;
		DesatAmt = 1;
	}
}
