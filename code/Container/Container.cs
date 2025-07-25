﻿namespace FUCKSHIT;

public sealed partial class Container 
	: Component
{
	[Property, Sync, Category( "Information" )]
	public string Name { get; set; }

	[Property, Sync, Category( "Information" )]
	public bool Networked { get; set; } = true;

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
		RefreshOrdering();

		return slotCollection;
	}

	/// <summary>
	/// Add a <see cref="SlotCollection"/> directly through reference.
	/// </summary>
	/// <param name="slotCollection"></param>
	/// <param name="refresh"></param>
	public SlotCollection AddSlotCollection( SlotCollection slotCollection, bool refresh = true )
	{
		Assert.True( slotCollection is not null, "Tried to add null SlotCollection to a Container." );
		Assert.True( !IsProxy, "Tried to add a slot collection to a container you don't have ownership of." );

		slotCollection.SetParent( this );
		_slotCollections.Add( slotCollection );
		if ( refresh ) RefreshOrdering();

		return slotCollection;
	}

	/// <summary>
	/// Add <see cref="SlotCollection"/>s to this <see cref="Container"/> from another.
	/// </summary>
	/// <param name="container"></param>
	/// <param name="source"></param>
	public void AddSlotCollections( Container container, Item source = null )
	{
		Assert.True( container.IsValid(), "Tried to add slot collections from an invalid container." );
		Assert.True( !IsProxy, "Tried to add slot collections to a container you don't have ownership of." );

		foreach ( var slot in container.SlotCollections )
		{
			var collection = AddSlotCollection( slot, false );
			if ( source is not null ) 
				collection = collection.WithSource( source );
		}

		RefreshOrdering();
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
		RefreshOrdering();
	}

	/// <summary>
	/// Remove <see cref="SlotCollection"/>s of another <see cref="Container"/> from this <see cref="Container"/>.
	/// </summary>
	/// <param name="container"></param>
	public void RemoveSlotCollections( Container container )
	{
		Assert.True( container.IsValid(), "Tried to add slot collections from an invalid container." );
		Assert.True( !IsProxy, "Tried to add slot collections to a container you don't have ownership of." );

		var count = _slotCollections.Count;
		for ( int i = 0; i < count; i++ )
		{
			var collection = _slotCollections.ElementAtOrDefault( i );
			if ( container.SlotCollections.Contains( collection ) )
				_slotCollections.Remove( collection );
		}

		RefreshOrdering();
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

	private void RefreshOrdering()
	{
		var count = _slotCollections.Count;
		var collections = _slotCollections.OrderBy( collection => collection?.Order ?? 0 );

		var temp = new List<SlotCollection>( count );
		for ( int i = 0; i < count; i++ )
		{
			var collection = collections.ElementAtOrDefault( i );
			if ( collection is null ) continue;

			temp.Add( collection );
		}

		_slotCollections = temp;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( !IsProxy ) Clear( true );
	}

	public override string ToString()
	{
		return $"Container {Name} - Collections: {_slotCollections.Count}";
	}
}
