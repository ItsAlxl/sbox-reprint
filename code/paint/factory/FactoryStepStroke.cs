using System;

namespace Reprint;

public abstract class FactoryStepStroke : FactoryStep
{
	public enum StrokeType { Paint, Burn, Sponge };
	public StrokeType stroke = StrokeType.Paint;
	private Action<Pixel> Stroke
	{
		get => stroke switch
		{
			StrokeType.Burn => StrokeBurn,
			StrokeType.Sponge => StrokeDesat,
			_ => StrokePaint,
		};
	}

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

	protected int InkPixel( Painting p, int x, int y )
	{
		var pxl = p.PixelAt( x, y );
		if ( pxl is null )
			return 0;

		Stroke( pxl );
		return 1;
	}
}

