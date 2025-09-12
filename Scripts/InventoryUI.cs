using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryUI : Control
{
	private Button _closeButton;
	private AudioStreamPlayer _clickPlayer;
	private AudioStreamPlayer _pickUpSound;
	public List<Item> inventoryItems = new List<Item>();

	public override void _Ready()
	{
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
		_pickUpSound = GetNode<AudioStreamPlayer>("PickUpSound");
		_closeButton = GetNode<Button>("Panel/CloseButton");
		_closeButton.Pressed += OnCloseButtonClicked;
		Hide();
		FillInventory();
	}

	private void OnCloseButtonClicked()
	{
		_clickPlayer.Play();
		Hide();
	}

	public void AddItem(Item item)
	{
		_pickUpSound.Play();
		inventoryItems.Add(item);
		FillInventory();
		Engine _engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		_engine.GS.Inventory = inventoryItems;
	}

	private void FillInventory()
	{
		// sort items by ID
		List<Item> sortedInventory = new List<Item>(inventoryItems);
		sortedInventory.Sort(
			delegate(Item item1, Item item2)
			{
				if (item1.ID == item2.ID)
					return 0;
				else if (item1.ID > item2.ID)
					return -1;
				else
					return 1;
			}
		);

		// find every "InventorySlot" node
		List<Node> inventorySlots = FindChildrenByName("InventorySlot");

		// change the type of InventorySlot to image
		// change image of InventorySlot to the Item's icon
		for (int i = 0; i < inventorySlots.Count; i++)
		{
			if (!(inventorySlots[i] is ColorRect slot))
				continue;

			// Remove any previous "Icon" child so we start clean
			var prevIcon = slot.GetNodeOrNull<Control>("Icon");
			if (prevIcon != null)
			{
				prevIcon.QueueFree();
			}

			if (i < sortedInventory.Count)
			{
				Item currentItem = sortedInventory[i];

				// Try to load texture (IconPath should be a res:// path)
				Texture2D tex = null;
				try
				{
					tex = GD.Load<Texture2D>(currentItem.IconPath);
				}
				catch
				{
					tex = null;
				}

				// Create TextureRect child to display the icon
				var icon = new TextureRect();
				icon.Name = "Icon";
				icon.Texture = tex;
				// Keep icon centered and maintain aspect ratio
				icon.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
				icon.StretchMode = TextureRect.StretchModeEnum.Scale;
				icon.SizeFlagsHorizontal = Control.SizeFlags.Fill;
				icon.SizeFlagsVertical = Control.SizeFlags.Fill;
				icon.CustomMinimumSize = new Vector2(50, 50);
				icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				icon.AnchorLeft = 0;
				icon.AnchorTop = 0;
				icon.AnchorRight = 1;
				icon.AnchorBottom = 1;
				icon.TooltipText = $"{currentItem.Name}. {currentItem.Description}";
				slot.AddChild(icon);
				// [_] save inventory to gameState
			}
		}
	}

	public List<Node> FindChildrenByName(string name)
	{
		List<Node> matching = new List<Node>();
		FindChildrenByNameRecursive(this, name, matching);
		return matching;
	}

	private void FindChildrenByNameRecursive(Node parent, string name, List<Node> list)
	{
		if (parent == null)
			return;

		foreach (Node child in parent.GetChildren())
		{
			if (child == null)
				continue;

			if (child.Name == name)
				list.Add(child);

			FindChildrenByNameRecursive(child, name, list);
		}
	}
}
