extends Area2D
class_name Bullet

@export var speed: float = 500.0
@export var damage: int = 1

var direction: Vector2 = Vector2.UP


func set_direction(d: Vector2) -> void:
	if d.length() > 0.0:
		direction = d.normalized()
	rotation = direction.angle() + PI / 2.0


func _process(delta: float) -> void:
	position += direction * speed * delta
	_check_offscreen()


func _check_offscreen() -> void:
	var vp := get_viewport_rect().size
	var margin := 48.0
	if position.x < -margin or position.x > vp.x + margin or position.y < -margin or position.y > vp.y + margin:
		queue_free()
