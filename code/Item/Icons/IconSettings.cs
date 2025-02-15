namespace FUCKSHIT;

public struct IconSettings : IEquatable<IconSettings>
{
	public Vector3 Offset { get; set; } = Vector3.Zero;
	public Rotation Rotation { get; set; } = Rotation.Identity;

	public IconSettings() { }

	public override int GetHashCode()
	{
		return HashCode.Combine( Offset, Rotation );
	}

	public override bool Equals( object obj )
	{
		return obj is IconSettings other
			&& Equals( other );
	}

	public bool Equals( IconSettings other )
	{
		return other.Offset.AlmostEqual( Offset ) && Rotation == other.Rotation;
	}
}
