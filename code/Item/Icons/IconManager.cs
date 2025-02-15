namespace FUCKSHIT;

public static class IconManager
{
	public const int RESOLUTION = 256;
	private static Dictionary<int, Texture> Cache { get; } = new();

	/// <summary>
	/// Clear the cached icons..
	/// </summary>
	public static void Clear()
	{
		Cache?.Clear();
	}

	/// <summary>
	/// Calculate the normalized size from a integer width and height.
	/// </summary>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <returns></returns>
	public static Vector2 CalculateSize( int width, int height )
	{
		var size = Vector2.Zero;
		if ( width >= height )
			size = new Vector2( 1f, height / (float)width ) * RESOLUTION;
		else
			size = new Vector2( width / (float)height, 1f ) * RESOLUTION;

		size = new Vector2( (int)size.x, (int)size.y );
		return size;
	}

	/// <summary>
	/// Request an icon from the manager.
	/// <para>NOTE: This is cached via hashes implemented with <see cref="IIconGenerator"/>.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="target"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <returns></returns>
	public static Texture Request<T>( T target, int width, int height )
		where T : IIconGenerator
	{
		Assert.NotNull( target, "Tried to request icon for null object." );

		// Look if we have a working icon in the cache.
		var hash = target.CreateIconHash();
		if ( Cache.TryGetValue( hash, out var texture ) )
			return texture;

		// Create scene and camera.
		var size = CalculateSize( width, height );
		var world = new SceneWorld();
		var camera = new SceneCamera()
		{
			World = world,
			AmbientLightColor = Color.White,
			AntiAliasing = false,
			BackgroundColor = Color.Transparent,
			FieldOfView = 40,
			Size = size,
			ZFar = 5000,
			ZNear = 2
		};

		var light = new SceneLight( world, Vector3.Forward * 15f, 1000f, Color.White * 0.7f );
		_ = new SceneDirectionalLight( world, global::Rotation.From( 45, -45, 45 ), Color.White * 10f );

		// Did we succeed?
		var transform = target.CreateScene( world, camera );
		var renderTarget = default( Texture );
		if ( transform is not null )
		{
			// Create RenderTarget and render camera.
			renderTarget = Texture.CreateRenderTarget( $"ItemIcon{hash}", ImageFormat.RGBA8888, size );

			camera.Position = transform.Value.Position;
			camera.Rotation = transform.Value.Rotation;

			light.Position = transform.Value.Position + camera.Rotation.Backward * 20f;
			light.Rotation = transform.Value.Rotation;

			Graphics.RenderToTexture( camera, renderTarget );

			if ( !Cache.ContainsKey( hash ) )
				Cache.Add( hash, renderTarget );
			else
				Cache[hash] = renderTarget;
		}

		camera.Dispose();
		world.Delete();

		return renderTarget;
	}
}
