extends Node2D

@onready var score_label: Label = $Control/ScoreLabel


func _ready() -> void:
	if Global.score > Global.high_score:
		Global.high_score = Global.score
	score_label.text = "SCORE: %06d\nHIGH SCORE: %06d" % [Global.score, Global.high_score]


func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("shoot") or Input.is_action_just_pressed("ui_accept"):
		get_tree().change_scene_to_file("res://scenes/UI/TitleScreen.tscn")
