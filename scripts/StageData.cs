using System.Collections.Generic;
using System.Linq;
using Godot;

// ステージ構成のデータ定義。StageDirector はこのデータを上から順に再生する。
// 1ステージ = 複数セクション。各セクションは「ウェーブの並び → 中ボス」で、最終セクションのみ中ボスの後にステージボスが出る。

public abstract record StageStep;

public sealed record Pause(double Seconds) : StageStep;

public sealed record SpawnBasic(float X, MovementPattern Pattern = MovementPattern.Straight) : StageStep;

public sealed record SpawnShooter(float X, float Y, ShotPattern Pattern, bool Rice = false) : StageStep;

public sealed record SectionDef(
	string Title,
	string Subtitle,
	Color Backdrop,
	IReadOnlyList<StageStep> Waves,
	string MidBossScene,
	string StageBossScene = null);

public sealed record StageDef(IReadOnlyList<SectionDef> Sections);

public static class StageData
{
	private const float ScreenW = 480.0f;

	// 間隔 interval で横一列に順番に出す
	private static IEnumerable<StageStep> Line(int count, float startX, float stepX, double interval, MovementPattern pattern = MovementPattern.Straight)
	{
		for (int i = 0; i < count; i++)
		{
			yield return new SpawnBasic(startX + i * stepX, pattern);
			yield return new Pause(interval);
		}
	}

	private static List<StageStep> Steps(params object[] parts)
	{
		var list = new List<StageStep>();
		foreach (object p in parts)
		{
			if (p is StageStep s)
				list.Add(s);
			else if (p is IEnumerable<StageStep> many)
				list.AddRange(many);
		}
		return list;
	}

	public static readonly StageDef Stage1 = new(new[]
	{
		// Section 1: 都市上空
		new SectionDef(
			"SECTION 1",
			"ABOVE THE CITY",
			new Color(0.05f, 0.07f, 0.16f),
			Steps(
				Line(5, 60, 70, 0.3),
				new Pause(2.0),
				Line(4, 80, 90, 0.4, MovementPattern.Sine),
				new Pause(2.0),
				new SpawnShooter(120, 120, ShotPattern.Aimed),
				new SpawnShooter(ScreenW - 120, 120, ShotPattern.Aimed),
				new Pause(5.0)),
			"res://scenes/Enemies/MidBoss1.tscn"),

		// Section 2: 降下してビル群を抜ける
		new SectionDef(
			"SECTION 2",
			"THROUGH THE SKYSCRAPERS",
			new Color(0.07f, 0.09f, 0.11f),
			Steps(
				Line(6, 50, 60, 0.25, MovementPattern.Sine),
				new SpawnShooter(ScreenW / 2.0f, 100, ShotPattern.Spread, true),
				new Pause(4.0),
				Line(5, ScreenW - 60, -70, 0.3),
				new SpawnShooter(100, 140, ShotPattern.Aimed, true),
				new SpawnShooter(ScreenW - 100, 140, ShotPattern.Aimed, true),
				new Pause(5.0)),
			"res://scenes/Enemies/MidBoss2.tscn"),

		// Section 3: ビルの合間の暗渠から巨大地下河川へ
		new SectionDef(
			"SECTION 3",
			"INTO THE UNDERGROUND RIVER",
			new Color(0.02f, 0.08f, 0.09f),
			Steps(
				Enumerable.Range(0, 3).SelectMany(i => new StageStep[]
				{
					new SpawnShooter(80 + i * 160, 100 + (i % 2) * 40, ShotPattern.Radial),
					new Pause(0.6),
				}),
				new Pause(3.0),
				Line(6, 50, 76, 0.25, MovementPattern.Sine),
				new Pause(5.0)),
			"res://scenes/Enemies/MidBoss3.tscn",
			"res://scenes/Enemies/SerpentBoss.tscn"),
	});
}
