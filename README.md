# Aquarium Project Unity side

This project was developed for display a simulation paper fishs aquarium

## Scenario Sequence
1. Access to StreamingAssets folder
2. Open config.json and edit what you want to initially 
3. Open the program and waiting for python TCP connect to step up add new fish in aquarium
4. You can add a new fish Manually by Spawner Key code ( 1 - 7 : Tuna, Shark, Parrotfish, Dolphin, Dugong, Whale, Turtle )

## Feature
- Each fish unit has own setting and move style
- Implement 2D fish move likes 3D by Shader graph
- Fish reach to the destination and find a new one after reached
- Destination cover by zone box of Camera field of view
- The new destination has prevent to change fish direction move toward to camera or move in depth side
- Spawner for each fish can manually add fish in aquarium or auto add by TCP Socket
- This aquarium can add more type of fish by incress a fish spawner and prefab
- Each spawner controls own fish setting and the maximum number of own fish in aquarium
- Can config for each spawner by config.json at StreamingAssets folder
- Connect backend python side by TCP Socket to send infomation to spawn new fish automatically

## Spawner Setting
- Unit prefab of fish
- Bound of spawn depth ( calculate spawn point by camera field of view)
- Spawn point offset
- Set Key Code to spawn unit manually
- The maximum number of unit
- Minimum duration is unit stay alive in aquarium
- Speed ratio of unit
- Default texture of fish
- Type of fish unit
- Number of unit that spawn initially
- Bound of box spawner with Field of view

## Spawn Unit
1. Units will be spawned after open the program 
2. The number of unit up to spawner initial unit
3. Add a new unit manually or automatically by user or TCP Socket
4. if number of unit reach to maximum, the oldest unit will be remove from aquarium a new one by using queue
5. if unit not has time duration left, the unit will move out off aquarium

## Display Ratio
This aquarium for display ratio 7:2 or 1920 * 824

<img width="970" alt="Screen Shot 2565-09-20 at 14 28 17" src="https://user-images.githubusercontent.com/108858548/191195051-770888c3-1da2-4d6d-84de-f50e1e87fc0d.png">
