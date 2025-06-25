namespace Reprint;

[Group( "Reprint" )]
[Title( "Workspace" )]
[Icon( "home_repair_service" )]

public sealed class Workspace : Component
{
	public const float CHILD_SPACING = 30.0f / 512.0f;
	const float SCRATCH_SPACING = CHILD_SPACING * 128.0f;
	const float QUICK_ADD_TIME = 0.1f;
	const float COSMETIC_ROT_MULT = 0.25f;

	public static float LeftBound { get => 0.0f; }
	public float RightBound { get => _rightBound; }
	private float _rightBound = 0.0f;

	public Painting scratchPaint;
	private GameObject scratchGo;
	private int scratchIdx = 0;

	private TimeSince addStart;
	private int dragIdx = -1;
	private bool showFirstDragGap = false;
	private Vector3 dragOffset = Vector3.Zero;
	private GameObject dragGo = null;
	public bool Dragging { get => dragGo is not null; }

	private CameraController camCont;
	private ScenarioData currentSceneData;
	public Scenario currentScene;
	public Painting targetPaint;
	public ScenarioData NextSceneData { get => Scenario.GetNext( currentSceneData ); }

	private readonly List<GameObject> sequence = [];
	public float sequenceScore = float.NaN;
	public (int time, int ink, int size) finalScores = (-1, -1, -1);
	public bool IsCompleted { get => sequenceScore.AlmostEqual( 1.0f ); }
	public string LeaderboardKey { get => currentScene?.LeaderboardKey ?? Score.GetLeaderboardKey( targetPaint.Serialize() ); }

	public FactoryStroke figStroke = new();

	public bool useConfigurator { get => currentScene.useConfigurator; }
	public bool useBurnSponge { get => currentScene.useBurnSponge; }
	public bool useBreakpoints { get => currentScene.useBreakpoints; }

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

		if ( scratchGo is not null )
			scratchGo.Destroy();
		scratchGo = null;

		figStroke.Reset();
		camCont.ResetPosition();
		targetPaint = null;
		scratchPaint = null;

		sequenceScore = float.NaN;
		finalScores = (-1, -1, -1);
		scratchIdx = 0;
	}

	public void BeginScenario( ScenarioData scene )
	{
		if ( scratchGo is not null )
			ResetLevel();

		currentSceneData = scene;
		currentScene = new( scene );
		targetPaint = new( currentScene.paint );
		scratchPaint = new( targetPaint.width, targetPaint.height );
		scratchGo = GameObject.GetPrefab( "prefabs/scratchpad.prefab" ).Clone();
		AdjustSequenceLayout();
	}

	private float GetWorldPanelSize( GameObject go )
	{
		return go.Components.Get<WorldPanel>().PanelSize.x * CHILD_SPACING;
	}

	private float ArrangeSequenceGo( GameObject go, float right, bool isDragIdx = false )
	{
		var size = GetWorldPanelSize( go );
		if ( isDragIdx )
			right += 128 * CHILD_SPACING;
		go.LocalPosition = Vector3.Zero.WithY( right + size * 0.5f );
		right += size;
		return right;
	}

	private void AdjustSequenceLayout()
	{
		var scratchDrag = scratchGo == dragGo;
		_rightBound = scratchDrag ? GetWorldPanelSize( scratchGo ) : 0.0f;
		var numAnchors = 0;
		for ( var i = 0; i <= sequence.Count; i++ )
		{
			if ( i == scratchIdx && !scratchDrag )
				_rightBound = ArrangeSequenceGo( scratchGo, _rightBound );

			if ( i < sequence.Count )
			{
				var go = sequence[i];
				_rightBound = ArrangeSequenceGo( go, _rightBound, i == dragIdx && (showFirstDragGap || i > 0) );

				if ( GetFactoryStep( go ) is FactoryAnchor anchor && anchor is not null )
				{
					anchor.id = numAnchors;
					anchor.idx = i;
					numAnchors++;
				}
			}
		}
	}

	private FactoryStep GetFactoryStep( GameObject factGo )
	{
		return factGo.Components.Get<FactoryPanel>()?.factory;
	}

	private FactoryStep GetFactoryStep( int idx )
	{
		return GetFactoryStep( sequence[idx] );
	}

	private void MoveInSequence( int fromIdx, int toIdx )
	{
		var item = sequence[fromIdx];
		sequence.RemoveAt( fromIdx );
		sequence.Insert( toIdx, item );
	}

	public void RemoveFromSquence( FactoryPanel pnl )
	{
		var go = pnl.GameObject;
		sequence.Remove( go );
		GetFactoryStep( go )?.Removed();
		go.Destroy();
		AdjustSequenceLayout();
		Sound.Play( "delete" );
	}

	private void StartDrag( GameObject go, bool playSound = true )
	{
		dragGo = go;
		dragIdx = sequence.FindIndex( ( go ) => go == dragGo );
		showFirstDragGap = dragIdx >= 0;
		if ( showFirstDragGap )
			sequence.RemoveAt( dragIdx );
		dragOffset = go.WorldPosition - camCont.MouseWorldPosition;
		AdjustSequenceLayout();
		if ( playSound )
			Sound.Play( "pickup" );
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
		StartDrag( go, false );
		Sound.Play( "create" );
	}

	public void EndDrag()
	{
		var placeIdx = addStart < QUICK_ADD_TIME ? sequence.Count : dragIdx;
		sequence.Insert( placeIdx, dragGo );
		GetFactoryStep( dragGo )?.Placed( placeIdx );
		dragIdx = -1;
		dragGo = null;
		dragOffset = Vector3.Zero;
		AdjustSequenceLayout();
		Sound.Play( "place" );
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

	public void PutInView( GameObject go )
	{
		if ( go is not null )
			camCont.PutInView( go );
	}

	private void PutScratchInView()
	{
		camCont.PutInView( scratchGo );
	}

	private void MoveScratchAfter( int idx )
	{
		scratchIdx = idx + 1;
	}

	private void MoveScratchBefore( int idx )
	{
		scratchIdx = idx;
	}

	public void AdvanceScratch()
	{
		if ( scratchIdx < sequence.Count )
		{
			var step = ApplyStepToScratch( scratchIdx );
			MoveScratchBefore( step.next == -1 ? scratchIdx + 1 : step.next );
			AdjustSequenceLayout();
			PutScratchInView();
			Sound.Play( "advance" );
		}
	}

	public void AdvanceToBreakpoint()
	{
		var stepIdx = scratchIdx;
		while ( stepIdx < sequence.Count )
		{
			var factory = GetFactoryStep( stepIdx );
			if ( stepIdx > scratchIdx && factory.panel.breakpoint )
				break;

			var step = ApplyStepToScratch( stepIdx );
			stepIdx = step.next == -1 ? stepIdx + 1 : step.next;
		}
		MoveScratchBefore( stepIdx );
		AdjustSequenceLayout();
		PutScratchInView();
		Sound.Play( "advance" );
	}

	public void ResetScratch( bool notice = true )
	{
		if ( scratchGo is not null )
		{
			foreach ( var go in sequence )
				GetFactoryStep( go )?.ResetInternal();
			figStroke.Reset();
			scratchPaint.Reset();

			if ( notice )
			{
				MoveScratchBefore( 0 );
				AdjustSequenceLayout();
				PutScratchInView();
				Sound.Play( "reset" );
			}
		}
	}

	public void ResetAndAdvanceTo( FactoryPanel pnl )
	{
		ResetScratch( false );

		var stepIdx = 0;
		var go = pnl.GameObject;
		while ( stepIdx < sequence.Count )
		{
			var step = ApplyStepToScratch( stepIdx );
			var isFinal = sequence[stepIdx] == go;
			var thisStep = stepIdx;
			stepIdx = step.next == -1 ? stepIdx + 1 : step.next;
			if ( isFinal && stepIdx > thisStep )
				break;
		}
		MoveScratchBefore( stepIdx );
		AdjustSequenceLayout();
		PutScratchInView();
		Sound.Play( "advance" );
	}

	public FactoryPanel CreateAnchor( int idx )
	{
		var anchorGo = GameObject.GetPrefab( "prefabs/Anchor.prefab" ).Clone();
		sequence.Insert( idx, anchorGo );
		AdjustSequenceLayout();
		return anchorGo.Components.Get<FactoryPanel>();
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
				dragGo.WorldPosition = (currDrag + dragOffset).WithX( 5.0f );
				ApplyCosmeticRotation( dragGo, 2.0f );
				var newIdx = FindDragIndex( currDrag.y );
				if ( dragIdx != newIdx )
				{
					dragIdx = newIdx;
					showFirstDragGap = showFirstDragGap || newIdx > 0;
					AdjustSequenceLayout();
				}
			}
		}

		foreach ( var go in sequence )
			ApplyCosmeticRotation( go );
	}

	private void ApplyCosmeticRotation( GameObject go, float factor = 1.0f )
	{
		go.WorldRotation = go.WorldRotation.Angles().WithYaw( factor * COSMETIC_ROT_MULT * (camCont.WorldPosition.y - go.WorldPosition.y) );
	}

	private void SnapToEndScratch()
	{
		MoveScratchBefore( sequence.Count );
		AdjustSequenceLayout();
		camCont.SnapTo( scratchGo.WorldPosition.y );
	}

	public (int next, int timeCost, int inkCost) ApplyStepToScratch( int stepIdx )
	{
		return GetFactoryStep( stepIdx )?.ApplyTo( scratchPaint ) ?? (-1, 0, 0);
	}

	public string GetLeaderboardKey( string varname = "" )
	{
		return LeaderboardKey + (varname == "" ? "" : ("_" + varname));
	}

	public void SubmitSequence()
	{
		scratchPaint.Reset();

		var stepIdx = 0;
		finalScores = (0, 0, 0);
		while ( stepIdx < sequence.Count )
		{
			var step = ApplyStepToScratch( stepIdx );
			finalScores.time += step.timeCost;
			finalScores.ink += step.inkCost;
			stepIdx = step.next == -1 ? stepIdx + 1 : step.next;
		}

		sequenceScore = scratchPaint.ScoreAgainst( targetPaint );
		if ( IsCompleted )
		{
			finalScores.size = sequence.Count( ( go ) => GetFactoryStep( go ) is not FactoryAnchor );
			Score.Send( LeaderboardKey, finalScores );
			Sound.Play( "success" );
		}
		else
		{
			SnapToEndScratch();
			Sound.Play( "failure" );
		}
	}

	public void BeginRetry()
	{
		SnapToEndScratch();
		sequenceScore = float.NaN;
	}
}
