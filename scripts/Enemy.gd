extends Area2D
class_name Enemy

signal died(enemy: Enemy)

@export var max_hp: int = 10
@export var score_value: int = 100
@export var move_speed: float = 80.0
@export var movement_pattern: String = "straight" # "straight", "sine", "hover"
@export var move_dir: Vector2 = Vector2.DOWN
@export var entry_target: Vector2 = Vector2.ZERO
@export var sine_amplitude: float = 60.0
@export var sine_frequency: float = 2.0

var hp: int
var elapsed: float = 0.0
var entering: bool = true
var base_x: float
var hit_tween: Tween

@onready var sprite: Sprite2D = $Sprite2D


func _ready() -> void:
	add_to_group("enemy")
	hp = max_hp
	base_x = position.x
	if entry_target == Vector2.ZERO:
		entry_target = position
	area_entered.connect(_on_area_entered)


func _process(delta: float) -> void:
	elapsed += delta
	_update_movement(delta)
	_check_offscreen()


func _update_movement(delta: float) -> void:
	match movement_pattern:
		"straight":
			position += move_dir.normalized() * move_speed * delta
		"sine":
			position.y += move_speed * delta
			position.x = base_x + sin(elapsed * sine_frequency) * sine_amplitude
		"hover":
			if entering:
				position = position.move_toward(entry_target, move_speed * delta)
				if position.distance_to(entry_target) < 2.0:
					position = entry_target
					entering = false


func _check_offscreen() -> void:
	if movement_pattern == "hover":
		return
	var vp := get_viewport_rect().size
	if position.y > vp.y + 60 or elapsed > 45.0:
		queue_free()


func take_damage(amount: int) -> void:
	hp -= amount
	_flash_hit()
	if hp <= 0:
		die()


func _flash_hit() -> void:
	if hit_tween and hit_tween.is_valid():
		hit_tween.kill()
	sprite.modulate = Color(1, 0.25, 0.25)
	hit_tween = create_tween()
	hit_tween.tween_property(sprite, "modulate", Color(1, 1, 1), 0.15)


func die() -> void:
	Global.add_score(score_value)
	_spawn_explosion()
	died.emit(self)
	queue_free()


func _spawn_explosion() -> void:
	var explosion_scene := preload("res://scenes/Effects/Explosion.tscn")
	var explosion := explosion_scene.instantiate()
	get_tree().current_scene.add_child(explosion)
	explosion.global_position = global_position


func _on_area_entered(area: Area2D) -> void:
	if area.is_in_group("player_bullet"):
		_spawn_hit_spark(area.global_position)
		take_damage(area.damage)
		area.queue_free()


func _spawn_hit_spark(pos: Vector2) -> void:
	var spark_scene := preload("res://scenes/Effects/HitSpark.tscn")
	var spark := spark_scene.instantiate()
	get_tree().current_scene.add_child(spark)
	spark.global_position = pos
