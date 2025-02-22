namespace FUCKSHIT;

[Flags]
public enum ItemTypeFlags : ulong
{
	None,

	[Icon( "🍔" )]
	Consumable = 1 << 0,

	[Icon( "❤" )]
	Medical = 1 << 1,

	[Icon( "🔪" )]
	Weapon = 1 << 2,

	[Icon( "🎒" )]
	Container = 1 << 3,

	[Icon( "💲" )]
	Valuable = 1 << 4,

	[Icon( "📏" )]
	Large = 1 << 5,

	[Icon( "🎩" )]
	Wearable = 1 << 6,

	[Icon( "🛡" )]
	Protective = 1 << 7,

	[Icon( "⚙" )]
	Part = 1 << 8,

	[Hide]
	All = Consumable | Medical | Weapon | Container | Valuable | Large | Wearable | Protective | Part,
}

public partial class Item : Component
{
	[Property, Category( "Item" )]
	public string Name { get; set; }

	[Property, Category( "Item" )]
	public ItemTypeFlags TypeFlags { get; set; }

	/// <summary>
	/// How many items can we stack on 1, max stack of 0 means it isn't stackable.
	/// </summary>
	[Property, Category( "Item" ), Range( 1, 120, 1 )]
	public int MaxStack { get; set; } = 1;

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

	public Vector2Int Size => GetSize( Rotated );
	public bool Stackable => MaxStack > 1;

	[Sync]
	public bool Rotated { get; set; } = false;

	[Sync]
	public int Amount
	{
		get => _amount;
		set => _amount = value.Clamp( 0, MaxStack );
	}
	private int _amount = 1;

	public ModelRenderer Renderer { get; private set; }
	public Rigidbody Rigidbody { get; private set; }
	public Collider Collider { get; private set; }

	[Sync]
	public string PrefabSource { get; set; }

	protected override void OnAwake()
	{
		PrefabSource = GameObject.PrefabInstanceSource;
	}

	public Vector2Int GetSize( bool rotated = false ) 
		=> rotated ? new Vector2Int( _size.y, _size.x ) : _size; 

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
