# Documentation
## Json Structure
The [locations.json](/Assets/Scripts/locations.json) file is used to build up the whole navigation system and link images to one another.

The Json Structure is built as following:
```json
    "image_name": {
        "path": "Assets/Images/image_name.jpg",
        "costumHotspots": [
            {
                "action": "2",
                "polygonString": "0.47452445652173914,0.6022418478260869;0.5322690217391305,0.6059782608695652;0.5407608695652174,0.37024456521739135;0.47927989130434784,0.3627717391304348"
            }
        ]
    }
```
**"image_name"** at the beginning represents the name of the image file. The **"path"** should contain the image path. In this structure, **"costumHotspots"** was used as the definition of an area one clicks to complete an action (like switching to the next image or starting a minigame). The to be completed action is defined as **"action"** and can either contain the image name (to switch to the next image):
```json
"action": "image_name",
```
or a unity scene name (to switch to the named scene like a minigame or a cutscene): 
```json
"action": "scene:scene_name",
```
This action has to be defined as **scene:scene_name** or else the code will think it's and image name. 

The polygon string contains coordinates of specific points on the 360° image:
```json
"polygonString": "0.47452445652173914,0.6022418478260869;0.5322690217391305,0.6059782608695652;0.5407608695652174,0.37024456521739135;0.47927989130434784,0.3627717391304348"
```
This string can be generated on [this website](https://lesieber.github.io/hotspots-website/polygon.html)

### Json generator:
Using the [Json generator](https://lesieber.github.io/hotspots-website) you can effortlessly create a json code with the above mentioned structure for a given image.

To use it, either drag and drop or upload an image. The input field at the top is the image name and will be autofilled on uploading an image but can be edited if needed. After uploading and image, click the **"new area"** button to create an area which can be clicked later ingame (to complete an action which was mentioned above). Click on the image to add points/dots (the corners of the desired area). The corners/dots/points have to be added/clicked **CLOCKWISE** to ensure that the area generates correctly when playing the game. When you're finished with the area, click on the input field named **"action"**. Enter either the image name to which the area should lead to or a unity scene name (in the above mentioned syntax with: **"scene:scene_name"**). After completing the area, click **"finish area"**, to finish the area. If you want to add more areas, press **"new area"** again and repeat the process. As soon as all desired areas have been created, click **"generate code"** to get the output. Either select and copy the code or just press **"copy code"** to simply copy the code
### Adding the code to the file:
To add the generated code to the [final file](/Assets/Scripts/locations.json), open the file and paste the code into the document. To add more code snippets you have to put a comma at the end (to seperate them) as following:
```json
    "image_name": {
        "path": "Assets/Images/image_name.jpg",
        "costumHotspots": [
            {
                "action": "2",
                "polygonString": "0.47452445652173914,0.6022418478260869;0.5322690217391305,0.6059782608695652;0.5407608695652174,0.37024456521739135;0.47927989130434784,0.3627717391304348"
            }
        ]
    }, <-------
        "image_name": {
        "path": "Assets/Images/image_name.jpg",
        "costumHotspots": [
            {
                "action": "2",
                "polygonString": "0.47452445652173914,0.6022418478260869;0.5322690217391305,0.6059782608695652;0.5407608695652174,0.37024456521739135;0.47927989130434784,0.3627717391304348"
            }
        ]
    }
```

