using System;
using Godot;

public partial class Projectile : Area2D
{
    [Export]
    public float Speed = 500f;

    [Export]
    public float MaxDistance = 1000f;

    [Export]
    public int Damage = 1;

    [Export]
    public float Radius = 3f; // visual & collision radius

    private Vector2 _direction = Vector2.Right;
    private Vector2 _startPos;

    public override void _Ready()
    {
        _startPos = GlobalPosition;

        // ensure collision shape exists; if not, create small circle collision
        if (GetNodeOrNull<CollisionShape2D>("CollisionShape2D") == null)
        {
            var cs = new CollisionShape2D();
            cs.Name = "CollisionShape2D";
            var circle = new CircleShape2D();
            circle.Radius = Radius;
            cs.Shape = circle;
            AddChild(cs);
        }

        BodyEntered += OnBodyEntered;

        // request a redraw (Godot 4) and enable processing
        QueueRedraw();
        SetProcess(true);
    }

    /// <summary>
    /// Initialize direction & damage after instancing.
    /// </summary>
    public void Initialize(Vector2 direction, int damage = 1)
    {
        _direction = direction.Normalized();
        Damage = damage;
    }

    public override void _Process(double delta)
    {
        Position += _direction * Speed * (float)delta;

        // lifetime/distance guard
        if ((_startPos - GlobalPosition).Length() >= MaxDistance)
            QueueFree();
    }

    private void OnBodyEntered(Node body)
    {
        if (body == null)
            return;

        // don't collide with the player who fired it
        if (body is Player)
            return;

        // if we hit another projectile, destroy
        if (body is Projectile)
        {
            QueueFree();
            return;
        }

        // Strong-typed enemy
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
            enemy.ApplyKnockback((enemy.GlobalPosition - GlobalPosition).Normalized() * 40f);
            QueueFree();
            return;
        }

        // Flexible: any node in "Enemy" group with a TakeDamage method
        if (body.IsInGroup("Enemy"))
        {
            if (body.HasMethod("TakeDamage"))
                body.Call("TakeDamage", Damage);
            QueueFree();
            return;
        }
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, Radius, new Color(1f, 0f, 0f));
    }
}
