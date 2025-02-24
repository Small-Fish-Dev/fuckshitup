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
	/// <param name="filter"></param>
	/// <returns></returns>
	public bool TryFindSpace( Vector2Int size, out ContainerResult result, Func<SlotCollection, bool> filter = null )
	{
		foreach ( var collection in _slotCollections )
		{
			var pass = filter?.Invoke( collection ) ?? true;
			if ( !pass ) continue;

			if ( TryFindCollectionSpace( collection, size, out result ) )
				return true;
		}

		result = default;
		return false;
	}

	/// <summary>
	/// Try to find this item's reference inside of this container.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public bool TryFind( Item item, out ContainerResult result )
	{
		result = (null, null, default, false);

		if ( item is null )
			return false;

		foreach ( var collection in _slotCollections )
		{
			var position = default( Vector2Int );
			var box = collection.Boxes.FirstOrDefault( box => box.TryGetPosition( item, out position ) );
			if ( box is null )
				continue;

			result.Position = position;
			result.Box = box;
			result.Collection = collection;
		}

		return result.Box is not null;
	}

	/// <summary>
	/// Try to find an item inside of this container that fullfils this predicate.
	/// </summary>
	/// <param name="predicate"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public bool TryFind( Func<Item, bool> predicate, out ContainerResult result )
	{
		result = (null, null, default, false);

		if ( predicate is null )
			return false;

		foreach ( var collection in _slotCollections )
		{
			var position = default( Vector2Int );
			var box = collection.Boxes.FirstOrDefault( box => box.TryFind( predicate, out position ) );
			if ( box is null )
				continue;

			result.Position = position;
			result.Box = box;
			result.Collection = collection;
		}

		return result.Box is not null;
	}
}
