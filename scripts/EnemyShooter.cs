using Godot;

public enum ShotPattern
{
	Aimed,
	Radial,
	Spiral,
	Spread,
}

public partial class EnemyShooter : Enemy
{
	[Export] public PackedScene BulletSceneRound { get; set; } = GD.Load<PackedScene>("res://scenes/Bullets/EnemyBulletRound.tscn");
	[Export] public PackedScene BulletSceneRice { get; set; } = GD.Load<PackedScene>("res://scenes/Bullets/EnemyBulletRice.tscn");
	[Export] public bool UseRiceBullets { get; set; } = false;
	[Export] public ShotPattern ShotPattern { get; set; } = ShotPattern.Aimed;
	[Export] public float FireInterval { get; set; } = 1.2f;
	[Export] public float BulletSpeed { get; set; } = 160.0f;
	[Export] public int BulletCount { get; set; } = 8;

	private float _fireTimer = 0.4f;
	private float _spiralAngle = 0.0f;
	private Node2D _player;

	public override void _Ready()
	{
		base._Ready();
		_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Entering)
			return;
		_fireTimer -= (float)delta;
		if (_fireTimer <= 0.0f)
		{
			Shoot();
			_fireTimer = FireInterval;
		}
	}

	private void Shoot()
	{
		switch (ShotPattern)
		{
			case ShotPattern.Aimed:
				ShootAimed();
				break;
			case ShotPattern.Radial:
				ShootRadial();
				break;
			case ShotPattern.Spiral:
				ShootSpiral();
				break;
			case ShotPattern.Spread:
				ShootSpread();
				break;
		}
	}

	private void ShootAimed()
	{
		if (!IsInstanceValid(_player))
			return;
		SpawnBullet(_player.GlobalPosition - GlobalPosition);
	}

	private void ShootRadial()
	{
		for (int i = 0; i < BulletCount; i++)
		{
			float angle = Mathf.Tau / BulletCount * i;
			SpawnBullet(Vector2.Right.Rotated(angle));
		}
	}

	private void ShootSpiral()
	{
		for (int i = 0; i < 3; i++)
		{
			float angle = _spiralAngle + (Mathf.Tau / 3.0f) * i;
			SpawnBullet(Vector2.Right.Rotated(angle));
		}
		_spiralAngle += 0.35f;
	}

	private void ShootSpread()
	{
		Vector2 baseDir = Vector2.Down;
		if (IsInstanceValid(_player))
			baseDir = _player.GlobalPosition - GlobalPosition;
		float spreadAngle = Mathf.DegToRad(40.0f);
		for (int i = 0; i < BulletCount; i++)
		{
			float t = (float)i / Mathf.Max(BulletCount - 1, 1) - 0.5f;
			SpawnBullet(baseDir.Rotated(spreadAngle * t));
		}
	}

	private void SpawnBullet(Vector2 dir)
	{
		PackedScene scene = UseRiceBullets ? BulletSceneRice : BulletSceneRound;
		var b = Effects.Spawn<Bullet>(this, scene, GlobalPosition);
		b.Speed = BulletSpeed;
		b.SetDirection(dir);
	}
}
