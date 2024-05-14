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
from game import Directions
import game
import numpy as np

#################
# Team creation #
#################

def createTeam(firstIndex, secondIndex, isRed,
               first='QLearningAgent', second='QLearningAgent'):
    """
    This function should return a list of two agents that will form the
    team, initialized using firstIndex and secondIndex as their agent
    index numbers.  isRed is True if the red team is being created, and
    will be False if the blue team is being created.
    """
    return [eval(first)(firstIndex), eval(second)(secondIndex)]

##########
# Agents #
##########

class QLearningAgent(CaptureAgent):
    """
    A Q-learning agent for the Pac-Man game.
    """

    def registerInitialState(self, gameState):
        """
        This method handles the initial setup of the
        agent to populate useful fields (such as what team
        we're on).

        A distanceCalculator instance caches the maze distances
        between each pair of positions, so your agents can use:
        self.distancer.getDistance(p1, p2)

        IMPORTANT: This method may run for at most 15 seconds.
        """
        CaptureAgent.registerInitialState(self, gameState)
        
        # Initialize Q-table
        self.q_table = {}
        self.alpha = 0.1  # Learning rate
        self.gamma = 0.9  # Discount factor
        self.epsilon = 0.1  # Exploration rate
        
        self.prev_state = None
        self.prev_action = None

    def chooseAction(self, gameState):
        """
        Picks an action based on Q-learning policy.
        """
        state = self.getState(gameState)
        actions = gameState.getLegalActions(self.index)
        
        # Epsilon-greedy action selection
        if np.random.uniform(0, 1) < self.epsilon:
            action = random.choice(actions)
        else:
            action = self.getBestAction(state, actions)
        
        # Update Q-table
        if self.prev_state is not None and self.prev_action is not None:
            self.updateQTable(self.prev_state, self.prev_action, gameState)
        
        # Store state and action
        self.prev_state = state
        self.prev_action = action
        
        return action

    def getState(self, gameState):
        """
        Extracts the relevant state information from the game state.
        """
        position = gameState.getAgentState(self.index).getPosition()
        food = self.getFood(gameState)
        walls = gameState.getWalls()
        ghosts = self.getOpponents(gameState)
        return (position, food, walls, ghosts)

    def getBestAction(self, state, actions):
        """
        Returns the best action for the given state based on Q-values.
        """
        q_values = [self.getQValue(state, action) for action in actions]
        max_q = max(q_values)
        best_actions = [actions[i] for i in range(len(actions)) if q_values[i] == max_q]
        return random.choice(best_actions)

    def getQValue(self, state, action):
        """
        Returns the Q-value for the given state-action pair.
        """
        return self.q_table.get((state, action), 0.0)

    def updateQTable(self, prev_state, prev_action, gameState):
        """
        Updates the Q-value for the previous state-action pair.
        """
        reward = self.getReward(gameState)
        state = self.getState(gameState)
        actions = gameState.getLegalActions(self.index)
        max_q = max([self.getQValue(state, action) for action in actions])
        
        current_q = self.q_table.get((prev_state, prev_action), 0.0)
        self.q_table[(prev_state, prev_action)] = current_q + self.alpha * (reward + self.gamma * max_q - current_q)

    def getReward(self, gameState):
        """
        Defines the reward structure.
        """
        reward = 0
        my_state = gameState.getAgentState(self.index)
        if my_state.isPacman:
            reward += 10  # Reward for being a Pacman
            if my_state.getPosition() in self.getFood(gameState).asList():
                reward += 50  # Reward for eating food
            if my_state.getPosition() in self.getCapsules(gameState):
                reward += 100  # Reward for eating power pellets
        if my_state.scaredTimer > 0:
            reward -= 5  # Penalty for being scared
        if my_state.getPosition() == my_state.start:
            reward -= 10  # Penalty for being eaten
        return reward

