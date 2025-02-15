global using ContainerResult = (FUCKSHIT.SlotCollection Collection, FUCKSHIT.SlotCollection.Box Box, Vector2Int Position, bool Rotate);

namespace FUCKSHIT;

partial class Container
{
	/// <summary>
	/// Find space from anywhere within our <see cref="Container"/>'s <see cref="SlotCollection"/>.
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="size"></param>
	/// <param name="result"></param>
	/// <param name="allowRotate"></param>
	/// <returns></returns>
	public bool TryFindCollectionSpace( SlotCollection collection, Vector2Int size, out ContainerResult result, bool allowRotate = true )
	{
		Assert.True( collection is not null, "Tried to remove non-existing SlotCollection from Container." );
		Assert.True( _slotCollections.Contains( collection ), "Tried to remove SlotCollection that doesn't belong to this Container." );
		Assert.True( size.x >= 1 && size.y >= 1, "Tried to find space for size that is < 1." );

		foreach ( var box in collection.Boxes )
			if ( box.TryFindSpace( size, out var position ) )
			{
				result = (collection, box, position, false);
				return true;
			}

		// Look for rotated space?
		if ( allowRotate && size.x != size.y )
		{
			var rotatedSize = new Vector2Int( size.y, size.x );
			foreach ( var box in collection.Boxes )
				if ( box.TryFindSpace( rotatedSize, out var position ) )
				{
					result = (collection, box, position, true);
					return true;
				}
		}

		result = default;
		return false;
	}

	/// <summary>
	/// Find space from anywhere within our <see cref="Container"/>.
	/// </summary>
	/// <param name="size"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public bool TryFindSpace( Vector2Int size, out ContainerResult result )
	{
		foreach ( var collection in _slotCollections )
			if ( TryFindCollectionSpace( collection, size, out result ) )
				return true;

		result = default;
		return false;
	}

	/// <summary>
	/// Insert an item into our <see cref="Container"/>, do note that this method will throw an exception if something doesn't go right.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	/// <exception cref="Exception"/>
	public bool InsertItem( Item item )
	{
		Assert.True( !IsProxy, "Tried to place an Item inside of a Container you don't own." );
		Assert.True( item.IsValid(), "Can't place an invalid Item into a Container." );
		Assert.True( item.Network.Active, "Can't place a non-networked Item into a Container." );

		if ( !item.Network.IsOwner )
		{
			var canTakeOwnership = item.Network.Owner is null;
			Assert.True( canTakeOwnership, "Can't take ownership of this Item, so we can't place it into the Container." );
		}

		if ( !TryFindSpace( item.AbsoluteSize, out var result ) )
			return false;

		item.Network.TakeOwnership();
		result.Box.StoreReference( result.Position, item );
		item.Rotated = result.Rotate;
		item.SetContainer( this );

		return true;
	}

	/// <summary>
	/// Let's try to insert an item into our <see cref="Container"/>.
	/// <para>Uses <see cref="InsertItem(Item)"/> internally.</para>
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool TryInsertItem( Item item )
	{
		try
		{
			var result = InsertItem( item );
			return result;
		}
		catch { return false; }
	}
}
