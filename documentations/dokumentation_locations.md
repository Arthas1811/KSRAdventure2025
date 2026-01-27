# Location JSON Documentation

## Note

Before reading this document, make sure you've read:
1. [General Navigation Documentation](documentation_general_navigation.md)
2. [Save File Documentation](documentation_save_file.md)

## Purpose

Location JSON files (e.g., `/Assets/Resources/Locations/start.json` or `main_hall.json`) define the entire navigation system. They specify:
- Which images exist in each area
- How images are connected via interactive buttons (hotspots)
- What actions occur when buttons are clicked
- Under what conditions buttons and image states are visible

## JSON Structure

### 1. Simplified Overview

Heres a basic structure to understand the concept:

```json
{
  "meta": {
    "music": "Music/music_name"
  },
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

#### Meta Tag
The `"meta"` section defines area-wide settings:
- `"music"` Specifies which audio track plays throughout this entire area. The syntax has to be: `"Music/music_name"`
- The same music plays regardless of which image the player is on (within the current area)
- Music changes when moving to a different area (if that area has different music defined)

#### Navigation Flow
In this example:
- From `"image_one"`, pressing `"button_one"` takes you to `"image_two"`
- From `"image_two"`, you can press `"button_one"` to return to `"image_one"`, or `"button_two"` to go to `"image_three"`
- This creates a chain of to be navigated locations

### 2. Structure of a Single Image

Each image location has this overall structure:

```json
"image_name": {
  "states": {},
  "customHotspots": []
}
```

- Replace **`"image_name"`** with the name for that image
- **`"states"`** defines different visual states the image can have (light on/off, door open/closed, ...)
- **`"customHotspots"`** defines all  buttons (hotspots) on the current image

### 2.1 States Structure

States allow a single "image" to display different images based on game conditions.

**Empty structure (not functional):**
```json
"states": {}
```

**Proper structure with states:**
```json
"states": {
  "main": {
    "requirements": [],
    "path": "Assets/Images/Navigation/start/1.jpg"
  },
  "lightOn": {
    "requirements": ["states:start:lightOn:true"],
    "path": "Assets/Images/Navigation/start/1_light_on.jpg"
  }
}
```

#### Main State
Every image must have a `"main"` state:
- **`"requirements"`**: An array of conditions that must be met to display this state (empty `[]` means always available)
- **`"path"`**: The file path to the image for this state

#### Additional States
You can add multiple states for different conditions:
- Example: `"lightOn"` state shows a different image when lights are on
- `"requirements"` contains the Conditions that must be met to show this state/image instead of the `"main"` image
- `"path"`: The file path to the image of the state

**Important:** The game checks states in order. The last state which requirements are met will be displayed.

### 2.1.1 Requirements Syntax

Requirements determine when a spefifi state (image) or button (looked at later) is available. Multiple requirements can be specified, and **all must be met** for the element to activate.

#### Check for Item Ownership

**Syntax:**
```json
"item:item_id:true"
```

**Parameters:**
- `item_id`: The ID of the item to check (e.g., `"redbull"`)
- `true`: Player must own the item
- `false`: Player must NOT own the item

**Examples:**
```json
"requirements": ["item:redbull:true"]
```
This requires the player to own the `"redbull"` item.

```json
"requirements": ["item:key:false"]
```
This requires the player to NOT own the `"key"` item.

#### Check for State Variable

**Syntax:**
```json
"states:location_name:variable_name:expected_value"
```

**Parameters:**
- `location_name`: The location/image identifier in the save file
- `variable_name`: The specific variable to check
- `expected_value`: The value the variable must equal (can be `true`, `false`, or any other value)

**Example:**
```json
"requirements": ["states:start:openDoor:true"]
```

**What this checks in the save file:**
```json
{
  "currentImage": "6",
  "currentLocation": "start",
  "itemsOwned": [
    "frog",
    "glycerin",
    "redbull"
  ],
  "states": {
    "start": {
      "openDoor": true  ← Must be true for above mentioned requirement to be met
    }
  }
}
```

**Multiple Requirements Example:**
```json
"requirements": [
  "item:redbull:true",
  "states:start:openDoor:false"
]
```
This requires BOTH: first the player needs to own `"redbull"` and `"openDoor"` is `false` (door is not open).

### 2.2 Custom Hotspots Structure

Hotspots are the interactive buttons/areas on images that players can click.

**Empty structure (not functional):**
```json
"customHotspots": []
```

**Proper structure with hotspots:**
```json
"customHotspots": [
  {
    "actions": ["action_one", "action_two"],
    "requirements": [],
    "polygonString": "0.123,0.456;0.789,0.012;0.345,0.678"
  },
  {
    "actions": ["action_one"],
    "requirements": ["item:key:true"],
    "polygonString": "0.111,0.222;0.333,0.444;0.555,0.666"
  }
]
```

Each object in the array represents one clickable button/hotspot with:
- `"actions"`: What happens when clicked
- `"requirements"`: requirements that must be met for the button to be visible/active
- `"polygonString"`: the shape of the clickable area and position

### 2.2.1 Actions Syntax

Actions define what happens when a hotspot is clicked. Multiple actions execute in the order they are listed.

---
**possible actions:**

#### Navigate to Another Image

**Syntax:**
```json
"image_name"
```

Effect: Transitions to the specified image (IMPORTANT: works only within the current area).

**Example:**
```json
"actions": ["6"]
```
Navigates to image `"6"`.

---
#### Add or Remove Items

**Syntax:**
```json
"item:add:item_id"
"item:remove:item_id"
```

**Parameters:**
- `add` or `remove`: Whether to add to or remove from inventory
- `item_id`: The ID of the item (must match the ID of the item, not the in game name)

**Examples:**
```json
"actions": ["item:add:redbull"]
```
Adds the `"redbull"` item to the player's inventory.

```json
"actions": ["item:remove:key"]
```
Removes the `"key"` item from the players inventory.

---

#### Start a Dialogue

**Syntax:**
```json
"dialogue:dialogue_name"
```

Effect: Activates the specified dialogue sequence.

**Example:**
```json
"actions": ["dialogue:library_dialogue"]
```
Starts the `"library_dialogue"` dialogue.

---
#### Play Sound Effect

**Syntax:**
```json
"sfx:sound_name"
```

Effect: Plays the specified sound effect.

**Example:**
```json
"actions": ["sfx:opening_door"]
```
Plays the `"opening_door"` sound effect.

---
#### Switch to Different Location/Area

**Syntax:**
```json
"location:location_name:image_name"
```

**Parameters:**
- `location_name`: The area to switch to (e.g. `"library"` or  `"underground"` or whatever)
- `image_name` defines the specific image within that area to land on

**Example:**
```json
"actions": ["location:main_hall:1"]
```
Switches to the `"main_hall"` area and displays image `"1"` in the main_hall.

---
#### Modify State Variables (in the save file)

**Syntax:**
```json
"data:states:image_name:variable_name:new_value"
```

**Parameters:**
- `image_name`: The image identifier in the save file
- `variable_name`: The variable to modify
- `new_value`: The value to set (can be `true`, `false`, or any other value)

**Example:**
```json
"actions": ["data:states:start:openDoor:true"]
```

Effect on save file:
```json
{
  "currentImage": "6",
  "currentLocation": "start",
  "itemsOwned": [
    "frog",
    "glycerin",
    "redbull"
  ],
  "states": {
    "start": {
      "openDoor": false  ← Changes this variable to true
    }
  }
}
```

---
#### Switch Scene
**Syntax:**
```json
"scene:scene_name"
```

**Parameters:**
- `scene_name`: The image of the scene you want to switch to (could be a minigame, cutscene or any othere scene in unity...)

**Example**:
```json
"actions": ["scene:chemie_keller"]
```

switches to scene `chemie_keller`

---
#### Multiple Actions Example

Actions execute in order:
```json
"actions": [
  "sfx:opening_door",
  "data:states:start:openDoor:true",
  "location:main_hall:1"
]
```
This plays a sound, updates the door state, then switches locations.

### 2.2.2 Hotspot Requirements

Requirements for hotspots work the same as state requirements (see [section 2.1.1](#211-requirements-syntax)).

These requirements determine whether a hotspot is clickable. Use them to create conditional interactions

### 2.2.3 Polygon String Format

The `"polygonString"` defines the clickable areas shape using coordinate points.

**Format:**
```
"x1,y1;x2,y2;x3,y3;x4,y4"
```

**Structure:**
- Each point has two coordinates: `x,y` (comma-separated)
- Points are separated by semicolons: `;`
- Coordinates are normalized (0.0 to 1.0) representing percentage positions on the image

**Example:**
```json
"polygonString": "0.041254125412541254,0.6328382838283828;0.13366336633663367,0.5932343234323432;0.13036303630363036,0.4744224422442245;0.03135313531353135,0.5008250825082508"
```

**Important Rules:**
- **Add points in CLOCKWISE order** becuase this is vital for proper rendering
- At least 3 points (triangle, else it would just be a line and not a area)
- No maximum amount of points

**Visual Example:**
```
Point 1 (x1,y1) -------- Point 2 (x2,y2)
    |                         |
    |    Clickable Area       |
    |                         |
Point 4 (x4,y4) -------- Point 3 (x3,y3)
```
Points should be added: 1 → 2 → 3 → 4 (clockwise).

### 3. Complete Single Image Example

Combining all elements:

```json
"image_name": {
  "states": {
    "main": {
      "requirements": [],
      "path": "Assets/Images/Navigation/start/1.jpg"
    },
    "lightOn": {
      "requirements": ["states:start:lightOn:true"],
      "path": "Assets/Images/Navigation/start/1_light_on.jpg"
    }
  },
  "customHotspots": [
    {
      "actions": ["2"],
      "requirements": [],
      "polygonString": "0.041254125412541254,0.6328382838283828;0.13366336633663367,0.5932343234323432;0.13036303630363036,0.4744224422442245;0.03135313531353135,0.5008250825082508"
    },
    {
      "actions": ["item:add:key", "data:states:start:keyTaken:true"],
      "requirements": ["item:key:false"],
      "polygonString": "0.041254125412541254,0.6328382838283828;0.13366336633663367,0.5932343234323432;0.13036303630363036,0.4744224422442245;0.03135313531353135,0.5008250825082508"
    }
  ]
}
```

**This configuration:**
- Shows the main image by default
- Shows the `"lightOn"` image when `"lightOn"` state is true
- Has a button that always navigates to image `"2"`
- Has a button that adds a key to the inventory and updates state (in save file) that the key was taken (picked up)

### 4. Complete Multi-Image JSON

A full location file with multiple images could look somewhat like this:

```json
{
  "meta": {
    "music": "Music/ambient_music_for_this_area"
  },
  "image_one": {
    "states": {
      "main": {
        "requirements": [],
        "path": "Assets/Images/Navigation/start/1.jpg"
      }
    },
    "customHotspots": [
      {
        "actions": ["image_two"],
        "requirements": [],
        "polygonString": "0.041254125412541254,0.6328382838283828;0.13366336633663367,0.5932343234323432;0.13036303630363036,0.4744224422442245;0.03135313531353135,0.5008250825082508"
      }
    ]
  },
  "image_two": {
    "states": {
      "main": {
        "requirements": [],
        "path": "Assets/Images/Navigation/start/2.jpg"
      }
    },
    "customHotspots": [
      {
        "actions": ["image_one"],
        "requirements": [],
        "polygonString": "0.041254125412541254,0.6328382838283828;0.13366336633663367,0.5932343234323432;0.13036303630363036,0.4744224422442245;0.03135313531353135,0.5008250825082508"
      }
    ]
  }
}
```

**You can add unlimited images** following this pattern.

## JSON Generator Tool

To simplify the creation of hotspots, use the [JSON Generator](https://lesieber.github.io/hotspots-website)

### How to Use the Generator

1. **Upload Image:**
   - Drag and drop or click to upload a 360-degree image
   - The image name field auto fills but can be edited **(and should be edited if the name does not match the final name of the image in the game files)**

2. **Image requirements**
    - Add requirments which have to be met that this image/location can be visited (even though there are also requiremnts for buttons, if you have multiple buttons pointing to one image then you dont have to add the same requirent for all these buttons)

3. **Create Hotspot:**
   - Click the **"new area"** button
   - Click on the image to place corner points
   - **IMPORTANT:** Add points in **CLOCKWISE** order
   - Each click adds a corner point to define the clickable area

4. **Define Actions:**
   - Click the **"action"** input field
   - Enter an action from [section 2.2.1](#221-actions-syntax) (e.g.  `"6"` or `"item:add:key"`)
   - Additional action fields appear automatically if you need multiple actions
   - Leave extra fields empty if not needed

5. **Define Requirements:**
   - Click the **"requirement"** input field
   - Enter a requiremnet (or leave empty if no requirements should be met) from [section 2.1.1](#211-requirements-syntax) (e.g. `"states:start:openDoor:true"` or `"item:redbull:true"`)
   - Additional requirement fields appear automatically if you need multiple

6. **Complete Hotspot:**
   - Click **"finish area"** when done with this hotspot
   - Repeat steps 3-6 for additional hotspots

7. **Generate Code:**
   - Click **"generate code"** when all hotspots are complete
   - Click **"copy code"** or manually select and copy the output

### Adding Generated Code to Your File

To integrate the generated code into your location JSON file (e.g., `/Assets/Resources/Locations/start.json`):

1. Open your location JSON file (located in `/Assets/Resources/Locations/your_JSON_file`)
2. Paste the generated code
3. Add a comma after each image object to separate them (except the last one)
4. Add the meta tag at the top for music in that area

**Example:**
```json
{
  "meta": {
    "music": "Music/music_start_location"
  },
  "image_one_name": {
    "states": {
      "main": {
        "requirements": [],
        "path": "Assets/Images/Navigation/start/1.jpg"
      }
    },
    "customHotspots": []
  },  ← Comma here to separate
  "image_two_name": {
    "states": {
      "main": {
        "requirements": [],
        "path": "Assets/Images/Navigation/start/2.jpg"
      }
    },
    "customHotspots": []
  }  ← No comma on the last entry
}
```

**Note:** After pasting generated code, you can still edit  manually:
- Replace the path in the `"states"` section with the correct image paths (if not already correct)
- Add requirements if more are needed (when forgot to add some in json generater) 
- Add the `"meta"` section if it's a new location file
