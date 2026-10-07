using Godot;

public partial class PlayerHitEffect : Node2D
{
	private const int BoltCount = 6;
	private const int BoltSegments = 4;
	private const float BoltLength = 24.0f;
	private const float FadeTime = 0.3f;

	public override void _Ready()
	{
		var particles = GetNode<CpuParticles2D>("CPUParticles2D");
		particles.Emitting = true;
		particles.Finished += QueueFree;
		SpawnBolts();
	}

	private void SpawnBolts()
	{
		for (int i = 0; i < BoltCount; i++)
		{
			var line = new Line2D
			{
				Width = 1.6f,
				DefaultColor = new Color(0.75f, 0.95f, 1.0f, 1.0f),
			};
			float angle = GD.Randf() * Mathf.Tau;
			Vector2 pos = Vector2.Zero;
			var points = new Vector2[BoltSegments + 1];
			points[0] = pos;
			float segLen = BoltLength / BoltSegments;
			for (int s = 0; s < BoltSegments; s++)
			{
				var jitter = new Vector2((float)GD.RandRange(-6.0, 6.0), (float)GD.RandRange(-6.0, 6.0));
				pos += Vector2.Right.Rotated(angle) * segLen + jitter;
				points[s + 1] = pos;
			}
			line.Points = points;
			AddChild(line);
			Tween tw = CreateTween();
			tw.TweenProperty(line, "modulate:a", 0.0f, FadeTime);
			tw.TweenCallback(Callable.From(line.QueueFree));
		}
	}
}
