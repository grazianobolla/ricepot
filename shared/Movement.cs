using Godot;
using NetMessage;

public class Movement
{
    public const float MaxWalkSpeed = 0.8f;
    public const float MaxRunSpeed = 2.5f;
    public const float Gravity = 9.0f;
    public const float JumpVelocity = 2.0f;

    public static double FRAME_DELTA = (1.0 / Engine.PhysicsTicksPerSecond);

    public static Vector3 ComputeMotion(CharacterBody3D body, Transform3D from, Vector3 velocity, Vector2 input, float lookAngle, bool isWalking, bool isJumping, int maxSlides = 4)
    {
        bool isOnFloor = false;

        Vector3 direction = new Vector3(input.X, 0, input.Y).Normalized();
        direction = direction.Rotated(Vector3.Up, lookAngle);

        if (!direction.IsZeroApprox())
        {
            velocity.X = direction.X * (isWalking ? MaxWalkSpeed : MaxRunSpeed);
            velocity.Z = direction.Z * (isWalking ? MaxWalkSpeed : MaxRunSpeed);
        }
        else
        {
            velocity.X = 0;
            velocity.Z = 0;
        }

        for (int i = 0; i < maxSlides; i++)
        {
            var coll = new KinematicCollision3D();
            bool hasCollided = body.TestMove(from, velocity * (float)FRAME_DELTA, coll, 0.001f, true, 6);

            if (!isOnFloor)
                isOnFloor = CheckOnFloor(coll);

            if (hasCollided)
            {
                velocity = velocity.Slide(coll.GetNormal());
            }
        }

        if (!isOnFloor)
            velocity.Y -= Gravity * (float)FRAME_DELTA;

        if (isJumping && isOnFloor)
            velocity.Y = JumpVelocity;

        return velocity;
    }

    private static bool CheckOnFloor(KinematicCollision3D collision)
    {
        for (int i = collision.GetCollisionCount() - 1; i >= 0; i--)
        {
            float floorAngle = collision.GetAngle(i, Vector3.Up);
            if (floorAngle <= Mathf.DegToRad(45.0f))
            {
                return true;
            }
        }

        return false;
    }

    public static Vector2 InputToDirection(byte input)
    {
        Vector2 direction = Vector2.Zero;

        if ((input & (byte)InputFlags.Right) > 0) direction.X += 1;
        if ((input & (byte)InputFlags.Left) > 0) direction.X -= 1;
        if ((input & (byte)InputFlags.Backward) > 0) direction.Y += 1;
        if ((input & (byte)InputFlags.Forward) > 0) direction.Y -= 1;

        return direction.Normalized();
    }

    public static bool ReadInput(byte input, InputFlags flag)
    {
        return (input & (byte)flag) > 0;
    }
}