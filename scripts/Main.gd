extends Node2D

@onready var hud: CanvasLayer = $HUD
@onready var stage_director: StageDirector = $StageDirector


func _ready() -> void:
	add_to_group("game_manager")
	Global.reset_game()
	stage_director.boss_spawned.connect(hud.show_boss_bar)
	stage_director.stage_cleared.connect(_on_stage_cleared)


func on_player_died() -> void:
	await get_tree().create_timer(1.5).timeout
	var overlay: CanvasLayer = preload("res://scenes/UI/GameOverOverlay.tscn").instantiate()
	add_child(overlay)


func _on_stage_cleared() -> void:
	get_tree().change_scene_to_file("res://scenes/UI/ClearScreen.tscn")
