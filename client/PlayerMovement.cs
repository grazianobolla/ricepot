using Godot;
using NetMessage;

public partial class PlayerMovement : Node
{
    private CharacterBody3D _body;

    public override void _Ready()
    {
        _body = GetParent<CharacterBody3D>();
    }

    public Vector3 GetLocalVelocity()
    {
        return _body.Velocity.Rotated(Vector3.Up, -_body.GlobalRotation.Y);
    }

    public Vector3 GetLocalVelocityNormalized()
    {
        return _body.Velocity.Rotated(Vector3.Up, -_body.GlobalRotation.Y) / Movement.MaxRunSpeed;
    }

    public Vector3 GetVelocityNormalized()
    {
        return _body.Velocity / Movement.MaxRunSpeed;
    }
}