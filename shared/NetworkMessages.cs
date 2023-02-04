using Godot;
using MessagePack;

namespace NetMessage
{
    [MessagePack.Union(0, typeof(UserCommand))]
    [MessagePack.Union(1, typeof(GameSnapshot))]
    [MessagePack.Union(2, typeof(Sync))]
    [MessagePack.Union(3, typeof(WeaponCommand))]
    public interface ICommand { }

    // Used to calculate latency
    [MessagePackObject]
    public partial struct Sync : ICommand
    {
        [Key(0)] public int ClientTime;
        [Key(1)] public int ServerTime;
    }

    // Encapsulates user input and other client actions
    [MessagePackObject]
    public partial struct UserCommand : ICommand
    {
        [Key(0)] public int Id;
        [Key(1)] public UserInput[] Commands;
    }

    [MessagePackObject]
    public partial struct WeaponCommand : ICommand
    {
        [Key(0)] public int Id;
        [Key(1)] public byte WeaponIndex;
        [Key(2)] public byte WeaponAction;
    }

    [MessagePackObject]
    public partial struct UserInput
    {
        [Key(0)] public int Stamp;
        [Key(1)] public byte Keys;
        [Key(2)] public float LateralLookAngle;
        [Key(3)] public float VerticalLookAngle;
    }

    // Game state for a given point in time
    [MessagePackObject]
    public partial struct GameSnapshot : ICommand
    {
        [Key(0)] public UserState[] States;
        [Key(1)] public int Time;
    }

    // Encapsulates current state for a player (gets sent with gameSnapshot)
    [MessagePackObject]
    public partial struct UserState
    {
        [Key(0)] public int Id;
        [Key(1)] public float[] PosArray;
        [Key(2)] public float[] VelArray;
        [Key(3)] public int Stamp;
        [Key(4)] public float LateralLookAngle;
        [Key(5)] public float VerticalLookAngle;

        [IgnoreMember]
        public Vector3 Position
        {
            get { return new Vector3(PosArray[0], PosArray[1], PosArray[2]); }
        }

        [IgnoreMember]
        public Vector3 Velocity
        {
            get { return new Vector3(VelArray[0], VelArray[1], VelArray[2]); }
        }
    }

    public enum InputFlags
    {
        Forward = 0b_0000_0001,
        Backward = 0b_0000_0010,
        Left = 0b_0000_0100,
        Right = 0b_0000_1000,
        Space = 0b_0001_0000,
        Shift = 0b_0010_0000,
    }

    public enum WeaponFlags
    {
        Fire = 0b_0000_0001,
        Reload = 0b_0000_0010,
        Inspect = 0b_0000_0100,
    }
}