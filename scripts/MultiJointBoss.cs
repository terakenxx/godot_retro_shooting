using System.Collections.Generic;
using Godot;

public partial class MultiJointBoss : Boss
{
	[Export] public PackedScene SegmentScene { get; set; } = GD.Load<PackedScene>("res://scenes/Enemies/BossSegment.tscn");
	[Export] public int SegmentCount { get; set; } = 8;
	[Export] public int SegmentGap { get; set; } = 6;
	[Export] public float WeaveAmplitudeX { get; set; } = 90.0f;
	[Export] public float WeaveAmplitudeY { get; set; } = 40.0f;
	[Export] public float WeaveSpeedX { get; set; } = 1.1f;
	[Export] public float WeaveSpeedY { get; set; } = 1.8f;

	private readonly List<BossSegment> _segments = new();
	// 頭の座標履歴(先頭が最新)。胴体はこれを SegmentGap フレームずつ遅れて追従する。
	private readonly LinkedList<Vector2> _history = new();
	private int _historyLimit;

	public override void _Ready()
	{
		base._Ready();
		_historyLimit = SegmentCount * SegmentGap + 4;
		SpawnSegments();
	}

	private void SpawnSegments()
	{
		for (int i = 0; i < SegmentCount; i++)
		{
			var seg = SegmentScene.Instantiate<BossSegment>();
			seg.MaxHp = 16 + i;
			GetTree().CurrentScene.AddChild(seg);
			seg.GlobalPosition = GlobalPosition;
			seg.Destroyed += OnSegmentDestroyed;
			_segments.Add(seg);
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		RecordHistory();
		UpdateSegments();
	}

	private void RecordHistory()
	{
		_history.AddFirst(GlobalPosition);
		while (_history.Count > _historyLimit)
			_history.RemoveLast();
	}

	private void UpdateSegments()
	{
		var history = new List<Vector2>(_history);
		for (int i = 0; i < _segments.Count; i++)
		{
			BossSegment seg = _segments[i];
			if (!IsInstanceValid(seg))
				continue;
			int idx = Mathf.Min((i + 1) * SegmentGap, history.Count - 1);
			if (idx < 0)
				continue;
			Vector2 target = history[idx];
			Vector2 prev = history[Mathf.Max(idx - 1, 0)];
			seg.GlobalPosition = target;
			Vector2 dir = target - prev;
			if (dir.Length() > 0.01f)
				seg.Rotation = dir.Angle();
		}
	}

	protected override void UpdateMovement(float delta)
	{
		if (MovementPattern == MovementPattern.Hover && Entering)
		{
			MoveToEntryTarget(delta);
		}
		else if (!Entering)
		{
			Position = new Vector2(
				EntryTarget.X + Mathf.Sin(Elapsed * WeaveSpeedX) * WeaveAmplitudeX,
				EntryTarget.Y + Mathf.Sin(Elapsed * WeaveSpeedY) * WeaveAmplitudeY);
		}
	}

	private void OnSegmentDestroyed(BossSegment seg)
	{
		_segments.Remove(seg);
	}

	public override void Die()
	{
		foreach (BossSegment seg in _segments)
		{
			if (IsInstanceValid(seg))
				seg.QueueFree();
		}
		_segments.Clear();
		base.Die();
	}
}
