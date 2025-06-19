namespace Reprint;

[Group( "Reprint" )]
[Title( "Workspace" )]
[Icon( "home_repair_service" )]

public sealed class Workspace : Component
{
	const float CHILD_SPACING = 30.0f / 512.0f;

	public static float LeftBound { get => 0.0f; }
	public float RightBound { get => _rightBound; }
	private float _rightBound = 0.0f;

	public Painting scratchPaint = new( 4, 4 );
	private GameObject scratchGo;
	private int ScratchIdx { get => sequence.FindIndex( ( go ) => go == scratchGo ); }

	private int dragIdx = -1;
	private Vector3 dragOffset = Vector3.Zero;
	private GameObject dragGo = null;
	private bool Dragging { get => dragGo is not null; }

	private CameraController camCont;
	public Scenario currentScene;
	public Painting targetPaint;
	private readonly List<GameObject> sequence = [];

	protected override void OnStart()
	{
		camCont = Scene.Get<CameraController>();
		scratchGo = GameObject.Children.Find( ( go ) => go.Name == "Scratch" );
		sequence.Add( scratchGo );
		AdjustSequenceLayout();
	}

	public void ResetLevel()
	{
		ResetScratch();
		for (var i = 1; i < sequence.Count; i++)
			sequence[i].Destroy();
		if (sequence.Count > 1)
			sequence.RemoveRange(1, sequence.Count);
		camCont.ResetPosition();
		targetPaint = null;
	}

	public void BeginScenario(Scenario scene)
	{
		ResetLevel();
		currentScene = scene;
		targetPaint = new(scene.paint);
	}

	public void AddStep( GameObject prefab )
	{
		sequence.Add( prefab.Clone() );
		AdjustSequenceLayout();
	}

	private void AdjustSequenceLayout()
	{
		_rightBound = 0;
		for ( var i = 0; i < sequence.Count; i++ )
		{
			var go = sequence[i];
			var size = go.GetComponent<WorldPanel>().PanelSize.x * CHILD_SPACING;
			if ( i == dragIdx )
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

	private int FindDragIndex( float yPos )
	{
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

	public void AdvanceScratch()
	{
		var scratchIdx = ScratchIdx;
		if ( scratchIdx < sequence.Count - 1 )
		{
			GetFactoryStep( scratchIdx + 1 ).ApplyTo( scratchPaint );
			MoveInSequence( scratchIdx, scratchIdx + 1 );
			AdjustSequenceLayout();
		}
	}

	public void ResetScratch()
	{
		MoveInSequence( ScratchIdx, 0 );
		scratchPaint.Reset();
		AdjustSequenceLayout();
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
				sequence.Insert( dragIdx, dragGo );
				dragIdx = -1;
				dragGo = null;
				dragOffset = Vector3.Zero;
				AdjustSequenceLayout();
			}
			else
			{
				var currDrag = camCont.MouseWorldPosition;
				dragGo.WorldPosition = currDrag + dragOffset;
				var newIdx = FindDragIndex( currDrag.y );
				if ( dragIdx != newIdx )
				{
					dragIdx = newIdx;
					AdjustSequenceLayout();
				}
			}
		}
	}
}
