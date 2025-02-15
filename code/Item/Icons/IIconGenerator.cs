namespace FUCKSHIT;

public interface IIconGenerator
{
	/// <summary>
	/// This hash determines if we should create a new icon or not.
	/// </summary>
	/// <returns></returns>
	public int CreateIconHash();

	/// <summary>
	/// This method creates the scene for the icon.
	/// </summary>
	/// <param name="world"></param>
	/// <param name="camera"></param>
	/// <returns>Camera's transform.</returns>
	public Transform? CreateScene( SceneWorld world, SceneCamera camera );
}
