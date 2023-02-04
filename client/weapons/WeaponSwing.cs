using Godot;
using System;

public partial class WeaponSwing : Node3D
{
    [Export] private NodePath _playerPath;
    [Export] private float _maxNoiseTravel = 512.0f;
    [Export] private float _holdShakeSpeed = 32.0f;
    [Export] private float _noiseOffsetX = 128.0f;
    [Export] private Vector3 _shakeMultiplier = new Vector3(0.004f, 0.004f, 0);

    [Export] private float _swingSpeed = 6.0f;
    [Export] private float _lateralSwingMultiplierFrontal = 0.12f;
    [Export] private float _lateralSwingMultiplierLateral = 0.15f;
    [Export] private float _verticalSwingMultiplier = 0.21f;
    [Export] private float _maxVerticalSwingOffset = 0.3f;
    [Export] private float _mouseSwingSpeed = 0.01f;

    private PlayerMovement _playerMovement;

    private Vector3 _originalPos, _originalRot;
    private FastNoiseLite _noise;
    private float _noiseTravelY = 0;
    private bool _travelBackwards = false;

    public override void _Ready()
    {
        _playerMovement = GetNode<PlayerMovement>(_playerPath);

        this._originalPos = this.Position;
        this._originalRot = this.Rotation;

        GenerateNoise();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            Vector2 relativeMouseMovement = mouseMotion.Relative * -_mouseSwingSpeed / 100.0f;
            Vector3 targetRot = _originalRot + (new Vector3(-relativeMouseMovement.Y, -relativeMouseMovement.X, 0));

            this.Rotation += targetRot;
            relativeMouseMovement = Vector2.Zero;
        }
    }

    public override void _Process(double delta)
    {
        HoldShake((float)delta);
        LateralSwing((float)delta);
    }

    // Frontal and lateral gun rotation based on velocity
    private void LateralSwing(float delta)
    {
        Vector3 localVelocity = _playerMovement.GetLocalVelocityNormalized();

        Vector3 targetRot = Vector3.Zero;
        targetRot.Z += -localVelocity.X * _lateralSwingMultiplierLateral;
        targetRot.X += -Mathf.Abs(localVelocity.Z) * _lateralSwingMultiplierFrontal;
        targetRot.X += Mathf.Clamp(-localVelocity.Y * _verticalSwingMultiplier, -_maxVerticalSwingOffset, _maxVerticalSwingOffset);

        this.Rotation = this.Rotation.Lerp(targetRot, delta * _swingSpeed);
    }

    // Organic hand shake 
    private void HoldShake(float delta)
    {
        var latVelNormalized = _playerMovement.GetVelocityNormalized();
        latVelNormalized.Y = 0;

        Vector3 variation = Vector3.Zero;
        variation += (Vector3.Up * _noise.GetNoise2D(0, _noiseTravelY)); //vertical shake
        variation += (Vector3.Left * _noise.GetNoise2D(_noiseOffsetX, _noiseTravelY)); //lateral shake
        variation *= _shakeMultiplier;

        if (_noiseTravelY > _maxNoiseTravel)
        {
            _travelBackwards = true;
        }
        else if (_noiseTravelY <= 0)
        {
            _travelBackwards = false;
            this.Position = _originalPos;
        }

        this.Position = variation;
        _noiseTravelY += delta * _holdShakeSpeed * (_travelBackwards ? -1 : 1);
    }

    private void GenerateNoise()
    {
        _noise = new FastNoiseLite();
        _noise.Seed = (int)GD.Randi();
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _noise.Frequency = 0.02f;

        _noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
        _noise.FractalOctaves = 2;
        _noise.FractalLacunarity = 2;
        _noise.FractalGain = 0.5f;
    }
}
