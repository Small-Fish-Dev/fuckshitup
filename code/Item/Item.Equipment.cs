﻿namespace FUCKSHIT;

partial class Item
{
	[Property, FeatureEnabled( "Equipment" )]
	public bool IsEquipment { get; set; } = false;

	[Property, Feature( "Equipment" )]
	public bool Holdable { get; set; }

	[Property, Feature( "Equipment" )]
	public EquipmentSlot Slot { get; set; }
}
