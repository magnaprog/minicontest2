def getFeatures(self, gameState, action):
    # extract the grid of food and wall locations and get the ghost locations
    food = self.agentInstance.getFood(gameState)
    walls = gameState.getWalls()
    capsules = self.agentInstance.getCapsules(gameState)
    enemies = [gameState.getAgentState(i) for i in self.agentInstance.getOpponents(gameState)]
    ghosts = [a.getPosition() for a in enemies if not a.isPacman and a.getPosition() != None]
    # ghosts = state.getGhostPositions()

    features = util.Counter()

    features["bias"] = 1.0

    # compute the location of pacman after he takes the action
    agentPosition = gameState.getAgentPosition(self.agentInstance.index)
    x, y = agentPosition
    dx, dy = Actions.directionToVector(action)
    next_x, next_y = int(x + dx), int(y + dy)

    # count the number of ghosts 1-step away
    features["#-of-ghosts-within-2-steps"] = sum(
            (next_x, next_y) in Actions.getLegalNeighbors(g, walls) or
            any((next_x, next_y) in Actions.getLegalNeighbors(nbr, walls) for nbr in Actions.getLegalNeighbors(g, walls))
            for g in ghosts
        )

    # if there is no danger of ghosts then add the food feature
    if not features["#-of-ghosts-within-2-steps"] and food[next_x][next_y]:
      features["eats-food"] = 1.0

    numCarrying = gameState.getAgentState(self.agentInstance.index).numCarrying
    if gameState.getAgentState(self.agentInstance.index).isPacman:
        # Add feature to incentivize returning home
        home_distance = self.agentInstance.getMazeDistance((next_x, next_y), self.agentInstance.start)
        features["home-distance"] = float(home_distance) / (walls.width * walls.height)

        # Scale the importance of returning home by the amount of food being carried
        home_distance_importance = numCarrying / 5.0  # Adjust this scaling factor as needed
        features["home-distance"] *= home_distance_importance
    else:
        features["home-distance"] = 0.0  # No incentive to return home when not a Pacman

    if len(capsules) > 0:
      # find distance to the closest capsule
      closestCap = min([self.agentInstance.getMazeDistance(agentPosition, cap) for cap in self.agentInstance.getCapsules(gameState)])
      features["closest-capsule"] = closestCap
      
      # if there is a capsule at the next position then add the feature
      for capsule in capsules:
        if capsule == (next_x, next_y):
          features["eats-capsule"] = 1.0

    # find the closest food
    dist = self.closestFood((next_x, next_y), food, walls)
    if dist is not None:
      # make the distance a number less than one otherwise the update
      # will diverge wildly
      features["closest-food"] = float(dist) / (walls.width * walls.height)
    
    # divide all values by 10.0 to scale them smaller
    features.divideAll(10.0)
    return features