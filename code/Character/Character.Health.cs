namespace FUCKSHIT;

partial class Character
{
	[Property, Category( "Health" )]
	public Dictionary<Limb, int> MaxHealth { get; set; }

	[Sync]
	public NetDictionary<Limb, int> Health { get; set; } = new();

	/// <summary>
	/// The current sum of all limbs.
	/// </summary>
	public int TotalHealth => Health?.Sum( x => x.Value ) ?? 0;

	/// <summary>
	/// The sum of all limbs' max health.
	/// </summary>
	public int TotalMaxHealth => MaxHealth?.Sum( x => x.Value ) ?? 0;

	/// <summary>
	/// Is our character alive?
	/// <para>Health must be at least 10 and head can't be blacked out.</para>
	/// </summary>
	public bool IsAlive => TotalHealth >= 10 && GetLimb( Limb.Head ).Value > 0;

	/// <summary>
	/// Get current and max health of a limb safely.
	/// </summary>
	/// <param name="limb"></param>
	/// <returns></returns>
	public (int Value, int Max) GetLimb( Limb limb )
	{
		var result = (Value: 0, Max: 0);
		Health.TryGetValue( limb, out result.Value );
		MaxHealth.TryGetValue( limb, out result.Max );
		return result;
	}

	/// <summary>
	/// Set current health of a max limb safely.
	/// <para>NOTE: Should only be called on owner of character.</para>
	/// </summary>
	/// <param name="limb"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public int SetLimb( Limb limb, int value )
	{
		if ( !Health.TryGetValue( limb, out _ ) )
			return 0;

		var limbStatus = GetLimb( limb );
		Health[limb] = Math.Clamp( value, 0, limbStatus.Max );
		return limbStatus.Value - Health[limb];
	}

	/// <summary>
	/// Try to damage a limb, get the remainder of damage to spread if couldn't do it all.
	/// <para>NOTE: Should only be called on owner of character.</para>
	/// </summary>
	/// <param name="damageInfo"></param>
	/// <returns></returns>
	public int TryDamageLimb( DamageInfo damageInfo )
	{
		var limbStatus = GetLimb( damageInfo.Limb );
		var amount = damageInfo.Amount;
		var damage = Math.Min( amount, limbStatus.Value );
		if ( damage == 0 ) return amount;

		Log.Info( $"Took {damage} damage to {damageInfo.Limb} [{limbStatus.Value - damage}/{limbStatus.Max}]." );

		var change = SetLimb( damageInfo.Limb, limbStatus.Value - damage );
		amount -= damage;

		return amount;
	}

	/// <summary>
	/// Spread damage within the body if a limb is already blacked out.
	/// <para>NOTE: Should only be called on owner of character.</para>
	/// </summary>
	/// <param name="damageInfo"></param>
	public void SpreadDamage( DamageInfo damageInfo )
	{
		var remainder = TryDamageLimb( damageInfo );
		if ( remainder > 0 )
		{
			var limbs = Health.Where( kvp => kvp.Value > 0 );
			var count = limbs.Count();
			if ( count <= 0 ) return;

			var index = Game.Random.Int( 0, count - 1 );
			var limbData = limbs.ElementAtOrDefault( index );

			SpreadDamage( damageInfo with
			{
				Amount = remainder,
				Limb = limbData.Key
			} );
		}
	}

	/// <summary>
	/// Tell this character to respawn themselves.
	/// <para>This will only be called for the owner of this character.</para>
	/// </summary>
	[Rpc.Owner]
	public void RequestRespawn()
	{
		LocalRagdolled = false;
		WorldTransform = global::Transform.Zero;

		if ( MaxHealth != null && Health != null )
		{
			foreach ( var (limb, max) in MaxHealth )
				Health[limb] = max;
		}

		// todo: clear status effects if there are any

		InitializeInventory();
	}

	/// <summary>
	/// Request this character to take damage.
	/// <para>This will only be called for the owner of this character.</para>
	/// </summary>
	/// <param name="damageInfo"></param>
	[Rpc.Owner]
	public void RequestTakeDamage( DamageInfo damageInfo )
	{
		if ( !IsAlive || damageInfo.IsHealing ) return;

		SpreadDamage( damageInfo );
		if ( !IsAlive ) Log.Info( "Died." );
	}
}
