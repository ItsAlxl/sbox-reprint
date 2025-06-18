using System;

namespace Reprint;

public sealed class Pixel
{
	public sealed class PrimeColor
	{
		public string name;
		public Color color;
		public char Initial { get => char.ToUpper( name[0] ); }
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

	public const float MAX_LEVEL = 3.0f;

	public static Color CalculateColor( ColorLookup clr, int darken, int desat )
	{
		return Colors[(int)clr].color.Desaturate( desat / MAX_LEVEL ).Darken( darken / MAX_LEVEL );
	}

	public static Color CalculateContrastColor( int darken )
	{
		return Color.White.Darken( (MAX_LEVEL - darken) / MAX_LEVEL );
	}

	public Color FinalColor { get => CalculateColor( _baseColor, darkenLevel, desatLevel ); }
	public Color ContrastGray { get => CalculateContrastColor( darkenLevel ); }

	private ColorLookup _baseColor;
	public int darkenLevel = 0;
	public int desatLevel = 0;

	public int LightLevel { get => (int)MAX_LEVEL - darkenLevel; }
	public int SatLevel { get => (int)MAX_LEVEL - desatLevel; }

	public string Readout { get => $"C:{GetColor( _baseColor ).Initial} S:{SatLevel} L:{LightLevel}"; }

	public Pixel( ColorLookup clr, int desat = (int)MAX_LEVEL, int darken = 0 )
	{
		_baseColor = clr;
		desatLevel = desat;
		darkenLevel = darken;
	}

	public Pixel( Pixel p )
	{
		_baseColor = p._baseColor;
	}

	public void Reset()
	{
		_baseColor = ColorLookup.Red;
		desatLevel = (int)MAX_LEVEL;
		darkenLevel = 0;
	}

	public void PaintOver( ColorLookup clr )
	{
		desatLevel = 0;
		darkenLevel = 0;
		_baseColor = clr;
	}

	public ColorLookup GetColorLookup()
	{
		return _baseColor;
	}

	public void Randomize( Random rng )
	{
		_baseColor = (ColorLookup)rng.Next( Colors.Length );
		desatLevel = rng.Next( (int)MAX_LEVEL );
		darkenLevel = rng.Next( (int)MAX_LEVEL );
	}

	public string Serialize()
	{
		var levels = 4 * darkenLevel + desatLevel;
		return "" + (char)('0' + _baseColor) + (char)('a' + levels);
	}

	public void Deserialize( string serial )
	{
		_baseColor = (ColorLookup)(serial[0] - '0');

		int levels = serial[1] - 'a';
		darkenLevel = levels / 4;
		desatLevel = levels - (4 * darkenLevel);
	}
}
