global using BoxCollection = System.Collections.Generic.List<FUCKSHIT.EquipmentBox>;

namespace FUCKSHIT.Editor;

[CustomEditor( typeof( BoxCollection ) )]
public sealed class ContainerControlWidget : ControlWidget
{
	public override bool SupportsMultiEdit => false;

	SerializedProperty Property { get; }
	SerializedObject SerializedObject { get; }

	public ContainerControlWidget( SerializedProperty property ) : base( property )
	{
		Property = property;
		SerializedObject = Property.GetValue<BoxCollection>()?.GetSerialized();

		if ( SerializedObject is null )
			return;

		Layout = Layout.Column();

		var row = Layout.AddRow(); 
		row.Alignment = TextFlag.Center;

		var containerWidget = new ContainerWidget( Property )
		{
			FixedWidth = 300,
			FixedHeight = 300
		};

		row.Add( containerWidget );
		Layout.AddSpacingCell( 8f );

		var listWidget = new ListControlWidget( Property );
		Layout.Add( listWidget );
	}

	protected override void OnPaint() { }
}
