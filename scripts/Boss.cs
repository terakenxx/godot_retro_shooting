using Godot;

public partial class Boss : EnemyShooter
{
	[Signal] public delegate void HpChangedEventHandler(int current, int maxHp);
	[Signal] public delegate void DefeatedEventHandler();

	[Export] public Godot.Collections.Array<ShotPattern> PhasePatterns { get; set; } = new() { ShotPattern.Spread, ShotPattern.Radial, ShotPattern.Spiral };

	private int _currentPhase = 0;

	public override void _Ready()
	{
		base._Ready();
		AddToGroup("boss");
		Callable.From(EmitInitialHp).CallDeferred();
	}

	private void EmitInitialHp()
	{
		EmitSignal(SignalName.HpChanged, Hp, MaxHp);
	}

	protected override void UpdateMovement(float delta)
	{
		base.UpdateMovement(delta);
		if (!Entering)
			Position = new Vector2(EntryTarget.X + Mathf.Sin(Elapsed * 1.5f) * 50.0f, Position.Y);
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
		base.Die();
	}
}
