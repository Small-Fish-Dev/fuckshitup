namespace FUCKSHIT;

partial class Character
{
	[Sync( SyncFlags.Interpolate )]
	public Transform? LeftIK {  get; set; }

	[Sync( SyncFlags.Interpolate )]
	public Transform? RightIK { get; set; }

	/// <summary>
	/// How fat is our character..?
	/// </summary>
	[Property, Sync, Category( "Appearance" ), Range( -0.5f, 1.5f )]
	public float Fatness { get; set; } = 0f;

	/// <summary>
	/// How tall is our character..?
	/// </summary>
	[Property, Sync, Category( "Appearance" ), Range( -1f, 1f )]
	public float Height { get; set; } = 0f;

	private void SimulateAnimations()
	{
		if ( !Renderer.IsValid() )
			return;

		HandleIK();

		WorldRotation = Rotation.FromYaw( EyeAngles.yaw );

		Renderer.Set( "lookat", EyeAngles.WithYaw( 0 ).Forward );
		Renderer.SceneModel?.Morphs.Set( "fat", Fatness );

		if ( FirstpersonView.IsValid() )
			FirstpersonView.SceneModel?.Morphs.Set( "fat", Fatness );

		SetAnimParameter( "height", Height );
		SetAnimParameter( "weight", Fatness );

		/*SetAnimParameter( "sitting", Sitting != null );
		if ( Sitting != null )
		{
			const float rate = 0.8f;
			var ang = (EyeAngles.ToRotation() * WorldRotation.Inverse).Angles();
			WorldPosition = WorldPosition.LerpTo( SittingTransform.Value.Position, rate );
			WorldRotation = Rotation.Slerp( WorldRotation, SittingTransform.Value.Rotation, rate );
			Renderer.Set( "lookat", ang.WithPitch( EyeAngles.pitch ).ToRotation().Forward );

			return;
		}*/

		SetAnimParameter( "grounded", Grounded );
		SetAnimParameter( "crouching", Crouched );

		var oldX = Renderer.GetFloat( "move_x" );
		var oldY = Renderer.GetFloat( "move_y" );
		var newX = Vector3.Dot( WishVelocity, WorldRotation.Forward ) / 150f;
		var newY = Vector3.Dot( WishVelocity, WorldRotation.Right ) / 150f;
		var x = MathX.Lerp( oldX, newX, Time.Delta * 5f );
		var y = MathX.Lerp( oldY, newY, Time.Delta * 5f );

		SetAnimParameter( "move_x", x );
		SetAnimParameter( "move_y", y );
	}

	private void HandleIK()
	{
		SetAnimParameter( "left_ik", LeftIK != null );
		SetAnimParameter( "right_ik", RightIK != null );

		if ( LeftIK != null )
		{
			SetAnimParameter( "left_ik_pos", LeftIK?.Position ?? default );
			SetAnimParameter( "left_ik_pos", LeftIK?.Rotation.Angles() ?? default );
		}

		if ( RightIK != null )
		{
			SetAnimParameter( "right_ik_pos", RightIK?.Position ?? default );
			SetAnimParameter( "right_ik_pos", RightIK?.Rotation.Angles() ?? default );
		}
	}

	private void SetAnimParameter( string name, float value )
	{
		if ( FirstpersonView.IsValid() && FirstpersonView.Enabled )
			FirstpersonView.Set( name, value );

		Renderer.Set( name, value );
	}

	private void SetAnimParameter( string name, Vector3 value )
	{
		if ( FirstpersonView.IsValid() && FirstpersonView.Enabled )
			FirstpersonView.Set( name, value );

		Renderer.Set( name, value );
	}

	private void SetAnimParameter( string name, Angles value )
	{
		if ( FirstpersonView.IsValid() && FirstpersonView.Enabled )
			FirstpersonView.Set( name, value );

		Renderer.Set( name, value );
	}

	private void SetAnimParameter( string name, int value )
	{
		if ( FirstpersonView.IsValid() && FirstpersonView.Enabled )
			FirstpersonView.Set( name, value );

		Renderer.Set( name, value );
	}

	private void SetAnimParameter( string name, bool value )
	{
		if ( FirstpersonView.IsValid() && FirstpersonView.Enabled )
			FirstpersonView.Set( name, value );

		Renderer.Set( name, value );
	}

	[Rpc.Broadcast( NetFlags.OwnerOnly | NetFlags.Reliable )]
	public void BroadcastBoolAnim( string name, bool state )
	{
		if ( !Renderer.IsValid() )
			return;

		Renderer.Set( name, state );
	}
}
