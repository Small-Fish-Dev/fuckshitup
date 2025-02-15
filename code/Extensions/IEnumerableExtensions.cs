namespace FUCKSHIT;

public static class EnumerableExtensions
{
	public static int HashCombine<T>( this IEnumerable<T> self, Func<T, decimal> selector )
	{
		var result = 0;

		foreach ( var element in self )
			result = HashCode.Combine( result, selector.Invoke( element ) );

		return result;
	}

	public static int IndexOf<T>( this IEnumerable<T> self, T elementToFind )
	{
		var i = 0;

		foreach ( var element in self )
		{
			if ( Equals( element, elementToFind ) )
				return i;

			i += 1;
		}

		return -1;
	}
}
