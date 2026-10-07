using Godot;

public partial class HUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _livesLabel;
	private ProgressBar _bossBar;
	private Control _sectionBanner;
	private Label _sectionTitle;
	private Label _sectionSubtitle;
	private Tween _bannerTween;

	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("Control/ScoreLabel");
		_livesLabel = GetNode<Label>("Control/LivesLabel");
		_bossBar = GetNode<ProgressBar>("Control/BossHealthBar");
		_sectionBanner = GetNode<Control>("Control/SectionBanner");
		_sectionTitle = GetNode<Label>("Control/SectionBanner/TitleLabel");
		_sectionSubtitle = GetNode<Label>("Control/SectionBanner/SubtitleLabel");
		_sectionBanner.Visible = false;

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
		boss.Gone += OnBossGone;
	}

	private void OnBossHpChanged(int current, int maxHp)
	{
		_bossBar.MaxValue = maxHp;
		_bossBar.Value = current;
	}

	private void OnBossGone()
	{
		_bossBar.Visible = false;
	}

	public void ShowSectionBanner(string title, string subtitle)
	{
		_sectionTitle.Text = title;
		_sectionSubtitle.Text = subtitle;
		_bannerTween?.Kill();
		_sectionBanner.Visible = true;
		_sectionBanner.Modulate = new Color(1, 1, 1, 0);
		_bannerTween = CreateTween();
		_bannerTween.TweenProperty(_sectionBanner, "modulate:a", 1.0f, 0.4);
		_bannerTween.TweenInterval(1.6);
		_bannerTween.TweenProperty(_sectionBanner, "modulate:a", 0.0f, 0.5);
		_bannerTween.TweenCallback(Callable.From(() => _sectionBanner.Visible = false));
	}
}
