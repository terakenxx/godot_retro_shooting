using Godot;

public partial class HUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _livesLabel;
	private ProgressBar _bossBar;

	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("Control/ScoreLabel");
		_livesLabel = GetNode<Label>("Control/LivesLabel");
		_bossBar = GetNode<ProgressBar>("Control/BossHealthBar");

		Global.Instance.ScoreChanged += OnScoreChanged;
		Global.Instance.LivesChanged += OnLivesChanged;
		OnScoreChanged(Global.Instance.Score);
		OnLivesChanged(Global.Instance.Lives);
		_bossBar.Visible = false;
	}

	public override void _ExitTree()
	{
		Global.Instance.ScoreChanged -= OnScoreChanged;
		Global.Instance.LivesChanged -= OnLivesChanged;
	}

	private void OnScoreChanged(int v)
	{
		_scoreLabel.Text = $"SCORE: {v:D6}";
	}

	private void OnLivesChanged(int v)
	{
		_livesLabel.Text = $"LIVES: {v}";
	}

	public void ShowBossBar(Boss boss)
	{
		_bossBar.Visible = true;
		_bossBar.MaxValue = boss.MaxHp;
		_bossBar.Value = boss.Hp;
		boss.HpChanged += OnBossHpChanged;
		boss.Defeated += OnBossDefeated;
	}

	private void OnBossHpChanged(int current, int maxHp)
	{
		_bossBar.MaxValue = maxHp;
		_bossBar.Value = current;
	}

	private void OnBossDefeated()
	{
		_bossBar.Visible = false;
	}
}
