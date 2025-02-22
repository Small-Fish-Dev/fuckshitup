namespace FUCKSHIT;

public enum EquipmentSlot
{
	None = 0,
	Headwear,
	Face,
	Body,
	Gear,
	Legs,
	Primary,
	Secondary
}

public static class EquipmentSlotExtensions
{
	public static bool IsHandSlot( this EquipmentSlot slot )
		=> slot is EquipmentSlot.Primary or EquipmentSlot.Secondary;
}
