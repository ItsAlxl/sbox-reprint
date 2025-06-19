namespace Reprint;

[Group( "Reprint" )]
[Title( "Workspace" )]
[Icon( "home_repair_service" )]

public sealed class Workspace : Component
{
	const float CHILD_SPACING = 30.0f / 512.0f;
	const float SCRATCH_SPACING = CHILD_SPACING * 128.0f;

	public static float LeftBound { get => 0.0f; }
	public float RightBound { get => _rightBound; }
	private float _rightBound = 0.0f;

	public Painting scratchPaint;
	private GameObject scratchGo;
	private int ScratchIdx { get => sequence.FindIndex( ( go ) => go == scratchGo ); }
	private Vector3 ScratchLeft { get => scratchGo.WorldPosition - new Vector3( 0, SCRATCH_SPACING, 0 ); }
	private Vector3 ScratchRight { get => scratchGo.WorldPosition + new Vector3( 0, SCRATCH_SPACING, 0 ); }

	private TimeSince addStart;
	private int dragIdx = -1;
	private bool showFirstDragGap = false;
	private Vector3 dragOffset = Vector3.Zero;
	private GameObject dragGo = null;
	public bool Dragging { get => dragGo is not null; }

	private CameraController camCont;
	public Scenario currentScene;
	public Painting targetPaint;
	private readonly List<GameObject> sequence = [];

	protected override void OnStart()
	{
		camCont = Scene.Get<CameraController>();
		ResetLevel();
	}

	public void ResetLevel()
	{
		if ( sequence.Count > 0 )
		{
			for ( var i = 0; i < sequence.Count; i++ )
				sequence[i].Destroy();
			sequence.RemoveRange( 0, sequence.Count );
		}

		camCont.ResetPosition();
		scratchGo = null;
		targetPaint = null;
		scratchPaint = null;
	}

	public void BeginScenario( ScenarioData scene )
	{
		currentScene = new( scene );
		targetPaint = new( currentScene.paint );
		scratchPaint = new( targetPaint.width, targetPaint.height );
		scratchGo = GameObject.GetPrefab( "prefabs/scratchpad.prefab" ).Clone();
		sequence.Add( scratchGo );
		AdjustSequenceLayout();
	}

	private void AdjustSequenceLayout()
	{
		_rightBound = 0;
		for ( var i = 0; i < sequence.Count; i++ )
		{
			var go = sequence[i];
			var size = go.GetComponent<WorldPanel>().PanelSize.x * CHILD_SPACING;
			if ( i == dragIdx && (showFirstDragGap || i > 0) )
				_rightBound += 128 * CHILD_SPACING;
			go.LocalPosition = Vector3.Zero.WithY( _rightBound + size * 0.5f );
			_rightBound += size;
		}
	}

	private FactoryStep GetFactoryStep( int idx )
	{
		return sequence[idx].Components.Get<FactoryPanel>().factory;
	}

	private void MoveInSequence( int fromIdx, int toIdx )
	{
		var item = sequence[fromIdx];
		sequence.RemoveAt( fromIdx );
		sequence.Insert( toIdx, item );
	}

	public void RemoveFromSquence( FactoryPanel pnl )
	{
		sequence.Remove( pnl.GameObject );
		pnl.GameObject.Destroy();
		AdjustSequenceLayout();
	}

	private void StartDrag( GameObject go )
	{
		dragGo = go;
		dragIdx = sequence.FindIndex( ( go ) => go == dragGo );
		showFirstDragGap = dragIdx >= 0;
		if ( showFirstDragGap )
			sequence.RemoveAt( dragIdx );
		dragOffset = go.WorldPosition - camCont.MouseWorldPosition + new Vector3( 5.0f, 0.0f, 0.0f );
		AdjustSequenceLayout();
	}

	public void StartDragFactory( FactoryPanel pnl )
	{
		StartDrag( pnl.GameObject );
	}

	public void StartDragScratch()
	{
		StartDrag( scratchGo );
	}

	public void AddStep( string prefabPath )
	{
		var go = GameObject.GetPrefab( prefabPath ).Clone();
		go.WorldPosition = camCont.MouseWorldPosition + new Vector3( 0.0f, 0.0f, -0.025f * go.Components.Get<WorldPanel>().PanelSize.y );
		addStart = 0;
		StartDrag( go );
	}

	public void EndDrag()
	{
		sequence.Insert( addStart < 0.5f ? sequence.Count : dragIdx, dragGo );
		dragIdx = -1;
		dragGo = null;
		dragOffset = Vector3.Zero;
		AdjustSequenceLayout();
	}

	private int FindDragIndex( float yPos )
	{
		if ( sequence.Count == 0 )
			return 0;

		var start = 0;
		if ( yPos < sequence[start].WorldPosition.y )
			return start;

		var end = sequence.Count - 1;
		if ( yPos > sequence[end].WorldPosition.y )
			return end + 1;

		while ( start <= end )
		{
			int mid = (start + end) / 2;

			float midY = sequence[mid].WorldPosition.y;
			if ( midY.AlmostEqual( yPos ) )
				return mid;

			if ( midY < yPos )
				start = mid + 1;
			else
				end = mid - 1;
		}
		return start;
	}

	private void PutScratchInView()
	{
		camCont.PutInView( ScratchLeft );
		camCont.PutInView( ScratchRight );
	}

	public void AdvanceScratch()
	{
		var scratchIdx = ScratchIdx;
		if ( scratchIdx < sequence.Count - 1 )
		{
			GetFactoryStep( scratchIdx + 1 ).ApplyTo( scratchPaint );
			MoveInSequence( scratchIdx, scratchIdx + 1 );
			AdjustSequenceLayout();
			PutScratchInView();
		}
	}

	public void ResetScratch()
	{
		if ( scratchGo is not null )
		{
			scratchPaint.Reset();
			MoveInSequence( ScratchIdx, 0 );
			AdjustSequenceLayout();
			PutScratchInView();
		}
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "AdvScratch" ) )
			AdvanceScratch();
		if ( Input.Pressed( "RstScratch" ) )
			ResetScratch();

		if ( Dragging )
		{
			if ( Input.Released( "ClickL" ) )
			{
				EndDrag();
			}
			else
			{
				var currDrag = camCont.MouseWorldPosition;
				dragGo.WorldPosition = currDrag + dragOffset;
				var newIdx = FindDragIndex( currDrag.y );
				if ( dragIdx != newIdx )
				{
					dragIdx = newIdx;
					showFirstDragGap = showFirstDragGap || newIdx > 0;
					AdjustSequenceLayout();
				}
			}
		}
	}

	public void SubmitSequence()
	{
		scratchPaint.Reset();
		for ( var i = 0; i < sequence.Count; i++ )
			if ( i != ScratchIdx )
				GetFactoryStep( i ).ApplyTo( scratchPaint );
		MoveInSequence( ScratchIdx, sequence.Count - 1 );
		AdjustSequenceLayout();
	}
}
