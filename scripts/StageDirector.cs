using System.Threading.Tasks;
using Godot;

public partial class StageDirector : Node2D
{
	[Signal] public delegate void SectionStartedEventHandler(int index, string title, string subtitle, Color backdrop);
	[Signal] public delegate void BossSpawnedEventHandler(Boss boss);
	[Signal] public delegate void StageClearedEventHandler();

	[Export] public PackedScene EnemyBasicScene { get; set; } = GD.Load<PackedScene>("res://scenes/Enemies/EnemyBasic.tscn");
	[Export] public PackedScene EnemyShooterScene { get; set; } = GD.Load<PackedScene>("res://scenes/Enemies/EnemyShooter.tscn");

	// セクション開始時のタイトル表示中、敵を出さずに待つ秒数
	private const double SectionIntroTime = 2.5;
	// 中ボスがいなくなってから次のセクションへ移るまでの秒数(後で3D視点移動の演出に置き換える)
	private const double SectionTransitionTime = 3.0;

	private StageDef _stage = StageData.Stage1;
	private Vector2 _screenSize;
	private bool _running = true;

	public override void _Ready()
	{
		_screenSize = GetViewportRect().Size;
		// Main が SectionStarted などを接続し終えてから開始する
		Callable.From(RunStage).CallDeferred();
	}

	public void Stop()
	{
		_running = false;
	}

	// 待機後もステージを続行してよいなら true。シーン遷移で解放済みなら false。
	private async Task<bool> Wait(double t)
	{
		await ToSignal(GetTree().CreateTimer(t), SceneTreeTimer.SignalName.Timeout);
		return StillRunning();
	}

	private bool StillRunning()
	{
		return IsInstanceValid(this) && IsInsideTree() && _running;
	}

	private async void RunStage()
	{
		for (int i = 0; i < _stage.Sections.Count; i++)
		{
			SectionDef section = _stage.Sections[i];
			EmitSignal(SignalName.SectionStarted, i, section.Title, section.Subtitle, section.Backdrop);
			if (!await Wait(SectionIntroTime)) return;

			if (!await RunWaves(section)) return;

			if (!await RunBoss(section.MidBossScene)) return;

			if (section.StageBossScene != null)
			{
				if (!await Wait(2.0)) return;
				if (!await RunBoss(section.StageBossScene)) return;
				if (!await Wait(1.5)) return;
				EmitSignal(SignalName.StageCleared);
				return;
			}

			RetreatRemainingEnemies();
			if (!await Wait(SectionTransitionTime)) return;
		}
	}

	private async Task<bool> RunWaves(SectionDef section)
	{
		foreach (StageStep step in section.Waves)
		{
			switch (step)
			{
				case Pause p:
					if (!await Wait(p.Seconds)) return false;
					break;
				case SpawnBasic b:
					SpawnBasicEnemy(b);
					break;
				case SpawnShooter s:
					SpawnShooterEnemy(s);
					break;
			}
		}
		return true;
	}

	// ボスを出し、撃破または退場するまで待つ
	private async Task<bool> RunBoss(string scenePath)
	{
		var boss = GD.Load<PackedScene>(scenePath).Instantiate<Boss>();
		boss.Position = new Vector2(_screenSize.X / 2.0f, -60);
		boss.EntryTarget = new Vector2(_screenSize.X / 2.0f, 160);
		boss.MovementPattern = MovementPattern.Hover;
		GetTree().CurrentScene.AddChild(boss);
		EmitSignal(SignalName.BossSpawned, boss);
		await ToSignal(boss, Boss.SignalName.Gone);
		return StillRunning();
	}

	private void SpawnBasicEnemy(SpawnBasic b)
	{
		var e = EnemyBasicScene.Instantiate<Enemy>();
		e.Position = new Vector2(b.X, -30);
		e.MovementPattern = b.Pattern;
		e.MoveDir = Vector2.Down;
		GetTree().CurrentScene.AddChild(e);
	}

	private void SpawnShooterEnemy(SpawnShooter s)
	{
		var e = EnemyShooterScene.Instantiate<EnemyShooter>();
		e.Position = new Vector2(s.X, -30);
		e.EntryTarget = new Vector2(s.X, s.Y);
		e.MovementPattern = MovementPattern.Hover;
		e.ShotPattern = s.Pattern;
		e.UseRiceBullets = s.Rice;
		GetTree().CurrentScene.AddChild(e);
	}

	// 定位置に居座る敵をセクション終了時に退場させる
	private void RetreatRemainingEnemies()
	{
		foreach (Node n in GetTree().GetNodesInGroup("enemy"))
		{
			if (n is Enemy e && IsInstanceValid(e))
				e.Retreat();
		}
	}
}
