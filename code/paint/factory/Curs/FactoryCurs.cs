using System;

namespace Reprint;

public sealed class FactoryCurs : FactoryStep
{
	private int LimitX { get => Workspace.targetPaint.width - 1; }
	public int AmtX
	{
		get => _amtX;
		set
		{
			_amtX = value.Clamp( _setX ? 0 : -LimitX, LimitX );
			ConfigUpdated();
		}
	}
	public bool SetX
	{
		get => _setX;
		set
		{
			_setX = value;
			_amtX = _setX ? Math.Abs( _amtX ) : _amtX;
			ConfigUpdated();
		}
	}
	private bool _setX = false;
	private int _amtX = 0;

	private int LimitY { get => Workspace.targetPaint.height - 1; }
	public int AmtY
	{
		get => _amtY;
		set
		{
			_amtY = value.Clamp( _setY ? 0 : -LimitY, LimitY );
			ConfigUpdated();
		}
	}
	public bool SetY
	{
		get => _setY;
		set
		{
			_setY = value;
			_amtY = _setY ? Math.Abs( _amtY ) : _amtY;
			ConfigUpdated();
		}
	}
	private bool _setY = false;
	private int _amtY = 0;

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		if ( SetX || AmtX != 0 )
			p.MoveCursorX( AmtX, SetX ? Painting.CursorMoveMode.Set : Painting.CursorMoveMode.Wrap );
		if ( SetY || AmtY != 0 )
			p.MoveCursorY( AmtY, SetY ? Painting.CursorMoveMode.Set : Painting.CursorMoveMode.Wrap );

		return (-1, 1, 0);
	}
}
