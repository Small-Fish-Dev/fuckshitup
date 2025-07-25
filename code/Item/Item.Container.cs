﻿namespace FUCKSHIT;

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

partial class Item
{
	/// <summary>
	/// Is our item also a container?
	/// </summary>
	[Property, FeatureEnabled( "Container" )]
	public bool IsContainer { get; set; }

	/// <summary>
	/// What items should we not allow in our container?
	/// </summary>
	[Property, FeatureEnabled( "Container" )]
	public ItemTypeFlags ExcludeFlags { get; set; } = ItemTypeFlags.None;

	/// <inheritdoc cref="SlotCollection.Order"/>
	[Property, FeatureEnabled( "Container" )]
	public int ContainerOrder { get; set; } = 0;

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

	protected void SetupContainer()
	{
		if ( !IsContainer || IsProxy ) return;

		Inventory = Components.GetOrCreate<Container>( FindMode.InSelf );
		if ( !Inventory.IsValid() )
			return;

		var boxes = EquipmentBoxes
			.Select( box => new SlotCollection.Box( box.Size, box.Margin, box.SameLine ) )
			.ToArray();

		var collection = Inventory.AddSlotCollection( Name, boxes )
			.WithSource( this )
			.WithOrder( ContainerOrder )
			.WithExcludeFlags( ExcludeFlags );
	}
}
