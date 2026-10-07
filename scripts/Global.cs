using Godot;

public partial class Global : Node
{
	[Signal] public delegate void ScoreChangedEventHandler(int newScore);
	[Signal] public delegate void LivesChangedEventHandler(int newLives);

	public const int MaxLives = 5;
	public const int StartLives = 3;

	public static Global Instance { get; private set; }

	public int Score { get; private set; }
	public int HighScore { get; set; }
	public int Lives { get; private set; } = StartLives;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public void ResetGame()
	{
		Score = 0;
		Lives = StartLives;
		EmitSignal(SignalName.ScoreChanged, Score);
		EmitSignal(SignalName.LivesChanged, Lives);
	}

	public void AddScore(int amount)
	{
		Score += amount;
		EmitSignal(SignalName.ScoreChanged, Score);
	}

	public bool LoseLife()
	{
		Lives -= 1;
		EmitSignal(SignalName.LivesChanged, Lives);
		if (Score > HighScore)
			HighScore = Score;
		return Lives <= 0;
	}
}
