# General Navigation

The navigation system is entirely image-based. Each image represents a location, and navigation between images is made using interactive buttons (hotspots) embedded within those images.

All navigation data is defined in **JSON files**.



## Core Concept

- Each area of the game (e.g. library, underground tunnels) is represented by **one JSON file**.
- That JSON file contains:
  - All images belonging to that area
  - All buttons (hotspots) on those images
  - All navigation logic between images

-> Images link to other images <br>
-> Buttons define how and when those links are activated



## Example

- The *library* could be one JSON file containing all library images.
- The *underground tunnels* would be a separate JSON file with its own images and navigation logic.
---

## Reading Order (Important)

Please read the documentations (the ones revolving around navigation) in the following order (after reading this one):

1. [Save File Documentation](SaveFile.md)  
2. [Location / JSON Structure Documentation](Locations.md)
