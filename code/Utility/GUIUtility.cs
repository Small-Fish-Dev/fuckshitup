namespace FUCKSHIT;

public static class GUIUtility
{
	/// <summary>
	/// Generates a stripe using linear-gradient.
	/// <para>Result will look something like: <code>linear-gradient(0deg, #FF0000 0%, #FF0000 10%, ...)</code></para>
	/// </summary>
	/// <param name="col1"></param>
	/// <param name="col2"></param>
	/// <param name="angle"></param>
	/// <param name="stops"></param>
	/// <param name="smoothing"></param>
	/// <returns></returns>
	public static string StripeGradient( Color col1, Color col2, int angle = 45, int stops = 20, float smoothing = 0f )
	{
		var hex1 = col1.Hex[0..7];
		var hex2 = col2.Hex[0..7];

		// Generate stripes...
		var stringBuilder = new StringBuilder();
		stringBuilder.Append( $"linear-gradient({angle}deg" );
		for ( int i = 0; i < stops; i++ )
		{
			var color = i % 2 == 0 ? hex1 : hex2;

			var first = i == 0 
				? 0
				: i / (float)stops * 100f + smoothing;

			var second = i == stops - 1
				? 100
				: ((i + 1) / (float)stops * 100f - smoothing);

			stringBuilder.Append($", {color} {first:N1}%, {color} {second:N1}%" );
		}

		// Finish StringBuilder.
		stringBuilder.Append( ')' );
		return stringBuilder.ToString();
	}
}
