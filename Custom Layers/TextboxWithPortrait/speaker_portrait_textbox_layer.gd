@tool
extends DialogicLayoutLayer

@export var path_left: NodePath = NodePath("Anchor/PortraitPanel/PortraitBackgroundColor/DialogicNode_PortraitContainer")
@export var path_right: NodePath = NodePath("Anchor/PortraitPanel2/PortraitBackgroundColor/DialogicNode_PortraitContainer")

var left_char: Object = null
var right_char: Object = null
var _container_left: Node = null
var _container_right: Node = null
var _signals_connected: bool = false
var _portrait_texture_map := {} # Dictionary: character_resource -> last_known_texture_instance_id (String)

func _ready() -> void:
	if Engine.is_editor_hint():
		return
	_container_left = get_node_or_null(path_left)
	_container_right = get_node_or_null(path_right)
	print("[PORTRAITS] left container ids:", _container_left.get("container_ids") if _container_left and _container_left.has_method("get") else "nil")
	print("[PORTRAITS] right container ids:",_container_right.get("container_ids") if _container_right and _container_right.has_method("get") else "nil")

	if not _signals_connected:
		Dialogic.Text.speaker_updated.connect(Callable(self, "_on_speaker_updated"))
		Dialogic.timeline_started.connect(Callable(self, "_on_timeline_started"))
		Dialogic.timeline_ended.connect(Callable(self, "_on_timeline_ended"))
		Dialogic.Portraits.character_joined.connect(Callable(self, "_on_portrait_joined"))
		Dialogic.Portraits.character_left.connect(Callable(self, "_on_portrait_left"))
		Dialogic.Portraits.character_portrait_changed.connect(Callable(self, "_on_portrait_changed"))
		_signals_connected = true

func _exit_tree() -> void:
	if not _signals_connected:
		return
	var c_speaker := Callable(self, "_on_speaker_updated")
	var c_started := Callable(self, "_on_timeline_started")
	var c_ended := Callable(self, "_on_timeline_ended")
	if Dialogic.Text.speaker_updated.is_connected(c_speaker):
		Dialogic.Text.speaker_updated.disconnect(c_speaker)
	if Dialogic.timeline_started.is_connected(c_started):
		Dialogic.timeline_started.disconnect(c_started)
	if Dialogic.timeline_ended.is_connected(c_ended):
		Dialogic.timeline_ended.disconnect(c_ended)
	_signals_connected = false

func _on_timeline_started() -> void:
	left_char = null
	right_char = null
	Dialogic.Portraits.leave_all_characters("", 0.0, false)
	_inspect_all_joined("after leave_all_characters (timeline started)")

func _on_timeline_ended() -> void:
	left_char = null
	right_char = null
	Dialogic.Portraits.leave_all_characters("", 0.0, false)
	_inspect_all_joined("after leave_all_characters (timeline ended)")

func _on_speaker_updated(character) -> void:
	print("========================================================")
	print("[DBG] speaker_updated raw:", character)
	if character == null:
		print("[DBG] null character")
		return
	if not _character_has_portrait(character):
		print("[DBG] character has no portrait -> return")
		return

	# skip if exactly the same resource is already shown
	if left_char == character or right_char == character:
		print("[DBG] character resource already displayed -> skip")
		_inspect_all_joined("skip (already shown)")
		return

	# FIFO logic
	if left_char == null:
		left_char = character
		print("[DBG] assign left ->", _get_character_display_name(character))
		_show_character(character, _container_left)
	elif right_char == null:
		right_char = character
		print("[DBG] assign right ->", _get_character_display_name(character))
		_show_character(character, _container_right)
	else:
		print("[DBG] shift FIFO: left<-right, right<-new")
		var old_right = right_char
		left_char = old_right
		# move the existing node to left position (explicit)
		var left_pos := _get_container_position_id(_container_left)
		if old_right != null and left_pos != "":
			print("[DBG] move_character old_right ->", left_pos)
			Dialogic.Portraits.move_character(old_right, left_pos)
		_show_character(left_char, _container_left)
		right_char = character
		_show_character(character, _container_right)

	_inspect_all_joined("after speaker handling")
	print("========================================================")

# --- show / helpers ---

func _get_container_position_id(container: Node) -> String:
	if container == null:
		return ""
	if container.has_method("get"):
		var ids = container.get("container_ids")
		if ids and ids.size() > 0:
			return String(ids[0])
	return ""

func _show_character(character, container: Node) -> void:
	if character == null or container == null:
		print("[DBG] _show_character null input")
		return
	var pos_id := _get_container_position_id(container)
	var portrait := _get_default_portrait_name(character)
	print("[DBG] _show_character: char=", _get_character_display_name(character), " portrait=", portrait, " pos_id=", pos_id, " container=", container)
	if Dialogic.Portraits.is_character_joined(character):
		print("[DBG] already joined -> move/change")
		if pos_id != "":
			Dialogic.Portraits.move_character(character, pos_id)
		if portrait != "":
			Dialogic.Portraits.change_character_portrait(character, portrait)
	else:
		print("[DBG] not joined -> add_character with explicit pos_id")
		Dialogic.Portraits.add_character(character, container, portrait, pos_id)

	# immediate inspection
	_inspect_character_node(character, "_show_character post-change")

func _inspect_all_joined(context: String = "") -> void:
	var joined := Dialogic.Portraits.get_joined_characters()
	print("[DBG] _inspect_all_joined —", context, "| count:", joined.size())
	for j in joined:
		_inspect_character_node(j, "joined-list")

func _inspect_character_node(character, ctx: String = "") -> void:
	if character == null:
		print("[DBG] _inspect_character_node: null char for ctx:", ctx)
		return
	var info = Dialogic.Portraits.get_character_info(character)
	var cname = _get_character_display_name(character)
	print("[DBG] inspect(", ctx, ") resource:", character, " display:", cname)
	print("      info keys:", info.keys())
	if info.has("position_id"):
		print("      position_id:", info["position_id"])
	if info.has("node"):
		var node = info["node"]
		print("      node:", node, " instance_id:", node.get_instance_id())
		print("      node path:", node.get_path())
		print("      parent path:", node.get_parent() and node.get_parent().get_path() or "<no parent>")
		# global transform / position
		if node is Node2D:
			print("      Node2D global_position:", node.global_position)
			print("      Node2D global_transform:", node.global_transform)
		elif node is Control:
			# Control: print rect and global position if available
			if node.has_method("get_global_position"):
				print("      Control global_position:", node.get_global_position())
			if node.has_method("get_global_rect"):
				print("      Control global_rect:", node.get_global_rect())
		# list visual children and textures
		_inspect_visual_children(node)

func _inspect_visual_children(node: Node) -> void:
	if node == null:
		return
	print("      children count:", node.get_child_count())
	for ch in node.get_children():
		print("        child:", ch, " type:", typeof(ch), " class:", ch.get_class())
		print("--- Texture info ---")
		_print_texture_info(ch as Sprite2D)
		print("------------")
		# Sprite2D
		if ch is Sprite2D:
			var tex = ch.texture
			print("          Sprite2D texture:", tex, " res_path:", tex and tex.resource_path or "<null>", " res_instance_id:", tex and tex.get_instance_id() or "<null>")
			print("          Sprite2D position:", ch.position, " global:", ch.global_position)
		# TextureRect (Control)
		if ch is TextureRect:
			var tex = ch.texture
			print("          TextureRect texture:", tex, " res_path:", tex and tex.resource_path or "<null>", " res_instance_id:", tex and tex.get_instance_id() or "<null>")
			if ch.has_method("get_global_position"):
				print("          TextureRect global_position:", ch.get_global_position())
		# AnimatedSprite2D
		if ch is AnimatedSprite2D:
			var spr = ch
			print("          AnimatedSprite2D sprite_frames:", spr.sprite_frames, " res_path:", spr.sprite_frames and spr.sprite_frames.resource_path or "<null>", " res_instance_id:", spr.sprite_frames and spr.sprite_frames.get_instance_id() or "<null>")
		# Recurse one level deeper (limited depth)
		if ch.get_child_count() > 0:
			for gch in ch.get_children():
				# print simple info for grandchildren
				print("            grandchild:", gch, " class:", gch.get_class())
				if gch is Sprite2D:
					var tex2 = gch.texture
					print("              Sprite2D texture:", tex2, " res_path:", tex2 and tex2.resource_path or "<null>", " res_instance_id:", tex2 and tex2.get_instance_id() or "<null>")

# small helpers
func _character_has_portrait(character: Object) -> bool:
	if character == null or not character.has_method("get"):
		return false
	var dp = character.get("default_portrait")
	if dp != null and String(dp) != "":
		return true
	var portraits = character.get("portraits")
	if portraits is Dictionary and portraits.size() > 0:
		return true
	if portraits is Array and portraits.size() > 0:
		return true
	return false

func _get_default_portrait_name(character: Object) -> String:
	if character == null:
		return ""
	if character.has_method("get") and character.get("default_portrait") != null:
		return String(character.get("default_portrait"))
	return ""

func _get_character_display_name(character: Object) -> String:
	if character == null:
		return ""
	if character.has_method("get_character_name"):
		return character.get_character_name()
	if character.has_method("get_display_name_translated"):
		return character.get_display_name_translated()
	if character.has_method("get") and character.get("display_name") != null:
		return String(character.get("display_name"))
	return "<unknown>"

# call this where you inspect visual children (replace your current print of texture info)
func _print_texture_info(sprite: Sprite2D) -> void:
	if sprite == null:
		print("  sprite null")
		return
	var tex = sprite.texture
	if tex == null:
		print("  sprite.texture: <null>")
		return
	var path = tex.resource_path if tex.resource_path != null else "<no-resource-path>"
	var iid = str(tex.get_instance_id())
	print("  Sprite2D texture path:", path, " instance_id:", iid)

# signal handlers
func _on_portrait_joined(info: Dictionary) -> void:
	# info typically contains character, position_id, node etc. Print everything.
	print("[PORTRAIT SIG] character_joined info:", info)
	# If character is present, inspect node and ensure unique textures
	if info.has("character") and info["character"] != null:
		var char_ = info["character"]
		_inspect_character_node(char_, "portrait_joined")
		_ensure_unique_textures_for(char_)
		_record_current_portrait_texture(char_)

func _on_portrait_left(info: Dictionary) -> void:
	print("[PORTRAIT SIG] character_left info:", info)
	if info.has("character") and info["character"] != null:
		var char_ = info["character"]
		_portrait_texture_map.erase(char_)

func _on_portrait_changed(info: Dictionary) -> void:
	# Fired when a portrait resource/pose is changed for an already-joined character
	print("[PORTRAIT SIG] character_portrait_changed info:", info)
	if info.has("character") and info["character"] != null:
		var char_ = info["character"]
		_inspect_character_node(char_, "portrait_changed (pre-dup)")
		_ensure_unique_textures_for(char_) # mitigate possible shared-mutation
		_inspect_character_node(char_, "portrait_changed (post-dup)")
		_check_texture_change_warn(char_)

# helper: record the current sprite texture instance id in the map
func _record_current_portrait_texture(character: Object) -> void:
	var info = Dialogic.Portraits.get_character_info(character)
	if not info.has("node"):
		return
	var root_node = info["node"]
	var sprite_iid := _get_first_sprite_instance_id(root_node)
	_portrait_texture_map[character] = sprite_iid
	print("[PORTRAIT MAP] recorded", _get_character_display_name(character), " -> tex_iid:", sprite_iid)

# helper: compare last known texture to current and warn if changed
func _check_texture_change_warn(character: Object) -> void:
	var prev = _portrait_texture_map.get(character, "")
	var curr = _get_first_sprite_instance_id(Dialogic.Portraits.get_character_node(character))
	if prev != curr:
		print("[PORTRAIT WATCHDOG] texture changed for", _get_character_display_name(character), " prev:", prev, " curr:", curr)
		_portrait_texture_map[character] = curr

# helper: find first Sprite2D in the portrait root and return texture instance id string
func _get_first_sprite_instance_id(root_node: Node) -> String:
	if root_node == null:
		return ""
	# search children & grandchildren (common layout)
	for ch in root_node.get_children():
		if ch is Sprite2D:
			var tex = ch.texture
			return tex and str(tex.get_instance_id()) or ""
		for gch in ch.get_children():
			if gch is Sprite2D:
				var tex2 = gch.texture
				return tex2 and str(tex2.get_instance_id()) or ""
	return ""

# reuse the ensure-unique-textures helper (duplicate textures to break sharing)
func _ensure_unique_textures_for(character: Object) -> void:
	if character == null:
		return
	var info = Dialogic.Portraits.get_character_info(character)
	if not info.has("node"):
		return
	var root_node = info["node"]
	if root_node == null:
		return
	for ch in root_node.get_children():
		if ch is Sprite2D:
			var tex = ch.texture
			if tex != null:
				var dup = tex.duplicate()
				ch.texture = dup
				print("[DBG] duplicated Sprite2D texture for", _get_character_display_name(character), " new_instance_id:", dup.get_instance_id())
		else:
			for gch in ch.get_children():
				if gch is Sprite2D:
					var tex2 = gch.texture
					if tex2 != null:
						var dup2 = tex2.duplicate()
						gch.texture = dup2
						print("[DBG] duplicated grandchild Sprite2D texture for", _get_character_display_name(character), " new_instance_id:", dup2.get_instance_id())
	# update our portrait map record
	_record_current_portrait_texture(character)
