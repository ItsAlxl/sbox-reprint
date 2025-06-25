using System;

namespace Reprint;

[Group( "Reprint" )]
[Title( "Camera Controller" )]
[Icon( "cameraswitch" )]

public sealed class CameraController : Component
{
	const float SCROLL_SPEED = 7.5f;

	[RequireComponent] public CameraComponent Camera { get; set; }
	private readonly Sandbox.UI.WorldInput worldInput = new();
	private Workspace workspace;

	public Vector3 MouseWorldPosition { get => Camera.ScreenPixelToRay( Mouse.Position ).Project( Camera.WorldPosition.x ); }
	public Frustum ScreenFrustum { get => Camera.GetFrustum( new Rect( 0, 0, Screen.Width, Screen.Height ) ); }

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
