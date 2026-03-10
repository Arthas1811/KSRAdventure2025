# Audio Documentation

## General stuff
The framework supports two types of audio: sound effects (SFX) and music. All audio files must be placed inside the folder:
```
Assets/Resources/Audio
```
Unity supports common audio formats such as MP3, WAV, OGG.

## SFX

### Where to put SFX
The sound effects are found in ```Assets/Resources/Audio/SFX```.
The full path of a sound effect would be ```Assets/Resources/Audio/SFX/audio_name.wav```

### How to trigger SFX
SFX are triggered through hotspot actions inside a location json:

```json
"customHotspots": [
            {
                "actions": ["sfx:audio_name", "location:room:x"],
                "requirements": [],
                "polygonString": "polygon_string"
            }
        ]
```
It doesn't matter whether the SFX-action is before or after other actions. The engine handles both.

### Correct syntax
SFX-actions must be written like this:
```sfx:audio_name```.\
Don't include the file extension or folder path.

---

## Music

### Where to put music
The music is in ```Assets/Resources/Audio/Music```
The full path would be ```Assets/Resources/Audio/Music/audio_name.wav```

### How to assign music to location
The music is stored in the ```meta``` section of a location json:

```json
{
    "meta": {
        "music": "Music/audio_name"
    },
    "1": {
        "states": {
            "main": {
                "requirements": [],
                "path": "path_to_img"
            }
        },
        "customHotspots": [
            {
                "actions": ["actions"],
                "requirements": [],
                "polygonString": "polygon_string"
            }
        ]
    }
}
```

### Correct syntax
Use this format for music: ```Music/audio_name```.\
Again, don't include the file extension or folder path.