namespace Reprint;

[Group( "Reprint" )]
[Title( "Camera Controller" )]
[Icon( "cameraswitch" )]

public sealed class CameraController : Component
{
	[RequireComponent] public CameraComponent Camera { get; set; }
	private readonly Sandbox.UI.WorldInput worldInput = new();

	protected override void OnStart()
	{
		worldInput.Enabled = true;
	}
	
	protected override void OnUpdate()
	{
		worldInput.Ray = Camera.ScreenPixelToRay( Mouse.Position );
		worldInput.MouseLeftPressed = Input.Down( "ClickL" );
		worldInput.MouseRightPressed = Input.Down( "ClickR" );
	}
}
