namespace FUCKSHIT;

public partial class Item : Component
{
	[Property, Category( "Item" )]
	public string Name { get; set; }

	[Property, Category( "Item" )]
	public IconSettings IconSettings { get; set; }

	[Property, Category( "Item" )]
	public ItemRarity Rarity { get; set; }

	[Property, Category( "Item" )]
	public Vector2Int AbsoluteSize
	{
		get => _size;
		set => _size = value.ComponentMax( Vector2Int.One );
	}
	private Vector2Int _size = Vector2Int.One;

	public Vector2Int Size
	{
		get => Rotated ? new Vector2Int( _size.y, _size.x ) : _size;
	}

	[Sync]
	public bool Rotated { get; set; } = false;

	public ModelRenderer Renderer { get; private set; }
	public Rigidbody Rigidbody { get; private set; }
	public Collider Collider { get; private set; }

	[Sync]
	public string PrefabSource { get; set; }

	protected override void OnAwake()
	{
		PrefabSource = GameObject.PrefabInstanceSource;
	}

	protected override void OnStart()
	{
		Renderer = Components.Get<ModelRenderer>( FindMode.EverythingInSelfAndChildren );
		Collider = Components.Get<BoxCollider>( FindMode.EverythingInSelfAndChildren );
		Rigidbody = Components.Get<Rigidbody>( FindMode.EverythingInSelfAndChildren );

		if ( !Network.Active )
		{
			GameObject.SetupNetworking( transfer: OwnerTransfer.Request );
		}

		SetupContainer();
	}
}
