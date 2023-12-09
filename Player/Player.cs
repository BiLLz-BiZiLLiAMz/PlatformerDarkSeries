using Godot;
using System;
using System.Data;

public partial class Player : CharacterBody2D
{

	#region Player Child Nodes
	private Sprite2D _sprite = null;
	private AnimationPlayer _animator = null;
	private Label _debugLabel = null;
	private Timer _coyoteTime = null;

	#endregion

	#region Physics Constants
	// Player Physics Constants
	private const float MoveSpeed = 190.0f;
	private const float MoveAcceleration = 1800.0f;
	private const float Friction = 2000.0f;
	private const float Gravity = 600.0f; //ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	private const float GravityScale = 1.0f;
	private const float JumpVelocity = -300.0f;
	private const float JumpDoubleVelocity = -200.0f;
	private const float AirResistance = 2000.0f;
	private const float AirAcceleration = 1800.0f;
	private const int MaxJumps = 2;
	private int jumps = 0;
	private const float WallSlideSpeed = 80.0f;
	private const float WallSlideAcceleration = 200.0f;
	private const float WallJumpVelocityY = -250.0f;
	private const float WallJumpVelocityX = 170.0f;

	// Actual Movement
	protected Vector2 velocity = Vector2.Zero;
	private Vector2 direction = Vector2.Zero;
	private float inputDirection = 0;

	// Player raycasts
	public RayCast2D topLeft = null;
	public RayCast2D topRight = null;
	public RayCast2D rightTop = null;
	public RayCast2D rightBottom = null;
	public RayCast2D bottomRight = null;
	public RayCast2D bottomLeft = null;
	public RayCast2D leftBottom = null;
	public RayCast2D leftTop = null;
	public int wallNormal = 0;

	public enum RayCast
	{
		TopLeft,
		TopRight,
		RightTop,
		RightBottom,
		BottomRight,
		BottomLeft,
		LeftBottom,
		LeftTop
	}

	#endregion

	#region PLAYER STATE DEFINITIONS ////////////////////////////////////////////////////////////////////////////////////////////////

	public enum PlayerStates
	{
		Locked,
		Grounded,
		Air,
		Jump,
		JumpDouble,
		WallSlide,
		WallSlideJump,
	}

	public string stateName = "NULL";

	PlayerStates currentState;
	PlayerStates previousState;

	#endregion

	#region MAIN FUNCTIONS ////////////////////////////////////////////////////////////////////////////////////////////////
	//Ready
	public override void _Ready()
	{
		// Get the player nodes
		_sprite = GetNode<Sprite2D>("Spritesheet");
		_animator = GetNode<AnimationPlayer>("AnimationPlayer");
		_debugLabel = GetNode<Label>("DebugLabel");
		_coyoteTime = GetNode<Timer>("CoyoteTime");

		// Get the player raycasts
		topLeft = GetNode<RayCast2D>("TopLeft");
		topRight = GetNode<RayCast2D>("TopRight");
		rightTop = GetNode<RayCast2D>("RightTop");
		rightBottom = GetNode<RayCast2D>("RightBottom");
		bottomRight = GetNode<RayCast2D>("BottomRight");
		bottomLeft = GetNode<RayCast2D>("BottomLeft");
		leftBottom = GetNode<RayCast2D>("LeftBottom");
		leftTop = GetNode<RayCast2D>("LeftTop");


		// Player States
		currentState = PlayerStates.Grounded;

		// Connect the animation finished signal
		_animator.AnimationFinished += OnAnimationFinished;
		_animator.AnimationChanged += OnAnimationChanged;
	}

	private void OnAnimationChanged(StringName oldName, StringName newName)
	{
		// the animation has changed
	}

	private void OnAnimationFinished(StringName animName)
	{
		// the current animation has finished
	}

	// UNHANDLED INPUT
	public override void _UnhandledInput(InputEvent @event)
	{
		// Handle input
	}

	// PROCESS
	public override void _Process(double delta)
	{
		// Set the label text to the current state
		_debugLabel.Text = stateName;
	}

	// PHYSICS PROCESS
	public override void _PhysicsProcess(double delta)
	{
		// Print the velocities
		//GD.Print(Velocity);

		// Get the current input
		inputDirection = Input.GetActionStrength("right") - Input.GetActionStrength("left");

		// Update the state
		switch (currentState)
		{
			case PlayerStates.Locked:

				PlayerStateLocked(delta);

			break;

			case PlayerStates.Grounded:

				PlayerStateGrounded(delta);

			break;

			case PlayerStates.Air:

				PlayerStateAir(delta);

			break;

			case PlayerStates.Jump:

				PlayerStateJump(delta);

			break;

			case PlayerStates.JumpDouble:

				PlayerStateJumpDouble(delta);

			break;

			case PlayerStates.WallSlide:

				PlayerStateWallSlide(delta);

			break;

			case PlayerStates.WallSlideJump:

				PlayerStateWallSlideJump(delta);

			break;

			default:
				PlayerStateAir(delta);
			break;



		}

		// Flip the sprite facing direction
		if (Input.IsActionPressed("left"))
		{
			_sprite.FlipH = true;
		}
		if (Input.IsActionPressed("right"))
		{
			_sprite.FlipH = false;
		}

	}

	public void GetVelocity()
	{
		velocity = Velocity;
	}

	public void SetVelocity()
	{
		Velocity = velocity;
	}

	#endregion

	#region RAYCAST COLLISIONS

	public bool GetRayCastCollision(RayCast ray)
	{
		bool value = false;

		switch (ray)
			{
				case (RayCast.TopLeft):
					value = topLeft.IsColliding();
				break;

				case (RayCast.TopRight):
					value = topRight.IsColliding();
				break;

				case (RayCast.RightTop):
					value = rightTop.IsColliding();
				break;

				case (RayCast.RightBottom):
					value = rightBottom.IsColliding();
				break;

				case (RayCast.BottomRight):
					value = bottomRight.IsColliding();
				break;

				case (RayCast.BottomLeft):
					value = bottomLeft.IsColliding();
				break;

				case (RayCast.LeftBottom):
					value = leftBottom.IsColliding();
				break;

				case (RayCast.LeftTop):
					value = leftTop.IsColliding();
				break;

				default:
					value = false;
				break;
			}
		return value;
	}

	public bool GetSideColliding(String side)
	{
		bool value = false;

		switch (side)
		{
			case ("Top"):
				value = (GetRayCastCollision(RayCast.TopLeft) && GetRayCastCollision(RayCast.TopRight));
			break;

			case ("Right"):
				value = (GetRayCastCollision(RayCast.RightTop) && GetRayCastCollision(RayCast.RightBottom));
			break;

			case ("Bottom"):
				value = (GetRayCastCollision(RayCast.BottomRight) && GetRayCastCollision(RayCast.BottomLeft));
			break;

			case ("Left"):
				value = (GetRayCastCollision(RayCast.LeftBottom) && GetRayCastCollision(RayCast.LeftTop));
			break;

			default:
				value = false;
			break;
		}

		return value;
	}

	#endregion

	#region PLAYER STATES

	public void ChangeState(PlayerStates state)
	{
		previousState = currentState;
		currentState = state;
	}

	public void PlayerStateLocked(double delta)
	{
		// Do nothing
	}

	public void PlayerStateGrounded(double delta)
	{
		stateName = "GROUNDED";
		// Get the current velocity vector
		GetVelocity();

		// Transition to air if in the air
		if (!IsOnFloor())
		{
			//start the coyote timer
			_coyoteTime.Start();
			ChangeState(PlayerStates.Air);
		}
		else
		{
			jumps = MaxJumps;
		}

		// Handle jumping
		if (Input.IsActionJustPressed("up"))
		{
			ChangeState(PlayerStates.Jump);
		}

		// Handle horizontal input
		if (inputDirection != 0)
		{
			velocity.X = Mathf.MoveToward(velocity.X, MoveSpeed * inputDirection, MoveAcceleration * (float)delta);
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * (float)delta);
		}

		// Commit movement
		SetVelocity();
		MoveAndSlide();

		// player animation
		if (inputDirection != 0 && Velocity.X != 0)
		{
			_animator.Play("Run");
		}
		else
		{
			_animator.Play("Idle");
		}
	}

	public void PlayerStateAir(double delta)
	{
		stateName = "AIR";
		// Get the current velocity vector
		GetVelocity();

		// Add the gravity.
		velocity.Y += Gravity * GravityScale * (float)delta;

		// Handle jumping
		if (Input.IsActionJustPressed("up"))
		{
			// Coyote time
			if ((_coyoteTime.TimeLeft > 0) && (jumps > 0))
			{
				// stop the coyote timer
				_coyoteTime.Stop();
				ChangeState(PlayerStates.Jump);
			}
			else if (jumps > 0)
			{
				// double jump
				ChangeState(PlayerStates.JumpDouble);
			}

		}

		// Horizontal movement in air
		if (inputDirection != 0)
		{
			velocity.X = Mathf.MoveToward(velocity.X, MoveSpeed * inputDirection, MoveAcceleration * (float)delta);
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, AirResistance * (float)delta);
		}

		// Commit movement
		SetVelocity();
		MoveAndSlide();

		// see if we have landed
		if (IsOnFloor())
		{
			ChangeState(PlayerStates.Grounded);
		}

		// wall grab
		var _wallNormal = GetWallNormal();
		if (GetSideColliding("Left") && Input.IsActionPressed("left"))
		{
			// wall is to the left
			velocity = Vector2.Zero;
			Velocity = Vector2.Zero;
			wallNormal = -1;
			ChangeState(PlayerStates.WallSlide);
		}
		else if (GetSideColliding("Right") && Input.IsActionPressed("right"))
		{
			// wall is to the right
			velocity = Vector2.Zero;
			Velocity = Vector2.Zero;
			wallNormal = 1;
			ChangeState(PlayerStates.WallSlide);
		}

		// Player animation
		if (Velocity.Y <= 0)
		{
			if (jumps < 1)
			{
				_animator.Play("Flip");
			}
			else
			{
				_animator.Play("JumpUp");
			}
		}
		else
		{
			_animator.Play("Fall");
		}

	}

	public void PlayerStateJump(double delta)
	{
		stateName = "JUMP";
		// Jump!
		velocity.Y = JumpVelocity;
		SetVelocity();
		jumps--;
		ChangeState(PlayerStates.Air);
	}

	public void PlayerStateJumpDouble(double delta)
	{
		stateName = "JUMP DOUBLE";
		// Jump!
		velocity.Y = JumpDoubleVelocity;
		SetVelocity();
		jumps--;
		ChangeState(PlayerStates.Air);
	}

	public void PlayerStateWallSlide(double delta)
	{
		stateName = "WALL SLIDE";
		// wall slide
		velocity.Y = Mathf.MoveToward(velocity.Y, WallSlideSpeed, WallSlideAcceleration * (float)delta);

		// commit to movement
		SetVelocity();
		MoveAndSlide();

		// see if the player wants to wall jump
		if (Input.IsActionJustPressed("up"))
		{
			// jump off the wall
			velocity.Y = WallJumpVelocityY;
			velocity.X = WallJumpVelocityX * wallNormal * -1;
			ChangeState(PlayerStates.WallSlideJump);
		}
		
		// see if the player has let go of the wall
		if (Input.IsActionJustReleased("left") || Input.IsActionJustReleased("right"))
		{
			ChangeState(PlayerStates.Air);
		}

		// see if a wall ends
		// potential ray cast off the top and bottom of the player collision rect in order to better adjust this: Position + (Vector2.up*(rect.Height/2))
		if (!GetSideColliding("Right") && !GetSideColliding("Left"))
		{
			ChangeState(PlayerStates.Air);
		}

		// see if we hit the floor
		if (IsOnFloor())
		{
			ChangeState(PlayerStates.Grounded);
		}

		// set the sprite to face the proper direction
		_sprite.FlipH = GetWallNormal().X > 0;
		_animator.Play("WallSlide");
		}

	public void PlayerStateWallSlideJump(double delta)
	{
		stateName = "WALL JUMP";
		// grab the current velocity
		//velocity = Velocity;

		// add the gravity
		velocity.Y += Gravity * GravityScale * (float)delta;

		// move
		SetVelocity();
		MoveAndSlide();

		// hand control back to the player
		if (velocity.Y > 0)
		{
			ChangeState(PlayerStates.Air);
		}

		// player animation
		_sprite.FlipH = (velocity.X < 0);
		_animator.Play("JumpUp");
	}

	#endregion

}
