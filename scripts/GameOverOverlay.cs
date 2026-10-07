using Godot;

public partial class GameOverOverlay : CanvasLayer
{
	private Label _gameOverLabel;
	private float _t;

	public override void _Ready()
	{
		_gameOverLabel = GetNode<Label>("Control/GameOverLabel");
		Global g = Global.Instance;
		GetNode<Label>("Control/ScoreLabel").Text = $"SCORE: {g.Score:D6}\nHIGH SCORE: {g.HighScore:D6}";
	}

	public override void _Process(double delta)
	{
		_t += (float)delta;
		_gameOverLabel.Visible = Mathf.PosMod(_t, 1.0f) < 0.6f;
		if (Input.IsActionJustPressed("shoot") || Input.IsActionJustPressed("ui_accept"))
			GetTree().ChangeSceneToFile("res://scenes/UI/TitleScreen.tscn");
	}
}
