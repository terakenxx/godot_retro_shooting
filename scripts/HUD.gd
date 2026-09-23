extends CanvasLayer

@onready var score_label: Label = $Control/ScoreLabel
@onready var lives_label: Label = $Control/LivesLabel
@onready var boss_bar: ProgressBar = $Control/BossHealthBar


func _ready() -> void:
	Global.score_changed.connect(_on_score_changed)
	Global.lives_changed.connect(_on_lives_changed)
	_on_score_changed(Global.score)
	_on_lives_changed(Global.lives)
	boss_bar.visible = false


func _on_score_changed(v: int) -> void:
	score_label.text = "SCORE: %06d" % v


func _on_lives_changed(v: int) -> void:
	lives_label.text = "LIVES: %d" % v


func show_boss_bar(boss: Boss) -> void:
	boss_bar.visible = true
	boss_bar.max_value = boss.max_hp
	boss_bar.value = boss.hp
	boss.hp_changed.connect(_on_boss_hp_changed)
	boss.defeated.connect(_on_boss_defeated)


func _on_boss_hp_changed(current: int, max_hp: int) -> void:
	boss_bar.max_value = max_hp
	boss_bar.value = current


func _on_boss_defeated() -> void:
	boss_bar.visible = false
