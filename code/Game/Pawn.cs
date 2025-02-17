namespace FUCKSHIT;

public abstract class Pawn : Component
{
	/// <summary>
	/// The owner client of this pawn.
	/// </summary>
	public Client Client => Network?.Active ?? false
		? (Network.IsOwner ? Client.Local : Client.Find( Network?.Owner ))
		: null;

	/// <summary>
	/// Create a name for this pawn when it's assigned to a client.
	/// </summary>
	/// <param name="client"></param>
	/// <returns></returns>
	public virtual string CreateName( Client client )
	{
		return $"Pawn";
	}
}
