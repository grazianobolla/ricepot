extends EditorExportPlugin

func _export_begin(features, is_debug, path, flags):
    var ver_stamp = int(Time.get_unix_time_from_system())
    _save_meta(str(ver_stamp))

func _save_meta(version_stamp: String):
    var data = ConfigFile.new()
    data.set_value("Metadata", "version", version_stamp)
    data.save("res://metadata.ini")
    print("Generated metadata file!")