extends "res://scripts/EnemyShooter.gd"
class_name Boss

signal hp_changed(current: int, max_hp: int)
signal defeated

@export var phase_patterns: Array[String] = ["spread", "radial", "spiral"]

var current_phase: int = 0


func _ready() -> void:
	super._ready()
	add_to_group("boss")
	call_deferred("_emit_initial_hp")


func _emit_initial_hp() -> void:
	hp_changed.emit(hp, max_hp)


func _update_movement(delta: float) -> void:
	super._update_movement(delta)
	if not entering:
		position.x = entry_target.x + sin(elapsed * 1.5) * 50.0


func take_damage(amount: int) -> void:
	super.take_damage(amount)
	hp_changed.emit(hp, max_hp)
	_check_phase()


func _check_phase() -> void:
	if phase_patterns.is_empty():
		return
	var phase_hp := max_hp / float(phase_patterns.size())
	var new_phase: int = clampi(int((max_hp - hp) / phase_hp), 0, phase_patterns.size() - 1)
	if new_phase != current_phase:
		current_phase = new_phase
		shot_pattern = phase_patterns[current_phase]
		fire_interval = max(0.4, fire_interval * 0.85)


func die() -> void:
	defeated.emit()
	super.die()
