using System;

namespace Reprint;

[Group( "Reprint" )]
[Title( "Camera Controller" )]
[Icon( "cameraswitch" )]

public sealed class CameraController : Component
{
	const float SCROLL_SPEED = 7.5f;
	const float ASPECT_TO_CAM_Y_MAX = 5.0f * 4.0f / 3.0f;
	const float ASPECT_TO_CAM_Y_MID = 17.5f * 4.0f / 3.0f;
	const float ASPECT_TO_CAM_Y_MIN = 25.0f * 4.0f / 3.0f;

	[RequireComponent] public CameraComponent Camera { get; set; }
	private readonly Sandbox.UI.WorldInput worldInput = new();
	private Workspace workspace;

	public Vector3 MouseWorldPosition { get => Camera.ScreenPixelToRay( Mouse.Position ).Project( Camera.WorldPosition.x ); }
	public Frustum ScreenFrustum { get => Camera.GetFrustum( new Rect( 0, 0, Screen.Width, Screen.Height ) ); }

	private float prevAspect = -1.0f;
	private int _ingameBriefSize = 2;
	private bool needBriefUpdate = false;
	public int IngameBriefSize
	{
		get => _ingameBriefSize;
		set
		{
			_ingameBriefSize = value;
			needBriefUpdate = true;
		}
	}

	protected override void OnStart()
	{
		worldInput.Enabled = true;
		workspace = Scene.Get<Workspace>();
	}

	protected override void OnUpdate()
	{
		worldInput.Ray = Camera.ScreenPixelToRay( Mouse.Position );
		worldInput.MouseLeftPressed = Input.Down( "ClickL" );
		worldInput.MouseRightPressed = Input.Down( "ClickR" );

		if ( Input.Down( "ScrollUp" ) )
			WorldPosition = WorldPosition.WithZ( Math.Min( WorldPosition.z + SCROLL_SPEED, Workspace.TopBound ) );
		if ( Input.Down( "ScrollDown" ) )
			WorldPosition = WorldPosition.WithZ( Math.Max( WorldPosition.z - SCROLL_SPEED, workspace.BotBound ) );

		var aspect = Screen.Aspect;
		if ( aspect != prevAspect || needBriefUpdate )
		{
			WorldPosition = WorldPosition.WithY( (IngameBriefSize switch
			{
				0 => ASPECT_TO_CAM_Y_MIN,
				1 => ASPECT_TO_CAM_Y_MID,
				_ => ASPECT_TO_CAM_Y_MAX,
			} + 2.0f * aspect) / aspect );
			prevAspect = aspect;
			needBriefUpdate = false;
		}
	}

	public void EnforceBounds()
	{
		WorldPosition = WorldPosition.WithZ( Math.Clamp( WorldPosition.z, workspace.BotBound, Workspace.TopBound ) );
	}

	public void SnapTo( float z )
	{
		WorldPosition = WorldPosition.WithZ( Math.Clamp( z, workspace.BotBound, Workspace.TopBound ) );
	}

	public void ResetPosition()
	{
		SnapTo( 0.0f );
	}

	public bool PutInView( Vector3 p, float padding = 0.0f )
	{
		var moved = !IsInCameraBounds( p );
		if ( moved )
			SnapTo( p.z + (WorldPosition.z > p.z ? -1 : 1) * padding );
		return moved;
	}

	public bool PutInView( GameObject go, float padding = 0.0f )
	{
		return PutInView( go.WorldPosition - new Vector3( 0.0f, 0.0f, Workspace.GetWorldPanelSize( go ).y ), padding );
	}

	public bool IsInCameraBounds( Vector3 pos )
	{
		return ScreenFrustum.IsInside( pos );
	}
}
