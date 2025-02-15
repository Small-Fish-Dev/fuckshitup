namespace FUCKSHIT;

public abstract class Pawn : Component
{
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
