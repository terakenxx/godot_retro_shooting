using Godot;

public partial class TitleScreen : Node2D
{
	private Label _blinkLabel;
	private float _t;

	public override void _Ready()
	{
		_blinkLabel = GetNode<Label>("Control/StartLabel");
	}

	public override void _Process(double delta)
	{
		_t += (float)delta;
		_blinkLabel.Visible = Mathf.PosMod(_t, 1.0f) < 0.6f;
		if (Input.IsActionJustPressed("shoot") || Input.IsActionJustPressed("ui_accept"))
			GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
	}
}
