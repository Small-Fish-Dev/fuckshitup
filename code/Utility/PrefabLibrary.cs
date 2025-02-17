namespace FUCKSHIT;

public static class PrefabLibrary
{
	public static IReadOnlyDictionary<PrefabFile, GameObject> All => _all;
	private static Dictionary<PrefabFile, GameObject> _all;

	public static void Initialize()
	{
		var prefabs = ResourceLibrary.GetAll<PrefabFile>().ToArray();

		_all = new Dictionary<PrefabFile, GameObject>();
		foreach ( var prefab in prefabs )
		{
			if ( _all.ContainsKey( prefab ) )
				continue;

			_all.Add( prefab, GameObject.GetPrefab( prefab.ResourcePath ) );
		}
	}

	/// <summary>
	/// Find all prefabs that contain a component.
	/// This is probably slow so avoid using it too much.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static IEnumerable<GameObject> FindByComponent<T>() where T : Component
	{
		if ( _all == null ) yield break;

		foreach ( var (_, obj) in _all )
		{
			var components = obj.Components.GetAll();
			var any = components.Any( component => component?.GetType().IsAssignableTo( typeof( T ) ) ?? false );
			if ( any )
				yield return obj;
		}
	}

	/// <summary>
	/// Tries to find a PrefabDefinition by Prefab path.
	/// </summary>
	/// <param name="path"></param>
	/// <param name="prefab"></param>
	/// <returns></returns>
	public static bool TryGetByPath( string path, out GameObject prefab )
	{
		prefab = GameObject.GetPrefab( path );
		return prefab.IsValid();
	}
}
