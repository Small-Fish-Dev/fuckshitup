global using ContainerResult = (FUCKSHIT.SlotCollection Collection, FUCKSHIT.SlotCollection.Box Box, Vector2Int Position, bool Rotate);

namespace FUCKSHIT;

partial class Container
{
	/// <summary>
	/// Insert an <see cref="Item"/> into our <see cref="Container"/>, do note that this method will throw an exception if something doesn't go right.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	/// <exception cref="Exception"/>
	public bool Insert( Item item )
	{
		Assert.True( !IsProxy, "Tried to place an Item inside of a Container you don't own." );
		Assert.True( item.IsValid(), "Can't place an invalid Item into a Container." );
		Assert.True( item.Network.Active, "Can't place a non-networked Item into a Container." );

		if ( item.Inventory == this ) return false;

		if ( !item.Network.IsOwner )
		{
			var canTakeOwnership = item.Network.Owner is null;
			Assert.True( canTakeOwnership, "Can't take ownership of this Item, so we can't place it into the Container." );
		}

		// Find a possible stack...
		if ( item.Stackable )
		{
			bool FindStacks()
			{
				if ( item.Amount <= 0 ) return true;

				if ( !TryFind( i => i.PrefabSource == item.PrefabSource && i.Amount < i.MaxStack, out var result ) )
					return false;

				var reference = result.Box.References[result.Position];
				var add = Math.Min( item.Amount, reference.MaxStack - reference.Amount );
				reference.Amount += add;
				item.Amount -= add;

				return FindStacks();
			}

			var destroy = FindStacks();
			if ( destroy )
			{
				item.Network.TakeOwnership();
				item.DestroyGameObject();
				return true;
			}
		}

		// Otherwise just try to add it in.
		if ( !TryFindSpace(
			item.AbsoluteSize, 
			out var result, 
			( collection ) => collection.PassesFilter( item ) 
		) ) return false;

		item.Network.TakeOwnership();
		result.Box.StoreReference( result.Position, item );
		item.Rotated = result.Rotate;
		item.SetContainer( this );

		return true;
	}

	/// <summary>
	/// Let's try to insert an <see cref="Item"/> into our <see cref="Container"/>.
	/// <para>Uses <see cref="Insert(Item)"/> internally.</para>
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool TryInsert( Item item )
	{
		try
		{
			var result = Insert( item );
			return result;
		}
		catch { return false; }
	}

	/// <summary>
	/// Drop an <see cref="Item"/> from this <see cref="Container"/>.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool Drop( Item item )
	{
		if ( !TryFind( item, out var result ) )
			return false;

		// todo: drop logic
		item.State = ItemState.InWorld;
		item.SetContainer( null );
		result.Box?.ClearReference( result.Position );

		return true;
	}

	/// <summary>
	/// Attempt to take and move an <see cref="Item"/> from this <see cref="Container"/> to another.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="item"></param>
	/// <param name="box"></param>
	/// <param name="position"></param>
	/// <param name="rotated"></param>
	/// <param name="merge"></param>
	public void TryTake( Container target, Item item, SlotCollection.Box box, Vector2Int position, bool rotated, Item merge = null )
	{
		if ( !item.IsValid() ) return;
		if ( !target.IsValid() ) return;

		if ( box is null ) return;

		// Try to stack item if we are trying to place it on a matching item stack...
		if ( merge.IsValid() && item.Stackable
		  && merge.PrefabSource == item.PrefabSource
		  && merge.Amount < merge.MaxStack )
		{
			var add = Math.Min( item.Amount, merge.MaxStack - merge.Amount );
			merge.Amount += add;

			var amount = item.Amount - add;
			SetAmount( item, amount );
			if ( amount <= 0 )
			{
				ClearItem( item );
				TakeOwnership( item );
				item.DestroyGameObject();
			}

			return;
		}

		if ( !box.CanFit( position, item.GetSize( rotated ), item ) )
			return;

		ClearItem( item );
		TakeOwnership( item );

		item.Rotated = rotated;
		box.StoreReference( position, item );
		item.SetContainer( target );
	}

	[Rpc.Owner( NetFlags.Reliable )]
	private void ClearItem( Item item )
	{
		if ( !item.IsValid() || !item.Network.Active || item.IsProxy ) return;

		if ( !TryFind( item, out var result ) )
			return;

		result.Box.ClearReference( result.Position );
	}

	[Rpc.Owner( NetFlags.Reliable )]
	private void TakeOwnership( Item item )
	{
		if ( !item.IsValid() || !item.Network.Active || item.IsProxy ) return;

		item.Network.AssignOwnership( Rpc.Caller );
	}


	[Rpc.Owner( NetFlags.Reliable )]
	private void SetAmount( Item item, int amount )
	{
		if ( !item.IsValid() || !item.Network.Active || item.IsProxy ) return;

		item.Amount = amount;
	}
}
