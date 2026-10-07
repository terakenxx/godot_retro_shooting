using Godot;

public enum MovementPattern
{
	Straight,
	Sine,
	Hover,
}

public partial class Enemy : Area2D
{
	[Signal] public delegate void DiedEventHandler(Enemy enemy);

	private static readonly PackedScene ExplosionScene = GD.Load<PackedScene>("res://scenes/Effects/Explosion.tscn");
	private static readonly PackedScene HitSparkScene = GD.Load<PackedScene>("res://scenes/Effects/HitSpark.tscn");

	[Export] public int MaxHp { get; set; } = 10;
	[Export] public int ScoreValue { get; set; } = 100;
	[Export] public float MoveSpeed { get; set; } = 80.0f;
	[Export] public MovementPattern MovementPattern { get; set; } = MovementPattern.Straight;
	[Export] public Vector2 MoveDir { get; set; } = Vector2.Down;
	[Export] public Vector2 EntryTarget { get; set; } = Vector2.Zero;
	[Export] public float SineAmplitude { get; set; } = 60.0f;
	[Export] public float SineFrequency { get; set; } = 2.0f;

	public int Hp { get; protected set; }

	protected float Elapsed;
	protected bool Entering = true;
	protected bool Retreating;
	protected float BaseX;
	protected Sprite2D Sprite;
	private Tween _hitTween;

	public override void _Ready()
	{
		AddToGroup("enemy");
		Hp = MaxHp;
		BaseX = Position.X;
		if (EntryTarget == Vector2.Zero)
			EntryTarget = Position;
		Sprite = GetNode<Sprite2D>("Sprite2D");
		AreaEntered += OnAreaEntered;
	}

	public override void _Process(double delta)
	{
		Elapsed += (float)delta;
		UpdateMovement((float)delta);
		CheckOffscreen();
	}

	protected virtual void UpdateMovement(float delta)
	{
		if (Retreating)
		{
			Position += Vector2.Up * MoveSpeed * 2.0f * delta;
			return;
		}
		switch (MovementPattern)
		{
			case MovementPattern.Straight:
				Position += MoveDir.Normalized() * MoveSpeed * delta;
				break;
			case MovementPattern.Sine:
				Position = new Vector2(
					BaseX + Mathf.Sin(Elapsed * SineFrequency) * SineAmplitude,
					Position.Y + MoveSpeed * delta);
				break;
			case MovementPattern.Hover:
				MoveToEntryTarget(delta);
				break;
		}
	}

	protected void MoveToEntryTarget(float delta)
	{
		if (!Entering)
			return;
		Position = Position.MoveToward(EntryTarget, MoveSpeed * delta);
		if (Position.DistanceTo(EntryTarget) < 2.0f)
		{
			Position = EntryTarget;
			Entering = false;
		}
	}

	// 画面上方へ退場させる。画面外に出たら OnRetreated() の後に解放される。
	public void Retreat()
	{
		Entering = false;
		Retreating = true;
	}

	protected virtual void OnRetreated()
	{
	}

	private void CheckOffscreen()
	{
		if (Retreating && Position.Y < -80)
		{
			OnRetreated();
			QueueFree();
			return;
		}
		if (MovementPattern == MovementPattern.Hover)
			return;
		Vector2 vp = GetViewportRect().Size;
		if (Position.Y > vp.Y + 60 || Elapsed > 45.0f)
			QueueFree();
	}

	public virtual void TakeDamage(int amount)
	{
		Hp -= amount;
		FlashHit();
		if (Hp <= 0)
			Die();
	}

	private void FlashHit()
	{
		if (_hitTween != null && _hitTween.IsValid())
			_hitTween.Kill();
		Sprite.Modulate = new Color(1, 0.25f, 0.25f);
		_hitTween = CreateTween();
		_hitTween.TweenProperty(Sprite, "modulate", new Color(1, 1, 1), 0.15);
	}

	public virtual void Die()
	{
		Global.Instance.AddScore(ScoreValue);
		Effects.Spawn(this, ExplosionScene, GlobalPosition);
		EmitSignal(SignalName.Died, this);
		QueueFree();
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("player_bullet") && area is Bullet bullet)
		{
			Effects.Spawn(this, HitSparkScene, bullet.GlobalPosition);
			TakeDamage(bullet.Damage);
			bullet.QueueFree();
		}
	}
}
