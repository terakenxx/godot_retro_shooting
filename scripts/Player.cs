using Godot;

public partial class Player : Area2D
{
	private static readonly PackedScene HitSparkScene = GD.Load<PackedScene>("res://scenes/Effects/PlayerHitSpark.tscn");
	private static readonly PackedScene DeathExplosionScene = GD.Load<PackedScene>("res://scenes/Effects/PlayerExplosion.tscn");

	[Export] public float MoveSpeed { get; set; } = 300.0f;
	[Export] public float FocusSpeed { get; set; } = 130.0f;
	[Export] public float FireRate { get; set; } = 0.08f;
	[Export] public PackedScene BulletScene { get; set; } = GD.Load<PackedScene>("res://scenes/Bullets/PlayerBullet.tscn");

	private bool _invincible;
	private bool _active = true;
	private float _shootCooldown;
	private Rect2 _playArea;

	private Sprite2D _sprite;
	private Node2D _hitboxDot;
	private CpuParticles2D _muzzleFlash;

	public override void _Ready()
	{
		AddToGroup("player");
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_hitboxDot = GetNode<Node2D>("HitboxDot");
		_muzzleFlash = GetNode<CpuParticles2D>("MuzzleFlash");
		Vector2 vp = GetViewportRect().Size;
		_playArea = new Rect2(new Vector2(16, 16), vp - new Vector2(32, 32));
		_hitboxDot.Visible = false;
		AreaEntered += OnAreaEntered;
	}

	public override void _Process(double delta)
	{
		if (!_active)
			return;
		HandleMovement((float)delta);
		HandleShooting((float)delta);
	}

	private void HandleMovement(float delta)
	{
		var dir = new Vector2(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
			Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up"));
		if (dir.Length() > 1.0f)
			dir = dir.Normalized();

		bool focused = Input.IsActionPressed("focus");
		_hitboxDot.Visible = focused;
		float speed = focused ? FocusSpeed : MoveSpeed;

		Vector2 pos = Position + dir * speed * delta;
		pos.X = Mathf.Clamp(pos.X, _playArea.Position.X, _playArea.End.X);
		pos.Y = Mathf.Clamp(pos.Y, _playArea.Position.Y, _playArea.End.Y);
		Position = pos;
	}

	private void HandleShooting(float delta)
	{
		bool shooting = Input.IsActionPressed("shoot");
		_muzzleFlash.Emitting = shooting;
		_shootCooldown -= delta;
		if (shooting && _shootCooldown <= 0.0f)
		{
			FireBullet();
			_shootCooldown = FireRate;
		}
	}

	private void FireBullet()
	{
		SpawnBullet(new Vector2(-7, -4));
		SpawnBullet(new Vector2(7, -4));
	}

	private void SpawnBullet(Vector2 offset)
	{
		var b = Effects.Spawn<Bullet>(this, BulletScene, GlobalPosition + offset);
		b.SetDirection(Vector2.Up);
	}

	private void OnAreaEntered(Area2D area)
	{
		if (_invincible || !_active)
			return;
		if (area.IsInGroup("enemy_bullet"))
		{
			area.QueueFree();
			TakeHit();
		}
		else if (area.IsInGroup("enemy"))
		{
			TakeHit();
		}
	}

	private void TakeHit()
	{
		bool dead = Global.Instance.LoseLife();
		Effects.Spawn(this, HitSparkScene, GlobalPosition);
		if (dead)
		{
			Effects.Spawn(this, DeathExplosionScene, GlobalPosition);
			_active = false;
			Visible = false;
			GetTree().CallGroup("game_manager", Main.MethodName.OnPlayerDied);
		}
		else
		{
			BlinkInvincible();
		}
	}

	private async void BlinkInvincible()
	{
		_invincible = true;
		for (int i = 0; i < 10; i++)
		{
			_sprite.Visible = !_sprite.Visible;
			await ToSignal(GetTree().CreateTimer(0.08), SceneTreeTimer.SignalName.Timeout);
			// シーン遷移などで自機が解放されていたら中断する
			if (!IsInstanceValid(this) || !IsInsideTree())
				return;
		}
		_sprite.Visible = true;
		_invincible = false;
	}
}
