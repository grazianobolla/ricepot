using Godot;

// Weapon class manages weapon values and animations
partial class Weapon : Node3D
{
    [Signal] public delegate void CooldownOverEventHandler();

    [Export] public int MaxAmmo { get; private set; } = 10;
    [Export] public int MaxRange { get; private set; } = 30;
    [Export] public float Damage { get; private set; } = 20.0f;
    [Export] public float FireRate { get; private set; } = 1.0f;
    [Export] public bool Automatic { get; private set; } = false;

    public AnimationPlayer Player;
    public int CurrentAmmo { get; private set; } = 0;

    private float _waitTime = 0;
    private double _remainingTime = 0;

    public override void _Ready()
    {
        _waitTime = 1.0f / (FireRate / 60.0f);
        Player = GetNode<AnimationPlayer>("AnimationPlayer");
        CurrentAmmo = MaxAmmo;

        Player.AnimationFinished += OnAnimationFinished;
    }

    public override void _Process(double delta)
    {
        if (_remainingTime > 0)
        {
            _remainingTime -= delta;

            if (_remainingTime <= 0)
                EmitSignal(SignalName.CooldownOver);
        }
    }

    private void OnAnimationFinished(StringName name)
    {
        switch (name)
        {
            case "away":
                SetEnabled(false);
                break;

            case "reload":
                CurrentAmmo = MaxAmmo;
                break;
        }
    }

    public void Shoot()
    {
        _remainingTime = _waitTime;
        CurrentAmmo--;

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

    public bool CanShoot()
    {
        return CurrentAmmo > 0;
    }

    public void SetEnabled(bool value)
    {
        this.Visible = value;
    }
}