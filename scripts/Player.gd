extends Area2D
class_name Player

@export var move_speed: float = 300.0
@export var focus_speed: float = 130.0
@export var fire_rate: float = 0.08
@export var bullet_scene: PackedScene = preload("res://scenes/Bullets/PlayerBullet.tscn")

var invincible: bool = false
var active: bool = true
var shoot_cooldown: float = 0.0
var play_area: Rect2

@onready var sprite: Sprite2D = $Sprite2D
@onready var hitbox_dot: Node2D = $HitboxDot
@onready var muzzle_flash: CPUParticles2D = $MuzzleFlash


func _ready() -> void:
	add_to_group("player")
	var vp := get_viewport_rect().size
	play_area = Rect2(Vector2(16, 16), vp - Vector2(32, 32))
	hitbox_dot.visible = false
	area_entered.connect(_on_area_entered)


func _process(delta: float) -> void:
	if not active:
		return
	_handle_movement(delta)
	_handle_shooting(delta)


func _handle_movement(delta: float) -> void:
	var dir := Vector2(
		Input.get_action_strength("move_right") - Input.get_action_strength("move_left"),
		Input.get_action_strength("move_down") - Input.get_action_strength("move_up")
	)
	if dir.length() > 1.0:
		dir = dir.normalized()

	var focused := Input.is_action_pressed("focus")
	hitbox_dot.visible = focused
	var speed := focus_speed if focused else move_speed

	position += dir * speed * delta
	position.x = clamp(position.x, play_area.position.x, play_area.end.x)
	position.y = clamp(position.y, play_area.position.y, play_area.end.y)


func _handle_shooting(delta: float) -> void:
	var shooting := Input.is_action_pressed("shoot")
	muzzle_flash.emitting = shooting
	shoot_cooldown -= delta
	if shooting and shoot_cooldown <= 0.0:
		_fire_bullet()
		shoot_cooldown = fire_rate


func _fire_bullet() -> void:
	_spawn_bullet(Vector2(-7, -4))
	_spawn_bullet(Vector2(7, -4))


func _spawn_bullet(offset: Vector2) -> void:
	var b: Bullet = bullet_scene.instantiate()
	get_tree().current_scene.add_child(b)
	b.global_position = global_position + offset
	b.set_direction(Vector2.UP)


func _on_area_entered(area: Area2D) -> void:
	if invincible or not active:
		return
	if area.is_in_group("enemy_bullet"):
		area.queue_free()
		_take_hit()
	elif area.is_in_group("enemy"):
		_take_hit()


func _take_hit() -> void:
	var dead := Global.lose_life()
	_spawn_hit_spark()
	if dead:
		_spawn_death_explosion()
		active = false
		visible = false
		get_tree().call_group("game_manager", "on_player_died")
	else:
		_blink_invincible()


func _spawn_hit_spark() -> void:
	var spark_scene := preload("res://scenes/Effects/PlayerHitSpark.tscn")
	var spark := spark_scene.instantiate()
	get_tree().current_scene.add_child(spark)
	spark.global_position = global_position


func _spawn_death_explosion() -> void:
	var explosion_scene := preload("res://scenes/Effects/PlayerExplosion.tscn")
	var explosion := explosion_scene.instantiate()
	get_tree().current_scene.add_child(explosion)
	explosion.global_position = global_position


func _blink_invincible() -> void:
	invincible = true
	for i in range(10):
		sprite.visible = not sprite.visible
		await get_tree().create_timer(0.08).timeout
	sprite.visible = true
	invincible = false
