using Godot;
using System;

public partial class CameraView : Node3D
{
    [Export] float mouse_sensitivity = 0.05f;
    [Export] private float _maxVerticalAngle = 90;

    Node3D player;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        player = GetParent<Node3D>();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
            RotateX(-Mathf.DegToRad(mouseEvent.Relative.Y * mouse_sensitivity));
            player.RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * mouse_sensitivity));

            Vector3 cameraRot = RotationDegrees;
            cameraRot.X = Mathf.Clamp(cameraRot.X, -_maxVerticalAngle, _maxVerticalAngle);
            RotationDegrees = cameraRot;

            //reset collider
            //TODO: do it right
            player.GetNode<CollisionShape3D>("CollisionShape").GlobalRotation = Vector3.Zero;
        }
    }
}