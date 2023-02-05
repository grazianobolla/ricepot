using Godot;
using System;

public partial class CustomSpawner : MultiplayerSpawner
{
    [Export] private PackedScene _playerScene;
    [Export] private PackedScene _serverPlayerScene;
    [Export] private PackedScene _dummyScene;
    [Export] private Vector3 _spawnPos = Vector3.Up * 3;

    public static ClientPlayer LocalPlayer;

    public override void _Ready()
    {
        Callable customSpawnFunctionCallable = new Callable(this, nameof(CustomSpawnFunction));
        this.SpawnFunction = customSpawnFunctionCallable;

        this.SetMultiplayerAuthority(Multiplayer.GetUniqueId());
    }

    private Node CustomSpawnFunction(Variant data)
    {
        int id = (int)data;

        // Server character for simulation
        if (Multiplayer.GetUniqueId() == 1)
        {
            GD.Print("Spawned server character");
            ServerPlayer player = _serverPlayerScene.Instantiate() as ServerPlayer;
            player.Name = id.ToString();
            player.MultiplayerID = id;
            player.Position = _spawnPos;
            return player;
        }

        // Client player
        if (id == Multiplayer.GetUniqueId())
        {
            GD.Print("Spawned client player");
            ClientPlayer player = _playerScene.Instantiate() as ClientPlayer;
            player.Name = id.ToString();
            player.SetMultiplayerAuthority(id);
            player.Position = _spawnPos;
            LocalPlayer = player;
            return player;
        }

        // Dummy player
        {
            GD.Print("Spawned dummy");
            Node3D player = _dummyScene.Instantiate() as Node3D;
            player.Name = id.ToString();
            player.Position = _spawnPos;
            player.SetMultiplayerAuthority(id);
            return player;
        }
    }
}
