using System;
using Godot;

public partial class DialogueUI : Control
{
	private Label _textLabel;
	private VBoxContainer _optionsContainer;
	private Engine _engine;

	public override void _Ready()
	{
		_engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		_textLabel = GetNode<Label>("PanelContainer/VBoxContainer/TextLabel");
		_optionsContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer/OptionsContainer");
		Hide();
	}

	public void ShowStory(StoryLine story)
	{
		_textLabel.Text = story.Text;
		foreach (var child in _optionsContainer.GetChildren())
			child.QueueFree();

		foreach (var option in story.Options)
		{
			var button = new Button();
			button.Text = option.Text;
			button.Pressed += () =>
			{
				_engine.ChooseOption(option.NextElement);
				if (option.NextElement is StoryLine nextElementStory)
					ShowStory(nextElementStory);
				else if (option.NextElement is Duel nextElementDuel)
					Hide();
				else
					Hide();
			};
			_optionsContainer.AddChild(button);
		}
	}
}
