using Godot;

public partial class Player : CharacterBody2D
{
    [Export]
    public float MovementSpeed = 200f;

    private AnimationPlayer _anim;
    private AudioStreamPlayer _footstepsPlayer;
    private float _footstepCooldown = 0.3f; // 0.3 seconds between footsteps
    private float _footstepTimer = 0f; // tracks cooldown

    private string _facing = "down";

    private enum Axis
    {
        None,
        Horizontal,
        Vertical
    }

    private Axis _lastAxis = Axis.Vertical; // tie-breaker when both pressed

    public override async void _Ready()
    {
        _anim = GetNode<AnimationPlayer>("AnimationPlayer");
        _footstepsPlayer = GetNode<AudioStreamPlayer>("FootstepsPlayer");

        // Wait a few frames to ensure Engine + GS are fully initialized
        for (int i = 0; i < 5; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // store level path
        Engine _engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
        var holder = GetNodeOrNull<Node>("/root/MainScene/LevelHolder");
        if (holder == null)
        {
            GD.PushError($"[Engine] LevelHolder not found");
        }
        _engine.GS.CurrentLevelPath = holder.GetChild(0).SceneFilePath;
        GD.Print($"[Engine] _engine.GS.CurrentLevelPath is {_engine.GS.CurrentLevelPath}");
    }

    // Read input, but allow only ONE axis at a time
    private Vector2 GetBlockedInput()
    {
        float x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        float y = Input.GetActionStrength("down") - Input.GetActionStrength("up");

        if (x != 0 || y != 0)
        {
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                y = 0;
                _lastAxis = Axis.Horizontal;
            }
            else if (Mathf.Abs(y) > Mathf.Abs(x))
            {
                x = 0;
                _lastAxis = Axis.Vertical;
            }
            else
            {
                // Equal strength (diagonal) → prefer the last axis we used
                if (_lastAxis == Axis.Horizontal)
                    y = 0;
                else
                    x = 0;
            }
        }
        return new Vector2(x, y);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Can't move if dialogic or duelUI or pauseUI is open
        var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
        bool dialogActive =
            dialogic != null && dialogic.Get("current_timeline").VariantType != Variant.Type.Nil;
        var duelUI = GetNode<DuelUI>("/root/MainScene/UIContainer/DuelUI");
        var pauseUI = GetNode<PauseMenu>("/root/MainScene/UIContainer/PauseMenu");
        bool menuOpen = duelUI.Visible || pauseUI.Visible;

        // Movement (no diagonals)
        var dir = GetBlockedInput();
        Velocity = dir * MovementSpeed;

        if (!dialogActive && !menuOpen)
            MoveAndSlide();

        // Footstep timer
        if (dir != Vector2.Zero)
        {
            _footstepTimer -= (float)delta;
            if (_footstepTimer <= 0f)
            {
                // Randomize pitch slightly (1.4 - 1.9)
                _footstepsPlayer.PitchScale = 1.4f + (float)GD.Randf() * 0.5f;
                _footstepsPlayer.Play();
                _footstepTimer = _footstepCooldown;
            }
        }
        else
        {
            _footstepTimer = 0f; // reset so footsteps start immediately when moving
            _footstepsPlayer.Stop();
        }

        // Animations
        string targetAnim;
        if (dir != Vector2.Zero)
        {
            if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
                _facing = dir.X > 0 ? "right" : "left";
            else
                _facing = dir.Y > 0 ? "down" : "up";

            targetAnim = $"walk_{_facing}";
        }
        else
        {
            targetAnim = $"idle_{_facing}";
        }

        if (_anim.CurrentAnimation != targetAnim)
            _anim.Play(targetAnim);
    }
}
