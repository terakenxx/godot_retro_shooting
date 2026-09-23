extends Node2D

@onready var blink_label: Label = $Control/StartLabel

var t: float = 0.0


func _process(delta: float) -> void:
	t += delta
	blink_label.visible = fmod(t, 1.0) < 0.6
	if Input.is_action_just_pressed("shoot") or Input.is_action_just_pressed("ui_accept"):
		get_tree().change_scene_to_file("res://scenes/Main.tscn")
