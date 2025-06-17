namespace Reprint;

public sealed class FactoryFlip : FactoryStep
{
	public bool vertical = true;

	override public string ApplyTo( Painting p )
	{
		if ( vertical )
			p.FlipV();
		else
			p.FlipH();

		return "_cont";
	}
}
