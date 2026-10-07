using Godot;

public partial class ClearScreen : Node2D
{
	public override void _Ready()
	{
		Global g = Global.Instance;
		if (g.Score > g.HighScore)
			g.HighScore = g.Score;
		GetNode<Label>("Control/ScoreLabel").Text = $"SCORE: {g.Score:D6}\nHIGH SCORE: {g.HighScore:D6}";
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("shoot") || Input.IsActionJustPressed("ui_accept"))
			GetTree().ChangeSceneToFile("res://scenes/UI/TitleScreen.tscn");
	}
}
