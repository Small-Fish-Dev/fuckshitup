namespace FUCKSHIT;

partial class GameManager
{
	[ConCmd( "give_item" )]
	private static void CmdGiveItem( string name = "<NOTHING>" )
	{
		if ( !Character.Local.IsValid() )
			return;

		var items = PrefabLibrary.FindByComponent<Item>();

		var foundItem = items.Where( prefab =>
		{
			var toFind = name.ToLower().Replace( " ", "" ).Replace( "_", "" ).Replace( ".", "" );
			var item = prefab.Components.Get<Item>();
			if ( !item.IsValid() )
				return false;

			var itemName = item.Name.ToLower().Replace( " ", "" ).Replace( "_", "" ).Replace( ".", "" );
			var objectName = prefab.Name.ToLower().Replace( " ", "" ).Replace( "_", "" ).Replace( ".", "" );

			if ( itemName == toFind || objectName == toFind )
				return true;

			if ( itemName.Contains( toFind, StringComparison.OrdinalIgnoreCase ) || objectName.Contains( toFind, StringComparison.OrdinalIgnoreCase ) )
				return true;

			return false;
		} ).FirstOrDefault();

		if ( foundItem is null )
		{
			Log.Info( $"The item was not found, here is a list of available items:" );

			var availableItems = "";

			foreach ( var availableItem in items )
				availableItems += $"[{availableItem.GetComponent<Item>()?.Name}], ";

			Log.Info( availableItems );
			Log.Info( "You may also use partial item names or any combination of words and letters, I'll try my best to find the item." );

			return;
		}

		using ( Game.ActiveScene.Push() )
		{
			var itemGameObject = foundItem.Clone();
			var itemComponent = itemGameObject.Components.Get<Item>();

			if ( !itemComponent.IsValid() )
			{
				itemGameObject.Destroy();
				return;
			}

			itemGameObject.SetupNetworking( Connection.Local );

			var fit = Character.Local.Inventory.TryInsert( itemComponent );
			if ( !fit )
			{
				Log.Warning( $"There was no space for this item so it has been placed on the ground next to you." );

				itemGameObject.WorldPosition = Character.Local.WorldPosition + Vector3.Forward * 50f;
			}
		}
	}

	[ConCmd( "reset_character" )]
	private static void CmdResetCharacter( string name = "<NOTHING>" )
	{
		if ( !Character.Local.IsValid() )
			return;

		Character.Local.RequestRespawn();
	}
}
