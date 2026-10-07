# AGENTS.md — star_shooting

Godot 4.3 製、縦画面(480x640)の2D見下ろし弾幕シューティング。
すべてのシーン(.tscn)・スクリプト(.gd)・project.godot はエディタのGUI操作ではなく、テキストとして直接記述して作成している。

## 実行方法

- `project.godot` の `run/main_scene` は `res://scenes/UI/TitleScreen.tscn`。
- エディタで再生(▶)するとタイトル画面 → Z/Space/ゲームパッドAボタンでゲーム本編(`res://scenes/Main.tscn`)へ遷移。
- 新規アセット(.svg)を追加した直後は `.import` キャッシュが無くロード時にパースエラーになることがあるため、一度エディタで開いて自動インポートさせてから実行確認すること。

## ディレクトリ構成

```
project.godot          エンジン設定・入力マップ・衝突レイヤー名
icon.svg                プロジェクトアイコン
assets/                 SVGアセット(自機・敵・弾・爆発フラッシュ)
scripts/                GDScript本体
scenes/
  Main.tscn              ゲーム本編(背景・自機・StageDirector・HUD)
  Player.tscn             自機
  Bullets/                自機弾・敵弾(丸/米粒)シーン
  Enemies/                雑魚(EnemyBasic)・ショット持ち(EnemyShooter)・ボス(Boss)・多関節ボス(SerpentBoss/BossSegment)
  Effects/                爆発・被弾スパーク等のワンショットエフェクト
  UI/                     タイトル・HUD・ゲームオーバー・クリア画面
```

## 主要システム

- **Global (autoload, `scripts/Global.gd`)**: スコア・残機を保持。`score_changed` / `lives_changed` シグナルでHUDへ通知。
- **Player (`scripts/Player.gd` + `scenes/Player.tscn`)**: Area2Dベース。移動・集中(低速)モード・自動連射・被弾判定・無敵点滅を管理。
- **Bullet (`scripts/Bullet.gd`)**: 自機弾・敵弾共通の直進弾スクリプト。画面外に出たら自動でqueue_free。
- **Enemy 階層**:
  - `scripts/Enemy.gd`: 基底クラス。HP・移動パターン(`straight` / `sine` / `hover`)・被弾処理・撃破時の爆発とスコア加算。
  - `scripts/EnemyShooter.gd`: Enemyを継承し弾幕パターン(`aimed` / `radial` / `spiral` / `spread`)を追加。
  - `scripts/Boss.gd`: EnemyShooterを継承。HPに応じて`phase_patterns`配列の弾幕パターンへ切り替わる多段階ボス。HP変化を`hp_changed`シグナルでHUDのボスゲージへ反映。
  - `scripts/MultiJointBoss.gd`(`scenes/Enemies/SerpentBoss.tscn`): Bossを継承した多関節(蛇型)ボス。頭の`global_position`を毎フレーム`history`配列に記録し、`scripts/BossSegment.gd`の胴体セグメント(`scenes/Enemies/BossSegment.tscn`、個別HPを持ち被弾で破壊可能)が`segment_gap`フレーム分ディレイした履歴座標を追従することで、頭に連なってうねる胴体を実現(古典的な「先頭追従」スネーク方式)。`_update_movement`をオーバーライドし、進入後はLissajous的な正弦波軌道で画面上部を漂う。頭のHPが尽きる(`die()`)と残ったセグメントもまとめて解放してボス撃破。HPバー等のシグナルはBossからそのまま継承。
- **StageDirector (`scripts/StageDirector.gd`)**: `await get_tree().create_timer()` を使ったコルーチンでウェーブをタイムライン管理し、最後に`SerpentBoss`(多関節ボス)を出現させる。
- **HUD (`scripts/HUD.gd` + `scenes/UI/HUD.tscn`)**: スコア・残機表示、ボス出現時のみ表示されるHPバー。
- **画面遷移**: `TitleScreen` → `Main` → (ボス撃破) `ClearScreen` → `TitleScreen` に戻る。残機0で自機が撃墜されても`Main`シーンからは遷移せず、`StageDirector`は止めずに動かし続けたまま`GameOverOverlay`(`scripts/GameOverOverlay.gd` + `scenes/UI/GameOverOverlay.tscn`)を`Main`に追加でオーバーレイ表示する。自機が消えただけでステージが進行し続ける昔のアーケード/コンシューマー機の「ゲームオーバー後も敵がプレイを続ける」演出。ボタン入力で`TitleScreen`へ戻る。

## エフェクト

- 敵の被弾: `Enemy._flash_hit()` でスプライトを赤くフラッシュ(Tweenで白に戻す)。
- 敵への着弾: `scenes/Effects/HitSpark.tscn`(黄白の小さな火花)を着弾座標に生成。
- 自機の発射: `Player.tscn` の `MuzzleFlash`(CPUParticles2D)をショット中のみ`emitting = true`。
- 自機の被弾(通常): `scenes/Effects/PlayerHitSpark.tscn`(`scripts/PlayerHitEffect.gd`)。ジグザグの稲妻(Line2D)が弾けて消える演出+火花パーティクル。
- 自機の被弾(残機0/撃墜時): `scenes/Effects/PlayerExplosion.tscn`(`scripts/PlayerExplosion.gd`)。拡大フェードするフラッシュ(`assets/flash_circle.svg`)+大きめの爆炎・火花パーティクルの組み合わせで、通常の敵撃破エフェクトより視認性の高い爆発にしてある。

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

- GDScriptの組み込み関数名(`exp()` 等)をローカル変数名に使うとコンパイルエラーになる。過去に `Enemy.gd` で `exp` という変数名を使ってしまい、スクリプトチェーン全体がコンパイル不能になったことがある(修正済み)。同様の命名は避けること。
- 新規SVGアセットは初回インポート前だと `ext_resource` の読み込みに失敗する。アセット追加後は一度エディタを起動してインポートを走らせてから動作確認すること。
