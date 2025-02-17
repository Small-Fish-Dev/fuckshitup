namespace FUCKSHIT;

public sealed class Projectile : Component
{
	private const float INCH_TO_METER = 0.0254f;
	public const float GRAVITY = 400f;

	public ProjectileResource Resource { get; private set; }
	public ProjectileSettings Settings { get; private set; }

	public Vector3 StartVelocity { get; private set; }
	public Vector3 Velocity { get; private set; }
	public float DistanceTravelled { get; private set; }
	public int Ricochets { get; private set; }

	private LineRenderer LineRenderer { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		StartVelocity = Velocity;

		// todo: Shitty line renderer....
		LineRenderer = Components.GetOrCreate<LineRenderer>();
		LineRenderer.StartCap = SceneLineObject.CapStyle.Rounded;
		LineRenderer.UseVectorPoints = true;
		LineRenderer.Color = new Gradient(
			new Gradient.ColorFrame( 0f, Color.Orange.WithAlpha( 0f ) ),
			new Gradient.ColorFrame( 0.8f, Color.Orange.WithAlpha( 0.5f ) ),
			new Gradient.ColorFrame( 0.9f, Color.Orange ),
			new Gradient.ColorFrame( 1f, Color.Red )
		);
		LineRenderer.Width = 0.2f;
		LineRenderer.VectorPoints = new();
		LineRenderer.EndCap = SceneLineObject.CapStyle.Triangle;
	}

	public int CalculateDamage()
	{
		// Calculate damage depending on the amount of velocity lost.
		var min = Settings.DamageOverride?.Min ?? Resource.DamageRange.Min;
		var max = Settings.DamageOverride?.Max ?? Resource.DamageRange.Max;
		var damage = MathX.Lerp( min, max, Velocity.Length / StartVelocity.Length );

		return MathX.CeilToInt( damage );
	}

	private bool TryRicochet( SceneTraceResult traceResult )
	{
		// We can't ricochet more than the maximum amount!
		if ( Ricochets >= Resource.MaxRicochets )
			return false;

		// We want to avoid the bullet from bouncing in directions 
		var dot = 1f - MathF.Abs( traceResult.Direction.Dot( traceResult.Normal ) );
		if ( dot < Resource.RicochetThreshold ) return false;

		// Let's reflect our velocity!
		var reflected = Vector3.Reflect( traceResult.Direction, traceResult.Normal );
		Velocity = reflected * Velocity.Length;
		Ricochets++;

		return true;
	}

	private bool TryPenetrate( SceneTraceResult traceResult )
	{
		// TODO!
		const float MAX_PENETARTION = 2f; // Let's only penetrate a maximum of 2 meters?

		// Trace from the penetration point to find the exit.
		var ray = new Ray( traceResult.EndPosition + traceResult.Direction, traceResult.Direction );
		var penetration = Scene.Trace.Ray( ray, MAX_PENETARTION * INCH_TO_METER )
			.Run();

		// We are inside of an object.
		WorldPosition = penetration.Hit
			? penetration.HitPosition + penetration.Direction
			: penetration.StartPosition;

		return true;
	}

	private void OnHit( SceneTraceResult traceResult )
	{
		// Penetration?
		var shouldPenetrate = false; // todo.. this should compare to something real
		if ( shouldPenetrate && TryPenetrate( traceResult ) )
			return;

		// Apply some forces to the body that we hit?
		var body = traceResult.Body;
		if ( body.IsValid() )
		{
			var force = traceResult.Direction * Velocity.Length * Resource.ForceMultiplier;
			body.ApplyImpulseAt( traceResult.EndPosition, force );
		}

		// Check if we hit something we can damage?
		var hitbox = traceResult.Hitbox;
		if ( hitbox is not null && hitbox.GameObject.IsValid() )
		{
			var character = hitbox.GameObject.Components.Get<Character>( FindMode.EverythingInSelfAndParent );
			if ( character.IsValid() )
				character.RequestTakeDamage( new DamageInfo
				{
					Amount = CalculateDamage(),
					Limb = LimbExtensions.GetByTag( hitbox.Tags.FirstOrDefault() )
				} );
		}
		else
		{
			// Lose some velocity due to collision.
			Velocity *= 1f - Resource.ForceLoss;

			// Roll chance to ricochet and try ricochet.
			var ricochetProbability = Game.Random.NextSingle();
			if ( ricochetProbability < Resource.RicochetChance && TryRicochet( traceResult ) )
				return;
		}

		// We hit and stopped.
		GameObject.Destroy();
	}

	private void OnFizzle()
	{
		GameObject.Destroy();
	}
	
	private void OnTravel( SceneTraceResult traceResult )
	{
		// todo: Shitty line renderer....
		if ( LineRenderer.VectorPoints.Count > 4 )
			LineRenderer.VectorPoints.RemoveAt( 0 );

		LineRenderer.VectorPoints.Add( WorldPosition );
	}

	private void Travel( float time )
	{
		var ray = new Ray( WorldPosition, Velocity.Normal );
		var distance = Velocity.Length * time;
		var traceResult = Scene.Trace.Ray( ray, distance )
			.UseHitboxes()
			.WithoutTags( "hitbox_ignore" )
			.Run();

		DistanceTravelled += distance * INCH_TO_METER;
		WorldPosition = traceResult.EndPosition;

		// We actually hit something..
		if ( traceResult.Hit )
		{
			OnHit( traceResult );
			return;
		}

		// What now? We didn't hit anything?
		OnTravel( traceResult );
	}

	protected override void OnFixedUpdate()
	{
		if ( !GameObject.IsValid() ) return;
		if ( !Resource.IsValid() )
		{
			GameObject.Destroy();
			return;
		}

		// We have travelled as much as we can!
		if ( DistanceTravelled >= Resource.TravelDistance || Velocity.IsNearZeroLength )
		{
			OnFizzle();
			return;
		}

		// Gravity and movement logic.
		Velocity += GRAVITY * Vector3.Down * Resource.GravityMultiplier * Time.Delta;
		Travel( Time.Delta );
	}

	/// <summary>
	/// Launch projectile(s) with settings and a resource.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="resource"></param>
	/// <param name="settings"></param>
	public static bool Launch( Vector3 position, ProjectileResource resource, ProjectileSettings settings = default )
	{
		var scene = Game.ActiveScene;
		if ( !resource.IsValid() || !scene.IsValid() || scene.IsEditor ) return false;

		var seed = Game.Random.Int( 0, int.MaxValue - 1 );
		BroadcastProjectile( position, resource, settings, seed );

		return true;
	}

	[Rpc.Broadcast]
	private static void BroadcastProjectile( Vector3 position, ProjectileResource resource, ProjectileSettings settings, int seed )
	{
		if ( !resource.IsValid() ) return;
		if ( !Game.ActiveScene.IsValid() ) return;

		using ( Game.ActiveScene.Push() )
		{
			var rotation = Rotation.LookAt( settings.Velocity.Normal );
			var length = settings.Velocity.Length;

			void SpawnProjectile( float spreadX, float spreadY )
			{
				var gameObject = new GameObject();
				gameObject.Name = resource.ResourceName;

				var projectile = gameObject.Components.Create<Projectile>();
				projectile.Resource = resource;
				projectile.Settings = settings;
				projectile.WorldPosition = position;

				// Set velocity by spread.
				var spread = Rotation.FromYaw( spreadX ) * Rotation.FromPitch( spreadY );
				projectile.Velocity = (rotation * spread).Forward * length;
			}

			// Spawn all projectiles using our random spread.
			Game.SetRandomSeed( seed );

			for ( int i = 0; i < settings.Count; i++ )
			{				
				var spreadX = Game.Random.Float( settings.HorizontalSpread.Min, settings.HorizontalSpread.Max );
				var spreadY = Game.Random.Float( settings.VerticalSpread.Min, settings.VerticalSpread.Max );

				SpawnProjectile( spreadX, spreadY );
			}
		}
	}
}
