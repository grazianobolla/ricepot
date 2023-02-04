using Godot;
using NetMessage;

public partial class PlayerMovement : Node
{
    [Export] private float _walkMaxSpeed = 0.5f;
    [Export] private float _runMaxSpeed = 2;
    [Export] private float _acceleartion = 9;
    [Export] private float _deAcceleartion = 15;
    [Export] private float _jumpSpeed = 2;
    [Export] private float _gravity = 9;
    [Export] private float _maxSlopeAngle = 60;

    private CharacterBody3D _body;

    private float _maxSpeed;
    private Node3D _playerCamera;

    public override void _Ready()
    {
        _body = GetParent<CharacterBody3D>();
        // _playerCamera = GetNode<Node3D>("../RotationHelper/Camera");

        // // CharacterBody3D setup
        // _body.FloorMaxAngle = Mathf.DegToRad(_maxSlopeAngle);
        // _body.UpDirection = Vector3.Up;
        // _body.FloorStopOnSlope = false;
        // _body.MaxSlides = 4;
    }

    // public override void _PhysicsProcess(double delta)
    // {
    //     //------------------------------------------------

    //     Vector2 inputDirection = Input.GetVector("forward", "backward", "left", "right");
    //     inputDirection = inputDirection.Normalized();

    //     if (Input.IsActionPressed("walk")) _maxSpeed = _walkMaxSpeed;
    //     else _maxSpeed = _runMaxSpeed;

    //     //------------------------------------------------

    //     Vector3 velocity = _body.Velocity;

    //     if (_body.IsOnFloor() && Input.IsActionPressed("ui_select"))
    //         velocity.Y = _jumpSpeed;

    //     Basis camera_basis = _playerCamera.GlobalTransform.Basis;

    //     Vector3 direction = Vector3.Zero;
    //     direction += inputDirection.X * camera_basis.Z;
    //     direction += inputDirection.Y * camera_basis.X;
    //     direction.Y = 0;
    //     direction = direction.Normalized();

    //     if (!_body.IsOnFloor())
    //         velocity.Y += -_gravity * (float)delta;

    //     Vector3 h_velocity = velocity;
    //     h_velocity.Y = 0;

    //     Vector3 target = direction * _maxSpeed;
    //     float acc = (direction.Dot(h_velocity) > 0) ? _acceleartion : _deAcceleartion;
    //     h_velocity = h_velocity.Lerp(target, acc * (float)delta);
    //     velocity.X = h_velocity.X;
    //     velocity.Z = h_velocity.Z;

    //     _body.Velocity = velocity;
    //     _body.MoveAndSlide();
    // }


    public Vector3 GetLocalVelocity()
    {
        return _body.Velocity.Rotated(Vector3.Up, -_body.GlobalRotation.Y);
    }

    public Vector3 GetLocalVelocityNormalized()
    {
        return _body.Velocity.Rotated(Vector3.Up, -_body.GlobalRotation.Y) / _runMaxSpeed;
    }

    public Vector3 GetVelocityNormalized()
    {
        return _body.Velocity / _runMaxSpeed;
    }
}