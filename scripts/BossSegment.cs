using Godot;

public partial class BossSegment : Area2D
{
	[Signal] public delegate void DestroyedEventHandler(BossSegment segment);

	private static readonly PackedScene ExplosionScene = GD.Load<PackedScene>("res://scenes/Effects/Explosion.tscn");
	private static readonly PackedScene HitSparkScene = GD.Load<PackedScene>("res://scenes/Effects/HitSpark.tscn");

	[Export] public int MaxHp { get; set; } = 18;
	[Export] public int ScoreValue { get; set; } = 150;

	private int _hp;
	private Tween _hitTween;
	private Sprite2D _sprite;

	public override void _Ready()
	{
		AddToGroup("enemy");
		_hp = MaxHp;
		_sprite = GetNode<Sprite2D>("Sprite2D");
		AreaEntered += OnAreaEntered;
	}

	public void TakeDamage(int amount)
	{
		_hp -= amount;
		FlashHit();
		if (_hp <= 0)
			Die();
	}

	private void FlashHit()
	{
		if (_hitTween != null && _hitTween.IsValid())
			_hitTween.Kill();
		_sprite.Modulate = new Color(1, 0.25f, 0.25f);
		_hitTween = CreateTween();
		_hitTween.TweenProperty(_sprite, "modulate", new Color(1, 1, 1), 0.15);
	}

	private void Die()
	{
		Global.Instance.AddScore(ScoreValue);
		Effects.Spawn(this, ExplosionScene, GlobalPosition);
		EmitSignal(SignalName.Destroyed, this);
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
