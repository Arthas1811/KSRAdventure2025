# Dialogue Documentation 

## General

Dialogue data (json, images) must be stored inside the folder:
```
Assets/Resources/Dialogue/your_dialogue
```
If the data is placed anywhere else, the framework can't load it.\
Each dialogue has its own folder with its images and json inside.
Every dialogue consists of a list of nodes. Each node represents one step in the conversation.

## How to trigger dialogue
A dialogue is triggered through a hotspot action in a location json.

```json
"customHotspots": [
            {
                "actions": ["dialogue:dialogue_json"],
                "requirements": [],
                "polygonString": "polygon_string"
            }
        ]
```
It's important that you write it like ```dialogue:dialogue_json```. "dialogue_json" should exactly match the file name of your json (without file extension). 

## Json structure

The json file is structured as follows:
```json
{
  "id": "your dialogue",
  "nodes": [
    {
      "id": 1,
      "speaker": "Speaker Name (capitalized pls)",
      "text": "dialogue text",
      "img_path": "Dialogue/your_dialogue/img_name",
      "choices": [
        { "text": "choice 1", "next": 2 },
        { "text": "choice 2", "next": -1 }
      ]
    },
    {
      "id": 2,
      "speaker": "Speaker Name",
      "text": "dialogue text",
      "img_path": "Dialogue/your_dialogue/img_name",
      "choices": [
        { "text": "choice 1", "next": 3 },
        { "text": "choice 1", "next": -1 }
      ]
    },
    {
      "id": 3,
      "speaker": "Speaker Name",
      "text": "dialogue text",
      "img_path": "Dialogue/your_dialogue/img_name",
      "next": -1
    }
  ]
}
```
## Explanation of fields

```id```:
The name of your dialogue. It doesn't need to have underscores.

```nodes```:
A list of all dialogue steps.\
Each node has the following fields:

1. ```id```:
A number for this node. This is what ```next``` refers to.

2. ```speaker```:
The name of the character speaking. The game shows the name exactly as written in the json.

3. ```text```:
The dialogue text, shown exactly as written in the json.

4. ```img_path```:
The image displayed. You can change the image per node if you just change the image path.\
The path must follow this structure:
```
Dialogue/your_dialogue/img_name (without file extension!)
```

An example would be:

```
Dialogue/library_dialogue/happy
```
5. ```choices```:
A list of choices the player can select. Each choice has:
  - "text" &rarr; text inside choice button
  - "next" &rarr; the id of the next node

This structure allows you to create different dialogue endings depending on what choice the player makes. It's also possible to make more than two choices.\
If "next" is -1, the dialogue ends.\
If a node has no choices (e.g. the final node), you can omit the field "choices":
```json
{
  "id": 3,
  "speaker": "Speaker Name",
  "text": "dialogue text",
  "img_path": "Dialogue/your_dialogue/img_name",
  "next": -1
}
```