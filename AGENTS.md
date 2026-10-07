# AGENTS.md — star_shooting

Godot 4.7.2 製(2026-10-07 に 4.3 から更新)、縦画面(480x640)の2D見下ろし弾幕シューティング。
すべてのシーン(.tscn)・スクリプト(C# .cs)・project.godot はエディタのGUI操作ではなく、テキストとして直接記述して作成している。

## 実行方法

- スクリプトは C#(2026-10-07 に GDScript から移行)。**Godot 4.7.2 の .NET 版エディタ**と .NET 8 SDK が必要。初回はエディタ右上の「ビルド」でC#をビルドしてから再生する。
- C# プロジェクト: `star_shooting.csproj` / `star_shooting.sln`(Godot.NET.Sdk 4.7.2, net8.0)。
- `project.godot` の `run/main_scene` は `res://scenes/UI/TitleScreen.tscn`。
- エディタで再生(▶)するとタイトル画面 → Z/Space/ゲームパッドAボタンでゲーム本編(`res://scenes/Main.tscn`)へ遷移。
- 新規アセット(.svg)を追加した直後は `.import` キャッシュが無くロード時にパースエラーになることがあるため、一度エディタで開いて自動インポートさせてから実行確認すること。

## ディレクトリ構成

```
project.godot          エンジン設定・入力マップ・衝突レイヤー名
icon.svg                プロジェクトアイコン
assets/                 SVGアセット(自機・敵・弾・爆発フラッシュ)
scripts/                C#スクリプト本体
scenes/
  Main.tscn              ゲーム本編(背景・自機・StageDirector・HUD)
  Player.tscn             自機
  Bullets/                自機弾・敵弾(丸/米粒)シーン
  Enemies/                雑魚(EnemyBasic)・ショット持ち(EnemyShooter)・中ボス(MidBoss1〜3)・多関節ボス(SerpentBoss/BossSegment)・未使用の旧ボス(Boss)
  Effects/                爆発・被弾スパーク等のワンショットエフェクト
  UI/                     タイトル・HUD・ゲームオーバー・クリア画面
```

## 主要システム

- **Global (autoload, `scripts/Global.cs`)**: スコア・残機を保持。`ScoreChanged` / `LivesChanged` シグナル(`Global.Instance` 経由でアクセス)でHUDへ通知。
- **Player (`scripts/Player.cs` + `scenes/Player.tscn`)**: Area2Dベース。移動・集中(低速)モード・自動連射・被弾判定・無敵点滅を管理。
- **Bullet (`scripts/Bullet.cs`)**: 自機弾・敵弾共通の直進弾スクリプト。画面外に出たら自動でqueue_free。
- **Enemy 階層**:
  - `scripts/Enemy.cs`: 基底クラス。HP・移動パターン(enum `MovementPattern`: Straight / Sine / Hover)・被弾処理・撃破時の爆発とスコア加算。
  - `scripts/EnemyShooter.cs`: Enemyを継承し弾幕パターン(enum `ShotPattern`: Aimed / Radial / Spiral / Spread)を追加。
  - `scripts/Boss.cs`: EnemyShooterを継承。HPに応じて`PhasePatterns`配列の弾幕パターンへ切り替わる多段階ボス(最初は先頭のパターン)。HP変化を`HpChanged`シグナルでHUDのボスゲージへ反映。撃破時は`Defeated`、撃破・退場どちらでも`Gone`を発火。
  - `scripts/MultiJointBoss.cs`(`scenes/Enemies/SerpentBoss.tscn`): Bossを継承した多関節(蛇型)ボス。頭の`global_position`を毎フレーム`history`配列に記録し、`scripts/BossSegment.cs`の胴体セグメント(`scenes/Enemies/BossSegment.tscn`、個別HPを持ち被弾で破壊可能)が`segment_gap`フレーム分ディレイした履歴座標を追従することで、頭に連なってうねる胴体を実現(古典的な「先頭追従」スネーク方式)。`_update_movement`をオーバーライドし、進入後はLissajous的な正弦波軌道で画面上部を漂う。頭のHPが尽きる(`die()`)と残ったセグメントもまとめて解放してボス撃破。HPバー等のシグナルはBossからそのまま継承。
- **ステージ構成 (`scripts/StageData.cs`)**: ステージをデータで定義する。1ステージ = 3セクション(`SectionDef`)で、各セクションはタイトル・背景色・ウェーブの並び(`Pause` / `SpawnBasic` / `SpawnShooter`)・中ボスのシーンを持つ。最終セクションのみ中ボスの後にステージボス(`SerpentBoss`)が出る。ステージ1は ①ABOVE THE CITY(都市上空)→ ②THROUGH THE SKYSCRAPERS(ビル群)→ ③INTO THE UNDERGROUND RIVER(暗渠から地下河川)。
- **StageDirector (`scripts/StageDirector.cs`)**: `StageData` を上から順に再生する。セクション開始時に `SectionStarted` を発火(`Main` がHUDのセクション名表示と背景色の切り替えを行う)→ ウェーブ → 中ボス出現、撃破または退場(`Boss.Gone`)を待つ → 居残った敵を退場させて次のセクションへ。セクション間の待ち時間は、後で3D視点移動の演出に置き換える予定の仮実装。待機は `async`/`await ToSignal(...)`。
- **中ボス (`scenes/Enemies/MidBoss1〜3.tscn`)**: `Boss.cs` を使い、HP・弾幕パターン(`PhasePatterns`)・色を変えたもの。`EscapeAfter` 秒以内に倒せないと画面上方へ退場する。
- **HUD (`scripts/HUD.cs` + `scenes/UI/HUD.tscn`)**: スコア・残機表示、ボス出現時のみ表示されるHPバー、セクション開始時にフェード表示するセクション名(`SectionBanner`)。
- **画面遷移**: `TitleScreen` → `Main`(3セクション) → (ステージボス撃破) `ClearScreen` → `TitleScreen` に戻る。残機0で自機が撃墜されても`Main`シーンからは遷移せず、`StageDirector`は止めずに動かし続けたまま`GameOverOverlay`(`scripts/GameOverOverlay.cs` + `scenes/UI/GameOverOverlay.tscn`)を`Main`に追加でオーバーレイ表示する。自機が消えただけでステージが進行し続ける昔のアーケード/コンシューマー機の「ゲームオーバー後も敵がプレイを続ける」演出。ボタン入力で`TitleScreen`へ戻る。

## エフェクト

- 敵の被弾: `Enemy._flash_hit()` でスプライトを赤くフラッシュ(Tweenで白に戻す)。
- 敵への着弾: `scenes/Effects/HitSpark.tscn`(黄白の小さな火花)を着弾座標に生成。
- 自機の発射: `Player.tscn` の `MuzzleFlash`(CPUParticles2D)をショット中のみ`emitting = true`。
- 自機の被弾(通常): `scenes/Effects/PlayerHitSpark.tscn`(`scripts/PlayerHitEffect.cs`)。ジグザグの稲妻(Line2D)が弾けて消える演出+火花パーティクル。
- 自機の被弾(残機0/撃墜時): `scenes/Effects/PlayerExplosion.tscn`(`scripts/PlayerExplosion.cs`)。拡大フェードするフラッシュ(`assets/flash_circle.svg`)+大きめの爆炎・火花パーティクルの組み合わせで、通常の敵撃破エフェクトより視認性の高い爆発にしてある。

## 操作方法

| 動作 | キーボード | ゲームパッド |
|---|---|---|
| 移動 | 矢印キー / WASD | 左スティック / D-Pad |
| 集中(低速)モード | Shift | L1/R1ショルダー(button 9/10) |
| ショット | Z / Space | A/Xボタン(button 0/2) |
| 決定(タイトル/結果画面) | Z / Space / Enter | Aボタン(Godot標準ui_accept) |

キーボードとゲームパッドは同一アクションに両方バインドされており、併用可能(`project.godot` の `[input]` セクション参照)。

## 衝突レイヤー(project.godot `[layer_names]`)

1. player
2. player_bullet
3. enemy
4. enemy_bullet

自機は enemy(3) と enemy_bullet(4) をmaskで検知。敵は player_bullet(2) をmaskで検知。各弾は自分から他者を検知しない(monitoring=false)ことで負荷を抑えている。

## 既知の注意点

- C# では `.tscn` に保存されるエクスポートプロパティ名が C# のメンバー名(PascalCase、例: `MaxHp`)になる。スクリプトのプロパティ名を変えたら `.tscn` 側も合わせること。
- C# の `async void` 内で `await` した後は、シーン遷移でノードが解放されている可能性がある。`IsInstanceValid(this) && IsInsideTree()` を確認してから処理を続けること(StageDirector / Player / Main で実施済み)。
- C# プロジェクトの Web(HTML5)エクスポートは制限がある(4.3 では非対応)。Web 向けに出す場合は使用中の Godot バージョンの対応状況を確認すること。
- 新規SVGアセットは初回インポート前だと `ext_resource` の読み込みに失敗する。アセット追加後は一度エディタを起動してインポートを走らせてから動作確認すること。
