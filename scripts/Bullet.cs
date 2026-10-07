using Godot;

public partial class Bullet : Area2D
{
	[Export] public float Speed { get; set; } = 500.0f;
	[Export] public int Damage { get; set; } = 1;

	public Vector2 Direction { get; private set; } = Vector2.Up;

	public void SetDirection(Vector2 d)
	{
		if (d.Length() > 0.0f)
			Direction = d.Normalized();
		Rotation = Direction.Angle() + Mathf.Pi / 2.0f;
	}

	public override void _Process(double delta)
	{
		Position += Direction * Speed * (float)delta;
		CheckOffscreen();
	}

	private void CheckOffscreen()
	{
		Vector2 vp = GetViewportRect().Size;
		const float margin = 48.0f;
		if (Position.X < -margin || Position.X > vp.X + margin || Position.Y < -margin || Position.Y > vp.Y + margin)
			QueueFree();
	}
}
