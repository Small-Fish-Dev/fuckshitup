namespace FUCKSHIT;

record struct BoneTransform( Vector3 pos, Rotation rot );

partial class Character
{
	private const float BONE_UPDATE_INTERVAL = 0.1f; // How long between bone updates (in seconds).

	[Sync]
	private bool NetRagdolled
	{
		get => _ragdolled;
		set
		{
			_ragdolled = value;

			if ( Renderer.IsValid() )
				Renderer.Tags.Set( "ragdoll", _ragdolled );
		}
	}
	private bool _ragdolled;

	private TimeSince _lastBoneUpdate;
	private Transform[] _previousTransforms;
	private BoneTransform[] _boneTransforms;

	/// <summary>
	/// NOTE: Only use this locally from the owner of this object.
	/// </summary>
	public bool LocalRagdolled // todo: for some reason the very first ragdoll is always at default of transform...?
	{
		get => RagdollController.IsValid() && RagdollController.MotionEnabled;
		set
		{
			if ( IsProxy ) return;

			// Reset renderer local transform.
			RagdollAngles = Angles.Zero;

			if ( Renderer.IsValid() && !value )
			{
				WorldPosition -= Renderer.LocalPosition.WithZ( -Renderer.LocalPosition.z );
				Renderer.LocalTransform = global::Transform.Zero;
			}

			// Toggle ragdoll controller.
			if ( RagdollController.IsValid() )
			{
				RagdollController.MotionEnabled = value;
				RagdollController.Enabled = value;

				if ( !value )
				{
					foreach ( var child in RagdollController.GameObject.Children )
						child.Destroy();
				}

				// Inherit velocity from controller and also vice-versa.
				if ( Controller.IsValid() )
				{
					if ( value )
					{
						RagdollController.SetVelocity( Controller.Velocity );
						Controller.Velocity = Vector3.Zero;
					}
					else
						Controller.Velocity = RagdollController.GetVelocity();
				}
			}

			if ( Collider.IsValid() )
				Collider.Enabled = !value;

			NetRagdolled = value;
			if ( value ) SendBoneUpdate( initial: true );
		}
	}

	private void SimulateRagdollMovement()
	{
		if ( !RagdollController.IsValid() ) return;

		void SimulateHand( string name, bool active )
		{
			var hand = RagdollController.Bodies?.FirstOrDefault( x => x.Bone.Name == name );
			if ( hand is null || !active )
				return;

			var rigidBody = hand.Component;
			if ( !rigidBody.IsValid() )
				return;

			if ( !Grabbed.IsValid() )
			{
				var target = Camera.WorldPosition + Camera.WorldRotation.Forward * 50f;
				rigidBody.SmoothMove( target, 0.2f, Time.Delta );

				return;
			}

			rigidBody.SmoothMove( GrabbedPosition, 0.25f, Time.Delta );
			var distance = GrabbedPosition.Distance( rigidBody.WorldPosition );
			var power = MathX.Remap( distance, 0f, 200f, 100f, 600f, true );

			foreach ( var body in RagdollController.Bodies )
			{
				var component = body.Component;
				if ( body == hand || !component.IsValid() ) continue;

				var normal = Vector3.Direction( component.WorldPosition, GrabbedPosition );
				rigidBody.ApplyImpulse( normal * rigidBody.Mass / 2f * power );
			}

			if ( GrabbedPosition.Distance( rigidBody.WorldPosition ) > 125f )
				Grabbed = null;
		}

		SimulateHand( "hand_R", Input.Down( InputAction.RIGHT_MOUSE ) );
	}

	private void SimulateRagdoll()
	{
		if ( !NetRagdolled ) 
			return;

		if ( !Renderer.IsValid() )
			return;
		
		var sceneObject = Renderer.SceneModel;
		if ( !sceneObject.IsValid() )
			return;

		// Interpolate all bones for other clients.
		if ( IsProxy )
		{
			UpdateBoneTransforms( sceneObject );
			return;
		}

		// Owner of this character will send data of bone transforms to other clients.
		SendBoneUpdate();
	}

	private void SendBoneUpdate( bool initial = false )
	{
		if ( !Renderer.IsValid() )
			return;

		var model = Renderer.Model;
		var sceneObject = Renderer.SceneModel;
		if ( model is null || !sceneObject.IsValid() )
			return;

		if ( _lastBoneUpdate < BONE_UPDATE_INTERVAL )
			return;

		using ( var stream = new MemoryStream() )
		using ( var writer = new BinaryWriter( stream ) )
		{
			writer.Write( initial );

			for ( int i = 0; i < model.BoneCount; i++ )
			{
				var transform = sceneObject.GetBoneWorldTransform( i );

				// Position
				writer.Write( transform.Position.x );
				writer.Write( transform.Position.y );
				writer.Write( transform.Position.z );

				// Rotation
				writer.Write( transform.Rotation.x );
				writer.Write( transform.Rotation.y );
				writer.Write( transform.Rotation.z );
				writer.Write( transform.Rotation.w );
			}

			var data = stream.ToArray();
			BroadcastBoneTransforms( data );
		}

		_lastBoneUpdate = 0f;
	}

	private void UpdateBoneTransforms( SceneModel sceneObject )
	{
		if ( _boneTransforms is null )
			return;

		var model = sceneObject.Model;
		if ( model is null )
			return;

		for ( int i = 0; i < model.BoneCount; i++ )
		{
			var transform = new Transform(
				Vector3.Lerp( _previousTransforms[i].Position, _boneTransforms[i].pos, 15f * Time.Delta ),
				Rotation.Lerp( _previousTransforms[i].Rotation, _boneTransforms[i].rot, 15f * Time.Delta ),
				1f
			);

			sceneObject.SetBoneWorldTransform( i, transform );
			_previousTransforms[i] = transform;
		}
	}

	[Rpc.Broadcast]
	private void BroadcastBoneTransforms( byte[] data )
	{
		if ( !Renderer.IsValid() || !IsProxy )
			return;

		var model = Renderer.Model;
		if ( model is null )
			return;

		using var stream = new MemoryStream( data );
		using var reader = new BinaryReader( stream );

		_boneTransforms ??= new BoneTransform[model.BoneCount];
		_previousTransforms ??= new Transform[model.BoneCount];

		var initial = reader.ReadBoolean();

		for ( int i = 0; i < model.BoneCount; i++ )
		{
			var pos = new Vector3( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() );
			var rot = new Rotation( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() );

			_boneTransforms[i] = new BoneTransform(
				pos, 
				rot 
			);

			if ( initial ) _previousTransforms[i] = new Transform( pos, rot, 1f );
		}

		var sceneObject = Renderer.SceneModel;
		if ( sceneObject.IsValid() )
			UpdateBoneTransforms( sceneObject );
	}
}
