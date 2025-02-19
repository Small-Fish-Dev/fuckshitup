namespace FUCKSHIT;

public sealed class SlotCollection
{
	public sealed class Box
	{
		/// <summary>
		/// Size of this <see cref="SlotCollection.Box"/>
		/// </summary>
		public Vector2Int Size { get; }

		/// <summary>
		/// Margin of this box.
		/// <para>There is a default slight offset in UI.</para>
		/// <para>Add a value to this if you want to offset the box, the scale is pixels.</para>
		/// </summary>
		public Vector2 Margin { get; }

		/// <summary>
		/// Display on the same line as the previous <see cref="SlotCollection.Box"/>.
		/// </summary>
		public bool SameLine { get; }

		/// <summary>
		/// All item references of this <see cref="SlotCollection.Box"/>.
		/// </summary>
		public IReadOnlyDictionary<Vector2Int, Item> References => _references;
		private Dictionary<Vector2Int, Item> _references { get; }

		public Box( Vector2Int size, Vector2? margin = null, bool sameLine = true )
		{
			Size = size.ComponentMax( Vector2Int.One );
			Margin = margin ?? Vector2.Zero;
			SameLine = sameLine;
			_references = new Dictionary<Vector2Int, Item>();
		}

		public Box( int width, int height, Vector2? margin = null, bool sameLine = true )
			: this( new Vector2Int( width, height ), margin, sameLine )
		{ }

		/// <summary>
		/// Does our <see cref="SlotCollection.Box"/> contain this item's reference?
		/// </summary>
		/// <param name="item"></param>
		/// <param name="position"></param>
		public bool TryGetPosition( Item item, out Vector2Int position )
		{
			position = default( Vector2Int );
			if ( item is null ) 
				return false;

			var query = _references.FirstOrDefault( kvp => kvp.Value == item );
			position = query.Key;

			return query.Value is not null;
		}

		/// <summary>
		/// Does our <see cref="SlotCollection.Box"/> have an item that fullfils this predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="position"></param>
		public bool TryFind( Func<Item, bool> predicate, out Vector2Int position )
		{
			position = default( Vector2Int );
			if ( predicate is null )
				return false;

			var query = _references.FirstOrDefault( kvp => predicate( kvp.Value ) );
			position = query.Key;

			return query.Value is not null;
		}

		/// <summary>
		/// Store a reference in our <see cref="SlotCollection.Box"/>.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="item"></param>
		public void StoreReference( Vector2Int position, Item item )
		{
			if ( _references.TryGetValue( position, out var reference ) )
			{
				if ( reference.IsValid() )
					return;

				_references.Remove( position );
			}

			if ( position.x >= Size.x || position.y >= Size.y ) return;
			if ( position.x < 0 || position.y < 0 ) return;

			_references.Add( position, item );
		}

		/// <summary>
		/// Store a reference in our <see cref="SlotCollection.Box"/>.
		/// </summary>
		/// <param name="position"></param>
		public void ClearReference( Vector2Int position )
		{
			if ( !_references.ContainsKey( position ) ) return;
			_references.Remove( position );
		}

		/// <summary>
		/// Store a reference in our <see cref="SlotCollection.Box"/>.
		/// </summary>
		/// <param name="item"></param>
		public void ClearReference( Item item )
		{
			Assert.True( item is not null, "Tried to clear an Item reference of a null Item." );
			
			// Find position of item and clear the reference.
			var position = _references.FirstOrDefault( kvp => kvp.Value == item ).Key;
			ClearReference( position );
		}

		/// <summary>
		/// Check if this position is occupied or not from a size.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="size"></param>
		/// <param name="ignore"></param>
		/// <returns></returns>
		public bool CanFit( Vector2Int position, Vector2Int size, Item ignore = null )
		{
			if ( position.x + size.x - 1 >= Size.x || position.y + size.y - 1 >= Size.y )
				return false;

			if ( position.x < 0 || position.y < 0 )
				return false;

			if ( size.x > Size.x || size.y > Size.y )
				return false;

			var slotsNeeded = new HashSet<Vector2Int>();
			for ( int x = position.x; x < position.x + size.x; x++ )
				for ( int y = position.y; y < position.y + size.y; y++ )
				{
					slotsNeeded.Add( new Vector2Int( x, y ) );
				}

			foreach ( var (pos, item) in _references)
			{
				if ( ignore is not null && item == ignore ) continue;
				if ( !item.IsValid() ) continue;

				for ( int x = pos.x; x < pos.x + item.Size.x; x++ )
					for ( int y = pos.y; y < pos.y + item.Size.y; y++ )
					{
						if ( slotsNeeded.Contains( new Vector2Int( x, y ) ) )
							return false;
					}
			}

			return true;
		}

		/// <summary>
		/// Try find space of specific size in this <see cref="SlotCollection.Box"/>.
		/// </summary>
		/// <param name="size"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool TryFindSpace( Vector2Int size, out Vector2Int position )
		{
			var occupiedSlots = new HashSet<Vector2Int>();
			position = Vector2Int.Zero;

			if ( size.x > Size.x || size.y > Size.y )
				return false;

			for ( int x = 0; x < Size.x - size.x + 1; x++ )
				for ( int y = 0; y < Size.y - size.y + 1; y++ )
				{
					var pos = new Vector2Int( x, y );
					if ( occupiedSlots.Contains( pos ) ) continue;
					if ( _references.TryGetValue( pos, out var item ) )
					{
						if ( item.Size.x == 1 && item.Size.y == 1 )
							continue;

						for ( int itemX = 0; itemX < item.Size.x; itemX++ )
							for ( int itemY = 0; itemY < item.Size.y; itemY++ )
							{
								occupiedSlots.Add( pos + new Vector2Int( itemX, itemY ) );
							}

						continue;
					}

					var fits = true;
					for ( int itemX = 0; itemX < size.x; itemX++ )
					{
						if ( !fits ) break;

						for ( int itemY = 0; itemY < size.y; itemY++ )
						{
							position = pos + new Vector2Int( itemX, itemY );
							if ( occupiedSlots.Contains( position ) ) continue;
							if ( !_references.TryGetValue( pos, out _ ) )
								continue;

							fits = false;
							break;
						}
					}

					position = pos;

					if ( fits ) return true;
				}

			return false;
		}
	}

	/// <summary>
	/// The <see cref="Item"/> source of this <see cref="SlotCollection"/>.
	/// </summary>
	public Item Source { get; private set; }

	/// <summary>
	/// The <see cref="Container"/> this <see cref="SlotCollection"/> is a part of.
	/// </summary>
	public Container Parent { get; private set; }

	/// <summary>
	/// Display name of this <see cref="SlotCollection"/>.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The ordering number of this <see cref="SlotCollection"/>, the higher it is, the more up it will be.
	/// </summary>
	public int Order { get; private set; }

	/// <summary>
	/// Read-only list of all the boxes inside of this <see cref="SlotCollection"/>.
	/// </summary>
	public IReadOnlyCollection<Box> Boxes => _boxes;
	private Box[] _boxes;

	public SlotCollection( Container parent, params Box[] boxes )
	{
		Assert.NotNull( parent is not null, "Tried create SlotCollection to a null Container." );
		Assert.True( boxes is not null && boxes.Length > 0, "Tried create SlotCollection with no boxes." );

		Parent = parent;
		_boxes = boxes;
	}

	public SlotCollection( Container parent, Vector2Int size ) 
		: this( parent, new Box( size ) )
	{ }

	/// <summary>
	/// Assign an <see cref="Item"/> source to a <see cref="SlotCollection"/>.
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public SlotCollection WithSource( Item source )
	{
		Assert.True( source.IsValid(), "Tried to assing an invalid source to a SlotCollection." );
		
		if ( Source.IsValid() )
			Source.OnComponentDestroy -= Destroy;

		Source = source;
		Source.OnComponentDestroy += Destroy;

		return this;
	}

	/// <summary>
	/// Assign an order to a <see cref="SlotCollection"/>.
	/// </summary>
	/// <param name="order"></param>
	/// <returns></returns>
	public SlotCollection WithOrder( int order )
	{
		Order = order;
		return this;
	}

	/// <summary>
	/// Override the parent <see cref="Container"/> of this <see cref="SlotCollection"/>.
	/// </summary>
	/// <param name="container"></param>
	public void SetParent( Container container )
	{
		Parent = container;
	}

	/// <summary>
	/// Destroy this <see cref="SlotCollection"/>.
	/// </summary>
	public void Destroy()
	{
		if ( !Parent.IsValid() )
			return;

		Parent.RemoveSlotCollection( this );
	}
}
