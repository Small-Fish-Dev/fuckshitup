namespace FUCKSHIT;

public sealed class Client : Component
{
	/// <summary>
	/// The local client.
	/// </summary>
	public static Client Local { get; private set; }

	/// <summary>
	/// All of the connected clients.
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public static NetList<Client> All { get; set; } = new();

	/// <summary>
	/// The GUID to this client's connection.
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public Guid ConnectionId
	{
		get => _connectionId;
		set
		{
			_connectionId = value;

			// Assign local connection.
			if ( Connection.Local?.Id == _connectionId )
				Local = this;
		}
	}
	private Guid _connectionId;

	/// <summary>
	/// The connection of this client.
	/// </summary>
	public Connection Connection => Connection.Find( ConnectionId );

	/// <summary>
	/// The current pawn of this client.
	/// </summary>
	[Sync]
	public Pawn Pawn { get; set; }

	/// <summary>
	/// Create a client for a connection.
	/// <para>NOTE: This should only be called from the host, and only once for any <see cref="Sandbox.Connection"/>.</para>
	/// </summary>
	/// <param name="connection"></param>
	/// <returns></returns>
	public static Client Create( Connection connection )
	{
		Assert.True( connection is not null, "Tried to create client for null connection." );
		Assert.True( Networking.IsHost, "Tried to create client while not a host." );
		
		using ( Game.ActiveScene.Push() )
		{
			var gameObject = new GameObject( true, $"{connection.DisplayName} / {connection.SteamId}" );
			var client = gameObject.AddComponent<Client>();
			client.ConnectionId = connection.Id;

			// Set values and spawn on network.
			gameObject.Flags |= GameObjectFlags.Hidden;
			gameObject.NetworkMode = NetworkMode.Object;
			gameObject.Network.SetOwnerTransfer( OwnerTransfer.Fixed );
			gameObject.Network.SetOrphanedMode( NetworkOrphaned.Destroy );
			gameObject.NetworkSpawn( connection );

			All.Add( client );

			return client;
		}
	}

	/// <summary>
	/// Find a client by connection.
	/// </summary>
	/// <param name="connection"></param>
	/// <returns></returns>
	public static Client Find( Connection connection )
	{
		Assert.True( connection is not null, "Tried to create client for null connection." );
		
		return All.FirstOrDefault( client => client.ConnectionId == connection.Id );
	}

	/// <summary>
	/// Attempts to assign a pawn from a GameObject.
	/// </summary>
	/// <param name="gameObject"></param>
	public void AssignPawn( GameObject gameObject )
	{
		Assert.True( !IsProxy, "Tried to assign pawn to client you don't own." );
		Assert.True( gameObject.IsValid(), "Tried to assign an invalid GameObject as the pawn." );
		
		var pawn = gameObject.Components.Get<Pawn>( FindMode.EverythingInSelfAndChildren );
		Assert.True( pawn.IsValid(), "No valid Pawn component found on GameObject." );

		// Discard previous pawn..
		if ( Pawn.IsValid() )
			Pawn.DestroyGameObject();

		// Setup networking..
		gameObject.Name = pawn.CreateName( this );
		gameObject.Network.SetOwnerTransfer( OwnerTransfer.Fixed );
		gameObject.Network.SetOrphanedMode( NetworkOrphaned.Destroy );
		if ( !gameObject.Network.Active )
			gameObject.NetworkSpawn( Connection );

		Pawn = pawn;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( !Networking.IsHost )
			return;

		All.Remove( this );
	}
}
