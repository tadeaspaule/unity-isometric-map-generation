# unity-isometric-map-generation

## Note on assets
Some assets are from Unity's isometric demo project, which you can learn more about [here](https://blogs.unity3d.com/2019/03/18/isometric-2d-environments-with-tilemap/). These include:
- Tiles (palettes, sprites, colliders, ...)
- Characters (sprites, animations, movement scripts)
- Basic setup for isometric (scene, camera)

## What's provided
- Can generate random square maps of variously shaped rectangular rooms (hallways, single block rooms, big chambers) connected by bridges
- This also sets up colliders so you can't walk out of the map
- Pathfinding methods to get AI characters around the map
- Some basic content for rooms (like pillars in hallways, shown below)
- Methods for creating elevated platforms and stairs, complete with colliders

## Screenshots
![Generated map example](Screenshots/mapgenscr.png?raw=true "Generated map example")

![Generated map example 2](Screenshots/mapgenscr2.png?raw=true "Generated map example 2")


## Known bugs
1. The stair colliders aren't set up 100% perfectly, so part of the stairs is blocked
![Stair collider bug 1](Screenshots/partlyblockingstairsbug.png?raw=true "Stair collider bug 1")

2. When not placed at the edge of an elevated platform, the stairs remove one top edge collider next to them
![Stair collider bug 2](Screenshots/removeonecolliderbug.png?raw=true "Stair collider bug 2")
