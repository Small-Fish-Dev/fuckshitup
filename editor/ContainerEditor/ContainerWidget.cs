using FUCKSHIT.UI;

namespace FUCKSHIT.Editor;

public sealed class ContainerWidget : Widget
{
	SerializedProperty Property { get; }

	private Vector2 _offset;

	public ContainerWidget( SerializedProperty property )
	{
		Property = property;
	}

	[EditorEvent.Frame]
	void Frame()
	{
		Update();
	}

	protected override void OnPaint()
	{
		const float SCALE = 0.4f;

		const float PADDING = 2f;
		const float GRID_SIZE = SlotCollectionDisplay.GRID_SIZE * SCALE;
		const float GAP = SlotCollectionDisplay.GAP * SCALE;

		// Center this shit...
		Paint.Translate( Size / 2f - _offset / 2f );
		_offset = Vector2.Zero;

		// Draw all of the container boxes we have on our property.
		var boxes = Property.GetValue<BoxCollection>();
		if ( boxes is null )
			return;

		var totalX = 0f;
		var totalY = 0f;
		var rowTallest = 0f;
		var boxCount = boxes.Count;
		var previous = default( EquipmentBox );

		for ( int i = 0; i < boxCount; i++ )
		{
			var box = boxes.ElementAtOrDefault( i );
			if ( box is null )
				continue;

			var top = totalY + box.Margin.y * SCALE;
			var left = box.SameLine && previous is not null
				? previous.Size.x * GRID_SIZE + GAP
				: 0f;
			left += box.Margin.x * SCALE + totalX;
			totalX = left;

			if ( _offset.x < totalX + (box.Size.x + 1) * GRID_SIZE )
				_offset.x = totalX + (box.Size.x + 1) * GRID_SIZE;

			if ( _offset.y < totalY + (box.Size.y + 1) * GRID_SIZE )
				_offset.y = totalY + (box.Size.y + 1) * GRID_SIZE;

			var rect = new Rect( (int)left, (int)top, (int)((GRID_SIZE - PADDING) * box.Size.x), (int)((GRID_SIZE - PADDING) * box.Size.y) );
			Paint.SetBrushAndPen( Color.Black, Color.White );
			Paint.DrawRect( rect );

			Paint.SetBrushAndPen( Color.Gray );
			for ( int x = 0; x < box.Size.x; x++ ) 
				for ( int y = 0; y < box.Size.y; y++ )
				{
					var px = (int)(left + x * (GRID_SIZE - PADDING) + 1);
					var py = (int)(top + y * (GRID_SIZE - PADDING) + 1);
					rect = new Rect( px, py, GRID_SIZE - PADDING - 2, GRID_SIZE - PADDING - 2 );
					Paint.DrawRect( rect );
				}

			var height = box.Size.y * GRID_SIZE;
			if ( rowTallest < height )
				rowTallest = height;

			var next = boxes.ElementAtOrDefault( i + 1 );
			if ( next is not null && !next.SameLine )
			{
				totalY += rowTallest + GAP;
				totalX = 0f;
				rowTallest = 0f;
			}

			previous = box;
		}
	}
}
