using Godot;
using System;

public partial class KeysHelper : Node
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.F11:
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                    break;

                case Key.Escape:
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                    break;
            }
        }
    }
}
