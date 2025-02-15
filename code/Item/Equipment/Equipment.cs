namespace FUCKSHIT;

public partial class Equipment : Item
{
	[Property, Category( "Equipment" )]
	public EquipmentSlot Slot { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		SetupContainer();
	}
}
