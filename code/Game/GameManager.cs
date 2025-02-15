namespace FUCKSHIT;

public sealed partial class GameManager : Component, Component.INetworkListener
{
	[Property]
	public GameObject Pawn { get; set; }

	protected override void OnStart()
	{
		IconManager.Clear();

		if ( IsProxy )
			return;

		var config = new LobbyConfig()
		{
			Name = $"FUCK",
			MaxPlayers = 32,
			DestroyWhenHostLeaves = true
		};

		Networking.CreateLobby( config );
	}

	void INetworkListener.OnActive( Connection channel )
	{
		// Host creates a client and tells that client to spawn a pawn.
		var client = Client.Create( channel );
		if ( client is not null )
		{
			using ( Rpc.FilterInclude( channel ) )
				SpawnPawn( client );
		}
	}

	[Rpc.Broadcast( NetFlags.HostOnly | NetFlags.Reliable )]
	private void SpawnPawn( Client client )
	{
		if ( client is null )
			return;

		var gameObject = Pawn.Clone();
		client.AssignPawn( gameObject );
	}
}
