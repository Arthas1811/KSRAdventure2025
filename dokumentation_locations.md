# Documentation Json
## Note
Before taking a look at this, read [this](documentation_save_file.md) dokumentation first.
## Json Structure
The [locations.json](/Assets/Resources/Locations/locations.json) file in thicase is used to build up the whole navigation system and link images to one another. Jsons are mostly used to store data as it is in this case.
### 1. Simplified structure
If you were to simplify the whole structure it would look like this:
```json
{
    "image_one": {
        "button_one": "image_two"
    },
    "image_two": {
        "button_one": "image_one",
        "button_two": "image_three"
    },
    "image_three": {
        "button_one": "image_two",
        "button_two": "image_four"
    },
    "image_four": {
        "button_one": "image_three"
    }
}
```
In this case each image would have one or two buttons leading to the next image. Images in this case are the locations where you can go. Here for example you could go from "image_one" to "image_two" using "button_one". When you're at image two you could take either button one back to the first image or the second button leading to the next (the third) image.


The real Json Structure is a little more complicated and will be looked at in more detail later.

### 2. Split structure of one image
Looking at the structure of just one image it would overall look like this:
```json
"image_name": {
    "states": {},
    "customHotspots": []
}
```
"image_name" should be replaced with the name of the image/location.

The first definition "states" lets an image have different states (for example: if a location can have a door closed/open, lights on/off it needs different images for the different states which are stored in states).
### 2.1 States structure
The states on top is empty (should not be empty):
```json
"states": {}
```
After editing, the structure inside "states" should look like somewhat like this:
```json
    "states": {
        "main": {
            "requirements": [],
            "path": "path_to_main_image"
        },
        "lightOn": {
            "requirements": ["requirement_one"],
            "path": "path_to"
        }
    },
```

the "main" part in states has two definitions: "requirements" and "path".
"requirements" should be the requirements which have to be filled that this location can be accessed and "path" must be set to the path of the image for the **main state** of the image.

If a image has more than one state (for example light on and light off) this specific state needs to be added. In this case the state "lightOn" was added and it also contains the requirements and the path to the image with the lights on (or off, depending on which the main state of the image is).
### 2.1.1 requirements
the list "requirements" in a specific state can be filled with one or more requirements.
possible requirements are:

check for item:
```json
"item:item_id:true"
```

which checks if an item is owned or not (true if it has to be owned and false if not)

check for state:
```json
"states:start:openDoor:true"
```

which goes into the save file and looks if the value is equal to the last element of the requirement which would be "true" here. This would be what gets checked:
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
      "openDoor": true <--------
    },
    "image_two": {
      "lightOn": false
    }
  }
}
```
### 2.2 Custom hotspots structure
The states on top is empty (should also not be empty):
```json
"customHotspots": []
```
After editing, the structure inside "customHotspots" should look like somewhat like this:
```json
"customHotspots": [
    {
        "actions": ["action_one", "action_two"],
        "requirements": [],
        "polygonString": "polygon_string"
    },
    {
        "actions": ["action_one", "action_two"],
        "requirements": ["requirements"],
        "polygonString": "polygon_string"
    }
]
```
Each segment represents one button (hotspot) which can be pressed to preform an action.

### 2.2.1 Possible actions
The list "actions" can contain unlimited actions. These actions will be preformed on button press.
possible actions are:

navigation:
```json
"image_name"
```
If the action is just set to an image name, the button will lead to the defined image and will switch on button press.

items:
```json
"item:add:item_id"
```
If you want a button to add or remove an item to the inventory you first need to do "item" then ":" and following either "add" or "remove" with another ":" and then finally the item you want to add (Note: it has to be the item id and not the item name)

dialogues:
```json
"dialogue:dialogue_name"
```
if you want to start a dialogue on button press, write "dialogue" at the beginning followed by ":" and then the dialogue name at the end.

change states:
```json
"data:states:start:openDoor:false"
```
to change a state (a variable in the save file) you first have to write "data:" to indicate that you want to change data in the save file and then the path to the variable

the example above would change the variable "openDoor" to false in the save file:
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
      "openDoor": true <-------- would be changed to false
    },
    "image_two": {
      "lightOn": false
    }
  }
}
```
### 2.2.2 possible requirements
The requirements list in costum hotspots are the requirements which have to be met that the button press will complete the given actions. Possible requirements are the same as in 2.1.1 requirements.

 ### 2.2.3 Polygonstring
The "polygonString" contains a string containing all corners (points added on a area/button/hotspot on the website which is mentioned later) the button has. It defines how the buttons shape is. The string is built like this (below is a polygonstring with example values):
```json
"polygonString": "0.041254125412541254,0.6328382838283828;0.13366336633663367,0.5932343234323432;0.13036303630363036,0.4744224422442245;0.03135313531353135,0.5008250825082508"
```

The numbers don't have to have this many digits after the comma but it's nice to have :). One corner of a button consits of two coordinates (x and y on the flat 360 image) these coordinates are formatted like this: x,y (split by a comma) and corners are split with ";" -> x,y;x,y;x,y (this would be for a triangle with three corners)
### 3. Complete structure of one image
If you would now put everything from above together, you would get something that would look like this:
```json
"image_name": {
    "states": {
        "main": {
            "requirements": [],
            "path": "path_to_main_image"
        },
        "lightOn": {
            "requirements": ["requirement_one"],
            "path": "path_to_state_image"
        }
    },
    "customHotspots": [
        {
            "actions": ["action_one", "action_two"],
            "requirements": [],
            "polygonString": "polygon_string"
        },
        {
            "actions": ["action_one", "action_two"],
            "requirements": ["requirement_one", "requirement_two"],
            "polygonString": "polygon_string"
        }
    ]
}
```
### 4. Complete structure
The structure above is just for one image/location, now if you want multiple images/locations you need to duplicate the structure above and edit it so it fits for the second image/location

the final json (without "states" and "costumHotpots" filled with data) would look something like this:
```json
{
    "image_one_name": {
        "states": {},
        "customHotspots": []
    },
    "image_two_name": {
        "states": {},
        "customHotspots": []
    },
    "image_three_name": {
        "states": {},
        "customHotspots": []
    }
}
```
unlimited locations/images can be added in this format.


## Json generator:
Using the [Json generator](https://lesieber.github.io/hotspots-website) you can effortlessly create the json code (for one image) with the above mentioned structure.

To use it, either drag and drop or upload an image. The input field at the top is the image name and will be autofilled on uploading an image but can be edited if needed.</br>
After uploading an image, click the **"new area"** button to create an area (hotspot/button) which can be clicked later ingame. Click on the image to add points (the corners of the desired area).</br> The corners/dots/points have to be added/clicked **CLOCKWISE** to ensure that the area generates correctly when playing the game. When you're finished with the area, click on the input field named **"action"**. Enter an action from **2.2.1 Possible actions**. After adding one action another action field appears (to add more actions) but can be ignored if only one action is needed. After completing the area, click **"finish area"**. If you want to add more areas, press **"new area"** and repeat the process. As soon as all desired areas have been created, click **"generate code"** to get the output.

 Either select and copy the code or just press **"copy code"** to simply copy the code
## Adding the code to the file:
To add the generated code to the [final file](/Assets/Resources/Locations/locations.json), open the file and paste the code into the document. To add more code snippets you have to put a comma at the end (to seperate them) as following:
```json
{
    "image_one_name": {
        "states": {},
        "customHotspots":[]
    }, <-------
    "image_two_name": {
        "states": {},
        "customHotspots":[]
    }
}
```