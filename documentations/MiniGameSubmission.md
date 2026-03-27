# Mini Game – General Requirements

## General Requirements

- Submission format: ZIP folder (compressed folder)
- Naming convention: All names (files, variables, and everything else) in PascalCase (first letter of each word capitalized) and everything in English (where possible)
- Every game must have a button that leads back to the main scene (or another scene depending on the story)
- The game must have a win condition and a lose condition → what happens when the player wins, and what happens when they lose?
- Canvases should be put to "Scale with screen size" so it scales accordingly
- The aspect ratio of your minigame should be 16:9 and "Low resolution aspect Ratios" should be turned off

`Use the template mentioned in the [Minigame documentation](MiniGames.md)


## Folder Structure

`MiniGameName` represents the name of your mini game — replace it accordingly.
```
MiniGameName/
├── Assets/
│   ├── Images/
│   │   └── ImageOne.png        ← placeholder name (replace with actual image name, e.g. "Door")
│   ├── SoundEffects/
│   │   └── SoundOne.mp3        ← placeholder name (replace with actual sound name, e.g. "OpenDoor")
│   ├── Scripts/
│   │   └── MiniGameName.cs
│   ├── Scenes/
│   │   └── MiniGameName.unity  ← IMPORTANT: the scene should be named after your minigame
│   └── …                       ← (add more folders for other asset categories if needed)
│       └── …
└── Documentation.md             ← Contains instructions on how everything works
```
