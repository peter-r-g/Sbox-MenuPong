using System;
using Sandbox;
using Sandbox.UI;

namespace Pong;

/// <summary>
/// Represents a ball that paddles can hit.
/// </summary>
public class Ball
{
	/// <summary>
	/// The defualt speed that balls move at.
	/// </summary>
	public const float DefaultMovementSpeed = 300;

	/// <summary>
	/// The current speed the ball is moving at.
	/// </summary>
	public float MovementSpeed { get; set; } = DefaultMovementSpeed;

	/// <summary>
	/// The current direction the ball is moving in.
	/// </summary>
	public Vector2 Direction { get; private set; }

	/// <summary>
	/// Gets or sets the pixel position of the balls top left corner.
	/// </summary>
	public Vector2 Position
	{
		get
		{
			if ( Visual.Style is null )
				return Vector2.Zero;

			if ( !Visual.Style.Left.HasValue || !Visual.Style.Top.HasValue )
				return Vector2.Zero;

			return new Vector2(
				Visual.Style.Left.Value.GetPixels( GameMenu.Instance.WindowSize.x ),
				Visual.Style.Top.Value.GetPixels( GameMenu.Instance.WindowSize.y ) );
		}
		set
		{
			var positionPercent = value / GameMenu.Instance.WindowSize * 100;
			var ballSizePercent = Size / GameMenu.Instance.WindowSize * 100;
			Visual.Style.Left = Length.Percent( Math.Clamp( positionPercent.x, 0, 100 - ballSizePercent.x ) );
			Visual.Style.Top = Length.Percent( Math.Clamp( positionPercent.y, 0, 100 - ballSizePercent.y ) );
		}
	}

	/// <summary>
	/// Gets the pixel size of the balls visual element.
	/// </summary>
	public Vector2 Size
	{
		get
		{
			if ( Visual.ComputedStyle is null )
				return Vector2.One;

			if ( !Visual.ComputedStyle.Width.HasValue || !Visual.ComputedStyle.Height.HasValue )
				return Vector2.One;

			return new Vector2(
				Visual.ComputedStyle.Width.Value.GetPixels( GameMenu.Instance.WindowSize.x ),
				Visual.ComputedStyle.Height.Value.GetPixels( GameMenu.Instance.WindowSize.y ) );
		}
	}

	/// <summary>
	/// The visual element of the ball.
	/// </summary>
	private Panel Visual { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="Ball"/>.
	/// </summary>
	/// <param name="ballVisual">The visual panel to give the ball.</param>
	public Ball( Panel ballVisual )
	{
		Visual = ballVisual;
		RandomizeDirection();
	}

	/// <summary>
	/// Ticks the balls behavior
	/// </summary>
	public virtual void Tick()
	{
		// Move ball.
		var ballDelta = Direction * MovementSpeed * Time.Delta;
		var oldPosition = Position;
		Position += ballDelta;

		// Check if we've hit the top or bottom wall.
		if ( Math.Abs( oldPosition.y - Position.y ) <= 0.0001f )
			Direction = new Vector2( Direction.x, -Direction.y );

		// Check if we've hit the paddles.
		if ( GameMenu.Instance.Player1.IsCollidingWith( Visual.Box.Rect ) && Direction.x != 1 )
		{
			Direction = new Vector2( 1, Direction.y );
			Event.Run( "pong.paddlecollide", this, GameMenu.Instance.Player1 );
		}

		if ( GameMenu.Instance.Player2.IsCollidingWith( Visual.Box.Rect ) && Direction.x != -1 )
		{
			Direction = new Vector2( -1, Direction.y );
			Event.Run( "pong.paddlecollide", this, GameMenu.Instance.Player2 );
		}

		// Check if we've scored.
		if ( Position.x <= 0 )
			Event.Run( "pong.ballhitgoal", this, true );

		if ( Position.x >= GameMenu.Instance.WindowSize.x - Size.x )
			Event.Run( "pong.ballhitgoal", this, false );
	}

	/// <summary>
	/// Deletes the ball and any elements associated with it.
	/// </summary>
	public virtual void Delete()
	{
		Visual.Delete();
	}

	/// <summary>
	/// Randomizes the direction the ball is going in.
	/// </summary>
	private void RandomizeDirection()
	{
		var ballDirectionX = Random.Shared.Next( -1, 2 );
		if ( ballDirectionX == 0 )
			ballDirectionX = -1;

		var ballDirectionY = Random.Shared.Next( -1, 2 );
		if ( ballDirectionY == 0 )
			ballDirectionY = -1;

		Direction = new Vector2( ballDirectionX, ballDirectionY );
	}

	/// <summary>
	/// Resets the ball once the game starts.
	/// </summary>
	[Event( "pong.gamestart" )]
	private void GameStart()
	{
		MovementSpeed = DefaultMovementSpeed;
		Position = GameMenu.Instance.WindowSize / 2;

		RandomizeDirection();
	}

	/// <summary>
	/// Resets the position of the ball when the game ends.
	/// </summary>
	[Event( "pong.gamereset" )]
	private void GameReset()
	{
		Position = GameMenu.Instance.WindowSize / 2;
	}
}
