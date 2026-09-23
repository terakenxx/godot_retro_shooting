extends Node2D

const BOLT_COUNT := 6
const BOLT_SEGMENTS := 4
const BOLT_LENGTH := 24.0
const FADE_TIME := 0.3

@onready var particles: CPUParticles2D = $CPUParticles2D


func _ready() -> void:
	particles.emitting = true
	particles.finished.connect(queue_free)
	_spawn_bolts()


func _spawn_bolts() -> void:
	for i in range(BOLT_COUNT):
		var line := Line2D.new()
		line.width = 1.6
		line.default_color = Color(0.75, 0.95, 1.0, 1.0)
		var angle := randf() * TAU
		var pos := Vector2.ZERO
		var points := PackedVector2Array([pos])
		var seg_len := BOLT_LENGTH / BOLT_SEGMENTS
		for s in range(BOLT_SEGMENTS):
			var jitter := Vector2(randf_range(-6.0, 6.0), randf_range(-6.0, 6.0))
			pos += Vector2.RIGHT.rotated(angle) * seg_len + jitter
			points.append(pos)
		line.points = points
		add_child(line)
		var tw := create_tween()
		tw.tween_property(line, "modulate:a", 0.0, FADE_TIME)
		tw.tween_callback(line.queue_free)
