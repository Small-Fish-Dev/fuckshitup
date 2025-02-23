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

	public static string GetBone( this Limb self )
		=> self switch
		{
			Limb.Head => "head",
			Limb.Torso => "spine2",
			Limb.RightArm => "arm_lower_R",
			Limb.LeftArm => "arm_lower_L",
			Limb.RightLeg => "leg_lower_R",
			Limb.LeftLeg => "leg_lower_L",
			_ => "pelvis",
		};
}
