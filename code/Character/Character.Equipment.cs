namespace FUCKSHIT;

partial class Character
{
	[Sync]
	public NetDictionary<EquipmentSlot, Item> Equipment { get; set; } = new();

	public bool TryEquip( Item item )
	{
		if ( !item.IsEquipment ) return false;
		if ( Equipment.TryGetValue( item.Slot, out _ ) )
			return false;

		// Clear reference from the container this item is inside of.
		if ( item.Container.IsValid() )
		{
			if ( item.Container.TryFind( item, out var result ) )
				result.Box?.ClearReference( item );
		}

		Equipment[item.Slot] = item;
		item.SetParentObject( GameObject );
		item.State = ItemState.Equipped;

		// Add slot collections to our character's inventory.
		if ( item.IsContainer && item.GameObject.TryGetContainer( out var container ) )
		{
			Inventory.AddSlotCollections( container, item );
		}

		return true;
	}

	public bool TryUnequip( Item item )
	{
		if ( !item.IsEquipment ) return false;
		if ( !Equipment.TryGetValue( item.Slot, out var equipped ) )
			return false;

		if ( equipped != item )
			return false;

		Equipment[item.Slot] = null;

		// Remove slot collections to our character's inventory.
		if ( item.IsContainer && item.GameObject.TryGetContainer( out var container ) )
		{
			Inventory.RemoveSlotCollections( container );
		}

		return true;
	}
}
