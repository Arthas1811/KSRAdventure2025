
# Save File Documentation

## Note

Before reading this document, read
[General Navigation](documentation_general_navigation.md).


## Purpose of the Save File

The save file contains all game state data.  
It is globally accessible and makes sure that the players progress is preserved when the game is closed or crashes.

Without this file, it would not be possible to resume the game from the last played position.


## Save File Structure

The save file is a single JSON object with the following keys:

- ```"currentLocation"```
- ```"currentImage"```
- ```"itemsOwned"```
- ```"states"```

---

### Empty / Template Save File

```json
{
  "currentLocation": "current_location",
  "currentImage": "current_image",
  "itemsOwned": [
    "item_one_id",
    "item_two_id",
    "item_three_id",
    "item_four_id"
  ],
  "states": {
    "image_one": {
      "variable_one": "variable_value",
      "variable_two": "variable_value"
    },
    "image_two": {
      "variable_one": "variable_value",
      "variable_two": "variable_value"
    }
  }
}
````


## Field Explanations

 `"currentLocation"`

Stores the currently active area, meaning the JSON file that is currently loaded.

Example:

```json
"currentLocation": "start"
```


---

`"currentImage"`

Stores the current image the player is on within the current location/area.

Example:

```json
"currentImage": "6"
```


---

`"itemsOwned"`

A list of item Ids  representing the players inventory.
Items are referenced by **ID**, **not** by display name.

Example:

```json
"itemsOwned": ["frog", "glycerin", "redbull"]
```

---

`"states"`

Stores variables that represent the current state of images, objects, or interactions.

* Each key under `"states"` corresponds to a image ID
* Each image can store an unlimited amount of variables
* Variables can be of any type (boolean, integer, string, etc.)

Example use case:

* A door that can be open or closed
* Lights that can be on or off
* A puzzle that has been solved (or not solved)


#### Example State Variable

```json
"states": {
  "start": {
    "openDoor": false
  }
}
```

Here:
* `"start"` is the name of the image containing the varibels
* `"openDoor"` is a variable (boolean) belonging to "start"
* `false` means the door is currently closed



## Filled Example Save File

```json
{
  "currentLocation": "start",
  "currentImage": "6",
  "itemsOwned": [
    "frog",
    "glycerin",
    "redbull"
  ],
  "states": {
    "start": {
      "openDoor": false
    }
  }
}
```


## Modifying the Save File

The save file can be edited if needed.

To Check items, Change states or do actions based on saved data, see:
[Location/JSON Structure Documentation](dokumentation_locations.md)
