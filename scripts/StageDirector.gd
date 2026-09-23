extends Node2D
class_name StageDirector

signal boss_spawned(boss: Boss)
signal stage_cleared

@export var enemy_basic_scene: PackedScene = preload("res://scenes/Enemies/EnemyBasic.tscn")
@export var enemy_shooter_scene: PackedScene = preload("res://scenes/Enemies/EnemyShooter.tscn")
@export var boss_scene: PackedScene = preload("res://scenes/Enemies/Boss.tscn")

var screen_size: Vector2
var running: bool = true


func _ready() -> void:
	screen_size = get_viewport_rect().size
	_run_stage()


func stop() -> void:
	running = false


func _wait(t: float) -> void:
	await get_tree().create_timer(t).timeout


func _spawn_basic(x: float, pattern: String = "straight", dir := Vector2.DOWN) -> void:
	if not running:
		return
	var e: Enemy = enemy_basic_scene.instantiate()
	e.position = Vector2(x, -30)
	e.movement_pattern = pattern
	e.move_dir = dir
	get_tree().current_scene.add_child(e)


func _spawn_shooter(x: float, y: float, pattern: String, rice: bool = false) -> void:
	if not running:
		return
	var e: EnemyShooter = enemy_shooter_scene.instantiate()
	e.position = Vector2(x, -30)
	e.entry_target = Vector2(x, y)
	e.movement_pattern = "hover"
	e.shot_pattern = pattern
	e.use_rice_bullets = rice
	get_tree().current_scene.add_child(e)


func _run_stage() -> void:
	await _wait(1.5)

	# Wave 1: straight descent, staggered
	for i in range(5):
		if not running:
			return
		_spawn_basic(60 + i * 70)
		await _wait(0.3)
	await _wait(2.0)

	# Wave 2: sine weavers
	for i in range(4):
		if not running:
			return
		_spawn_basic(80 + i * 90, "sine")
		await _wait(0.4)
	await _wait(2.0)

	# Wave 3: aimed shooters flank the player
	_spawn_shooter(120, 120, "aimed")
	_spawn_shooter(screen_size.x - 120, 120, "aimed")
	await _wait(4.0)

	# Wave 4: mixed pressure
	for i in range(6):
		if not running:
			return
		_spawn_basic(50 + i * 60, "sine")
		await _wait(0.25)
	_spawn_shooter(screen_size.x / 2.0, 100, "spread", true)
	await _wait(5.0)

	# Wave 5: radial shooter squad
	for i in range(3):
		if not running:
			return
		_spawn_shooter(80 + i * 160, 100 + (i % 2) * 40, "radial")
		await _wait(0.6)
	await _wait(6.0)

	if not running:
		return
	_spawn_boss()


func _spawn_boss() -> void:
	var boss: Boss = boss_scene.instantiate()
	boss.position = Vector2(screen_size.x / 2.0, -60)
	boss.entry_target = Vector2(screen_size.x / 2.0, 130)
	boss.movement_pattern = "hover"
	boss.max_hp = 400
	boss.score_value = 5000
	boss.fire_interval = 1.0
	boss.bullet_speed = 150.0
	get_tree().current_scene.add_child(boss)
	boss_spawned.emit(boss)
	boss.defeated.connect(_on_boss_defeated)


func _on_boss_defeated() -> void:
	await _wait(1.5)
	stage_cleared.emit()
