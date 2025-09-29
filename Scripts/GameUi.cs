using System;
using Godot;

public partial class GameUI : Control
{
    private AudioStreamPlayer _clickPlayer;
    private AudioStreamPlayer _taskPlayer;
    private Button _mapButton;
    private Button _settingsButton;
    private Button _inventoryButton;
    private Button _questsButton;
    private Button _statsButton;
    private Label _taskCompletedLabel;
    public Label _calendarTextLabel;

    public override void _Ready()
    {
        _clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
        _taskPlayer = GetNode<AudioStreamPlayer>("TaskPlayer");
        _mapButton = GetNode<Button>("MapButton");
        _settingsButton = GetNode<Button>("PauseButton");
        _inventoryButton = GetNode<Button>("InventoryButton");
        _questsButton = GetNode<Button>("QuestsButton");
        _statsButton = GetNode<Button>("StatsButton");
        _taskCompletedLabel = GetNode<Label>("Panel/TaskCompletedLabel");
        _calendarTextLabel = GetNode<Label>("Calendar/Panel/CalendarText");
        _mapButton.Pressed += OnMapPressed;
        _settingsButton.Pressed += OnSettingsPressed;
        _inventoryButton.Pressed += OnInventoryPressed;
        _questsButton.Pressed += OnQuestsPressed;
        _statsButton.Pressed += OnStatsPressed;

        // hide quest notification by default
        _taskCompletedLabel.Visible = false;
    }

    private void PlayClickSound()
    {
        if (_clickPlayer != null)
            _clickPlayer.Play();
    }

    private void PlayTaskSound()
    {
        if (_taskPlayer != null)
            _taskPlayer.Play();
    }

    private void OnMapPressed()
    {
        PlayClickSound();
        GetTree().ChangeSceneToFile("res://Scenes/map_container.tscn");
    }

    private void OnSettingsPressed()
    {
        PlayClickSound();
        var pauseUI = GetNode<PauseMenu>("/root/MainScene/UIContainer/PauseMenu");
        pauseUI.TogglePauseMenu();
    }

    private void OnInventoryPressed()
    {
        PlayClickSound();
        var inventoryUI = GetNode<InventoryUI>("/root/MainScene/UIContainer/InventoryUI");
        inventoryUI.Show();
    }

    private void OnQuestsPressed()
    {
        PlayClickSound();
        var questUI = GetNode<QuestUI>("/root/MainScene/UIContainer/QuestUI");
        questUI.Show();
    }

    private void OnStatsPressed()
    {
        PlayClickSound();
        var statsUI = GetNode<StatsUI>("/root/MainScene/UIContainer/StatsUI");
        statsUI.Show();
    }

    public void ShowQuestNotification(string taskText)
    {
        _taskCompletedLabel.Text = taskText;
        _taskPlayer.Play();

        // Make sure it's visible and reset initial transform
        _taskCompletedLabel.Visible = true;
        _taskCompletedLabel.Modulate = new Color(1, 1, 1, 0); // start transparent
        float originalY = _taskCompletedLabel.Position.Y;
        _taskCompletedLabel.Scale = new Vector2(1f, 1f); // reset scale

        // Parameters
        float fadeIn = 0.5f;
        float popUp = 0.15f;
        float popDown = 0.15f;
        float bobAmount = 30f; // how high it bobs (increase to make it more playful)
        float bobUp1 = 0.45f;
        float bobDown1 = 0.45f;
        float bobUp2 = 0.35f;
        float bobDown2 = 0.35f;
        float holdBeforeFadeOut = 0.6f;
        float fadeOut = 0.8f;

        // Create tween and run animations in parallel (staggered via SetDelay)
        var tween = CreateTween();
        tween.SetParallel();

        // Fade in (stronger, slower)
        tween
            .TweenProperty(_taskCompletedLabel, "modulate:a", 1f, fadeIn)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        // Quick "pop" scale to make it more visible, then settle back
        tween
            .TweenProperty(_taskCompletedLabel, "scale", new Vector2(1.15f, 1.15f), popUp)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
        tween
            .TweenProperty(_taskCompletedLabel, "scale", new Vector2(1f, 1f), popDown)
            .SetDelay(popUp)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);

        // Bobbing sequence (two bobs, stronger amplitude)
        tween
            .TweenProperty(_taskCompletedLabel, "position:y", originalY - bobAmount, bobUp1)
            .SetDelay(fadeIn)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        tween
            .TweenProperty(_taskCompletedLabel, "position:y", originalY, bobDown1)
            .SetDelay(fadeIn + bobUp1)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);

        tween
            .TweenProperty(_taskCompletedLabel, "position:y", originalY - bobAmount * 0.6f, bobUp2)
            .SetDelay(fadeIn + bobUp1 + bobDown1)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        tween
            .TweenProperty(_taskCompletedLabel, "position:y", originalY, bobDown2)
            .SetDelay(fadeIn + bobUp1 + bobDown1 + bobUp2)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);

        // Fade out after a short hold (so it's readable)
        float fadeOutDelay = fadeIn + bobUp1 + bobDown1 + bobUp2 + bobDown2 + holdBeforeFadeOut;
        tween
            .TweenProperty(_taskCompletedLabel, "modulate:a", 0f, fadeOut)
            .SetDelay(fadeOutDelay)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);

        // On finished: hide and restore transforms so next show starts clean
        tween.Finished += () =>
        {
            _taskCompletedLabel.Visible = false;
            _taskCompletedLabel.Position = new Vector2(_taskCompletedLabel.Position.X, originalY);
            _taskCompletedLabel.Scale = new Vector2(1f, 1f);
        };
    }
}
