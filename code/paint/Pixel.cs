namespace Reprint;

public sealed class Pixel
{
	public sealed class PrimeColor
	{
		public string name;
		public Color color;
	}

	public static readonly PrimeColor[] Colors = [
		new(){name = "red", color = Color.Red},
		new(){name = "orange", color = Color.Orange},
		new(){name = "yellow", color = Color.Yellow},
		new(){name = "green", color = Color.Green},
		new(){name = "cyan", color = Color.Cyan},
		new(){name = "blue", color = Color.Blue},
		new(){name = "purple", color = Color.Magenta},
	];
	public enum ColorLookup { Red, Orange, Yellow, Green, Cyan, Blue, Purple }
	public static PrimeColor GetColor( ColorLookup lookup ) => Colors[(int)lookup];

	const int MAX_LEVEL = 4;

	private Color BaseColor { get => Colors[(int)_baseColor].color; }
	public Color Color { get => BaseColor.Desaturate( desatLevel / MAX_LEVEL ).Darken( darkenLevel / MAX_LEVEL ); }

	private ColorLookup _baseColor;
	public int desatLevel = 0;
	public int darkenLevel = 0;

	public Pixel( ColorLookup? clr = null )
	{
		if ( clr is ColorLookup cl )
		{
			_baseColor = cl;
		}
		else
		{
			_baseColor = ColorLookup.Red;
			desatLevel = MAX_LEVEL;
		}
	}

	public Pixel( Pixel p )
	{
		_baseColor = p._baseColor;
	}

	public void PaintOver( ColorLookup clr )
	{
		desatLevel = 0;
		darkenLevel = 0;
		_baseColor = clr;
	}
}
