using Sandbox;
using System;

namespace Reprint;

public class FactoryStroke
{
	public enum Mode { Paint, Burn, Sponge };

	public event Action OnConfigChanged;

	public FactoryStroke()
	{
		Reset();
	}

	private Mode _strokeMode = Mode.Paint;
	public Mode StrokeMode
	{
		get => _strokeMode;
		set
		{
			_strokeMode = value;
			OnConfigChanged?.Invoke();
		}
	}

	private Pixel.ColorLookup _color = Pixel.ColorLookup.Red;
	public Pixel.ColorLookup Color
	{
		get => _color;
		set
		{
			_color = value;
			OnConfigChanged?.Invoke();
		}
	}

	private int _darkenAmt = 1;
	public int DarkenAmt
	{
		get => _darkenAmt;
		set
		{
			_darkenAmt = ClampLevelValue( _darkenAmt, value );
			OnConfigChanged?.Invoke();
		}
	}

	private int _desatAmt = 1;
	public int DesatAmt
	{
		get => _desatAmt;
		set
		{
			_desatAmt = ClampLevelValue( _desatAmt, value );
			OnConfigChanged?.Invoke();
		}
	}

	private Action<Pixel> StrokeCb
	{
		get => StrokeMode switch
		{
			Mode.Burn => StrokeBurn,
			Mode.Sponge => StrokeDesat,
			_ => StrokePaint,
		};
	}

	private static int ClampLevelValue( int from, int to )
	{
		return to == 0 ? (from > 0 ? -1 : 1) : to.Clamp( -Pixel.MAX_LEVEL, Pixel.MAX_LEVEL );
	}

	private void StrokePaint( Pixel pxl )
	{
		pxl.PaintOver( Color );
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
		StrokeMode = other.StrokeMode;
		Color = other.Color;
		DarkenAmt = other.DarkenAmt;
		DesatAmt = other.DesatAmt;
	}

	public void Reset()
	{
		StrokeMode = Mode.Paint;
		Color = Pixel.ColorLookup.Red;
		DarkenAmt = 1;
		DesatAmt = 1;
	}
}
