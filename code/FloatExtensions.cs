namespace Pong;

/// <summary>
/// Contains extension methods for <see cref="float"/>s.
/// </summary>
public static class FloatExtensions
{
	/// <summary>
	/// Converts a pixel amount to a 0-100 percentage according to the dimension it fits in.
	/// </summary>
	/// <param name="pixels">The pixel amount to convert to a percentage.</param>
	/// <param name="dimension">The length of the dimension to fit pixels into.</param>
	/// <returns>A 0-100 percentage according to the dimension it fits in.</returns>
	public static float FromPixelToPercent( this float pixels, float dimension )
	{
		return pixels / dimension * 100;
	}

	/// <summary>
	/// Converts a pixel amount to a 0-100 percentage according to the width of the game window.
	/// </summary>
	/// <param name="pixels">The pixel amount to convert to a percentage.</param>
	/// <returns>A 0-100 percentage according to the width of the game window.</returns>
	public static float FromPixelToWidthPercent( this float pixels )
	{
		return pixels.FromPixelToPercent( GameMenu.Instance.WindowSize.x );
	}

	/// <summary>
	/// Converts a pixel amount to a 0-100 percentage according to the height of the game window.
	/// </summary>
	/// <param name="pixels">The pixel amount ot convert to a percentage.</param>
	/// <returns>A 0-100 percentage according to the height of the game window.</returns>
	public static float FromPixelToHeightPercent( this float pixels )
	{
		return pixels.FromPixelToPercent( GameMenu.Instance.WindowSize.y );
	}
}
