namespace FUCKSHIT;

partial class Container
{
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
