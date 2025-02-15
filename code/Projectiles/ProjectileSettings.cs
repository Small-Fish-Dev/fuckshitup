namespace FUCKSHIT;

public struct ProjectileSettings
{
	public Vector3 Velocity { get; set; }
	public float Count { get; set; } = 1;
	public RangedFloat VerticalSpread { get; set; } = new RangedFloat( 0f, 0f );
	public RangedFloat HorizontalSpread { get; set; } = new RangedFloat( 0f, 0f );
	public RangedFloat? DamageOverride { get; set; } = null;
	public Item Source { get; set; }

	public ProjectileSettings() { }
}
