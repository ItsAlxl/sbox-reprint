using System;

namespace Reprint;

[Group( "Reprint" )]
[Title( "Camera Controller" )]
[Icon( "cameraswitch" )]

public sealed class CameraController : Component
{
	const float SCROLL_SPEED = 10.0f;

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

		if ( Input.Down( "ScrollL" ) )
		{
			WorldPosition = WorldPosition.WithY( Math.Max( WorldPosition.y - SCROLL_SPEED, Workspace.LeftBound ) );
		}
		if ( Input.Down( "ScrollR" ) )
		{
			WorldPosition = WorldPosition.WithY( Math.Min( WorldPosition.y + SCROLL_SPEED, workspace.RightBound ) );
		}
	}

	public void SnapTo( float y )
	{
		WorldPosition = WorldPosition.WithY( y );
	}

	public void ResetPosition()
	{
		SnapTo( 0.0f );
	}

	public bool PutInView( Vector3 p, float padding = 38.0f )
	{
		var moved = !IsInCameraBounds( p );
		if ( moved )
			SnapTo( p.y + (WorldPosition.y > p.y ? 1 : -1) * padding );
		return moved;
	}

	public bool PutInView( Vector3 left, Vector3 right, float padding = 38.0f )
	{
		if ( !PutInView( left, padding ) )
			return PutInView( right, padding );
		return true;
	}

	public bool IsInCameraBounds( Vector3 pos )
	{
		return ScreenFrustum.IsInside( pos );
	}
}
