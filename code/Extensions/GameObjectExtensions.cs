namespace FUCKSHIT;

public static class GameObjectExtensions
{
	/// <summary>
	/// Try get the first <see cref="Container"/> from a <see cref="GameObject"/>.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="container"></param>
	/// <returns></returns>
	public static bool TryGetContainer( this GameObject self, out Container container )
	{
		container = null;
		if ( !self.IsValid() )
			return false;
		return self.Components.TryGet( out container, FindMode.EverythingInSelfAndDescendants );
	}

	/// <summary>
	/// Set up a <see cref="GameObject"/>'s networking.
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="owner"></param>
	/// <param name="transfer"></param>
	/// <param name="orphaned"></param>
	/// <param name="clearParent"></param>
	public static void SetupNetworking(
		this GameObject obj,
		Connection owner = null,
		OwnerTransfer transfer = OwnerTransfer.Takeover,
		NetworkOrphaned orphaned = NetworkOrphaned.ClearOwner,
		bool clearParent = false )
	{
		if ( !obj.IsValid() )
			return;

		obj.NetworkMode = NetworkMode.Object;

		if ( clearParent && obj.Parent.IsValid() )
			obj.SetParent( null, true );

		if ( !obj.Network.Active )
			obj.NetworkSpawn( owner );
		else if ( Networking.IsActive && owner != null )
			obj.Network.AssignOwnership( owner );

		obj.Network.SetOwnerTransfer( transfer );
		obj.Network.SetOrphanedMode( orphaned );
	}
}
