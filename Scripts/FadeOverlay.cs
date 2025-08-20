using System.Threading.Tasks;
using Godot;

public partial class FadeOverlay : CanvasLayer
{
	private AnimationPlayer _anim;
	private CanvasItem _overlay; // your ColorRect

	public override async void _Ready()
	{
		_anim = GetNode<AnimationPlayer>("AnimationPlayer");
		_overlay = GetNode<CanvasItem>("Overlay");
		await FadeIn();
	}

	public async Task FadeOut() // clear -> black
	{
		_overlay.Visible = true;
		_anim.Play("fade_out"); // 0 -> 1
		await ToSignal(_anim, AnimationPlayer.SignalName.AnimationFinished);
	}

	public async Task FadeIn() // black -> clear
	{
		_overlay.Visible = true;
		_anim.Play("fade_in"); // 1 -> 0
		await ToSignal(_anim, AnimationPlayer.SignalName.AnimationFinished);
		_overlay.Visible = false; // hide when fully clear
	}

	public void DimScreen()
	{
		_overlay.Visible = true;
		_overlay.Modulate = new Color(0, 0, 0, 0.5f);
	}

	public void UnDimScreen()
	{
		_overlay.Modulate = new Color(0, 0, 0, 0.0f);
		_overlay.Visible = false;
	}
}
