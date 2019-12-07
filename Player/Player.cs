using Godot;
using System;
using static MovementState;

public class Player : KinematicBody2D
{
    [Export] private float WalkSpeed = 150f;
    [Export] private float SprintSpeed = 300f;
    [Export] private float Gravity = 40f;
    [Export] private float MaxJumpHeight = 600f;

    private RayCast2D GroundRayCast;
    private MovementState PMovementState = MovementState.Still;
    private Vector2 Velocity = new Vector2();
    private float JumpSpeed = 0f;
    private Hook _Hook;

    public bool IsGrounded
    {
        get
        {
            return GroundRayCast.IsColliding();
        }
    }

    public override void _Ready()
    {
        GroundRayCast = GetNode<RayCast2D>("GroundRayCast");
        _Hook = GetNode<Hook>("../Hook");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Velocity.y >= 0)
        {
            Velocity.y += Gravity;
        }
        else
        {
            /* In platformers, you generally don't feel as strong of a gravitational pull
               when you're falling vs. when you're falling */
            Velocity.y += Gravity * 0.55f;
        }

        Vector2 platformVelocity = Vector2.Zero;
        if (IsGrounded)
        {
            Godot.Object floor = GroundRayCast.GetCollider();
            if (floor is MovingPlatform)
            {
                MovingPlatform movingPlatform = (MovingPlatform)floor;
                platformVelocity = movingPlatform.GetVelocity(delta);
            }
        }

        Velocity.x += platformVelocity.x;

        if (Input.IsActionPressed("right"))
        {
            if (PMovementState == Still)
            {
                if (Input.IsActionPressed("sprint"))
                {
                    PMovementState = SprintingRight;
                    Velocity.x += SprintSpeed;
                }
                else
                {
                    PMovementState = WalkingRight;
                    Velocity.x += WalkSpeed;
                }
            }
        }

        if (Input.IsActionPressed("left"))
        {
            if (PMovementState == Still)
            {
                if (Input.IsActionPressed("sprint"))
                {
                    PMovementState = SprintingLeft;
                    Velocity.x -= SprintSpeed;
                }
                else
                {
                    PMovementState = WalkingLeft;
                    Velocity.x -= WalkSpeed;
                }
            }
        }

        if (PMovementState == WalkingRight)
        {
            if (Input.IsActionPressed("sprint"))
            {
                PMovementState = SprintingRight;
                Velocity.x += (SprintSpeed - WalkSpeed);
            }
        }

        if (PMovementState == WalkingLeft)
        {
            if (Input.IsActionPressed("sprint"))
            {
                PMovementState = SprintingLeft;
                Velocity.x -= (SprintSpeed - WalkSpeed);
            }
        }

        if (Input.IsActionJustReleased("right"))
        {
            if (PMovementState == WalkingRight)
            {
                PMovementState = Still;
                if (Velocity.x != 0)
                    Velocity.x -= WalkSpeed;
            }

            if (PMovementState == SprintingRight)
            {
                PMovementState = Still;
                if (Velocity.x != 0)
                    Velocity.x -= SprintSpeed;
            }
        }

        if (Input.IsActionJustReleased("left"))
        {
            if (PMovementState == WalkingLeft)
            {
                PMovementState = Still;
                if (Velocity.x != 0)
                    Velocity.x += WalkSpeed;
            }
            
            if (PMovementState == SprintingLeft)
            {
                PMovementState = Still;
                if (Velocity.x != 0)
                    Velocity.x += SprintSpeed;
            }
        }

        if (Input.IsActionJustReleased("sprint"))
        {
            if (PMovementState == SprintingRight)
            {
                PMovementState = WalkingRight;
                if (Velocity.x != 0)
                    Velocity.x -= (SprintSpeed - WalkSpeed);
            }

            if (PMovementState == SprintingLeft)
            {
                PMovementState = WalkingLeft;
                if (Velocity.x != 0)
                    Velocity.x += (SprintSpeed - WalkSpeed);
            }
        }

        if (JumpSpeed != 0)
        {
            Velocity.y -= JumpSpeed;
            JumpSpeed = 0;
        }

        if (Input.IsActionJustReleased("jump"))
        {
            if (!IsGrounded)
            {
                if (Velocity.y < 0)
                {
                    Velocity.y = (Velocity.y * 0.6f);
                }
            }
        }

        Velocity = MoveAndSlide(Velocity, infiniteInertia: false);

        Velocity.x -= platformVelocity.x;

        if ((JumpSpeed == 0) && (PMovementState == Still))
        {
            Velocity.x = 0;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("jump"))
        {
            if (IsGrounded)
            {
                JumpSpeed = MaxJumpHeight;
            }
        }

        if (@event.IsActionPressed("fireHook"))
        {
            _Hook.FireHook(this.Position, GetGlobalMousePosition());
        }

        if (@event.IsActionReleased("fireHook"))
        {
            _Hook.ReleaseHook();
        }
    }
}