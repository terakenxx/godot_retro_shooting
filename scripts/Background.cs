using Godot;

public partial class Background : Node2D
{
	private const int StarCount = 90;

	private struct Star
	{
		public Vector2 Pos;
		public float Speed;
		public float Size;
		public float Brightness;
	}

	private readonly Star[] _stars = new Star[StarCount];

	public override void _Ready()
	{
		GD.Randomize();
		Vector2 vp = GetViewportRect().Size;
		for (int i = 0; i < StarCount; i++)
		{
			_stars[i] = new Star
			{
				Pos = new Vector2(GD.Randf() * vp.X, GD.Randf() * vp.Y),
				Speed = (float)GD.RandRange(30.0, 130.0),
				Size = (float)GD.RandRange(1.0, 2.5),
				Brightness = (float)GD.RandRange(0.3, 1.0),
			};
		}
	}

	public override void _Process(double delta)
	{
		Vector2 vp = GetViewportRect().Size;
		for (int i = 0; i < _stars.Length; i++)
		{
			ref Star s = ref _stars[i];
			s.Pos.Y += s.Speed * (float)delta;
			if (s.Pos.Y > vp.Y)
			{
				s.Pos.Y = 0.0f;
				s.Pos.X = GD.Randf() * vp.X;
			}
		}
		QueueRedraw();
	}

	public override void _Draw()
	{
		foreach (Star s in _stars)
			DrawCircle(s.Pos, s.Size, new Color(1, 1, 1, s.Brightness));
	}
}
