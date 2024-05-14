# Q-Learning Implementation Plan for Mini-Contest 2

This document outlines the plan for implementing a Q-learning agent in the `myTeam.py` file for the Mini-Contest 2: Multi-Agent Adversarial Pacman project.

## Objectives

1. Implement a Q-learning agent that can learn optimal strategies for the Pacman game.
2. Define a robust state representation and reward structure to facilitate effective learning.
3. Integrate the Q-learning agent into the existing game framework.

## Plan

### 1. State Representation

The state should include relevant information that impacts the agent's decision-making process. We will represent the state as a combination of:
- Agent's position
- Food locations
- Wall locations
- Opponent positions
- Power pellet locations

### 2. Reward Structure

The reward structure will guide the agent towards desirable actions. The rewards and penalties are defined as:
- Positive reward for eating food
- Positive reward for returning food to the home side
- Positive reward for eating power pellets
- Penalty for being eaten by ghosts
- Penalty for staying idle

### 3. Q-Table Initialization

Initialize the Q-table to store Q-values for state-action pairs. The Q-table will be updated based on the agent's experience.

### 4. Q-Learning Algorithm

Implement the Q-learning update rule:
- Choose an action using an epsilon-greedy policy.
- Update the Q-value based on the received reward and the maximum Q-value of the next state.

### 5. Implementation Steps

1. **Initialize Q-table and Parameters:**
   - Learning rate (`alpha`)
   - Discount factor (`gamma`)
   - Exploration rate (`epsilon`)

2. **Action Selection:**
   - Implement epsilon-greedy action selection in the `chooseAction` method.

3. **State Extraction:**
   - Define the `getState` method to extract relevant state information from the game state.

4. **Q-Table Update:**
   - Implement the `updateQTable` method to update Q-values based on the agent's experience.

5. **Reward Calculation:**
   - Define the `getReward` method to calculate rewards based on game events.

### 6. Code Implementation

Here is the proposed implementation for `myTeam.py`:

```python
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
