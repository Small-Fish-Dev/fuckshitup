namespace FUCKSHIT.Editor;

public sealed class IconEditor : GraphicsView
{
	private SerializedObject Object { get; }
	private SerializedProperty Property { get; }
	private GameObject GameObject { get; }
	private NativeRenderingWidget Renderer { get; }
	private IconSettings Icon { get; set; }

	private Vector2Int ItemSize => Property?.Parent?.TryGetProperty( "AbsoluteSize", out var property ) ?? false
		? property.GetValue( Vector2Int.One )
		: Vector2Int.One;

	private SceneObject _obj;
	private SceneCamera _camera;
	private SceneLight _light;

	public IconEditor( Widget parent ) : base( parent )
	{
		// Scene
		var world = new SceneWorld();
		_camera = new SceneCamera()
		{
			World = world,
			AmbientLightColor = Color.White,
			AntiAliasing = false,
			Size = IconManager.CalculateSize( ItemSize.x, ItemSize.y ),
			BackgroundColor = Color.Transparent,
			FieldOfView = 40,
			ZFar = 5000,
			ZNear = 2
		};

		_light = new SceneLight( world, Vector3.Forward * 15f, 1000f, Color.White * 0.7f );
		_ = new SceneDirectionalLight( world, global::Rotation.From( 45, -45, 45 ), Color.White * 10f );

		Property = (parent as IconEditorPopup).Property;
		GameObject = Property.Parent.GetProperty( "GameObject" ).GetValue<GameObject>();
		Object = Property.GetValue<IconSettings>().GetSerialized();
		Icon = Property.GetValue<IconSettings>();
		
		// Layout
		Layout = Layout.Column();

		Layout.Margin = 10;
		{
			// Properties
			if ( Object.TryGetProperty( "Rotation", out var rotation ) )
				Layout.Add( new RotationControlWidget( rotation ), 0 );
			Layout.AddSpacingCell( 8 );
			if ( Object.TryGetProperty( "Offset", out var position ) )
				Layout.Add( new VectorControlWidget( position ), 0 );
		}

		Layout.AddSpacingCell( 8 );

		var row = Layout.Add( Layout.Row() );
		row.Alignment = TextFlag.CenterHorizontally;
		{
			// Scene
			Renderer = row.Add( new NativeRenderingWidget( this )
			{
				Camera = _camera,
				FixedSize = _camera.Size,
				TranslucentBackground = true
			}, 1 );
		}

		{
			// Save Button
			var button = Layout.Add( new global::Editor.Button( this )
			{
				Text = "Save Icon Settings",
				Clicked = () =>
				{
					Property.SetValue( Icon = new IconSettings()
					{
						Offset = Object.GetProperty( "Offset" ).GetValue<Vector3>(),
						Rotation = Object.GetProperty( "Rotation" ).GetValue<Rotation>(),
					} );

					parent.Close();
				}
			}, 1 );
		}

		// Object
		_obj = new SceneObject(
			world,
			Model.Load( "models/dev/box.vmdl" )
		);
	}

	[EditorEvent.Frame]
	private void Frame()
	{
		if ( _obj == null )
			return;


		// Update camera and light.
		var transform = _camera.FitModel( _obj );
		_camera.Position = transform.Position;
		_camera.Rotation = transform.Rotation;
		
		_camera.Size = IconManager.CalculateSize( ItemSize.x, ItemSize.y );
		Renderer.FixedSize = _camera.Size;

		_light.Position = _camera.Position + _camera.Rotation.Backward * 20f;

		// Update object transform.
		_obj.Position = Object.GetProperty( "Offset" ).GetValue<Vector3>();
		_obj.Rotation = Object.GetProperty( "Rotation" ).GetValue<Rotation>();

		var modelRenderer = GameObject?.Components.Get<ModelRenderer>( FindMode.EverythingInSelfAndChildren );
		if ( modelRenderer.IsValid() )
			_obj.Model = modelRenderer.Model;
	}
}
