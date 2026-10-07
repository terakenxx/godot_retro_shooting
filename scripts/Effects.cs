using Godot;

// シーン生成のヘルパー。current_scene に追加して指定座標へ置く。
public static class Effects
{
	public static T Spawn<T>(Node from, PackedScene scene, Vector2 globalPosition) where T : Node2D
	{
		var node = scene.Instantiate<T>();
		from.GetTree().CurrentScene.AddChild(node);
		node.GlobalPosition = globalPosition;
		return node;
	}

	public static Node2D Spawn(Node from, PackedScene scene, Vector2 globalPosition)
	{
		return Spawn<Node2D>(from, scene, globalPosition);
	}
}
