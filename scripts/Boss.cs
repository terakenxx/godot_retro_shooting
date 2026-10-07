using Godot;

public partial class Boss : EnemyShooter
{
	[Signal] public delegate void HpChangedEventHandler(int current, int maxHp);
	[Signal] public delegate void DefeatedEventHandler();
	// 撃破・退場のどちらでも画面から消えるときに発火する
	[Signal] public delegate void GoneEventHandler();

	[Export] public Godot.Collections.Array<ShotPattern> PhasePatterns { get; set; } = new() { ShotPattern.Spread, ShotPattern.Radial, ShotPattern.Spiral };

	// 0 より大きければ、登場後この秒数で撃破されないまま退場する(中ボス用)
	[Export] public float EscapeAfter { get; set; } = 0.0f;

	private int _currentPhase = 0;
	private float _timeSinceEntered;

	public override void _Ready()
	{
		base._Ready();
		AddToGroup("boss");
		if (PhasePatterns != null && PhasePatterns.Count > 0)
			ShotPattern = PhasePatterns[0];
		Callable.From(EmitInitialHp).CallDeferred();
	}

	private void EmitInitialHp()
	{
		EmitSignal(SignalName.HpChanged, Hp, MaxHp);
	}

	protected override void UpdateMovement(float delta)
	{
		base.UpdateMovement(delta);
		if (!Entering && !Retreating)
			Position = new Vector2(EntryTarget.X + Mathf.Sin(Elapsed * 1.5f) * 50.0f, Position.Y);
		CheckEscape(delta);
	}

	protected void CheckEscape(float delta)
	{
		if (Entering || Retreating || EscapeAfter <= 0.0f)
			return;
		_timeSinceEntered += delta;
		if (_timeSinceEntered >= EscapeAfter)
			Retreat();
	}

	protected override void OnRetreated()
	{
		EmitSignal(SignalName.Gone);
	}

	public override void TakeDamage(int amount)
	{
		base.TakeDamage(amount);
		EmitSignal(SignalName.HpChanged, Hp, MaxHp);
		CheckPhase();
	}

	private void CheckPhase()
	{
		if (PhasePatterns == null || PhasePatterns.Count == 0)
			return;
		float phaseHp = MaxHp / (float)PhasePatterns.Count;
		int newPhase = Mathf.Clamp((int)((MaxHp - Hp) / phaseHp), 0, PhasePatterns.Count - 1);
		if (newPhase != _currentPhase)
		{
			_currentPhase = newPhase;
			ShotPattern = PhasePatterns[_currentPhase];
			FireInterval = Mathf.Max(0.4f, FireInterval * 0.85f);
		}
	}

	public override void Die()
	{
		EmitSignal(SignalName.Defeated);
		EmitSignal(SignalName.Gone);
		base.Die();
	}
}
