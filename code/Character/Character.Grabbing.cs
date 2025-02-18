namespace FUCKSHIT;

partial class Character
{
	private const float MAX_HOLD_DISTANCE = 75f;
	private const float MIN_HOLD_DISTANCE = 30f;

	public GameObject Grabbed { get; set; }
	public Vector3 GrabbedPosition => _grabbedBody.IsValid()
		? _grabbedBody.Transform.PointToWorld( _grabbedPosition ) 
		: Grabbed?.WorldPosition ?? Vector3.Zero;

	private PhysicsBody _grabbedBody;
	private float _grabbedDistance;
	private float _grabLerpedDistance;
	private Vector3 _grabbedPosition;
	private float _grabbedWeight;

	private void HandleGrabbing()
	{
		var trace = Scene.Trace.Ray( EyeRay, MAX_HOLD_DISTANCE )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( Input.Pressed( InputAction.RIGHT_MOUSE ) )
			Grab( trace );

		if ( Grabbed.IsValid() )
		{
			_grabbedDistance = MathX.Clamp( _grabbedDistance + Input.MouseWheel.y * 5f, MIN_HOLD_DISTANCE, MAX_HOLD_DISTANCE );
			_grabLerpedDistance = MathX.Lerp( _grabLerpedDistance, _grabbedDistance, 15f * Time.Delta );

			if ( Input.Down( InputAction.RIGHT_MOUSE ) )
			{
				MoveGrabbed();
			}
			else Grabbed = null;
		}

		if ( Input.Down( InputAction.RIGHT_MOUSE ) ) // todo: doesn't work properly in ragdoll :D
		{
			var localPosition = Grabbed.IsValid()
				? (GrabbedPosition - WorldPosition) * Renderer.WorldRotation.Inverse
				: EyeForward * MAX_HOLD_DISTANCE * Renderer.WorldRotation.Inverse;
			var length = localPosition.Length;

			var angles = Rotation.LookAt( localPosition.Normal ).Angles();
			angles.pitch = angles.pitch.Clamp( -70, 10 );
			angles.yaw = angles.yaw.Clamp( -45, 45 );
			localPosition = angles.Forward * length;

			var targetPosition = Grabbed.IsValid() ? GrabbedPosition : EyeRay.Position + EyeForward * MAX_HOLD_DISTANCE;
			var direction = Vector3.Direction( EyeRay.Position, targetPosition );
			var pitch = Rotation.LookAt( direction ).Pitch();
			var transform = new Transform( localPosition, Rotation.From( pitch.Clamp( -70, 40 ), 0, 90 ) );
			RightIK ??= transform;
			RightIK = global::Transform.Lerp( RightIK.GetValueOrDefault(), transform, 10f * Time.Delta, true );
		}
		else RightIK = null;

		// Lost reach...
		if ( Grabbed.IsValid() && !LocalRagdolled )
		{
			if ( GrabbedPosition.Distance( WorldPosition ) > MAX_HOLD_DISTANCE + 10f )
				Grabbed = null;
		}
	}

	void Grab( SceneTraceResult tr )
	{
		if ( !tr.Hit ) return;

		var target = tr.GameObject;
		var body = tr.Body;

		if ( !target.IsValid() || !body.IsValid() ) return;
		if ( body.BodyType != PhysicsBodyType.Dynamic ) return;

		Grabbed = target;
		_grabbedBody = body;
		_grabLerpedDistance = _grabbedDistance = tr.Distance;
		_grabbedPosition = body.Transform.PointToLocal( tr.HitPosition );

		_grabbedWeight = body.Mass;
	}

	void MoveGrabbed()
	{
		if ( !_grabbedBody.IsValid() ) return;

		var worldPosition = GrabbedPosition;
		var worldVelocity = _grabbedBody.Velocity;
		var goalPosition = Camera.WorldPosition + Camera.WorldRotation.Forward * _grabLerpedDistance;
		var difference = goalPosition - worldPosition;
		var strength = Math.Min( _grabbedWeight, 50f ) / 50f;
		var force = difference * 1000000f * strength;

		_grabbedBody.ApplyForceAt( worldPosition, force * Time.Delta );
		_grabbedBody.ApplyForceAt( worldPosition, -worldVelocity * 50000 * strength * Time.Delta );
	}
}
