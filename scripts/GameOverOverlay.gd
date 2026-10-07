extends CanvasLayer

@onready var game_over_label: Label = $Control/GameOverLabel
@onready var score_label: Label = $Control/ScoreLabel

var t: float = 0.0


func _ready() -> void:
	score_label.text = "SCORE: %06d\nHIGH SCORE: %06d" % [Global.score, Global.high_score]


func _process(delta: float) -> void:
	t += delta
	game_over_label.visible = fmod(t, 1.0) < 0.6
	if Input.is_action_just_pressed("shoot") or Input.is_action_just_pressed("ui_accept"):
		get_tree().change_scene_to_file("res://scenes/UI/TitleScreen.tscn")
