using System;
using System.Collections.Generic;
using Godot;

public partial class PopupUI : Control
{
    private Label _popupText;
    private Button _okButton;
    private Button _confirmButton;
    private Button _cancelButton;
    private AudioStreamPlayer _clickSound;
    private Vector2 buttonScale;

    // Keep track of which tutorials have been shown
    private HashSet<string> _shownTutorials = new HashSet<string>();

    // State for multi-slide tutorials
    private List<string> _currentSlides = new List<string>();
    private int _currentSlideIndex = 0;

    public override async void _Ready()
    {
        _clickSound = GetNode<AudioStreamPlayer>("ClickSound");
        _confirmButton = GetNode<Button>("PopupPanel/ConfirmButton");
        _cancelButton = GetNode<Button>("PopupPanel/CancelButton");
        _cancelButton.Pressed += () =>
        {
            _clickSound.Play();
            Hide();
        };

        Hide();
        _popupText = GetNode<Label>("PopupPanel/PopupText");
        _okButton = GetNode<Button>("PopupPanel/OKButton");
        buttonScale = _okButton.Scale;
        _okButton.PivotOffset = _okButton.Size / 2;
        _okButton.Text = "Continue";
        _okButton.Pressed += OnOkPressed;
        _okButton.MouseEntered += () => onButtonHoverEnter(_okButton);
        _okButton.MouseExited += () => onButtonHoverExit(_okButton);

        // Wait a few frames to ensure Engine + GS are fully initialized
        for (int i = 0; i < 5; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Restore tutorials from GameState.Flags
        var engine = GetTree().Root.GetNodeOrNull<Engine>("GlobalEngine");
        if (engine?.GS?.Flags != null)
        {
            foreach (var flag in engine.GS.Flags)
            {
                if (flag.StartsWith("tutorial_"))
                    _shownTutorials.Add(flag.Substring("tutorial_".Length));
            }
        }
    }

    public void ShowPopup(string id, List<string> slides)
    {
        if (_shownTutorials.Contains(id))
            return;

        if (slides == null || slides.Count == 0)
            return;

        _currentSlides = slides;
        _currentSlideIndex = 0;

        _confirmButton.Visible = false;
        _cancelButton.Visible = false;
        _okButton.Visible = true;

        _popupText.Text = _currentSlides[_currentSlideIndex];
        _okButton.Text = "Continue";
        Show();

        _shownTutorials.Add(id);

        // also save this state to GameState
        var engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
        engine.GS.Flags.Add($"tutorial_{id}");
    }

    public void ShowQuestion(string questionText, Action callback)
    {
        _confirmButton.Visible = true;
        _cancelButton.Visible = true;
        _okButton.Visible = false;
        _popupText.Text = questionText;

        // One-time handler that removes itself after firing
        Action? confirmHandler = null;
        confirmHandler = () =>
        {
            // remove handler (one-time)
            _confirmButton.Pressed -= confirmHandler;
            // feedback + hide + invoke callback
            _clickSound?.Play();
            Hide();
            try
            {
                callback?.Invoke();
            }
            catch (Exception e)
            {
                GD.PrintErr($"[Engine] Callback error: {e}");
            }
        };

        // Attach the one-time handlers
        _confirmButton.Pressed += confirmHandler;

        Show();
    }

    private void OnOkPressed()
    {
        _clickSound.Play();
        if (_currentSlides == null || _currentSlides.Count == 0)
        {
            Hide();
            return;
        }

        _currentSlideIndex++;
        if (_currentSlideIndex < _currentSlides.Count)
        {
            // Show next slide
            _popupText.Text = _currentSlides[_currentSlideIndex];

            // Update button text depending on whether this is the last slide
            if (_currentSlideIndex + 1 < _currentSlides.Count)
                _okButton.Text = "Continue";
            else
                _okButton.Text = "Done";
        }
        else
        {
            // Finished all slides
            _currentSlides.Clear();
            Hide();
        }
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
            .TweenProperty(btn, "scale", buttonScale, 0.05f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
    }

    /// Check if a tutorial has already been shown.
    public bool HasShown(string id) => _shownTutorials.Contains(id);
}
