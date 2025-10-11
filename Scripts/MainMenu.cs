using Godot;

public partial class MainMenu : Control
{
    private Button _playButton;
    private Button _optionsButton;
    private Button _quitButton;
    private AudioStreamPlayer _clickPlayer;
    private AudioStreamPlayer _musicPlayer;
    private Vector2 buttonScales;

    public override void _Ready()
    {
        _playButton = GetNode<Button>("VBoxContainer/PlayButton");
        _optionsButton = GetNode<Button>("VBoxContainer/OptionsButton");
        _quitButton = GetNode<Button>("VBoxContainer/QuitButton");
        _clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
        _musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
        // _musicPlayer.Play();
        buttonScales = _playButton.Scale;
        _playButton.PivotOffset = _playButton.Size / 2;
        _optionsButton.PivotOffset = _optionsButton.Size / 2;
        _quitButton.PivotOffset = _quitButton.Size / 2;

        _playButton.Pressed += OnPlayPressed;
        _optionsButton.Pressed += OnOptionsPressed;
        _quitButton.Pressed += OnQuitPressed;

        _playButton.MouseEntered += () => onButtonHoverEnter(_playButton);
        _playButton.MouseExited += () => onButtonHoverExit(_playButton);
        _optionsButton.MouseEntered += () => onButtonHoverEnter(_optionsButton);
        _optionsButton.MouseExited += () => onButtonHoverExit(_optionsButton);
        _quitButton.MouseEntered += () => onButtonHoverEnter(_quitButton);
        _quitButton.MouseExited += () => onButtonHoverExit(_quitButton);
    }

    private void PlayClickSound()
    {
        if (_clickPlayer != null)
            _clickPlayer.Play();
    }

    private async void OnPlayPressed()
    {
        PlayClickSound();
        var fade = (FadeOverlay)GetNode("/root/FadeOverlay");
        await fade.FadeOut();
        GetTree().ChangeSceneToFile("res://Scenes/main_container.tscn");
        await fade.FadeIn();
    }

    private void OnOptionsPressed()
    {
        PlayClickSound();
        // Navigate to options scene
    }

    private void OnQuitPressed()
    {
        PlayClickSound();
        if (OS.HasFeature("web"))
            return;
        GetTree().Quit();
    }

    private void onButtonHoverEnter(Button btn)
    {
        Tween tween = GetTree().CreateTween();
        tween
            .TweenProperty(btn, "scale", btn.Scale * 1.10f, 0.05f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
    }

    private void onButtonHoverExit(Button btn)
    {
        Tween tween = GetTree().CreateTween();
        tween
            .TweenProperty(btn, "scale", buttonScales, 0.05f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
    }
}
