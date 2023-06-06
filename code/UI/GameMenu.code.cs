using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Pong;

/// <summary>
/// The entry point to the menu game.
/// </summary>
partial class GameMenu
{
	/// <summary>
	/// The only instance of this game in existance.
	/// </summary>
	public static GameMenu Instance { get; private set; } = null!;

	/// <summary>
	/// The first player.
	/// </summary>
	public Paddle Player1 { get; private set; } = null!;
	/// <summary>
	/// The second player.
	/// </summary>
	public Paddle Player2 { get; private set; } = null!;
	/// <summary>
	/// A read only list of all balls currently in play.
	/// </summary>
	public IReadOnlyList<Ball> Balls => balls;
	private readonly List<Ball> balls = new();

	/// <summary>
	/// The size of the playable window in pixels.
	/// </summary>
	public Vector2 WindowSize
	{
		get
		{
			if ( ComputedStyle is null || !ComputedStyle.Width.HasValue || !ComputedStyle.Height.HasValue )
				return 0;

			return new Vector2( ComputedStyle.Width.Value.Value, ComputedStyle.Height.Value.Value );
		}
	}

	private Panel Player1Visual { get; set; } = null!;
	private Panel Player2Visual { get; set; } = null!;

	/// <summary>
	/// The time in seconds until gameplay can begin.
	/// </summary>
	private RealTimeUntil TimeUntilPlay { get; set; } = 3;
	/// <summary>
	/// Whether or not <see cref="StartGame"/> has been called for this round.
	/// </summary>
	private bool RoundStarted { get; set; } = false;

	public GameMenu()
	{
		Instance = this;
	}

	~GameMenu()
	{
		// Cleanup after ourselves.
		Event.Unregister( Player1 );
		Event.Unregister( Player2 );
		foreach ( var ball in Balls )
			Event.Unregister( ball );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		base.Tick();

		// Allow the player to move around whenever.
		Player1.Tick();

		if ( TimeUntilPlay > 0 )
			return;

		if ( !RoundStarted )
			StartGame();

		Player2.Tick();
		foreach ( var ball in Balls )
			ball.Tick();
	}

	/// <inheritdoc/>
	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		AddBall();
	}

	/// <inheritdoc/>
	protected override void OnMiddleClick( MousePanelEvent e )
	{
		base.OnMiddleClick( e );

		if ( Balls.Count > 1 )
			RemoveBall();
	}

	/// <inheritdoc/>
	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
			return;

		Player1 = new Paddle( true, Game.UserName, Player1Visual );
		Event.Register( Player1 );
		Player2 = new Paddle( false, "CPU", Player2Visual );
		Event.Register( Player2 );
	}

	/// <inheritdoc/>
	protected override int BuildHash()
	{
		if ( Player1 is null || Player2 is null || Balls.Count == 0 )
			return HashCode.Combine( (float)TimeUntilPlay, RoundStarted );

		var hashCode = HashCode.Combine( Player1.GetHashCode(), Player2.GetHashCode(), (float)TimeUntilPlay, RoundStarted );
		foreach ( var ball in Balls )
			hashCode ^= ball.GetHashCode();

		return hashCode;
	}

	/// <summary>
	/// Adds a new ball to the game.
	/// </summary>
	private void AddBall()
	{
		var ball = new Ball( AddChild<Panel>( "ball" ) )
		{
			Position = WindowSize / 2
		};

		balls.Add( ball );
		Event.Register( ball );
	}

	/// <summary>
	/// Removes a ball from the game.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when trying to remove a ball when there is only one left.</exception>
	private void RemoveBall()
	{
		if ( Balls.Count == 1 )
			throw new InvalidOperationException();

		var ball = Balls[0];
		balls.Remove( ball );
		Event.Unregister( ball );
		ball.Delete();
	}

	/// <summary>
	/// Starts a new round.
	/// </summary>
	private void StartGame()
	{
		if ( Balls.Count == 0 )
			AddBall();

		Event.Run( "pong.gamestart" );
		RoundStarted = true;
	}

	/// <summary>
	/// Resets the game so that a new round can begin.
	/// </summary>
	/// <param name="playerWon">The player that won the round that just passed.</param>
	private void ResetGame( Paddle? playerWon )
	{
		if ( playerWon is not null )
			playerWon.Score++;

		TimeUntilPlay = 3;
		RoundStarted = false;

		Event.Run( "pong.gamereset" );
	}

	/// <summary>
	/// Invoked when a paddle has intercepted a ball.
	/// </summary>
	/// <param name="ball">The ball that was intercepted.</param>
	/// <param name="paddle">The paddle that intercepted the ball.</param>
	[Event( "pong.paddlecollide" )]
	private void OnPaddleCollide( Ball ball, Paddle paddle )
	{
		ball.MovementSpeed += 40;
	}

	/// <summary>
	/// Invoked when a ball has reached a goal.
	/// </summary>
	/// <param name="ball">The ball that reached a goal.</param>
	/// <param name="isLeftGoal">Whether or not the goal was the left one.</param>
	[Event( "pong.ballhitgoal" )]
	private void OnBallHitGoal( Ball ball, bool isLeftGoal )
	{
		ResetGame( isLeftGoal ? Player2 : Player1 );
	}
}
