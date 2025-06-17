namespace Reprint;

public sealed class FactoryRot : FactoryStep
{
	public bool clockwise = true;

	override public string ApplyTo( Painting p )
	{
		if ( clockwise )
			p.RotateCW();
		else
			p.RotateCCW();

		return "_cont";
	}
}
