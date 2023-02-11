@tool
extends EditorPlugin

var export_plugin: EditorExportPlugin

func _enter_tree():
	export_plugin = preload("export_plugin.gd").new()
	add_export_plugin(export_plugin)

func _exit_tree():
	remove_export_plugin(export_plugin)
	export_plugin = null