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
	public static readonly PrimeColor NeutralColor = new() { name = "Neutral", color = Color.Gray };

	public enum ColorLookup { Red, Orange, Yellow, Green, Cyan, Blue, Purple }
	public static PrimeColor GetColor( ColorLookup lookup ) => Colors[(int)lookup];

	public const int MAX_LEVEL = 3;

	public static Color CalculateColor( ColorLookup clr, int darken, int desat )
	{
		return Colors[(int)clr].color.Desaturate( (desat == 2 ? 1.8f : (desat == 1 ? 1.2f : desat)) / (float)MAX_LEVEL ).Darken( (float)Math.Pow( darken / (float)MAX_LEVEL, 1.4 ) );
	}

	public static Color CalculateContrastColor( int darken )
	{
		return Color.White.Darken( (MAX_LEVEL - darken) / (float)MAX_LEVEL );
	}

	public Color FinalColor { get => CalculateColor( _baseColor, DarkenLevel, DesatLevel ); }
	public Color ContrastGray { get => CalculateContrastColor( DarkenLevel ); }

	private ColorLookup _baseColor;
	public ColorLookup BaseColor { get => _baseColor; }
	private int _darkenlevel = 0;
	public int DarkenLevel { get => _darkenlevel; set => _darkenlevel = value.Clamp( 0, MAX_LEVEL ); }
	private int _desatlevel = 0;
	public int DesatLevel { get => _desatlevel; set => _desatlevel = value.Clamp( 0, MAX_LEVEL ); }

	public int LightLevel { get => MAX_LEVEL - DarkenLevel; }
	public int SatLevel { get => MAX_LEVEL - DesatLevel; }

	public bool IsNeutral { get => DarkenLevel == MAX_LEVEL || DesatLevel == MAX_LEVEL; }
	public PrimeColor DisplayColor { get => IsNeutral ? NeutralColor : GetColor( _baseColor ); }
	public string Readout { get => $"C:{DisplayColor.Initial} S:{SatLevel} V:{LightLevel}"; }
	public string ReadoutVerbose { get => $"Color: {DisplayColor.name}\nSaturation: {SatLevel}\nValue: {LightLevel}"; }

	public Pixel( ColorLookup clr, int desat = MAX_LEVEL, int darken = 0 )
	{
		_baseColor = clr;
		DesatLevel = desat;
		DarkenLevel = darken;
	}

	public Pixel( Pixel p )
	{
		_baseColor = p._baseColor;
	}

	public void Reset()
	{
		_baseColor = ColorLookup.Red;
		DesatLevel = MAX_LEVEL;
		DarkenLevel = 0;
	}

	public void PaintOver( ColorLookup clr )
	{
		DesatLevel = 0;
		DarkenLevel = 0;
		_baseColor = clr;
	}

	public float ScoreAgainst( Pixel p )
	{
		if ( IsNeutral || p.IsNeutral )
		{
			var nscore = 0.0f;
			if ( p.IsNeutral == IsNeutral )
				nscore += 0.75f;
			if ( p.DarkenLevel == DarkenLevel )
				nscore += 0.25f;
			return nscore;
		}

		var score = 0.0f;
		if ( p.DisplayColor == DisplayColor )
			score += 0.5f;
		if ( p.DesatLevel == DesatLevel )
			score += 0.25f;
		if ( p.DarkenLevel == DarkenLevel )
			score += 0.25f;
		return score;
	}

	public void Randomize( Random rng )
	{
		_baseColor = (ColorLookup)rng.Next( Colors.Length );
		DesatLevel = rng.Next( MAX_LEVEL );
		DarkenLevel = rng.Next( MAX_LEVEL );
	}

	public string Serialize()
	{
		var levels = 4 * DarkenLevel + DesatLevel;
		return "" + (char)('0' + _baseColor) + (char)('a' + levels);
	}

	public void Deserialize( string serial )
	{
		_baseColor = (ColorLookup)(serial[0] - '0');

		int levels = serial[1] - 'a';
		int rawDarken = levels / 4;
		DarkenLevel = rawDarken;
		DesatLevel = levels - (4 * rawDarken);
	}
}
