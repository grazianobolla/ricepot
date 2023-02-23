using Godot;
using System;
using MessagePack;

// Code executed on the client side only, handles network events
public partial class ClientManager : Node
{
    public string ServerAddress { get; set; }
    public int ServerPort { get; set; }

    [Export] private int _lerpBufferWindow = 50;
    [Export] private int _maxLerp = 150;

    private SceneMultiplayer _multiplayer = new();
    private SnapshotInterpolator _snapshotInterpolator = new();
    private NetworkClock _netClock;
    private Node _entityArray;

    // Debug only
    private double _sentPerSecond = 0, _recPerSecond = 0, _packetsPerSecond = 0;

    public override void _Ready()
    {
        Connect();

        _entityArray = GetNode("/root/Main/EntityArray");

        _netClock = GetNode<NetworkClock>("NetworkClock");
        _netClock.Initialize(_multiplayer);
        _netClock.LatencyCalculated += OnLatencyCalculated;
    }

    public override void _Process(double delta)
    {
        _snapshotInterpolator.InterpolateStates(_entityArray, NetworkClock.Clock);
        DebugInfo(delta);
    }

    private void OnPacketReceived(long id, byte[] data)
    {
        var command = MessagePackSerializer.Deserialize<NetMessage.ICommand>(data);

        switch (command)
        {
            case NetMessage.GameSnapshot snapshot:
                PushSnapshot(snapshot);
                break;

            case NetMessage.WeaponCommand weaponCmd:
                PushWeaponCmd(weaponCmd);
                break;
        }
    }

    private void PushWeaponCmd(NetMessage.WeaponCommand weaponCmd)
    {
        // Ignore local player
        if (weaponCmd.Id == Multiplayer.GetUniqueId())
            return;

        var dummy = _entityArray.GetNode<DummyPlayer>(weaponCmd.Id.ToString());
        dummy.HandleCommand(weaponCmd);
    }

    private void PushSnapshot(NetMessage.GameSnapshot snapshot)
    {
        _snapshotInterpolator.PushState(snapshot);

        foreach (NetMessage.PlayerState state in snapshot.States)
        {
            if (state.Id == Multiplayer.GetUniqueId())
            {
                CustomSpawner.LocalPlayer.ReceiveState(state);
            }
        }
    }

    private void DebugInfo(double delta)
    {
        var label = GetNode<Label>("Debug/Label2");
        label.Modulate = Colors.White;

        label.Text = $"buf {_snapshotInterpolator.BufferCount} ";
        label.Text += String.Format("int {0:0.00}", _snapshotInterpolator.InterpolationFactor);
        label.Text += $" len {_snapshotInterpolator.BufferTime}ms \nclk {NetworkClock.Clock} ofst {_netClock.Offset}ms";
        label.Text += $"\nping {_netClock.InmediateLatency}ms pps {_packetsPerSecond} jit {_netClock.Jitter}";

        if (CustomSpawner.LocalPlayer != null)
        {
            label.Text += $"\nrdt {CustomSpawner.LocalPlayer.RedundantInputs} tx {_sentPerSecond} rx {_recPerSecond}";
        }

        if (_snapshotInterpolator.InterpolationFactor > 1)
            label.Modulate = Colors.Red;
    }

    private void OnLatencyCalculated(int latencyAverage, int offsetAverage, int jitter)
    {
        _snapshotInterpolator.BufferTime = Mathf.Clamp(latencyAverage + _lerpBufferWindow, 0, _maxLerp);
    }

    private void OnConnectedToServer()
    {
        GetNode<Label>("Debug/Label").Text += $"\n{Multiplayer.GetUniqueId()}";
    }

    private void Connect()
    {
        _multiplayer.ConnectedToServer += OnConnectedToServer;
        _multiplayer.PeerPacket += OnPacketReceived;

        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateClient(ServerAddress, ServerPort);
        _multiplayer.MultiplayerPeer = peer;
        GetTree().SetMultiplayer(_multiplayer);
        GD.Print("Client connected to ", ServerAddress, ":", ServerPort);
    }

    private void OnDebugTimerOut()
    {
        var enetHost = (Multiplayer.MultiplayerPeer as ENetMultiplayerPeer).Host;
        _sentPerSecond = enetHost.PopStatistic(ENetConnection.HostStatistic.SentData);
        _recPerSecond = enetHost.PopStatistic(ENetConnection.HostStatistic.ReceivedData);
        _packetsPerSecond = enetHost.PopStatistic(ENetConnection.HostStatistic.ReceivedPackets);
    }
}