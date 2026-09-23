extends Node

signal score_changed(new_score: int)
signal lives_changed(new_lives: int)

const MAX_LIVES := 5
const START_LIVES := 3

var score: int = 0
var high_score: int = 0
var lives: int = START_LIVES


func reset_game() -> void:
	score = 0
	lives = START_LIVES
	score_changed.emit(score)
	lives_changed.emit(lives)


func add_score(amount: int) -> void:
	score += amount
	score_changed.emit(score)


func lose_life() -> bool:
	lives -= 1
	lives_changed.emit(lives)
	if score > high_score:
		high_score = score
	return lives <= 0
