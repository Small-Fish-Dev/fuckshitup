namespace FUCKSHIT;

[Flags]
public enum DamageFlags : byte
{
	None,

	/// <summary>
	/// Damage type for something like explosions.
	/// <para>This causes concussions and ear ringing more commonly.</para>
	/// </summary>
	[Icon( "💣" )]
	Force = 1 << 0,

	/// <summary>
	/// Damage type for something like knives, swords...
	/// <para>These cause bleeding more commonly.</para>
	/// </summary>
	[Icon( "🔪" )]
	Sharp = 1 << 1,

	/// <summary>
	/// Damage type for blunt objects like baseball bats, fists...
	/// <para>These cause broken bones and concussions more commonly.</para>
	/// </summary>
	[Icon( "🔪" )]
	Blunt = 1 << 2,

	/// <summary>
	/// Damage type for bullets.
	/// <para>These can cause bleeding, broken bones and concussions.</para>
	/// </summary>
	[Icon( "🔫" )]
	Bullet = 1 << 3,
}

public struct DamageInfo
{
	public int Amount { get; set; }
	public Limb Limb { get; set; }
	public DamageFlags Type { get; set; } = DamageFlags.Force;
	public bool IsHealing => Amount < 0;

	public DamageInfo() { }
}
