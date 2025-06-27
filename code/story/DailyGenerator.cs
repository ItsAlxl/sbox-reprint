using System;

namespace Reprint;

public sealed class DailyGenerator
{
	const int BRUSH_COUNT = 3;
	const int MOVE_COUNT = 3;

	public static string Generate( int year, int month, int day )
	{
		Random rng = new( int.Parse(year.ToString() + month.ToString() + day.ToString()) );
		List<FactoryStep> seq = [];

		var p = new Painting( rng.Next( 4, 10 ) );
		FactoryStep.TargetPaint = p;

		var endChance = -0.5f;
		var movChance = 0.0f;
		var gotoChance = 0.15f;
		while ( rng.NextDouble() >= endChance )
		{
			FactoryStep step;
			if ( seq.Count == 0 )
			{
				step = new FactoryDunk();
				step.Randomize( rng );
			}
			else
			{
				if ( seq.Count > 3 && rng.NextDouble() < gotoChance )
				{
					step = new FactoryIter();
					(step as FactoryIter).anchorIdxOverride = rng.Next( seq.Count );
					endChance += 0.1f;
					gotoChance -= 0.05f;
				}
				else
				{
					if ( rng.NextDouble() < movChance )
					{
						step = RandomMove( rng );
						movChance = 0.0f;
					}
					else
					{
						step = RandomStroke( rng );
						movChance += 0.4f;
					}
				}
			}
			step.Randomize( rng );
			seq.Add( step );
			endChance += 0.01f;
		}
		Workspace.ApplySequence( seq, p );
		return p.Serialize();
	}

	private static FactoryStep RandomStroke( Random rng )
	{
		return rng.Next( BRUSH_COUNT ) switch
		{
			1 => new FactoryDiag(),
			2 => new FactoryLine(),
			_ => new FactoryPlus(),
		};
	}

	private static FactoryStep RandomMove( Random rng )
	{
		return rng.Next( MOVE_COUNT ) switch
		{
			1 => new FactoryCurs(),
			2 => new FactoryFlip(),
			_ => new FactoryRot(),
		};
	}
}
