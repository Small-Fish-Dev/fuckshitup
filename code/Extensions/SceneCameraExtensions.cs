namespace FUCKSHIT;

public static class SceneCameraExtensions
{
	public static Transform FitModel( this SceneCamera camera, SceneObject model )
	{
		var bounds = model.Model.Bounds;
		var max = bounds.Size;
		var radius = MathF.Max( max.x, MathF.Max( max.y, max.z ) );
		var dist = radius / MathF.Sin( camera.FieldOfView.DegreeToRadian() );

		var viewDirection = Vector3.Forward;
		var pos = viewDirection * dist + bounds.Center;
		
		return new Transform( pos, Rotation.LookAt( bounds.Center - pos ) );
	}
}
