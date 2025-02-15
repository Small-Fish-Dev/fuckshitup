namespace FUCKSHIT;

public enum ItemRarity : byte
{
	/// <summary>
	/// Meh... It can suck..  
	/// <para>BUT..! some could come in handy.</para>
	/// </summary>
	[Icon( "🙁" )]
	Default,

	/// <summary>
	/// This item is quite good.
	/// <para>Think of normal weapons like the AK-47, </para>
	/// or cool looking clothes or something...
	/// </summary>
	[Icon( "😎" )]
	Rare,

	/// <summary>
	/// This item is pretty great.
	/// <para>Should only be given to items with great value</para>
	/// like expensive currencies and equipment.
	/// </summary>
	[Icon( "🤑" )]
	Legendary,

	/// <summary>
	/// This item is the best of the best...
	/// <para>Think LMGs, metal facemasks and such..</para>
	/// </summary>
	[Icon( "😱" )]
	Mythical
}

public static class ItemRarityExtensions
{
	public static Color GetColor( this ItemRarity rarity )
		=> rarity switch
		{
			ItemRarity.Mythical => new Color32( 255, 61, 61 ),
			ItemRarity.Legendary => new Color32( 255, 235, 81 ),
			ItemRarity.Rare => new Color( 142, 185, 255 ),
			_ => new Color32( 125, 125, 125 ),
		};
}
