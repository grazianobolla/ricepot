using Godot;
using NetMessage;

public static class PlayerMovement
{
    public static double FRAME_DELTA = (1.0 / Engine.PhysicsTicksPerSecond);

    public static Vector3 ComputeMotion(Rid rid, Transform3D from, Vector3 velocity, Vector2 input)
    {
        Vector3 direction = new Vector3(input.X, 0, input.Y).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * 5;
            velocity.Z = direction.Z * 5;
        }
        else
        {
            velocity *= 0.85f;
        }

        var testParameters = new PhysicsTestMotionParameters3D();
        testParameters.From = from;
        testParameters.Motion = velocity * (float)FRAME_DELTA;

        var collResult = new PhysicsTestMotionResult3D();

        bool hasCollided = PhysicsServer3D.BodyTestMotion(rid, testParameters, collResult);

        if (hasCollided)
        {
            velocity = velocity.Slide(collResult.GetCollisionNormal());
        }

        return velocity;
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
}