using Godot;

partial class DummyWeapon : Node3D
{
    public AnimationPlayer Player;

    public override void _Ready()
    {
        Player = GetNode<AnimationPlayer>("AnimationPlayer");
        Player.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName name)
    {
        switch (name)
        {
            case "away":
                SetEnabled(false);
                break;
        }
    }

    public void Shoot()
    {
        Player.Stop();
        Player.Play("shoot");
    }

    public void Reload()
    {
        Player.Play("reload");
    }

    public void Draw()
    {
        Player.Play("draw");
        SetEnabled(true);
    }

    public void Away()
    {
        Player.Play("away");
    }

    public void Inspect()
    {
        Player.Play("inspect");
    }

    public void SetEnabled(bool value)
    {
        this.Visible = value;
    }
}