# Unity: Pacman - Capture The Flag Variation 
- Unity is free for student, you can apply for free activation code through its official website (need to setup a student account first).
- Ended up implementing in 2D because it's too hard to make 3D animation for each game object.
## Current Progress
### Game Stage
- Built with 4 2D Tilemaps
    - red walls
    - blue walls
    - red foods (including capsule)
    - blue foods (including capsule)
- current game stage design: `layout.JPG`
### Game Agent
- GameObject Hierarchy
```
- RedAgent
  - RedPacman
  - RedGhost (deactivated by default)
    - Body
    - Eyes
    - Frightened
    - FrightenedEnd

- BlueAgent
  - BluePacman
    - Death
  - BlueGhost (deactivated by default)
    - Body
    - Eyes
    - Frightened
    - FrightenedEnd
```
- switch agent state when entering/quitting opponent's side (simply implemented by checking `transform.position.x`)
- currently contolled by keyboard
    - red: AWSD
    - blue: Up/Down/Left/Right

### Pacman
- Eat food on opponent's side 
    - red(blue) pacman eat blue(red) food/capsule
- Deposit food/capsule when coming back to its side
    - simply add the score for its team
- Die when colliding w/ opponent's ghost (food/capsule eaten since last deposit will be sent back to opponent's side)
- Eat capsule to turn opponent's ghosts into frightened mode

### Ghost
- Become frightned when opponent's pacman eats a capsule
    - 2 stage of animations, flashing shortly before returning to normal mode
    - frightened ghost cannot eat opponent's pacman
    - frightened ghost will be eaten by opponent's pacman

### TODO
- UI: game start & game over scene, scoreboard, etc.
- game over detection
- multi-agents for both teams (currently only 1 agent/team)
    - shouldn't be too hard to do so, probably can be done by using array and looping through each agent
- Unity makes mistake in each agent's first states switching (bouncing back and forth between 2 states, and will move to position (0, 0, 0) while it should stay at where it switches to another state), but from the second time it becomes normal