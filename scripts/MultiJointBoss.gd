extends "res://scripts/Boss.gd"
class_name MultiJointBoss

@export var segment_scene: PackedScene = preload("res://scenes/Enemies/BossSegment.tscn")
@export var segment_count: int = 8
@export var segment_gap: int = 6
@export var weave_amplitude_x: float = 90.0
@export var weave_amplitude_y: float = 40.0
@export var weave_speed_x: float = 1.1
@export var weave_speed_y: float = 1.8

var segments: Array[BossSegment] = []
var history: Array[Vector2] = []
var history_limit: int = 0


func _ready() -> void:
	super._ready()
	history_limit = segment_count * segment_gap + 4
	_spawn_segments()


func _spawn_segments() -> void:
	for i in range(segment_count):
		var seg: BossSegment = segment_scene.instantiate()
		seg.max_hp = 16 + i
		get_tree().current_scene.add_child(seg)
		seg.global_position = global_position
		seg.destroyed.connect(_on_segment_destroyed)
		segments.append(seg)


func _process(delta: float) -> void:
	super._process(delta)
	_record_history()
	_update_segments()


func _record_history() -> void:
	history.push_front(global_position)
	if history.size() > history_limit:
		history.resize(history_limit)


func _update_segments() -> void:
	for i in range(segments.size()):
		var seg := segments[i]
		if not is_instance_valid(seg):
			continue
		var idx: int = min((i + 1) * segment_gap, history.size() - 1)
		if idx < 0:
			continue
		var target: Vector2 = history[idx]
		var prev_idx: int = max(idx - 1, 0)
		var prev: Vector2 = history[prev_idx]
		seg.global_position = target
		var dir := target - prev
		if dir.length() > 0.01:
			seg.rotation = dir.angle()


func _update_movement(delta: float) -> void:
	if movement_pattern == "hover" and entering:
		position = position.move_toward(entry_target, move_speed * delta)
		if position.distance_to(entry_target) < 2.0:
			position = entry_target
			entering = false
	elif not entering:
		position.x = entry_target.x + sin(elapsed * weave_speed_x) * weave_amplitude_x
		position.y = entry_target.y + sin(elapsed * weave_speed_y) * weave_amplitude_y


func _on_segment_destroyed(seg: BossSegment) -> void:
	segments.erase(seg)


func die() -> void:
	for seg in segments.duplicate():
		if is_instance_valid(seg):
			seg.queue_free()
	segments.clear()
	super.die()
