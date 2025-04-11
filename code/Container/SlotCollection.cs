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
		/// Find the first reference's position matching the predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="position"></param>
		/// <returns>True if we found a position, false if not.</returns>
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
		/// Find all of the references' positions matching the predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns>All of the found reference positions.</returns>
		public IEnumerable<Vector2Int> TryFindAll( Func<Item, bool> predicate )
		{
			if ( predicate is null )
				yield break;

			foreach ( var (position, item) in _references )
			{
				if ( !predicate( item ) )
					continue;

				yield return position;
			}
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
		/// Clear all references from our <see cref="SlotCollection.Box"/>.
		/// </summary>
		public void ClearReferences()
		{
			_references.Clear();
		}

		/// <summary>
		/// Clear a reference from our <see cref="SlotCollection.Box"/>.
		/// </summary>
		/// <param name="position"></param>
		public void ClearReference( Vector2Int position )
		{
			if ( !_references.ContainsKey( position ) ) return;
			_references.Remove( position );
		}

		/// <summary>
		/// Clear an <see cref="Item"/> reference from our <see cref="SlotCollection.Box"/>.
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
		/// Can we fit at a position with a specific size?
		/// </summary>
		/// <param name="position"></param>
		/// <param name="size"></param>
		/// <param name="ignore"></param>
		/// <returns>True if the position is valid for the size, false if not.</returns>
		public bool CanFit( Vector2Int position, Vector2Int size, Item ignore = null )
		{
			if ( position.x + size.x - 1 >= Size.x || position.y + size.y - 1 >= Size.y )
				return false;

			if ( position.x < 0 || position.y < 0 )
				return false;

			if ( size.x > Size.x || size.y > Size.y )
				return false;

			var selfRect = new RectInt( position, size - 1 );
			foreach ( var (pos, item) in _references )
			{
				if ( ignore is not null && item == ignore ) continue;
				if ( !item.IsValid() ) continue;

				var rect = new RectInt( pos, item.Size - 1 );
				if ( rect.IsInside( selfRect ) )
					return false;
			}

			return true;
		}

		/// <summary>
		/// Try to get the item that is occupying these slots.
		/// <para>This is the only method that finds </para>
		/// </summary>
		/// <param name="position"></param>
		/// <param name="ignore"></param>
		/// <returns>null or the item that occupies this slot in some way.</returns>
		public Item GetReferenceAt( Vector2Int position, Item ignore = null )
		{
			if ( position.x >= Size.x || position.y >= Size.y )
				return null;

			if ( position.x < 0 || position.y < 0 )
				return null;

			if ( _references.TryGetValue( position, out var reference ) )
				return reference;

			foreach ( var (pos, item) in _references )
			{
				if ( ignore is not null && item == ignore ) continue;
				if ( !item.IsValid() ) continue;

				var rect = new RectInt( pos, item.Size - 1 );
				if ( rect.IsInside( position ) )
					return item;
			}

			return null;
		}

		/// <summary>
		/// Try to find space of this size that isn't taken inside of this Container.
		/// <para>Use absolute Item sizes for the parameter.</para>
		/// </summary>
		/// <param name="size"></param>
		/// <param name="result"></param>
		/// <param name="allowRotate"></param>
		/// <returns>True if we found space, false if not.</returns>
		public bool TryFindSpace( Vector2Int size, out (Vector2Int Position, bool Rotate) result, bool allowRotate = true )
		{
			// Find space normally.
			result = (default, default);
			if ( FindSpace( size, out result.Position ) )
				return true;

			// Find rotated space if we want that...
			if ( !allowRotate )
				return false;

			var rotsize = new Vector2Int( size.y, size.x );
			if ( !FindSpace( rotsize, out result.Position ) )
				return false;

			result.Rotate = true;
			return true;
		}

		private bool FindSpace( Vector2Int size, out Vector2Int position )
		{
			position = Vector2Int.Zero;

			if ( size.x > Size.x || size.y > Size.y )
				return false;

			for ( int y = 0; y < Size.y - size.y + 1; y++ )
				for ( int x = 0; x < Size.x - size.x + 1; x++ )
				{
					var selfPos = new Vector2Int( x, y );
					var selfRect = new RectInt( selfPos, size - 1 );

					var valid = true;
					foreach ( var (pos, item) in _references )
					{
						if ( !item.IsValid() ) continue;

						var rect = new RectInt( pos, item.Size - 1 );
						if ( rect.IsInside( selfRect ) )
						{
							valid = false;
							break;
						}
					}

					if ( valid )
					{
						position = selfPos;
						return true;
					}
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
	/// Unique identifier of this <see cref="SlotCollection"/>.
	/// </summary>
	public Guid Guid { get; set; } = Guid.Empty;

	/// <summary>
	/// The ordering number of this <see cref="SlotCollection"/>, the higher it is, the more up it will be.
	/// </summary>
	public int Order { get; private set; }

	/// <summary>
	/// The <see cref="ItemTypeFlags"/> exclude flags filter of this <see cref="SlotCollection"/>.
	/// </summary>
	public ItemTypeFlags ExcludeFlags { get; private set; }

	/// <summary>
	/// Read-only list of all the boxes inside of this <see cref="SlotCollection"/>.
	/// </summary>
	public IReadOnlyCollection<Box> Boxes => _boxes;
	private Box[] _boxes;

	public SlotCollection( Container parent, params Box[] boxes )
	{
		Assert.NotNull( parent is not null, "Tried create SlotCollection to a null Container." );

		Parent = parent;
		Guid = Guid.NewGuid();
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
	/// Assign a custom <see cref="ItemTypeFlags"/> exclude flags to a <see cref="SlotCollection"/>.
	/// </summary>
	/// <param name="flags"></param>
	/// <returns></returns>
	public SlotCollection WithExcludeFlags( ItemTypeFlags flags )
	{
		ExcludeFlags = flags;
		return this;
	}

	/// <summary>
	/// Assign a custom <see cref="ItemTypeFlags"/> exclude flags to a <see cref="SlotCollection"/>.
	/// </summary>
	/// <param name="flags"></param>
	/// <returns></returns>
	public SlotCollection With( ItemTypeFlags flags )
	{
		ExcludeFlags = flags;
		return this;
	}

	/// <summary>
	/// Override the <see cref="SlotCollection.Box"/>es of this <see cref="SlotCollection"/>.
	/// </summary>
	/// <param name="boxes"></param>
	public void SetBoxes( Box[] boxes )
	{
		_boxes = boxes;
	}

	/// <summary>
	/// Override the parent <see cref="Container"/> of this <see cref="SlotCollection"/>.
	/// </summary>
	/// <param name="container"></param>
	public void SetParent( Container container )
	{
		Parent = container;
	}

	// todo: add item blacklisting and whitelisting
	/// <summary>
	/// Does this <see cref="Item"/> pass our filters?
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool PassesFilter( Item item ) 
		=> ExcludeFlags == ItemTypeFlags.None || !ExcludeFlags.HasFlag( item.TypeFlags );

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
