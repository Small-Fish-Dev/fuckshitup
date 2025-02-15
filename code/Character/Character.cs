namespace FUCKSHIT;

public sealed partial class Character : Pawn
{
	/// <summary>
	/// The local player...
	/// </summary>
	public static Character Local => Client.Local.IsValid()
		? Client.Local.Pawn as Character
		: null;

	public bool Ragdolled
	{
		get => RagdollController.IsValid() && RagdollController.MotionEnabled;
		set
		{
			// Reset renderer local transform.
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
		}
	}

	public SkinnedModelRenderer Renderer { get; private set; }
	public BoxCollider Collider { get; private set; }
	public CameraComponent Camera { get; private set; }
	public ShrimpleCharacterController Controller { get; private set; }
	public RagdollController RagdollController { get; private set; }
	public Container Inventory { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndChildren );
		
		RagdollController = Components.Get<RagdollController>( FindMode.EverythingInSelfAndChildren );

		Collider = Components.Get<BoxCollider>( FindMode.EverythingInSelfAndChildren );

		Controller = Components.Get<ShrimpleCharacterController>( FindMode.EverythingInSelfAndChildren );

		Camera = Components.Get<CameraComponent>( FindMode.EverythingInSelfAndChildren );
		Camera.Enabled = !IsProxy;

		Inventory = Components.Get<Container>( FindMode.EverythingInSelfAndChildren );

		if ( !IsProxy ) RequestRespawn();
		/*Inventory
			.AddSlotCollection( "Tactical Rig", [
				new SlotCollection.Box( 1, 2 ),
				new SlotCollection.Box( 1, 2 ),
				new SlotCollection.Box( 1, 2 ),
				new SlotCollection.Box( 1, 2 ),
				new SlotCollection.Box( 1, 2 ),

				new SlotCollection.Box( 1, 1, sameLine: false ),
				new SlotCollection.Box( 1, 1 ),
				new SlotCollection.Box( 1, 1 ),
				new SlotCollection.Box( 1, 1 ),
				new SlotCollection.Box( 1, 1 ),

				new SlotCollection.Box( 2, 2, margin: Vector2.Left * 30.5f, sameLine: false ),
				new SlotCollection.Box( 2, 2 ),
			] )
			.WithSource( Scene.GetAllComponents<Item>().FirstOrDefault() );

		var source = Scene.GetAllComponents<Item>().FirstOrDefault( item => item is Equipment equipment && equipment.IsContainer && equipment.Network.TakeOwnership() );
		if ( !IsProxy && source.IsValid() && source.GameObject.TryGetContainer( out var container ) )
		{
			source.GameObject.Network.TakeOwnership();
			Inventory.AddSlotCollections( container );
		}

		var components = Scene.GetAllComponents<Item>();
		foreach ( var item in components )
			Inventory.TryInsertItem( item );*/
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsProxy )
		{
			SimulateMovement();
			HandleGrabbing();

			if ( Input.Pressed( InputAction.RELOAD ) )
				Ragdolled = !Ragdolled;

			if ( Input.Pressed( InputAction.LEFT_MOUSE ) )
			{
				var resource = ResourceLibrary.GetAll<ProjectileResource>().FirstOrDefault();
				if ( resource is not null )
					Projectile.Launch(
						Camera.WorldPosition + Camera.WorldRotation.Forward * 20f,
						resource,
						new ProjectileSettings
						{
							Count = 25,
							HorizontalSpread = new RangedFloat( -1f, 1f ),
							VerticalSpread = new RangedFloat( -1f, 1f ),
							Velocity = Camera.WorldRotation.Forward * 3000f
						} 
					);
			}
		}
	}

	protected override void OnUpdate()
	{		
		SimulateAnimations();
	}

	protected override void OnPreRender()
	{
		if ( !IsProxy )
		{
			SimulateCamera();
			SimulateView();
		}
	}
}
