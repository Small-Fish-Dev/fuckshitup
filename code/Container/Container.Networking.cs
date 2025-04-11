namespace FUCKSHIT;

partial class Container : Component.INetworkSnapshot
{
	/// <summary>
	/// Serialize this <see cref="Container"/>'s targeted <see cref="SlotCollection"/>s to bytes, defaulted to all collections.
	/// </summary>
	/// <param name="collections"></param>
	/// <returns></returns>
	public byte[] Serialize( IEnumerable<SlotCollection> collections = null )
	{
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter( stream );

		void WriteBox( SlotCollection.Box box )
		{
			if ( box is null ) return;

			writer.Write( box.Size.x );
			writer.Write( box.Size.y );
			writer.Write( box.SameLine );
			writer.Write( box.Margin == Vector2.Zero );
			if ( box.Margin != Vector2.Zero )
			{
				writer.Write( box.Margin.x );
				writer.Write( box.Margin.y );
			}

			var count = box.References.Count;
			writer.Write( count );
			for ( int i = 0; i < count; i++ )
			{ 
				var kvp = box.References.ElementAtOrDefault( i );
				var pos = kvp.Key;
				var reference = kvp.Value;
				if ( !reference.IsValid() )
					continue;

				writer.Write( pos.x );
				writer.Write( pos.y );
				writer.Write( reference.Id.ToByteArray() );
			}
		}

		void WriteCollection( SlotCollection collection )
		{
			if ( collection is null ) return;

			writer.Write( collection.Name );
			writer.Write( collection.Guid.ToByteArray() );
			writer.Write( (ulong)collection.ExcludeFlags );
			
			var count = collection.Boxes.Count;
			writer.Write( count );
			for ( int i = 0; i < count; i++ )
			{
				var box = collection.Boxes.ElementAtOrDefault( i );
				WriteBox( box );
			}
		}

		collections ??= _slotCollections;
		
		var count = collections.Count();
		writer.Write( count );
		for ( int i = 0; i < count; i++ )
		{
			var collection = collections.ElementAtOrDefault( i );
			WriteCollection( collection );
		}

		return stream.ToArray();
	}

	/// <summary>
	/// Deserialize the serialized bytes of this <see cref="Container"/>'s full state.
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	public bool Deserialize( byte[] data )
	{
		if ( data is null ) return false;
		
		using var stream = new MemoryStream( data );
		using var reader = new BinaryReader( stream );

		SlotCollection.Box ReadBox( SlotCollection.Box box )
		{
			var size = new Vector2Int( reader.ReadInt32(), reader.ReadInt32() );
			var sameLine = reader.ReadBoolean();
			var margin = default( Vector2? );
			var noMargin = reader.ReadBoolean();
			if ( !noMargin )
			{
				margin = new Vector2( reader.ReadSingle(), reader.ReadSingle() );
			}

			box ??= new SlotCollection.Box( size, margin, sameLine );
			box.ClearReferences();

			var count = reader.ReadInt32();
			for ( int i = 0; i < count; i++ )
			{
				var position = new Vector2Int( reader.ReadInt32(), reader.ReadInt32() );
				var guid = new Guid( reader.ReadBytes( 16 ) );
				if ( Scene.Directory.FindComponentByGuid( guid ) is not Item item ) // todo cache this if there isn't an item found at that moment..
					continue;

				box.StoreReference( position, item );
			}

			return box;
		}

		SlotCollection ReadCollection( out bool existing )
		{
			var name = reader.ReadString();
			var guid = new Guid( reader.ReadBytes( 16 ) );
			var excludeFlags = (ItemTypeFlags)reader.ReadUInt64();

			var collection = _slotCollections?.FirstOrDefault( collection => collection.Guid == guid ); // Find existing collection.
			existing = collection is not null;

			collection ??= new SlotCollection( this, null ) // Create new collection.
			{
				Name = name,
				Guid = guid,
			};

			collection.WithExcludeFlags( excludeFlags );

			var count = reader.ReadInt32();
			if ( !existing )
			{
				var boxes = new SlotCollection.Box[count];
				for ( int i = 0; i < count; i++ )
					boxes[i] = ReadBox( null );

				collection.SetBoxes( boxes );

				return collection;
			}

			for ( int i = 0; i < count; i++ )
			{
				var box = collection.Boxes.ElementAtOrDefault( i );
				ReadBox( box );
			}

			return collection;
		}

		try
		{
			var refresh = false;
			var valid = new HashSet<SlotCollection>();
			var count = reader.ReadInt32();
			for ( int i = 0; i < count; i++ )
			{
				var collection = ReadCollection( out var existing );
				valid.Add( collection );
				refresh |= !existing;
				if ( !existing )
				{
					_slotCollections.Add( collection );
				}
			}

			foreach ( var collection in _slotCollections )
			{
				if ( valid.Contains( collection ) )
					continue;

				_slotCollections.Remove( collection );
			}

			if ( refresh ) RefreshOrdering();
		} 
		catch { return false; }

		return true;
	}

	/// <summary>
	/// Refresh this <see cref="Container"/>'s <see cref="SlotCollection"/>s state to all client's except the owner of this <see cref="Container"/>...
	/// </summary>
	public void Refresh( IEnumerable<SlotCollection> collections = null )
	{
		if ( IsProxy )
		{
			Log.Warning( $"Tried to refresh container you don't own..." );
			return;
		}

		var data = Serialize( collections );
		BroadcastRefresh( data );
	}

	void INetworkSnapshot.ReadSnapshot( ref ByteStream reader )
	{
		var length = reader.ReadRemaining;
		if ( length == 0 )
			return;

		var buffer = new byte[length];
		reader.Read( buffer, 0, length );

		Deserialize( buffer );
	}

	void INetworkSnapshot.WriteSnapshot( ref ByteStream writer )
	{
		var serialized = Serialize();
		if ( serialized?.Length == 0 )
			return;

		writer.Write( serialized );
	}

	[Rpc.Broadcast]
	private void BroadcastRefresh( byte[] data )
	{
		// No need to run this shit on owner of object...
		if ( !IsProxy ) return;
		
		Deserialize( data );
	}
}
