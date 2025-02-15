namespace FUCKSHIT;

partial class Item : IIconGenerator
{
	int IIconGenerator.CreateIconHash()
	{
		if ( !GameObject.IsValid() ) return 0;
		return HashCode.Combine( PrefabSource );
	}

	Transform? IIconGenerator.CreateScene( SceneWorld world, SceneCamera camera )
	{
		if ( !Renderer.IsValid() )
			return null;

		var transform = new Transform( IconSettings.Offset, IconSettings.Rotation );
		var sceneObject = new SceneObject( world, Renderer.Model, transform );
		return camera.FitModel( sceneObject );
	}
}
