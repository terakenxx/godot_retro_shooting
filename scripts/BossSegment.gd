extends Area2D
class_name BossSegment

signal destroyed(segment: BossSegment)

@export var max_hp: int = 18
@export var score_value: int = 150

var hp: int
var hit_tween: Tween

@onready var sprite: Sprite2D = $Sprite2D


func _ready() -> void:
	add_to_group("enemy")
	hp = max_hp
	area_entered.connect(_on_area_entered)


func take_damage(amount: int) -> void:
	hp -= amount
	_flash_hit()
	if hp <= 0:
		_die()


func _flash_hit() -> void:
	if hit_tween and hit_tween.is_valid():
		hit_tween.kill()
	sprite.modulate = Color(1, 0.25, 0.25)
	hit_tween = create_tween()
	hit_tween.tween_property(sprite, "modulate", Color(1, 1, 1), 0.15)


func _die() -> void:
	Global.add_score(score_value)
	_spawn_explosion()
	destroyed.emit(self)
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
