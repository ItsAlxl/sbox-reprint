namespace Reprint;

public sealed class FactoryRot : FactoryStep
{
	private bool _clockwise = true;
	public bool Clockwise
	{
		get => _clockwise;
		set
		{
			_clockwise = value;
			ConfigUpdated();
		}
	}

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		if ( Clockwise )
			p.RotateCW();
		else
			p.RotateCCW();

		return (-1, 1, 0);
	}
}
