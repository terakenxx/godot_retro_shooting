using Godot;

public partial class PlayerExplosion : Node2D
{
	public override void _Ready()
	{
		var flash = GetNode<Sprite2D>("FlashSprite");
		var mainBurst = GetNode<CpuParticles2D>("MainBurst");
		var sparks = GetNode<CpuParticles2D>("Sparks");

		mainBurst.Emitting = true;
		sparks.Emitting = true;
		Tween tw = CreateTween();
		tw.SetParallel(true);
		tw.TweenProperty(flash, "scale", new Vector2(3.5f, 3.5f), 0.35).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tw.TweenProperty(flash, "modulate:a", 0.0f, 0.4);
		mainBurst.Finished += QueueFree;
	}
}
