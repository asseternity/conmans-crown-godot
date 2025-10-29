@tool
extends DialogicLayoutLayer

@onready var panel: Control = $"Anchor2/Panel"

func _ready() -> void:
	super() # Godot 4+ parent _ready()

	# Don't run runtime connections while editing in the editor
	if Engine.is_editor_hint():
		return

	# hook into Dialogic's text/visibility signals
	if Dialogic.Text:
		Dialogic.Text.speaker_updated.connect(_on_speaker_updated)
		Dialogic.Text.textbox_visibility_changed.connect(_on_textbox_visibility_changed)
		Dialogic.Text.about_to_show_text.connect(_on_about_to_show_text)

	# make sure the state is correct on startup
	_refresh_panel()

func _on_speaker_updated(character: DialogicCharacter) -> void:
	# character will be null for narrator / no speaker
	_refresh_panel(character)

func _on_textbox_visibility_changed(visible: bool) -> void:
	# if textbox is hidden, kill the panel too
	if not visible:
		panel.visible = false
	else:
		_refresh_panel()

func _on_about_to_show_text(info: Dictionary) -> void:
	# fires before each new line shows, including the first line
	var char: DialogicCharacter = null
	if info.has("character"):
		char = info["character"] as DialogicCharacter
	_refresh_panel(char)

func _refresh_panel(speaker: DialogicCharacter = null) -> void:
	var who := speaker
	if who == null:
		# fallback to whatever Dialogic thinks the current speaker is
		# (returns null for narrator lines)
		who = Dialogic.Text.get_current_speaker()

	# show the portrait background panel if there *is* a speaking character,
	# hide it if it's narration / no portrait
	panel.visible = who != null
