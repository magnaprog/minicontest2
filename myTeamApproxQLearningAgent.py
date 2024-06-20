# myTeam.py
# ---------
# Licensing Information:  You are free to use or extend these projects for
# educational purposes provided that (1) you do not distribute or publish
# solutions, (2) you retain this notice, and (3) you provide clear
# attribution to UC Berkeley, including a link to http://ai.berkeley.edu.
#
# Attribution Information: The Pacman AI projects were developed at UC Berkeley.
# The core projects and autograders were primarily created by John DeNero
# (denero@cs.berkeley.edu) and Dan Klein (klein@cs.berkeley.edu).
# Student side autograding was added by Brad Miller, Nick Hay, and
# Pieter Abbeel (pabbeel@cs.berkeley.edu).


from captureAgents import CaptureAgent
import random, time, util
from game import Directions, Actions
import game
from util import nearestPoint

#################
# Team creation #
#################

NUM_TRAINING = 100
TRAINING = False

def createTeam(firstIndex, secondIndex, isRed,
               first = 'ApproxQLearningOffense', second = 'ApproxQLearningDefense', numTraining = 0, **args):
  """
  This function should return a list of two agents that will form the
  team, initialized using firstIndex and secondIndex as their agent
  index numbers.  isRed is True if the red team is being created, and
  will be False if the blue team is being created.
  As a potentially helpful development aid, this function can take
  additional string-valued keyword arguments ("first" and "second" are
  such arguments in the case of this function), which will come from
  the --redOpts and --blueOpts command-line arguments to capture.py.
  For the nightly contest, however, your team will be created without
  any extra arguments, so you should make sure that the default
  behavior is what you want for the nightly contest.
  """

  return [eval(first)(firstIndex), eval(second)(secondIndex)]


class ApproxQLearningOffense(CaptureAgent):

  def registerInitialState(self, gameState):
    self.epsilon = 0.1
    self.alpha = 0.2
    self.discount = 0.9

    self.loadWeights()
    # print(f"Offense weights: {self.weights}")

    self.start = gameState.getAgentPosition(self.index)
    self.featuresExtractor = FeaturesExtractor(self)
    CaptureAgent.registerInitialState(self, gameState)

  def chooseAction(self, gameState):
    """
        Picks among the actions with the highest Q(s,a).
    """
    legalActions = gameState.getLegalActions(self.index)
    if len(legalActions) == 0:
      return None

    foodLeft = len(self.getFood(gameState).asList())

    if foodLeft <= 2:
      bestDist = 9999
      for action in legalActions:
        successor = self.getSuccessor(gameState, action)
        pos2 = successor.getAgentPosition(self.index)
        dist = self.getMazeDistance(self.start, pos2)
        if dist < bestDist:
          bestAction = action
          bestDist = dist
      return bestAction

    action = None
    if TRAINING:
      for action in legalActions:
        self.updateWeights(gameState, action)

      if not util.flipCoin(self.epsilon):
        # exploit
        action = self.computeActionFromQValues(gameState)
      else:
        # explore
        action = random.choice(legalActions)
    else:
      action = self.computeActionFromQValues(gameState)
    return action

  def getWeights(self):
    return self.weights

  def getQValue(self, gameState, action):
    """
      Should return Q(state,action) = w * featureVector
      where * is the dotProduct operator
    """
    # features vector
    features = self.featuresExtractor.getFeatures(gameState, action)
    return features * self.weights

  def update(self, gameState, action, nextState, reward):
    """
       Should update your weights based on transition
    """
    features = self.featuresExtractor.getFeatures(gameState, action)
    oldValue = self.getQValue(gameState, action)
    futureQValue = self.getValue(nextState)
    difference = (reward + self.discount * futureQValue) - oldValue
    # for each feature
    for feature in features:
      newWeight = self.alpha * difference * features[feature]
      self.weights[feature] += newWeight

  def updateWeights(self, gameState, action):
    nextState = self.getSuccessor(gameState, action)
    reward = self.getReward(gameState, nextState)
    self.update(gameState, action, nextState, reward)

  def getReward(self, gameState, nextState):
    reward = 0
    agentPosition = gameState.getAgentPosition(self.index)

    # check if I have updated the score
    if self.getScore(nextState) > self.getScore(gameState):
      diff = self.getScore(nextState) - self.getScore(gameState)
      reward = diff * 10

    # check if food eaten in nextState
    myFoods = self.getFood(gameState).asList()
    distToFood = min([self.getMazeDistance(agentPosition, food) for food in myFoods])
    # I am 1 step away, will I be able to eat it?
    if distToFood == 1:
      nextFoods = self.getFood(nextState).asList()
      if len(myFoods) - len(nextFoods) == 1:
        reward = 10

    # check if I am eaten
    enemies = [gameState.getAgentState(i) for i in self.getOpponents(gameState)]
    ghosts = [a for a in enemies if not a.isPacman and a.getPosition() != None]
    if len(ghosts) > 0:
      minDistGhost = min([self.getMazeDistance(agentPosition, g.getPosition()) for g in ghosts])
      if minDistGhost == 1:
        nextPos = nextState.getAgentState(self.index).getPosition()
        if nextPos == self.start:
          # I die in the next state
          reward = -100

    return reward

  def storeWeights(self):
    with open('offense_weights.py', 'w') as f:
        f.write(f"weights = {self.weights}")

  def loadWeights(self):
    from offense_weights import weights
    self.weights = weights

  def final(self, state):
    "Called at the end of each game."
    # call the super-class final method
    self.storeWeights()
    CaptureAgent.final(self, state)

  def getSuccessor(self, gameState, action):
    """
    Finds the next successor which is a grid position (location tuple).
    """
    successor = gameState.generateSuccessor(self.index, action)
    pos = successor.getAgentState(self.index).getPosition()
    if pos != nearestPoint(pos):
      # Only half a grid position was covered
      return successor.generateSuccessor(self.index, action)
    else:
      return successor

  def computeValueFromQValues(self, gameState):
    """
      Returns max_action Q(state,action)
      where the max is over legal actions.  Note that if
      there are no legal actions, which is the case at the
      terminal state, you should return a value of 0.0.
    """
    allowedActions = gameState.getLegalActions(self.index)
    if len(allowedActions) == 0:
      return 0.0
    bestAction = self.computeActionFromQValues(gameState)
    return self.getQValue(gameState, bestAction)

  def computeActionFromQValues(self, gameState):
    """
      Compute the best action to take in a state.  Note that if there
      are no legal actions, which is the case at the terminal state,
      you should return None.
    """
    legalActions = gameState.getLegalActions(self.index)
    if len(legalActions) == 0:
      return None
    actionVals = {}
    bestQValue = float('-inf')
    for action in legalActions:
      targetQValue = self.getQValue(gameState, action)
      actionVals[action] = targetQValue
      if targetQValue > bestQValue:
        bestQValue = targetQValue
    bestActions = [k for k, v in actionVals.items() if v == bestQValue]
    # random tie-breaking
    return random.choice(bestActions)

  def getValue(self, gameState):
    return self.computeValueFromQValues(gameState)

class FeaturesExtractor:

  def __init__(self, agentInstance):
    self.agentInstance = agentInstance

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

    # if len(ghosts) > 0:
    #   minGhostDistance = min([self.agentInstance.getMazeDistance(agentPosition, g) for g in ghosts])
    #   if minGhostDistance < 3:
    #     features["minGhostDistance"] = minGhostDistance

    # successor = self.agentInstance.getSuccessor(gameState, action)
    # features['successorScore'] = self.agentInstance.getScore(successor)

    

    # capsules = self.agentInstance.getCapsules(gameState)
    if len(capsules) > 0:
      closestCap = min([self.agentInstance.getMazeDistance(agentPosition, cap) for cap in self.agentInstance.getCapsules(gameState)])
    #   # print(f"Closest capsule: {closestCap}")
      features["closest-capsule"] = closestCap
    # if there is a capsule at the next position then add the feature
      for capsule in capsules:
        if capsule == (next_x, next_y):
          features["eats-capsule"] = 1.0

    dist = self.closestFood((next_x, next_y), food, walls)
    if dist is not None:
      # make the distance a number less than one otherwise the update
      # will diverge wildly
      features["closest-food"] = float(dist) / (walls.width * walls.height)
    features.divideAll(10.0)
    return features

  def closestFood(self, pos, food, walls):
    """
    closestFood -- this is similar to the function that we have
    worked on in the search project; here its all in one place
    """
    fringe = [(pos[0], pos[1], 0)]
    expanded = set()
    while fringe:
      pos_x, pos_y, dist = fringe.pop(0)
      if (pos_x, pos_y) in expanded:
        continue
      expanded.add((pos_x, pos_y))
      # if we find a food at this location then exit
      if food[pos_x][pos_y]:
        return dist
      # otherwise spread out from the location to its neighbours
      nbrs = Actions.getLegalNeighbors((pos_x, pos_y), walls)
      for nbr_x, nbr_y in nbrs:
        fringe.append((nbr_x, nbr_y, dist + 1))
    # no food found
    return None


class ApproxQLearningDefense(CaptureAgent):

    def registerInitialState(self, gameState):
        self.epsilon = 0.1
        self.alpha = 0.2
        self.discount = 0.9

        self.loadWeights()
        # print(f"Defense weights: {self.weights}")
        self.start = gameState.getAgentPosition(self.index)
        self.featuresExtractor = DefensiveFeaturesExtractor(self)
        CaptureAgent.registerInitialState(self, gameState)

    def chooseAction(self, gameState):
        """
            Picks among the actions with the highest Q(s,a).
        """

        return self.computeActionFromQValues(gameState)

    def getQValue(self, gameState, action):
        """
            Should return Q(state,action) = w * featureVector
            where * is the dotProduct operator
        """
        # features vector
        features = self.featuresExtractor.getFeatures(gameState, action)
        return features * self.weights
    
    def storeWeights(self):
        with open('defense_weights.py', 'w') as f:
            f.write(f"weights = {self.weights}")

    def loadWeights(self):
        from defense_weights import weights
        self.weights = weights

    def final(self, state):
        "Called at the end of each game."
        # call the super-class final method
        self.storeWeights()
        CaptureAgent.final(self, state)

    def getSuccessor(self, gameState, action):
        """
        Finds the next successor which is a grid position (location tuple).
        """
        successor = gameState.generateSuccessor(self.index, action)
        pos = successor.getAgentState(self.index).getPosition()
        if pos != nearestPoint(pos):
            # Only half a grid position was covered
            return successor.generateSuccessor(self.index, action)
        else:
            return successor

    def computeValueFromQValues(self, gameState):
        """
            Returns max_action Q(state,action)
            where the max is over legal actions.  Note that if
            there are no legal actions, which is the case at the
            terminal state, you should return a value of 0.0.
        """
        allowedActions = gameState.getLegalActions(self.index)
        if len(allowedActions) == 0:
            return 0.0
        bestAction = self.computeActionFromQValues(gameState)
        return self.getQValue(gameState, bestAction)

    def computeActionFromQValues(self, gameState):
        """
            Compute the best action to take in a state.  Note that if there
            are no legal actions, which is the case at the terminal state,
            you should return None.
        """
        legalActions = gameState.getLegalActions(self.index)
        if len(legalActions) == 0:
            return None
        actionVals = {}
        bestQValue = float('-inf')
        for action in legalActions:
            targetQValue = self.getQValue(gameState, action)
            actionVals[action] = targetQValue
            if targetQValue > bestQValue:
                bestQValue = targetQValue
        bestActions = [k for k, v in actionVals.items() if v == bestQValue]
        # random tie-breaking
        return random.choice(bestActions)

class DefensiveFeaturesExtractor:

    def __init__(self, agentInstance):
        self.agentInstance = agentInstance

    def getFeatures(self, gameState, action):
      features = util.Counter()
      successor = self.agentInstance.getSuccessor(gameState, action)

      myState = successor.getAgentState(self.agentInstance.index)
      myPos = myState.getPosition()

      # Computes whether we're on defense (1) or offense (0)
      features['on-defense'] = 1
      if myState.isPacman: features['on-defense'] = 0

      # Computes distance to invaders we can see
      enemies = [successor.getAgentState(i) for i in self.agentInstance.getOpponents(successor)]
      invaders = [a for a in enemies if a.isPacman and a.getPosition() != None]
      features['num-invaders'] = len(invaders)
      if len(invaders) > 0:
        dists = [self.agentInstance.getMazeDistance(myPos, a.getPosition()) for a in invaders]
        features['invader-distance'] = min(dists)
        # check if scared timer is active
        # if myState.scaredTimer > 0:
        #   features['invader-distance'] = -features['invader-distance']

      if action == Directions.STOP: features['stopped'] = 1
      rev = Directions.REVERSE[gameState.getAgentState(self.agentInstance.index).configuration.direction]
      if action == rev: features['reversed'] = 1

      return features