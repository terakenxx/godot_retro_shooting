extends Node2D

@onready var flash: Sprite2D = $FlashSprite
@onready var main_burst: CPUParticles2D = $MainBurst
@onready var sparks: CPUParticles2D = $Sparks


func _ready() -> void:
	main_burst.emitting = true
	sparks.emitting = true
	var tw := create_tween()
	tw.set_parallel(true)
	tw.tween_property(flash, "scale", Vector2(3.5, 3.5), 0.35).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	tw.tween_property(flash, "modulate:a", 0.0, 0.4)
	main_burst.finished.connect(queue_free)
