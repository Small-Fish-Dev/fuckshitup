namespace FUCKSHIT;

partial class Container
{
	public void TryTake( Container target, Item item, SlotCollection.Box box, Vector2Int position, bool rotated )
	{
		if ( !item.IsValid() ) return;
		if ( !target.IsValid() ) return;

		if ( box is null ) return;
		if ( !box.CanFit( position, item.GetSize( rotated ), item ) )
			return;

		TakeItem( item );

		item.Rotated = rotated;
		box.StoreReference( position, item );
		item.SetContainer( target );
	}

	[Rpc.Owner( NetFlags.Reliable )]
	private void TakeItem( Item item )
	{
		if ( !item.IsValid() || !item.Network.Active || item.IsProxy ) return;

		if ( !TryFind( item, out var result ) )
			return;

		result.Box.ClearReference( result.Position );
		item.Network.AssignOwnership( Rpc.Caller );
	}
}
