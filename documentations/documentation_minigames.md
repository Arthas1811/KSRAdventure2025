# Minigame Documentation
This documentation explains the basics of the minigame template.

## End Game on Win
To end the game on a win, call the win function.

```win() ```

The win function is predefined with examples in the template script. Changes in this function may need to be made regarding the items gained after the minigame [Manage Items](#manage-items), changed [game states](#change-a-gamestate) (for example, opened doors) or the spawn location when the game is won.

## End Game on a Loss
To end the game on a loss use the predefined lose function ```lose()```. 
It sets the player back to the position from where the minigame was started. The items are set to the same items as the player possessed before attempting the minigame. No changes need to be made here.

But if a loss that has an impact on the game, wheter a different spawn location or used items or other changes, needs to be implemented, a new function must be created independently. If there are questions feel free to ask the TALITS for further help.

## Manage Items
### Create a New Item
The Items that can be added to the inventory can be found in the ```Assets/Scripts/Inventory/items.json``` file in the template minigame. The format is as follows:

``` 
{ "id": "example_id", "data": { "name": "example_name", "description": "example_description", "image": "example_image_path" } }
```

There are a number of parameters that need to be adjusted:

```example_id```: A short unique term to give the object a unique ID

```example_name```: The name that shows in the ingame inventory

```example_description```: A short sentence that shows when the player hoveres over the item in the inventory. It should explain more or less what the item does 

```example_image_path```: The path of the image that the item should have. The images have to be stored in ```Inventory/inventory_items/"example_image_name"```

As an example item a redbull is added below. Additionally Multiple other items are already present within ```items.json```the template minigame.
``` 
{ "id": "redbull", "data": { "name": "RedBull", "description": "Energydrink mit mythischer Heilkraft", "image": "Inventory/inventory_items/redbull" } },
```

### Gain an Item
If an Item should be gained after the game is won the following command can be used within the ``` void  win()``` function.

```inventory.add("example_id")```

e.g. ```inventory.add("redbull")```



## Change a GameState 
If a game state needs to be changed, e.g. a door needs to open after a won minigame.

### Add a New Game State
See [saveDataDocumentation](documentation_save_file.md) script ...

### Access/ Change a Game State
To change an already defined game state use ```saveData["states"]["example_image"]["example_state_name"] = true;```

Parameters:

```example_image```: The name of the image where the targeted object is located. E.g. "forumEntrance", "mediothekGrouptable" ...

```example_state_name```: The name of the state that is intended to change. E.g. "doorOpen", "lightsOn", "spawnNPC1" ...

```true```: Can be set to true or false if only two states exist. Otherwise it can be set to the corresponding state number. (An example would be if there were 5 different brightnesses of the room light in a room. Then a lightsOn function could be set to a number from 1-5 each corresponding to a different light level)

To save the newly defined state the following appendix is needed:

```saveDataManager.saveData(saveData);```

Example Game State Change that opens the entrance door of the Forum:
```
saveData["states"]["forumEntrance"]["openDoor"] = true; 
saveDataManager.saveData(saveData);
```

## Exit Game
To exit the game and bring the player back to the main scene use ```SceneManager.LoadScene("main");```

Is already implemented in the template code and most likely does not have to be changed