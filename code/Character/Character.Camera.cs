namespace FUCKSHIT;

partial class Character
{
	private SkinnedModelRenderer FirstpersonView { get; set; }

	[Sync]
	public Angles EyeAngles { get; set; }

	public Ray EyeRay => new Ray( Camera?.WorldPosition ?? WorldPosition, EyeAngles.Forward );

	private void SimulateCamera()
	{
		if ( !Camera.IsValid() || !FirstpersonView.IsValid() )
			return;

		var angles = EyeAngles + Input.AnalogLook;
		angles.pitch = angles.pitch.Clamp( -89, 89 );
		EyeAngles = angles;

		var eyes = FirstpersonView.GetAttachment( "eyes" ) ?? Transform.World;
		Camera.WorldPosition = eyes.Position + eyes.Rotation.Forward * 3f;
		Camera.WorldRotation = Rotation.From( EyeAngles );
	}

	private void CreateFirstpersonView()
	{
		// Clone renderer for firstperson view legs...
		var gameObject = Renderer.GameObject.Clone();
		gameObject.Name = $"Firstperson View";
		gameObject.SetParent( GameObject, false );

		var renderer = gameObject.GetComponent<SkinnedModelRenderer>();
		if ( renderer.IsValid() )
		{
			renderer.RenderOptions.Game = false;
			renderer.RenderOptions.Overlay = true;
		}

		var collider = gameObject.GetComponent<Collider>();
		if ( collider.IsValid() )
			collider.Destroy();

		FirstpersonView = renderer;
	}

	private void SimulateView()
	{
		if ( !FirstpersonView.IsValid() )
			CreateFirstpersonView();

		if ( !Renderer.IsValid() ) 
			return;

		// Set visibility of Firstperson View and actual Renderer.
		Renderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		FirstpersonView.RenderType = ModelRenderer.ShadowRenderType.Off;
		FirstpersonView.BoneMergeTarget = Ragdolled ? Renderer : null;

		// Move head out of the way for Firstperson View.
		const int HEAD_BONE = 7;

		var eyes = FirstpersonView.GetAttachment( "eyes" ) ?? Transform.World;
		var rot = WorldRotation;
		var transform = new Transform( eyes.Position + eyes.Backward * 10 + eyes.Down * 10f, Rotation.Identity, 0 );

		FirstpersonView.SceneModel?.SetBoneWorldTransform( HEAD_BONE, transform );
	}
}
