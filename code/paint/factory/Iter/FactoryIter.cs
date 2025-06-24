using System;

namespace Reprint;

public sealed class FactoryIter : FactoryStep
{
	public bool resetOnContinue = true;
	public int _maxCount = 1;
	private int _currentCount = 1;
	public int Counter { get => _currentCount; }
	public int MaxCount
	{
		get => _maxCount;
		set
		{
			var updCurrent = _currentCount == _maxCount;
			_maxCount = value.Clamp( 1, 16 );
			if ( updCurrent )
				_currentCount = _maxCount;
		}
	}

	private FactoryPanel anchorPanel = null;
	private FactoryAnchor Anchor { get => anchorPanel?.factory as FactoryAnchor; }
	public int AnchorIdx { get => Anchor?.idx ?? -1; }
	public string AnchorLabel { get => Anchor?.Label ?? "__"; }
	public GameObject AnchorGo { get => anchorPanel?.GameObject; }

	override public (int next, int timeCost, int inkCost) ApplyTo( Painting p )
	{
		var next = Counter > 0 ? AnchorIdx + 1 : -1;
		_currentCount--;
		if ( next == -1 && resetOnContinue )
			ResetInternal();

		return (next, 0, 0);
	}

	public override void ResetInternal()
	{
		_currentCount = _maxCount;
	}

	public override void Placed( int atIdx )
	{
		anchorPanel ??= Workspace.CreateAnchor( atIdx );
		if ( anchorPanel is not null )
			Anchor.source = this;
	}

	public override void Removed()
	{
		if ( anchorPanel is not null )
			Workspace.RemoveFromSquence( anchorPanel );
	}
}
