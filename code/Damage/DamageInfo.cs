namespace FUCKSHIT;

public struct DamageInfo
{
	public int Amount { get; set; }
	public Limb Limb { get; set; }
	public bool IsHealing => Amount < 0;
}
