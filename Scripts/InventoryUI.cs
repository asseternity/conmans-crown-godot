using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryUI : Control
{
	private Button _closeButton;
	private AudioStreamPlayer _clickPlayer;
	public List<Item> inventoryItems = new List<Item>();

	public override void _Ready()
	{
		_closeButton = GetNode<Button>("Panel/Panel/CloseButton");
		_closeButton.Pressed += OnCloseButtonClicked;
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
		Hide();

		// testing
		Item item0 = new Item(0, "TestItem", "res://Images/icon.svg");
		Item item1 = new Item(1, "TestItem", "res://Images/icon.svg");
		Item item2 = new Item(2, "TestItem", "res://Images/icon.svg");
		Item item3 = new Item(3, "TestItem", "res://Images/icon.svg");
		Item item4 = new Item(4, "TestItem", "res://Images/icon.svg");
		inventoryItems.Add(item0);
		inventoryItems.Add(item1);
		inventoryItems.Add(item2);
		inventoryItems.Add(item3);
		inventoryItems.Add(item4);
		FillInventory();
	}

	private void OnCloseButtonClicked()
	{
		_clickPlayer.Play();
		Hide();
	}

	private void FillInventory()
	{
		foreach (var item in inventoryItems)
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
					icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					// Make it fill the parent slot while keeping centered scaling
					icon.AnchorLeft = 0;
					icon.AnchorTop = 0;
					icon.AnchorRight = 1;
					icon.AnchorBottom = 1;
					slot.AddChild(icon);

					// [_] name and description on hover
					// [_] frame around empty slots
					// [_] save inventory to gameState
				}
				// for the rest - change the icon to just a frame with borders
				else
				{
					// Empty slot: add an empty TextureRect placeholder and give the ColorRect a border so it looks like a frame
					var frame = new TextureRect();
					frame.Name = "Icon";
					frame.Texture = null;
					frame.AnchorLeft = 0;
					frame.AnchorTop = 0;
					frame.AnchorRight = 1;
					frame.AnchorBottom = 1;
					frame.StretchMode = TextureRect.StretchModeEnum.Scale;
					slot.AddChild(frame);
				}
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
