namespace FUCKSHIT.Editor;

public sealed class DropdownWidget<T> : DropdownControlWidget<T>
{
	public List<T> Options { get; set; }

	public DropdownWidget( SerializedProperty property ) : base( property )
	{
	}

	protected override IEnumerable<object> GetDropdownValues()
	{
		if ( Options == null ) yield break;

		foreach ( var option in Options )
			yield return option;
	}
}
