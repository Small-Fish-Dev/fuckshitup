namespace FUCKSHIT;

partial class Character
{
	public static float FLY_SPEED => 1000f;
	public static float JUMP_FORCE => 250f;

	public static float RUN_SPEED => 350f;
	public static float WALK_SPEED => 125f;

	public static BBox DEFAULT_BOUNDS => BBox.FromPositionAndSize( new Vector3( 0f, 0f, 35.5f ), new Vector3( 22f, 22f, 71f ) );
	public static BBox CROUCH_BOUNDS => BBox.FromPositionAndSize( new Vector3( 0f, 0f, 28f ), new Vector3( 22f, 22f, 56f ) );

	public bool AllowSprintDirection => WishVelocity != 0 && WishVelocity.Normal.Dot( WorldRotation.Forward ) > 0.1f;

	[Sync] public GameObject GroundObject { get; set; }
	[Sync] public Vector3 WishVelocity { get; set; }
	[Sync] public bool Crouched { get; set; }
	[Sync] public bool Reloading { get; set; }

	public BBox Bounds => Crouched ? CROUCH_BOUNDS : DEFAULT_BOUNDS;
	public bool Grounded => GroundObject.IsValid();

	public bool Noclip { get; set; }

	private void HandleJump()
	{
		if ( !IsAlive || !Controller.IsOnGround )
			return;

		if ( Input.Pressed( InputAction.JUMP ) )
		{
			if ( Controller.Velocity.z > JUMP_FORCE )
				return;

			Controller.Velocity = Controller.Velocity.WithZ( 0f );
			Controller.Punch( Vector3.Up * JUMP_FORCE );

			BroadcastBoolAnim( "jump", true );
		}
	}

	private void HandleCrouching()
	{
		Crouched = Input.Down( InputAction.CROUCH )
			|| (Crouched && Controller.BuildTrace( Bounds.Grow( -2f ), WorldPosition, WorldPosition + Vector3.Up * (DEFAULT_BOUNDS.Extents.z - 5f) ).Hit);

		Controller.TraceHeight = Bounds.Size.z;
		Controller.TraceWidth = MathF.Max( Bounds.Size.x, Bounds.Size.y );

		if ( Collider.IsValid() )
		{
			Collider.Center = Bounds.Center;
			Collider.Scale = Bounds.Size;
		}
	}

	private void HandleNoclipInput()
	{
		if ( Input.Down( InputAction.CROUCH ) ) WorldPosition += Vector3.Down * FLY_SPEED * Time.Delta;
		if ( Input.Down( InputAction.JUMP ) ) WorldPosition += Vector3.Up * FLY_SPEED * Time.Delta;

		WorldPosition += (Input.AnalogMove * WorldRotation).Normal
			* FLY_SPEED
			* Time.Delta;

		Controller.Velocity = 0f;
	}

	private void SimulateMovement()
	{
		if ( !Controller.IsValid() || !Collider.IsValid() || !IsAlive || LocalRagdolled )
			return;

		if ( Noclip )
		{
			HandleNoclipInput();
			return;
		}

		Controller.IgnoreTags ??= new();

		var running = AllowSprintDirection && Input.Down( InputAction.SPRINT );
		var wishDirection = (Input.AnalogMove * WorldRotation).Normal;
		var wishSpeed = running
			? RUN_SPEED
			: WALK_SPEED;
		WishVelocity = !IsAlive ? Vector3.Zero : wishDirection * wishSpeed;

		if ( !Controller.IsValid() )
			return;
		
		Controller.WishVelocity = WishVelocity;
		GroundObject = Controller.GroundObject;

		HandleJump();
		HandleCrouching();

		var controllerResult = Controller.Move( true );
		var currentPosition = controllerResult.Position;
		var currentVelocity = controllerResult.Velocity;

		WorldPosition = currentPosition;
		Controller.Velocity = currentVelocity;
	}
}
