using System;
using System.Collections.Generic;
using Godot;

public partial class PopupUI : Control
{
	private Label _popupText;
	private Button _okButton;

	// Keep track of which tutorials have been shown
	private HashSet<string> _shownTutorials = new HashSet<string>();

	public override async void _Ready()
	{
		Hide();
		_popupText = GetNode<Label>("PopupPanel/PopupText");
		_okButton = GetNode<Button>("PopupPanel/OKButton");
		_okButton.Pressed += () =>
		{
			Hide();
			_popupText.Text = "";
		};

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

	public void ShowPopup(string id, string popupText)
	{
		if (_shownTutorials.Contains(id))
			return;

		_popupText.Text = popupText;
		Show();
		_shownTutorials.Add(id);

		// also save this state to GameState
		var engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		engine.GS.Flags.Add($"tutorial_{id}");
	}

	/// Check if a tutorial has already been shown.
	public bool HasShown(string id) => _shownTutorials.Contains(id);
}
