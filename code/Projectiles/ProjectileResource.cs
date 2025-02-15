namespace FUCKSHIT;

[GameResource( 
	"Projecitle Resource", 
	"prjctle", 
	"Define all the charasteristics for a projectile.",
	Category = "FUCKSHIT", 
	Icon = "remove"
)]
public sealed class ProjectileResource : GameResource
{
	/// <summary>
	/// How much damage should this bullet deal depending on the velocity from 0, to beginning velocity.
	/// </summary>
	[Property, Category( "Generic" )]
	public RangedFloat DamageRange { get; set; } = new RangedFloat( 10f, 100f );

	/// <summary>
	/// How big is this bullet?
	/// </summary>
	[Property, Category( "Generic" )]
	public BulletCaliber Caliber { get; set; } = BulletCaliber._9mm;

	/// <summary>
	/// How strong should gravity be on this bullet?
	/// <para>1 is the default.</para>
	/// </summary>
	[Property, Category( "Generic" ), Range( 0f, 2f )]
	public float GravityMultiplier { get; set; } = 1f;

	/// <summary>
	/// Maximum bullet travel distance in meters.
	/// </summary>
	[Property, Category( "Generic" ), Range( 100f, 2000f )]
	public float TravelDistance { get; set; } = 600f;

	/// <summary>
	/// How well should this bullet penetrate walls and armour?
	/// </summary>
	[Property, Category( "Collision" ), Range( 0f, 1f )]
	public float Penetration { get; set; } = 0.1f;

	/// <summary>
	/// How much force should we lose if we collide?
	/// </summary>
	[Property, Category( "Collision" ), Range( 0f, 1f )]
	public float ForceLoss { get; set; } = 0.3f;

	/// <summary>
	/// How strong should the force of this bullet be?
	/// <para>1 is the default.</para>
	/// </summary>
	[Property, Category( "Collision" ), Range( 0f, 10f )]
	public float ForceMultiplier { get; set; } = 1f;

	/// <summary>
	/// What are the odds our bullet bouncess off of a surface?
	/// </summary>
	[Property, Category( "Ricochet" ), Range( 0f, 1f )]
	public float RicochetChance { get; set; } = 0.1f;

	/// <summary>
	/// What are the angles we allow ricochet to happen from?
	/// <para>0 is all angles allowed, 1 is 180 degrees.</para>
	/// </summary>
	[Property, Category( "Ricochet" ), Range( 0f, 1f )]
	public float RicochetThreshold { get; set; } = 0.15f;

	/// <summary>
	/// How many ricochets can we allow to happen per bullet?
	/// </summary>
	[Property, Category( "Ricochet" ), Range( 0f, 10f, 1f )]
	public int MaxRicochets { get; set; } = 2;
}
