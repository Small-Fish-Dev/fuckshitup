namespace FUCKSHIT;

public sealed partial class Character : Pawn
{
	public static Character Local => Client.Local.IsValid()
		? Client.Local.Pawn as Character
		: null;

	public SkinnedModelRenderer Renderer { get; private set; }
	public BoxCollider Collider { get; private set; }
	public CameraComponent Camera { get; private set; }
	public ShrimpleCharacterController Controller { get; private set; }
	public RagdollController RagdollController { get; private set; }
	public Container Inventory { get; private set; }
	public Voice Voice { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndChildren );
		if ( Renderer.IsValid() )
			Renderer.RenderType = ModelRenderer.ShadowRenderType.On;

		RagdollController = Components.Get<RagdollController>( FindMode.EverythingInSelfAndChildren );
		if ( RagdollController.IsValid() )
			RagdollController.Enabled = false;

		Collider = Components.Get<BoxCollider>( FindMode.EverythingInSelfAndChildren );

		Controller = Components.Get<ShrimpleCharacterController>( FindMode.EverythingInSelfAndChildren );

		Camera = Components.Get<CameraComponent>( FindMode.EverythingInSelfAndChildren );
		if ( Camera.IsValid() )
			Camera.Enabled = !IsProxy;

		Voice = Components.Get<Voice>( FindMode.EverythingInSelfAndChildren );
		if ( Voice.IsValid() )
			Voice.Enabled = !IsProxy;

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
			.WithSource( Scene.GetAllComponents<Item>().FirstOrDefault() );*/

		var equippables = Scene.GetAllComponents<Item>().Where( item => item.IsEquipment && item.IsContainer && item.Network.TakeOwnership() ).ToList();
		
		if ( !IsProxy && equippables is { Count: > 0 } )
			foreach ( var source in equippables )
			{
				TryEquip( source, source.Slot );
			}

		var components = Scene.GetAllComponents<Item>();
		foreach ( var item in components )
			if ( !item.IsEquipment )
				Inventory.TryInsert( item );
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsProxy )
		{
			SimulateMovement();
			HandleGrabbing();

			if ( Input.Pressed( InputAction.RELOAD ) && !Mouse.Visible )
				LocalRagdolled = !LocalRagdolled;

			if ( Input.Pressed( InputAction.LEFT_MOUSE ) )
			{
				var resource = ResourceLibrary.GetAll<ProjectileResource>().FirstOrDefault();
				if ( resource is not null )
					Projectile.Launch(
						EyeRay.Position + EyeForward * 20f,
						resource,
						new ProjectileSettings
						{
							Count = 25,
							HorizontalSpread = new RangedFloat( -1f, 1f ),
							VerticalSpread = new RangedFloat( -1f, 1f ),
							Velocity = EyeForward * 3000f
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

		SimulateRagdoll();
	}

	private void InitializeInventory()
	{
		if ( !Inventory.IsValid() )
			return;

		Inventory.Name = $"{Client.Name}";
		Inventory.Clear();
		Inventory.AddSlotCollection( "Pockets", [
			new SlotCollection.Box( 1, 1 ),
			new SlotCollection.Box( 1, 2 ),
			new SlotCollection.Box( 1, 2 ),
			new SlotCollection.Box( 1, 1 ),
		] );
	}
}
