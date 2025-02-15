namespace FUCKSHIT;

public enum Limb : byte
{
	[Icon( "🤕" )]
	Head,

	[Icon( "🤰" )]
	Torso,

	[Icon( "💪" )]
	RightArm,

	[Icon( "💪" )]
	LeftArm,

	[Icon( "🦶" )]
	RightLeg,

	[Icon( "🦶" )]
	LeftLeg
}

public static class LimbExtensions
{
	public static Limb GetByTag( string tag )
		=> tag switch
		{
			"head" => Limb.Head,
			"right_arm" => Limb.RightArm,
			"left_arm" => Limb.LeftArm,
			"right_leg" => Limb.RightLeg,
			"left_leg" => Limb.LeftLeg,
			_ => Limb.Torso
		};
}
