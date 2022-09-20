# Aquarium Project Unity side

This project was developed for display a simulation paper fishs aquarium

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
