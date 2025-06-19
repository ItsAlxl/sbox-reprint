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

	public void ResetPosition()
	{
		WorldPosition = WorldPosition.WithY(0.0f);
	}
}
