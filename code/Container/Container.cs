namespace FUCKSHIT;

public sealed partial class Container 
	: Component
{
	[Property, Category( "Information" )]
	public string Name { get; set; }

	/// <summary>
	/// Look up all items this <see cref="Container"/> has inside of it.
	/// </summary>
	public IEnumerable<Item> Items => GameObject.Children
		.Select( gameObject =>
		{
			if ( gameObject.IsValid() )
				return gameObject.Components.Get<Item>( FindMode.EverythingInSelfAndChildren );

			return null;
		} );

	/// <summary>
	/// Read-only list of all <see cref="SlotCollection"/>s this container has.
	/// </summary>
	public IReadOnlyList<SlotCollection> SlotCollections => _slotCollections;
	private List<SlotCollection> _slotCollections = new();

	/// <summary>
	/// Add a slot collection to this <see cref="Container"/>.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="size"></param>
	public SlotCollection AddSlotCollection( string name, Vector2Int size )
	{
		Assert.True( !IsProxy, "Tried to add a slot collection to a container you don't have ownership of." );

		var slotCollection = new SlotCollection( this, size )
		{
			Name = name,
		};

		_slotCollections.Add( slotCollection );

		return slotCollection;
	}

	/// <summary>
	/// Add a <see cref="SlotCollection"/> to this <see cref="Container"/>.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	public SlotCollection AddSlotCollection( string name, int width, int height )
		=> AddSlotCollection( name, new Vector2Int( width, height ) );

	/// <summary>
	/// Add a <see cref="SlotCollection"/> to this <see cref="Container"/>.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="boxes"></param>
	public SlotCollection AddSlotCollection( string name, params SlotCollection.Box[] boxes )
	{
		Assert.True( !IsProxy, "Tried to add a slot collection to a container you don't have ownership of." );

		var slotCollection = new SlotCollection( this, boxes )
		{
			Name = name,
		};

		_slotCollections.Add( slotCollection );

		return slotCollection;
	}

	/// <summary>
	/// Add a <see cref="SlotCollection"/> directly through reference.
	/// </summary>
	/// <param name="slotCollection"></param>
	public SlotCollection AddSlotCollection( SlotCollection slotCollection )
	{
		Assert.True( slotCollection is not null, "Tried to add null SlotCollection to a Container." );
		Assert.True( !IsProxy, "Tried to add a slot collection to a container you don't have ownership of." );

		slotCollection.SetParent( this );
		_slotCollections.Add( slotCollection );

		return slotCollection;
	}

	/// <summary>
	/// Add <see cref="SlotCollection"/>s to this <see cref="Container"/> from another.
	/// </summary>
	/// <param name="container"></param>
	public void AddSlotCollections( Container container )
	{
		Assert.True( container.IsValid(), "Tried to add slot collections from an invalid container." );
		Assert.True( !IsProxy, "Tried to add slot collections to a container you don't have ownership of." );

		foreach ( var slot in container.SlotCollections )
			AddSlotCollection( slot );
	}

	/// <summary>
	/// Remove a slot collection from this <see cref="Container"/>.
	/// </summary>
	/// <param name="collection"></param>
	public void RemoveSlotCollection( SlotCollection collection )
	{
		Assert.True( collection is not null, "Tried to remove non-existing SlotCollection from Container." );
		Assert.True( _slotCollections.Contains( collection ), "Tried to remove SlotCollection that doesn't belong to this Container." );
		
		_slotCollections.Remove( collection );
	}

	/// <summary>
	/// Clears the whole <see cref="Container"/>, and optionally deletes all the items.
	/// </summary>
	/// <param name="deleteItems"></param>
	public void Clear( bool deleteItems = true )
	{
		Assert.True( !IsProxy, "Tried to clear a container you don't have ownership of." );
		
		if ( deleteItems )
			foreach ( var item in Items )
			{
				if ( !item.IsValid() || !item.GameObject.IsValid() )
					continue;

				item.GameObject.Destroy();
			}

		_slotCollections.Clear();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( !IsProxy ) Clear( true );
	}
}
