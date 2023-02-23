using Godot;
using System.Collections.Generic;
using MessagePack;
using NetMessage;

// Wrapper scene spawned by the MultiplayerSpawner
public partial class ClientPlayer : CharacterBody3D
{
    public int RedundantInputs { get; private set; } = 0;

    private List<NetMessage.UserInput> _userInputs = new();
    private int _seqStamp = 0;

    private WeaponManager _weaponManager;
    private Node3D _rotationHelper;

    public override void _Ready()
    {
        _rotationHelper = GetNode<Node3D>("RotationHelper");
        _weaponManager = GetNode<WeaponManager>("WeaponManager");

        _weaponManager.WeaponAction += OnWeaponAction;
    }

    public override void _PhysicsProcess(double delta)
    {
        var userInput = GenerateUserInput();
        _userInputs.Add(userInput);
        SendInputs();
        MoveLocally(userInput);
        _seqStamp++;
    }

    private void OnWeaponAction(byte weaponIndex, NetMessage.WeaponFlags action)
    {
        var weaponCmd = new WeaponCommand
        {
            Id = Multiplayer.GetUniqueId(),
            WeaponIndex = weaponIndex,
            WeaponAction = (byte)action
        };

        var data = MessagePackSerializer.Serialize<NetMessage.ICommand>(weaponCmd);

        (Multiplayer as SceneMultiplayer).SendBytes(data, 1,
            MultiplayerPeer.TransferModeEnum.Reliable, 2);
    }

    public void ReceiveState(NetMessage.PlayerState state)
    {
        _userInputs.RemoveAll(input => input.Stamp <= state.Stamp);

        Transform3D expectedTransform = this.GlobalTransform;
        expectedTransform.Origin = state.Position;

        Vector3 expectedVelocity = state.Velocity;

        foreach (var userInput in _userInputs)
        {
            expectedVelocity = Movement.ComputeMotion(
                this,
                expectedTransform,
                expectedVelocity,
                Movement.InputToDirection(userInput.Keys),
                userInput.LateralLookAngle,
                Movement.ReadInput(userInput.Keys, NetMessage.InputFlags.Shift),
                Movement.ReadInput(userInput.Keys, NetMessage.InputFlags.Space));


            expectedTransform.Origin += expectedVelocity * (float)Movement.FRAME_DELTA;
        }

        var deviation = expectedTransform.Origin - Position;

        if (deviation.Length() > 0.05f)
        {
            // Reconciliation with authoritative state
            this.GlobalTransform = expectedTransform;
            this.Velocity = expectedVelocity;

            GD.PrintErr($"Client {this.Multiplayer.GetUniqueId()} prediction mismatch!");
        }
    }

    private void SendInputs()
    {
        var userCmd = new NetMessage.UserCommand
        {
            Id = Multiplayer.GetUniqueId(),
            Commands = _userInputs.ToArray()
        };

        RedundantInputs = userCmd.Commands.Length;

        if (this.IsMultiplayerAuthority() && Multiplayer.GetUniqueId() != 1)
        {
            byte[] data = MessagePackSerializer.Serialize<NetMessage.ICommand>(userCmd);

            (Multiplayer as SceneMultiplayer).SendBytes(data, 1,
                MultiplayerPeer.TransferModeEnum.UnreliableOrdered, 0);
        }
    }

    private void MoveLocally(NetMessage.UserInput userInput)
    {
        this.Velocity = Movement.ComputeMotion(
            this,
            this.GlobalTransform,
            this.Velocity,
            Movement.InputToDirection(userInput.Keys),
            userInput.LateralLookAngle,
            Movement.ReadInput(userInput.Keys, NetMessage.InputFlags.Shift),
            Movement.ReadInput(userInput.Keys, NetMessage.InputFlags.Space));

        Position += this.Velocity * (float)Movement.FRAME_DELTA;
    }

    private NetMessage.UserInput GenerateUserInput()
    {
        byte keys = 0;

        if (Input.IsActionPressed("right")) keys |= (byte)InputFlags.Right;
        if (Input.IsActionPressed("left")) keys |= (byte)InputFlags.Left;
        if (Input.IsActionPressed("forward")) keys |= (byte)InputFlags.Forward;
        if (Input.IsActionPressed("backward")) keys |= (byte)InputFlags.Backward;
        if (Input.IsActionPressed("space")) keys |= (byte)InputFlags.Space;
        if (Input.IsActionPressed("shift")) keys |= (byte)InputFlags.Shift;

        var userInput = new NetMessage.UserInput
        {
            Stamp = _seqStamp,
            Keys = keys,
            LateralLookAngle = this.GlobalRotation.Y,
            VerticalLookAngle = _rotationHelper.Rotation.X
        };

        return userInput;
    }
}
