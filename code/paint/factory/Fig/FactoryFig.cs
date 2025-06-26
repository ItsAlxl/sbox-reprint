namespace Reprint;

public sealed class FactoryFig : FactoryStep
{
	private FactoryStroke.Mode _strokeMode = FactoryStroke.Mode.Paint;
	public FactoryStroke.Mode StrokeMode
	{
		get => _strokeMode;
		set
		{
			if ( value != _strokeMode )
			{
				if ( !AddMode && (value == FactoryStroke.Mode.Paint || _strokeMode == FactoryStroke.Mode.Paint) )
				{
					if ( value == FactoryStroke.Mode.Paint )
						RaisePaintValue();
					else
						LowerPaintValue();
				}
				_strokeMode = value;
				ConfigUpdated();
			}
		}
	}

	public bool _addMode = false;
	public bool AddMode
	{
		get => _addMode;
		set
		{
			if ( value != _addMode )
			{
				if ( StrokeMode == FactoryStroke.Mode.Paint )
				{
					if ( value )
						LowerPaintValue();
					else
						RaisePaintValue();
				}
				_addMode = value;
				ConfigUpdated();
			}
		}
	}

	public int _modif = 0;
	public int Modif
	{
		get => _modif;
		set
		{
			_modif = value;
			ConfigUpdated();
		}
	}

	private void RaisePaintValue()
	{
		_modif += 3;
	}

	private void LowerPaintValue()
	{
		_modif -= 3;
		if ( _modif == 0 )
			_modif = -1;
	}

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		Workspace.figStroke.Modify( StrokeMode, AddMode, Modif );
		return (-1, 0, 0);
	}
}
