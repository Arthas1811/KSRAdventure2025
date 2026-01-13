## Save File structure
The save file contains all important information about the game state which can be accessed from everywhere. This file is very important that all infos stay saved when the game gets closed (or crashes unexpectedly) so you can continue playing where you left off.
The current structure looks like this:
```json
{
  "currentImage": "start",
  "itemsOwned": [
    "frog",
    "glycerin",
    "redbull",
    "redbull"
  ],
  "states": {
    "start": {
      "openDoor": true
    }
  }
}
```
"currentImage" saves the current image (location) the player is on.

"itemsOwned" are all items the player has obtained. These are equivalent to the inventory.

"states" contains all states for various different use cases. The "start" category in "states" contains  all states the image "start" can have like in this case an open door: "openDoor": true. In this case "openDoor" is a bool which means it can either be set to true or false. More variables can be added for various images and these variables can be put to anything like an int, a string, a bool...

the save json without specific data would look like this:
```json
{
  "currentImage": "current_image",
  "itemsOwned": [
    "item_one_id",
    "item_two_id",
    "item_three_oid",
    "item_four_id"
  ],
  "states": {
    "image_one": {
      "variable": variable_value (like true, false, "value", 1, 2, ...)
    }
  }
}
```