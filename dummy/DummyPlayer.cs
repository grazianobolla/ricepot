using Godot;
using NetMessage;

public partial class DummyPlayer : Node3D
{
    [Export]
    private NodePath _weaponHolder;

    private Node3D _rotationHelper;

    public override void _Ready()
    {
        _rotationHelper = GetNode<Node3D>("RotationHelper");
    }

    public void IntepolateState(NetMessage.PlayerState past, NetMessage.PlayerState future, float weight)
    {
        Position = past.Position.Lerp(future.Position, weight);

        var rotation = this.Rotation;
        rotation.Y = Mathf.LerpAngle(past.LateralLookAngle, future.LateralLookAngle, weight);
        this.Rotation = rotation;

        var verticalRotation = Mathf.LerpAngle(past.VerticalLookAngle, future.VerticalLookAngle, weight);
        _rotationHelper.Rotation = (Vector3.Right * verticalRotation);
    }

    public void HandleCommand(NetMessage.WeaponCommand command)
    {
        var weapon = GetNode(_weaponHolder).GetChild<DummyWeapon>(command.WeaponIndex);

        switch (command.WeaponAction)
        {
            case (byte)WeaponFlags.Fire:
                weapon.Shoot();
                break;

            case (byte)WeaponFlags.Reload:
                weapon.Reload();
                break;

            case (byte)WeaponFlags.Inspect:
                weapon.Inspect();
                break;
        }
    }
}
