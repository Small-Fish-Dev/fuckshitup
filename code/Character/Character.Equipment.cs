namespace FUCKSHIT;

partial class Character
{
	[Sync]
	public NetDictionary<EquipmentSlot, Item> Equipment { get; set; } = new();

	public bool TryEquip( Item item, EquipmentSlot slot )
	{
		var isValid = (slot.IsHandSlot() && item.Holdable)
				   || item.Slot == slot;

		if ( !item.IsEquipment ) return false;
		if ( !isValid ) return false;
		if ( Equipment.TryGetValue( slot, out var slotEquipped ) && slotEquipped.IsValid() )
			return false;

		// Clear reference from the container this item is inside of.
		if ( item.Container.IsValid() )
		{
			if ( item.Container.TryFind( item, out var result ) )
				result.Box?.ClearReference( item );
		}

		Equipment[slot] = item;
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
		bool CheckSlot( EquipmentSlot slot )
		{
			if ( !Equipment.TryGetValue( slot, out var equipped ) ) 
				return false;
			return equipped == item;
		}

		if ( !item.IsEquipment ) 
			return false;

		var slot = default( EquipmentSlot );
		var result = CheckSlot( slot = item.Slot );

		if ( item.Holdable )
		{
			if ( !result ) result = CheckSlot( slot = EquipmentSlot.Primary );
			if ( !result ) result = CheckSlot( slot = EquipmentSlot.Secondary );
		}

		if ( !result ) 
			return false;

		Equipment[slot] = null;

		// Remove slot collections to our character's inventory.
		if ( item.IsContainer && item.GameObject.TryGetContainer( out var container ) )
		{
			Inventory.RemoveSlotCollections( container );
		}

		return true;
	}
}
