using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace Pong;

/// <summary>
/// Represents a playable paddle that protects a goal.
/// </summary>
public class Paddle
{
	/// <summary>
	/// The movement speed of the AI paddle.
	/// </summary>
	public const float CpuMovementSpeed = 800;

	/// <summary>
	/// Whether or not this paddle is controlled by a real human.
	/// </summary>
	public bool IsPlayer { get; }
	/// <summary>
	/// The name of the player controlling the paddle.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The current score of the paddle.
	/// </summary>
	public int Score { get; set; }

	/// <summary>
	/// Gets or sets the pixel height of the paddle.
	/// </summary>
	public float Height
	{
		get
		{
			if ( Visual.Style is null || Visual.ComputedStyle is null || !Visual.ComputedStyle.Height.HasValue )
				return 0;

			if ( !Visual.Style.Height.HasValue )
				Visual.Style.Height = Visual.ComputedStyle.Height;

			return Visual.Style.Height.Value.GetPixels( GameMenu.Instance.WindowSize.y );
		}
		set
		{
			Visual.Style.Height = Length.Percent( value.FromPixelToHeightPercent() );
		}
	}

	/// <summary>
	/// Gets or sets the pixel Y position of the paddle.
	/// </summary>
	public float Position
	{
		get
		{
			if ( Visual.Style is null || Visual.ComputedStyle is null || !Visual.ComputedStyle.Top.HasValue )
				return 0;

			if ( !Visual.Style.Top.HasValue )
				Visual.Style.Top = Visual.ComputedStyle.Top;

			return Visual.Style.Top.Value.GetPixels( GameMenu.Instance.WindowSize.y );
		}
		set
		{
			var desiredPercent = value.FromPixelToHeightPercent();
			Visual.Style.Top = Length.Percent( Math.Clamp( desiredPercent, 0, 100 - Height.FromPixelToHeightPercent() ) );
		}
	}

	/// <summary>
	/// The visual element associated with this paddle.
	/// </summary>
	private Panel Visual { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="Paddle"/>.
	/// </summary>
	/// <param name="isPlayer">Whether or not the paddle will be controlled by a human.</param>
	/// <param name="name">The name of the player controlling the paddle.</param>
	/// <param name="paddle">The visual element of the paddle.</param>
	public Paddle( bool isPlayer, string name, Panel paddle )
	{
		IsPlayer = isPlayer;
		Name = name;
		Visual = paddle;
	}

	/// <summary>
	/// Returns whether or not the paddle is colliding with any part of a <see cref="Rect"/>.
	/// </summary>
	/// <param name="rect">The rect to check for collisions on.</param>
	/// <returns></returns>
	public bool IsCollidingWith( in Rect rect )
	{
		return Visual.IsInside( rect, false );
	}

	/// <summary>
	/// Ticks the paddles behavior.
	/// </summary>
	public virtual void Tick()
	{
		if ( IsPlayer )
			TickPlayerMovement();
		else
			TickCpuMovement();
	}

	/// <summary>
	/// Ticks the human controlled movement.
	/// </summary>
	protected virtual void TickPlayerMovement()
	{
		Position = Mouse.Position.y - Height / 2;
	}

	/// <summary>
	/// Ticks the AI controlled movement.
	/// </summary>
	protected virtual void TickCpuMovement()
	{
		var position = Position;
		var middlePosition = position + Height / 2;

		// Find closest ball heading towards the paddle.
		var closestBall = GameMenu.Instance.Balls.FirstOrDefault( ball => ball.Direction.x > 0 );
		closestBall ??= GameMenu.Instance.Balls[0];

		var currentDistance = Math.Abs( (closestBall.Position - Visual.Box.Rect.Position).LengthSquared );
		foreach ( var ball in GameMenu.Instance.Balls )
		{
			if ( ball.Direction.x < 0 )
				continue;

			var distance = Math.Abs( (ball.Position - Visual.Box.Rect.Position).LengthSquared );
			if ( distance >= currentDistance )
				continue;

			closestBall = ball;
			currentDistance = distance;
		}

		// Move to intercept the ball.
		if ( closestBall.Position.y > middlePosition )
			position += CpuMovementSpeed * Time.Delta;
		else if ( closestBall.Position.y < middlePosition )
			position -= CpuMovementSpeed * Time.Delta;

		Position = position;
	}
}
