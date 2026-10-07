using System.Threading.Tasks;
using Godot;

public partial class StageDirector : Node2D
{
	[Signal] public delegate void BossSpawnedEventHandler(Boss boss);
	[Signal] public delegate void StageClearedEventHandler();

	[Export] public PackedScene EnemyBasicScene { get; set; } = GD.Load<PackedScene>("res://scenes/Enemies/EnemyBasic.tscn");
	[Export] public PackedScene EnemyShooterScene { get; set; } = GD.Load<PackedScene>("res://scenes/Enemies/EnemyShooter.tscn");
	[Export] public PackedScene BossScene { get; set; } = GD.Load<PackedScene>("res://scenes/Enemies/SerpentBoss.tscn");

	private Vector2 _screenSize;
	private bool _running = true;

	public override void _Ready()
	{
		_screenSize = GetViewportRect().Size;
		RunStage();
	}

	public void Stop()
	{
		_running = false;
	}

	// 待機後もステージを続行してよいなら true。シーン遷移で解放済みなら false。
	private async Task<bool> Wait(double t)
	{
		await ToSignal(GetTree().CreateTimer(t), SceneTreeTimer.SignalName.Timeout);
		return IsInstanceValid(this) && IsInsideTree() && _running;
	}

	private void SpawnBasic(float x, MovementPattern pattern = MovementPattern.Straight)
	{
		if (!_running)
			return;
		var e = EnemyBasicScene.Instantiate<Enemy>();
		e.Position = new Vector2(x, -30);
		e.MovementPattern = pattern;
		e.MoveDir = Vector2.Down;
		GetTree().CurrentScene.AddChild(e);
	}

	private void SpawnShooter(float x, float y, ShotPattern pattern, bool rice = false)
	{
		if (!_running)
			return;
		var e = EnemyShooterScene.Instantiate<EnemyShooter>();
		e.Position = new Vector2(x, -30);
		e.EntryTarget = new Vector2(x, y);
		e.MovementPattern = MovementPattern.Hover;
		e.ShotPattern = pattern;
		e.UseRiceBullets = rice;
		GetTree().CurrentScene.AddChild(e);
	}

	private async void RunStage()
	{
		if (!await Wait(1.5)) return;

		// Wave 1: straight descent, staggered
		for (int i = 0; i < 5; i++)
		{
			SpawnBasic(60 + i * 70);
			if (!await Wait(0.3)) return;
		}
		if (!await Wait(2.0)) return;

		// Wave 2: sine weavers
		for (int i = 0; i < 4; i++)
		{
			SpawnBasic(80 + i * 90, MovementPattern.Sine);
			if (!await Wait(0.4)) return;
		}
		if (!await Wait(2.0)) return;

		// Wave 3: aimed shooters flank the player
		SpawnShooter(120, 120, ShotPattern.Aimed);
		SpawnShooter(_screenSize.X - 120, 120, ShotPattern.Aimed);
		if (!await Wait(4.0)) return;

		// Wave 4: mixed pressure
		for (int i = 0; i < 6; i++)
		{
			SpawnBasic(50 + i * 60, MovementPattern.Sine);
			if (!await Wait(0.25)) return;
		}
		SpawnShooter(_screenSize.X / 2.0f, 100, ShotPattern.Spread, true);
		if (!await Wait(5.0)) return;

		// Wave 5: radial shooter squad
		for (int i = 0; i < 3; i++)
		{
			SpawnShooter(80 + i * 160, 100 + (i % 2) * 40, ShotPattern.Radial);
			if (!await Wait(0.6)) return;
		}
		if (!await Wait(6.0)) return;

		SpawnBoss();
	}

	private void SpawnBoss()
	{
		var boss = BossScene.Instantiate<Boss>();
		boss.Position = new Vector2(_screenSize.X / 2.0f, -60);
		boss.EntryTarget = new Vector2(_screenSize.X / 2.0f, 160);
		boss.MovementPattern = MovementPattern.Hover;
		GetTree().CurrentScene.AddChild(boss);
		EmitSignal(SignalName.BossSpawned, boss);
		boss.Defeated += OnBossDefeated;
	}

	private async void OnBossDefeated()
	{
		await ToSignal(GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);
		if (!IsInstanceValid(this) || !IsInsideTree())
			return;
		EmitSignal(SignalName.StageCleared);
	}
}
