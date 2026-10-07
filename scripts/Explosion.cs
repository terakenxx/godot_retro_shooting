using Godot;

public partial class Explosion : Node2D
{
	public override void _Ready()
	{
		var particles = GetNode<CpuParticles2D>("CPUParticles2D");
		particles.Emitting = true;
		particles.Finished += QueueFree;
	}
}
