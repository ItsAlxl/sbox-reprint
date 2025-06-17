using System;

namespace Reprint;

public sealed class FactoryCurs : FactoryStep
{
	public bool setX = false;
	public int amtX = 0;
	public bool setY = false;
	public int amtY = 0;

	override public string ApplyTo( Painting p )
	{
		if ( setX || amtX != 0 )
			p.MoveCursorX( setX ? Math.Abs(amtX) : amtX, setX ? Painting.CursorMoveMode.Set : Painting.CursorMoveMode.Wrap );
		if ( setY || amtY != 0 )
			p.MoveCursorY( setY ? Math.Abs(amtY) : amtY, setY ? Painting.CursorMoveMode.Set : Painting.CursorMoveMode.Wrap );

		return "_cont";
	}
}
