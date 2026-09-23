extends "res://scripts/Enemy.gd"
class_name EnemyShooter

@export var bullet_scene_round: PackedScene = preload("res://scenes/Bullets/EnemyBulletRound.tscn")
@export var bullet_scene_rice: PackedScene = preload("res://scenes/Bullets/EnemyBulletRice.tscn")
@export var use_rice_bullets: bool = false
@export var shot_pattern: String = "aimed" # "aimed", "radial", "spiral", "spread"
@export var fire_interval: float = 1.2
@export var bullet_speed: float = 160.0
@export var bullet_count: int = 8

var fire_timer: float = 0.4
var spiral_angle: float = 0.0
var player_ref: Node2D


func _ready() -> void:
	super._ready()
	player_ref = get_tree().get_first_node_in_group("player")


func _process(delta: float) -> void:
	super._process(delta)
	if entering:
		return
	fire_timer -= delta
	if fire_timer <= 0.0:
		_shoot()
		fire_timer = fire_interval


func _shoot() -> void:
	match shot_pattern:
		"aimed":
			_shoot_aimed()
		"radial":
			_shoot_radial()
		"spiral":
			_shoot_spiral()
		"spread":
			_shoot_spread()


func _shoot_aimed() -> void:
	if not is_instance_valid(player_ref):
		return
	var dir: Vector2 = (player_ref.global_position - global_position)
	_spawn_bullet(dir)


func _shoot_radial() -> void:
	for i in range(bullet_count):
		var angle := TAU / bullet_count * i
		_spawn_bullet(Vector2.RIGHT.rotated(angle))


func _shoot_spiral() -> void:
	for i in range(3):
		var angle := spiral_angle + (TAU / 3.0) * i
		_spawn_bullet(Vector2.RIGHT.rotated(angle))
	spiral_angle += 0.35


func _shoot_spread() -> void:
	var base_dir := Vector2.DOWN
	if is_instance_valid(player_ref):
		base_dir = (player_ref.global_position - global_position)
	var spread_angle := deg_to_rad(40.0)
	for i in range(bullet_count):
		var t := float(i) / float(max(bullet_count - 1, 1)) - 0.5
		_spawn_bullet(base_dir.rotated(spread_angle * t))


func _spawn_bullet(dir: Vector2) -> void:
	var scene := bullet_scene_rice if use_rice_bullets else bullet_scene_round
	var b: Bullet = scene.instantiate()
	get_tree().current_scene.add_child(b)
	b.global_position = global_position
	b.speed = bullet_speed
	b.set_direction(dir)
