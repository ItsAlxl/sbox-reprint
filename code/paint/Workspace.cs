namespace Reprint;

[Group( "Reprint" )]
[Title( "Workspace" )]
[Icon( "home_repair_service" )]

public sealed class Workspace : Component
{
	const float CHILD_SPACING = 30.0f / 512.0f;
	const float QUICK_ADD_TIME = 0.1f;
	const float COSMETIC_ROT_MULT = -1.0f;
	const float ANIM_STEP_TIME = 0.65f;

	public static float TopBound { get => 0.0f; }
	public float BotBound { get => _botBound; }
	private float _botBound = 0.0f;

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
	public bool Completed { get => sequenceScore.AlmostEqual( 1.0f ); }
	public bool Submitted { get; set; }
	public string LeaderboardKey { get => currentScene?.LeaderboardKey ?? Score.GetLeaderboardKey( targetPaint.Serialize() ); }

	private int animRecapStep = -1;
	private TimeUntil animRecapAdvance;

	public Painting scratchPaint;
	public FactoryStroke figStroke = new();
	private FactoryStep _breakpoint = null;
	public FactoryStep Breakpoint
	{
		get => _breakpoint;
		set
		{
			_breakpoint = value;
			UpdateResult();
		}
	}

	public bool UseConfigurator { get => currentScene.useConfigurator; }
	public bool UseBurnSponge { get => currentScene.useBurnSponge; }
	public bool UseBreakpoints { get => currentScene.useBreakpoints; }

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

		figStroke.Reset();
		camCont.ResetPosition();
		targetPaint = null;
		scratchPaint = null;

		Submitted = false;
		sequenceScore = float.NaN;
		finalScores = (-1, -1, -1);
	}

	public void BeginScenario( ScenarioData scene )
	{
		ResetLevel();
		currentSceneData = scene;
		currentScene = new( scene );
		targetPaint = new( currentScene.paint );
		FactoryStep.TargetPaint = targetPaint;
		scratchPaint = new( targetPaint.width, targetPaint.height );
		AdjustSequenceLayout();
	}

	public static Vector2 GetWorldPanelSize( GameObject go )
	{
		return (go.Components.Get<WorldPanel>()?.PanelSize ?? Vector2.Zero) * CHILD_SPACING;
	}

	private static float ArrangeSequenceGo( GameObject go, float bottom, bool showDragGap = false )
	{
		var size = GetWorldPanelSize( go );
		if ( showDragGap )
			bottom -= 128 * CHILD_SPACING;
		go.WorldPosition = new Vector3( 0.0f, size.x * 0.4f, bottom - size.y * 0.5f );
		bottom -= size.y;
		return bottom;
	}

	private void AdjustSequenceLayout()
	{
		_botBound = 0.0f;
		var numAnchors = 0;
		for ( var i = 0; i < sequence.Count; i++ )
		{
			var go = sequence[i];
			if ( i == 0 )
				_botBound += GetWorldPanelSize( go ).y;
			_botBound = ArrangeSequenceGo( go, _botBound, i == dragIdx && (showFirstDragGap || i > 0) );

			if ( GetFactoryStep( go ) is FactoryAnchor anchor && anchor is not null )
			{
				anchor.id = numAnchors;
				anchor.idx = i;
				numAnchors++;
			}
		}
		UpdateResult();
		camCont.EnforceBounds();
	}

	public static FactoryStep GetFactoryStep( GameObject factGo )
	{
		return factGo.Components.Get<FactoryPanel>()?.factory;
	}

	public void RemoveFromSquence( FactoryPanel pnl, bool alert = true )
	{
		var go = pnl.GameObject;
		sequence.Remove( go );
		GetFactoryStep( go )?.Removed();
		go.Destroy();
		UpdateResult();
		AdjustSequenceLayout();
		if ( alert )
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
		UpdateResult();
		AdjustSequenceLayout();
		if ( playSound )
			Sound.Play( "pickup" );
	}

	public void StartDragFactory( FactoryPanel pnl )
	{
		StartDrag( pnl.GameObject );
	}

	public void AddStep( string prefabPath )
	{
		var go = GameObject.GetPrefab( prefabPath ).Clone();
		go.WorldPosition = camCont.MouseWorldPosition + new Vector3( 0.0f, 0.4f * GetWorldPanelSize( go ).x, 0.0f );
		addStart = 0;
		StartDrag( go, false );
		Sound.Play( "create" );
	}

	public void EndDrag()
	{
		var placeIdx = addStart < QUICK_ADD_TIME ? sequence.Count : dragIdx;
		sequence.Insert( placeIdx, dragGo );
		GetFactoryStep( dragGo )?.Placed( placeIdx );
		ApplyCosmeticRotation( dragGo, 0.0f );
		dragIdx = -1;
		dragGo = null;
		dragOffset = Vector3.Zero;
		UpdateResult();
		AdjustSequenceLayout();
		Sound.Play( "place" );
	}

	private int FindDragIndex( float zPos )
	{
		if ( sequence.Count == 0 )
			return 0;

		var start = 0;
		if ( zPos > sequence[start].WorldPosition.z )
			return start;

		var end = sequence.Count - 1;
		if ( zPos < sequence[end].WorldPosition.z )
			return end + 1;

		while ( start <= end )
		{
			int mid = (start + end) / 2;

			float midZ = sequence[mid].WorldPosition.z;
			if ( midZ.AlmostEqual( zPos ) )
				return mid;

			if ( midZ > zPos )
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

	public static (int next, int time, int ink) AdvanceSequence( int stepIdx, FactoryStep step, Painting p )
	{
		var result = step?.ApplyTo( p ) ?? (-1, 0, 0);
		result.next = result.next == -1 ? (stepIdx + 1) : result.next;
		return result;
	}

	public static (int time, int ink, int size) ApplySequence( List<FactoryStep> seq, Painting p, FactoryStep breakpoint = null )
	{
		var stepIdx = 0;
		var finalScores = (time: 0, ink: 0, size: seq.Count( ( step ) => step is not FactoryAnchor ));
		while ( stepIdx < seq.Count )
		{
			var step = seq[stepIdx];
			if ( step == breakpoint )
				break;

			var (next, timeCost, inkCost) = AdvanceSequence( stepIdx, step, p );
			finalScores.time += timeCost;
			finalScores.ink += inkCost;
			stepIdx = next;
		}
		return finalScores;
	}

	public void UpdateResult( bool alert = true )
	{
		foreach ( var go in sequence )
			GetFactoryStep( go )?.ResetInternal();
		figStroke.Reset();
		scratchPaint.Reset();

		finalScores = ApplySequence(
			sequence.Select( GetFactoryStep ).ToList(),
			scratchPaint,
			Breakpoint
		);

		var wasCompleted = Completed;
		sequenceScore = scratchPaint.ScoreAgainst( targetPaint );
		if ( alert && Completed != wasCompleted )
		{
			Sound.Play( wasCompleted ? "failure" : "success" );
		}
	}

	public FactoryPanel CreateAnchor( int idx )
	{
		var anchorGo = GameObject.GetPrefab( "prefabs/Anchor.prefab" ).Clone();
		sequence.Insert( idx, anchorGo );
		UpdateResult();
		AdjustSequenceLayout();
		return anchorGo.Components.Get<FactoryPanel>();
	}

	protected override void OnUpdate()
	{
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
				var newIdx = FindDragIndex( currDrag.z );
				if ( dragIdx != newIdx )
				{
					dragIdx = newIdx;
					showFirstDragGap = showFirstDragGap || newIdx > 0;
					UpdateResult();
					AdjustSequenceLayout();
				}
			}
		}

		if ( Submitted && animRecapAdvance )
		{
			StepAnimRecap();
		}
	}

	private void StepAnimRecap()
	{
		if ( animRecapStep == sequence.Count )
		{
			scratchPaint.Copy( targetPaint );
			animRecapStep = -1;
			animRecapAdvance = 4 * ANIM_STEP_TIME;
		}
		else
		{
			if ( animRecapStep == -1 )
			{
				scratchPaint.Reset();
				animRecapStep++;
			}
			else
			{
				(animRecapStep, _, _) = AdvanceSequence( animRecapStep, GetFactoryStep( sequence[animRecapStep] ), scratchPaint );
			}
			animRecapAdvance = ANIM_STEP_TIME;
		}
	}

	private void ApplyCosmeticRotation( GameObject go, float factor = 1.0f )
	{
		go.WorldRotation = new Angles( factor * COSMETIC_ROT_MULT * (camCont.WorldPosition.z - go.WorldPosition.z), 0.0f, 0.0f );
	}

	public string GetLeaderboardKey( string varname = "" )
	{
		return LeaderboardKey + (varname == "" ? "" : ("_" + varname));
	}

	public void SubmitSequence()
	{
		UpdateResult();
		Submitted = Completed;
		if ( Completed )
		{
			Score.Send( LeaderboardKey, finalScores );
			animRecapStep = -1;
			StepAnimRecap();
			Sound.Play( "submit" );
		}
	}

	public void BeginRetry()
	{
		sequenceScore = float.NaN;
		Submitted = false;
		UpdateResult( false );
	}
}
