extends Node2D

const STAR_COUNT := 90

var stars: Array = []


func _ready() -> void:
	randomize()
	var vp := get_viewport_rect().size
	for i in range(STAR_COUNT):
		stars.append({
			"pos": Vector2(randf() * vp.x, randf() * vp.y),
			"speed": randf_range(30.0, 130.0),
			"size": randf_range(1.0, 2.5),
			"brightness": randf_range(0.3, 1.0),
		})


func _process(delta: float) -> void:
	var vp := get_viewport_rect().size
	for s in stars:
		s.pos.y += s.speed * delta
		if s.pos.y > vp.y:
			s.pos.y = 0.0
			s.pos.x = randf() * vp.x
	queue_redraw()


func _draw() -> void:
	for s in stars:
		draw_circle(s.pos, s.size, Color(1, 1, 1, s.brightness))
