using System;
using Godot;

public partial class Item2D : CharacterBody2D
{
    [Export]
    public int Id { get; set; }

    [Export]
    public string ItemName { get; set; } = "";

    [Export]
    public string Description { get; set; } = "";

    [Export]
    public string IconPath { get; set; } = "";

    [Export]
    public string Effect { get; set; } = "";

    public Item ItemData { get; private set; }

    private Sprite2D _sprite;
    private CollisionShape2D _interactionArea;
    private Sprite2D _indicator = null!;
    private bool _playerInRange = false;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _interactionArea = GetNode<CollisionShape2D>("InteractionArea/CollisionShape2D");

        // Build the item from exported fields
        ItemData = new Item(Id, ItemName, Description, IconPath, Effect);

        // Load and resize sprite
        if (!string.IsNullOrEmpty(IconPath))
        {
            var texture = GD.Load<Texture2D>(IconPath);
            if (texture != null)
            {
                float _targetWidth = 20f;
                ApplyTextureAndResize(_sprite, texture, _targetWidth);
            }
            else
            {
                GD.PrintErr($"Failed to load texture at {IconPath}");
            }
        }

        // Create a simple indicator sprite above the item
        _indicator = new Sprite2D();
        Texture2D _indicatorTexture = GD.Load<Texture2D>("res://Images/arrow-down-svgrepo-com.svg");
        ApplyTextureAndResize(_indicator, _indicatorTexture, 10f);
        _indicator.Position = new Vector2(0, -20); // above the item
        _indicator.Visible = false;
        AddChild(_indicator);

        // Connect signals for interaction area
        var area2D = GetNode<Area2D>("InteractionArea");
        area2D.BodyEntered += OnBodyEntered;
        area2D.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("Player"))
        {
            _indicator.Visible = true;
            _playerInRange = true;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body.IsInGroup("Player"))
        {
            _indicator.Visible = false;
            _playerInRange = false;
        }
    }

    public override void _Process(double delta)
    {
        if (_playerInRange && Input.IsActionJustPressed("interact"))
        {
            if (ItemName != "bed")
            {
                PickUp();
            }
            else
            {
                var popup = GetNode<PopupUI>("/root/MainScene/UIContainer/PopupUI");
                popup.ShowQuestion("Sleep for a day?", this.Sleep);
            }
        }
    }

    public void PickUp()
    {
        var inventoryUI = GetNode<InventoryUI>("/root/MainScene/UIContainer/InventoryUI");
        inventoryUI.AddItem(ItemData);
        QueueFree();
    }

    public async void Sleep()
    {
        var fade = (FadeOverlay)GetNode("/root/FadeOverlay");
        await fade.FadeOut();
        Engine _engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
        _engine.ProgressDay();
        var gameUI = GetNode<GameUI>("/root/MainScene/UIContainer/GameUI");
        gameUI._calendarTextLabel.Text =
            $"{_engine.GS.Seasons[_engine.GS.CurrentSeasonIndex]}, {_engine.GS.CurrentDay.ToString()}";
        await fade.FadeIn();
    }

    private void ApplyTextureAndResize(Sprite2D sprite, Texture2D texture, float targetWidth)
    {
        sprite.RegionEnabled = false;
        sprite.Hframes = 1;
        sprite.Vframes = 1;
        sprite.Frame = 0;

        sprite.Texture = texture;

        var texSize = texture?.GetSize() ?? Vector2.Zero;
        if (texSize.X <= 0)
            return;

        float scaleFactor = targetWidth / texSize.X;
        sprite.Scale = new Vector2(scaleFactor, scaleFactor);
    }
}
