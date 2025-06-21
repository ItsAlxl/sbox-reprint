using System;

namespace Reprint;

public sealed class Pixel
{
	public sealed class PrimeColor
	{
		public string name;
		public Color color;
		public char Initial { get => name[0]; }
	}

	public static readonly PrimeColor[] Colors = [
		new(){name = "Red", color = Color.Red},
		new(){name = "Orange", color = Color.Orange},
		new(){name = "Yellow", color = Color.Yellow},
		new(){name = "Green", color = Color.Green},
		new(){name = "Cyan", color = Color.Cyan},
		new(){name = "Blue", color = Color.Blue},
		new(){name = "Purple", color = Color.Magenta},
	];
	public static readonly PrimeColor DontcareColor = new() { name = "Neutral", color = Color.Gray };

	public enum ColorLookup { Red, Orange, Yellow, Green, Cyan, Blue, Purple }
	public static PrimeColor GetColor( ColorLookup lookup ) => Colors[(int)lookup];

	public const float MAX_LEVEL = 3.0f;

	public static Color CalculateColor( ColorLookup clr, int darken, int desat )
	{
		return Colors[(int)clr].color.Desaturate( (desat == 2 ? 1.8f : (desat == 1 ? 1.2f : desat)) / MAX_LEVEL ).Darken( (float)Math.Pow( (double)darken / MAX_LEVEL, 1.4 ) );
	}

	public static Color CalculateContrastColor( int darken )
	{
		return Color.White.Darken( (MAX_LEVEL - darken) / MAX_LEVEL );
	}

	public Color FinalColor { get => CalculateColor( _baseColor, darkenLevel, desatLevel ); }
	public Color ContrastGray { get => CalculateContrastColor( darkenLevel ); }

	public ColorLookup BaseColor { get => _baseColor; }
	private ColorLookup _baseColor;
	public int darkenLevel = 0;
	public int desatLevel = 0;

	public int LightLevel { get => (int)MAX_LEVEL - darkenLevel; }
	public int SatLevel { get => (int)MAX_LEVEL - desatLevel; }

	public PrimeColor DisplayColor { get => darkenLevel == (int)MAX_LEVEL || desatLevel == (int)MAX_LEVEL ? DontcareColor : GetColor( _baseColor ); }
	public string Readout { get => $"C:{DisplayColor.Initial} S:{SatLevel} V:{LightLevel}"; }
	public string ReadoutVerbose { get => $"Color: {DisplayColor.name}\nSaturation: {SatLevel}\nValue: {LightLevel}"; }

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

	public float ScoreAgainst( Pixel p )
	{
		var score = 0.0f;
		if ( p.DisplayColor == DisplayColor )
			score += 0.5f;
		if ( p.desatLevel == desatLevel )
			score += 0.25f;
		if ( p.darkenLevel == darkenLevel )
			score += 0.25f;
		return score;
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
