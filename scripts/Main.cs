using Godot;

public partial class Main : Node2D
{
	private static readonly PackedScene GameOverOverlayScene = GD.Load<PackedScene>("res://scenes/UI/GameOverOverlay.tscn");

	public override void _Ready()
	{
		AddToGroup("game_manager");
		Global.Instance.ResetGame();
		var hud = GetNode<HUD>("HUD");
		var stageDirector = GetNode<StageDirector>("StageDirector");
		stageDirector.BossSpawned += hud.ShowBossBar;
		stageDirector.StageCleared += OnStageCleared;
	}

	public async void OnPlayerDied()
	{
		await ToSignal(GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);
		if (!IsInstanceValid(this) || !IsInsideTree())
			return;
		AddChild(GameOverOverlayScene.Instantiate());
	}

	private void OnStageCleared()
	{
		GetTree().ChangeSceneToFile("res://scenes/UI/ClearScreen.tscn");
	}
}
