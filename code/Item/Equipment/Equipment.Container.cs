namespace FUCKSHIT;

/// <summary>
/// Class for the editor to turn these into proper <see cref="Container"/> <see cref="SlotCollection.Box"/>es.
/// </summary>
public sealed class EquipmentBox
{
	/// <inheritdoc cref="SlotCollection.Box.Size"/>
	[KeyProperty, JsonInclude]
	public Vector2Int Size
	{
		get => _size;
		set => _size = value.ComponentMax( Vector2Int.One );
	}
	
	[JsonIgnore, Hide] 
	private Vector2Int _size = Vector2Int.One;

	/// <inheritdoc cref="SlotCollection.Box.SameLine"/>
	[KeyProperty, JsonInclude]
	public bool SameLine { get; set; } = true;

	/// <inheritdoc cref="SlotCollection.Box.Margin"/>
	[Property, JsonInclude]
	public Vector2 Margin { get; set; }
}

partial class Equipment
{
	/// <summary>
	/// List of all equipment boxes used for constructing the <see cref="Container"/>, this is for the editor.
	/// </summary>
	[Property, WideMode, Title( "Container" ), Feature( "Container" )]
	public List<EquipmentBox> EquipmentBoxes { get; set; }

	/// <summary>
	/// The container that this equipment owns, can be invalid.
	/// </summary>
	[Sync]
	public Container Inventory { get; set; }

	/// <summary>
	/// True if we have any <see cref="EquipmentBoxes"/> defined.
	/// </summary>
	public bool IsContainer => EquipmentBoxes?.Any() ?? false;

	protected void SetupContainer()
	{
		if ( !IsContainer || IsProxy ) return;

		Inventory = Components.GetOrCreate<Container>( FindMode.InSelf );
		if ( !Inventory.IsValid() )
			return;

		// Clear just in case the Container existed before, it might contain some old shit...
		Inventory.Clear();

		// Convert the EquipmentBoxes to an actual SlotColletion that the Container can use.
		var boxes = EquipmentBoxes
			.Select( box => new SlotCollection.Box( box.Size, box.Margin, box.SameLine ) )
			.ToArray();

		Inventory.AddSlotCollection( Name, boxes )
			.WithSource( this );
	}
}
