# unity-isometric-map-generation

## What's included:
- Generates a random square map of variously shaped rectangular rooms (hallways, single block rooms, big chambers) connected by bridges
- Sets up colliders so you can't walk out of the map
- Pathfinding methods to get AI characters around the map
- Some basic content for rooms (like pillars in hallways, shown below)
- Methods for creating elevated platforms and stairs, complete with colliders

![Generated map example](Screenshots/mapgenscr.png?raw=true "Generated map example")


## Known bugs
1. The stair colliders aren't set up 100% perfectly, so part of the stairs is blocked
![Stair collider bug 1](Screenshots/partlyblockingstairsbug.png?raw=true "Stair collider bug 1")

2. When not placed at the edge of an elevated platform, the stairs remove one top edge collider next to them
![Stair collider bug 2](Screenshots/removeonecolliderbug.png?raw=true "Stair collider bug 2")
