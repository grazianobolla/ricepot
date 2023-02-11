extends Label

var metadata = ConfigFile.new()

func _ready():
    var err = metadata.load("res://metadata.ini")
    if err != OK:
        return

    self.text = metadata.get_value("Metadata", "version")    