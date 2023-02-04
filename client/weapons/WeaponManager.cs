using Godot;

// Handles weapon interaction
public partial class WeaponManager : Node
{
    [Signal] public delegate void WeaponActionEventHandler(byte weaponIndex, NetMessage.WeaponFlags action);

    [Export] private NodePath _weaponHolder;
    [Export] private NodePath _debugLabel;

    private enum WeaponState { SHOOTING, RELOADING, IDLE, MOVING };

    private WeaponState _currentState = WeaponState.IDLE;
    private Godot.Collections.Array<Node> _inventory;

    private int _currentWeaponIndex = 0;
    private int _lastWeaponIndex = 0;
    private Camera3D _camera;

    private bool _shotDenied = false;

    public override void _Ready()
    {
        _camera = GetTree().Root.GetCamera3D();

        InitializeWeapons();
        ConnectWeaponSignals();
    }

    public override void _Process(double delta)
    {
        ProcessWeaponActions();
        GetNode<Label>(_debugLabel).Text = $"state:{_currentState}\nweapon:{_currentWeapon.Name}\nlast_weapon:{_lastWeapon.Name}\nammo:{_currentWeapon.CurrentAmmo}\nshot_denied:{_shotDenied}";
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Pressed)
            {
                if (eventKey.Keycode >= Key.Key1 && eventKey.Keycode <= Key.Key9)
                {
                    int slot = (int)eventKey.Keycode - (int)Key.Key1;

                    if (slot < _inventory.Count)
                        SwitchCurrentWeapon(slot);
                }
            }
        }

        if (@event is InputEventMouseButton eventMouseBtn)
        {
            if (eventMouseBtn.Pressed == false && eventMouseBtn.ButtonIndex == MouseButton.Left)
            {
                // If the player liftes the fire button we
                // allow him to fire again 
                _shotDenied = false;
            }
        }
    }

    private void OnWeaponAnimationFinished(StringName name)
    {
        switch (name)
        {
            case "draw":
            case "reload":
                _currentState = WeaponState.IDLE;
                break;

            case "away":
                _currentWeapon.Draw();
                break;
        }
    }

    private void OnWeaponCooldownOver()
    {
        // After the weapon cooldown is over, we reset
        // to IDLE so we can fire again or do something else
        _currentState = WeaponState.IDLE;
    }

    // Handles when should weapon actions be called, each actions is independent
    // and can only be called if the current state is IDLE
    private void ProcessWeaponActions()
    {
        if (_currentState != WeaponState.IDLE)
        {
            return;
        }

        if (Input.IsActionJustPressed("reload"))
        {
            ReloadCurrentWeapon();
        }

        if (Input.IsActionPressed("inspect"))
        {
            _currentWeapon.Inspect();
            EmitSignal(SignalName.WeaponAction, (byte)_currentWeaponIndex, (byte)NetMessage.WeaponFlags.Inspect);
        }

        if (Input.IsActionPressed("shoot"))
        {
            if (_currentWeapon.Automatic) // Auto
            {
                ShootCurrentWeapon();
            }
            else if (_shotDenied == false) // Semi
            {
                ShootCurrentWeapon();

                // After we fire a shot, we wont allow a second shot until 
                // the player releases the mouse button (_shotDenied is resetted on _Input)
                _shotDenied = true;
            }
        }
    }

    private void ShootCurrentWeapon()
    {
        if (_currentWeapon.CanShoot())
        {
            _currentWeapon.Shoot();
            CalculateRayCollision(_currentWeapon.MaxRange);
            _currentState = WeaponState.SHOOTING;

            EmitSignal(SignalName.WeaponAction, (byte)_currentWeaponIndex, (byte)NetMessage.WeaponFlags.Fire);
        }
    }

    private void ReloadCurrentWeapon()
    {
        _currentWeapon.Reload();
        _currentState = WeaponState.RELOADING;
        EmitSignal(SignalName.WeaponAction, (byte)_currentWeaponIndex, (byte)NetMessage.WeaponFlags.Reload);
    }

    private void SwitchCurrentWeapon(int slot)
    {
        if (_currentState == WeaponState.IDLE && slot != _currentWeaponIndex)
        {
            _lastWeaponIndex = _currentWeaponIndex;
            _currentWeapon.Away();
            _currentWeaponIndex = slot;
            _currentState = WeaponState.MOVING;
        }
    }

    private void CalculateRayCollision(float range)
    {
        Vector2 center = GetTree().Root.GetViewport().GetVisibleRect().Size / 2.0f;

        var rayQuery = new PhysicsRayQueryParameters3D();
        rayQuery.From = _camera.ProjectRayOrigin(center);
        rayQuery.To = rayQuery.From + _camera.ProjectRayNormal(center) * range;

        Godot.Collections.Dictionary ray = GetTree().Root.World3D.DirectSpaceState.IntersectRay(rayQuery);

        if (ray.Count > 0)
        {
            Vector3 position = (Vector3)ray["position"];
            Vector3 normal = (Vector3)ray["normal"];
            Node collider = (Node)ray["collider"];

            var hitIndicator = GetNode<Node3D>("/root/Main/HitIndicator");
            hitIndicator.GlobalPosition = position;

            if (collider.IsInGroup("PhysicsProp"))
            {
                Vector3 dir = (rayQuery.To - rayQuery.From).Normalized();

                RigidBody3D body = (RigidBody3D)collider;
                body.ApplyImpulse(dir, position - body.GlobalPosition);
            }
        }
    }

    // Disables all weapons except for the first one
    private void InitializeWeapons()
    {
        _inventory = GetNode(_weaponHolder).GetChildren();

        for (int i = 0; i < _inventory.Count; i++)
        {
            Weapon weapon = _inventory[i] as Weapon;
            weapon.SetEnabled(false);
        }

        Weapon firstWeapon = _inventory[_currentWeaponIndex] as Weapon;
        firstWeapon.SetEnabled(true);
    }

    private void ConnectWeaponSignals()
    {
        foreach (Weapon weapon in _inventory)
        {
            weapon.Player.AnimationFinished += OnWeaponAnimationFinished;
            weapon.CooldownOver += OnWeaponCooldownOver;
        }
    }

    private Weapon _currentWeapon
    {
        get { return (Weapon)_inventory[_currentWeaponIndex]; }
    }

    private Weapon _lastWeapon
    {
        get { return (Weapon)_inventory[_lastWeaponIndex]; }
    }
}
