using Godot;
using MessagePack;

// Code executed on the server side only, handles network events
public partial class ServerManager : Node
{
    private const int NetworkTicksPerSecond = 30;
    public int ListeningPort { get; set; } = 7777;

    private SceneMultiplayer _multiplayer = new();
    private Godot.Collections.Array<Godot.Node> entityArray;

    private double _netTickCounter = 0;

    public override void _Ready()
    {
        Create();
    }

    public override void _Process(double delta)
    {
        _netTickCounter += delta;
        if (_netTickCounter >= (1.0 / NetworkTicksPerSecond))
        {
            BroadcastSnapshot(); // Broadcast snapshot at NetworkTickrate rate
            _netTickCounter = 0;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        entityArray = GetNode("/root/Main/EntityArray").GetChildren();
        ProcessPendingPackets();

        DebugInfo();
    }

    // Process corresponding packets for this tick
    private void ProcessPendingPackets()
    {
        foreach (ServerPlayer player in entityArray)
        {
            player.ProcessPendingCommands();
        }
    }

    // Pack and send GameSnapshot with all entities and their information
    private void BroadcastSnapshot()
    {
        var snapshot = new NetMessage.GameSnapshot
        {
            Time = (int)Time.GetTicksMsec(),
            States = new NetMessage.PlayerState[entityArray.Count]
        };

        for (int i = 0; i < entityArray.Count; i++)
        {
            var player = entityArray[i] as ServerPlayer;
            snapshot.States[i] = player.GetCurrentPlayerState();
        }

        byte[] data = MessagePackSerializer.Serialize<NetMessage.ICommand>(snapshot);

        _multiplayer.SendBytes(data, 0,
            MultiplayerPeer.TransferModeEnum.UnreliableOrdered, 0);
    }

    private void OnPacketReceived(long id, byte[] data)
    {
        var command = MessagePackSerializer.Deserialize<NetMessage.ICommand>(data);

        switch (command)
        {
            case NetMessage.UserCommand userCmd:
                ServerPlayer player = GetNode($"/root/Main/EntityArray/{userCmd.Id}") as ServerPlayer;
                player.PushCommand(userCmd);
                break;

            case NetMessage.Sync sync:
                sync.ServerTime = (int)Time.GetTicksMsec();
                _multiplayer.SendBytes(MessagePackSerializer.Serialize<NetMessage.ICommand>(sync), (int)id,
                MultiplayerPeer.TransferModeEnum.Unreliable, 1);
                break;

            case NetMessage.WeaponCommand weaponCmd:
                _multiplayer.SendBytes(data, 0, MultiplayerPeer.TransferModeEnum.Reliable, 2); //Re-broadcast
                break;
        }
    }

    private void OnPeerConnected(long id)
    {
        Node playerInstance = GetNode<MultiplayerSpawner>("/root/Main/MultiplayerSpawner").Spawn(id);
        GD.Print("Peer ", id, " connected");
    }

    private void OnPeerDisconnected(long id)
    {
        GetNode($"/root/Main/EntityArray/{id}").QueueFree();
        GD.Print("Peer ", id, " disconnected");
    }

    private void Create()
    {
        _multiplayer.PeerConnected += OnPeerConnected;
        _multiplayer.PeerDisconnected += OnPeerDisconnected;
        _multiplayer.PeerPacket += OnPacketReceived;

        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateServer(ListeningPort);

        _multiplayer.MultiplayerPeer = peer;
        GetTree().SetMultiplayer(_multiplayer);

        GD.Print("Server listening on ", ListeningPort);
    }

    private void DebugInfo()
    {
        var label = GetNode<Label>("Debug/Label2");
        label.Text = $"clk {Time.GetTicksMsec()}";
    }
}
