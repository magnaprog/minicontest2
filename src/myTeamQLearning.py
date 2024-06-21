import os
import numpy as np
from captureAgents import CaptureAgent
from baselineTeam import ReflexCaptureAgent
import distanceCalculator
import random, time, util, sys, heapq
import pickle
from game import Directions, Actions
from capture import readCommand
from util import nearestPoint
from numpy import exp, log10, sqrt


def createTeam(firstIndex, secondIndex, isRed,
               first='QLearningAgent', second='QLearningAgent'):
    return [eval(first)(firstIndex), eval(second)(secondIndex)]

class QLearningAgent(CaptureAgent):
    def __init__(self, index):
        super().__init__(index)
        self.epsilon = 0.2
        self.alpha = 0.3
        self.discount = 0.8
        self.q_table = util.Counter()
        self.episode_states = []
        self.episode_actions = []

    def chooseAction(self, curState):
        legal_actions = curState.getLegalActions(self.index)
        if util.flipCoin(self.epsilon):
            action = random.choice(legal_actions)
        else:
            # print(self.q_table.items())
            q_values = [(self.q_table[(str(curState.data), action)], action) for action in legal_actions]
            max_q_value = max(q_values, key=lambda x: x[0])[0]
            best_actions = [action for q, action in q_values if q == max_q_value]
            action = random.choice(best_actions)

        self.episode_states.append(curState)
        self.episode_actions.append(action)
        return action

    def update(self, state, action, reward, nextState):
        q_value = self.q_table[(str(state.data), action)]
        # print("Q-value: ", q_value)
        next_legal_actions = nextState.getLegalActions(self.index)
        if next_legal_actions:
            next_q_values = [self.q_table[(str(nextState.data), next_action)] for next_action in next_legal_actions]
            max_next_q_value = max(next_q_values)
        else:
            max_next_q_value = 0.0

        self.q_table[(str(state.data), action)] = q_value + self.alpha * (reward + self.discount * max_next_q_value - q_value)

    def printQTable(self):
        for state_action, q_value in self.q_table.items():
            # print(state_action)
            state, action = state_action
            print(f"State: {state}, Action: {action}, Q-value: {q_value}")
    
    def saveQTable(self, filename):
        with open(filename, 'wb') as f:
            pickle.dump(self.q_table, f)
        print(f"Q-table saved to {filename}")

    def loadQTable(self, filename):
        with open(filename, 'rb') as f:
            self.q_table = pickle.load(f)
        print(f"Q-table loaded from {filename}")



def state_representation(state):
    """
    Convert the game state to a state representation tensor.
    """
    features = []

    # Add wall information
    walls = state.getWalls().data
    for x in range(len(walls)):
        for y in range(len(walls[0])):
            features.append(1 if walls[x][y] else 0)

    # Add food information
    red_food = state.getRedFood().data
    blue_food = state.getBlueFood().data
    for x in range(len(red_food)):
        for y in range(len(red_food[0])):
            features.append(1 if red_food[x][y] else 0)
            features.append(1 if blue_food[x][y] else 0)

    # Add capsule information
    red_capsules = state.getRedCapsules()
    blue_capsules = state.getBlueCapsules()
    capsule_positions = [(x, y) for x in range(len(walls)) for y in range(len(walls[0]))]
    for x, y in capsule_positions:
        features.append(1 if (x, y) in red_capsules else 0)
        features.append(1 if (x, y) in blue_capsules else 0)

    # Add agent positions
    num_agents = state.getNumAgents()
    for i in range(num_agents):
        pos = state.getAgentPosition(i)
        if pos is not None:
            features.append(pos[0])
            features.append(pos[1])
        else:
            features.append(-1)
            features.append(-1)

    # Add agent states (whether they are Pacman or Ghost)
    for i in range(num_agents):
        agent_state = state.getAgentState(i)
        features.append(1 if agent_state.isPacman else 0)

    # Add score
    features.append(state.getScore())

    return np.array(features)



def trainAgent(agent, num_episodes):
    from capture import runGames, CaptureRules

    for episode in range(num_episodes):
        print(f"\n----- EPISODE {episode + 1} -----")

        # Initialize the game with the specific agents
        options = readCommand(sys.argv[1:], red_agent=agent)
        games = runGames(**options)

        # Get the final state and reward
        final_state = games[0].state
        # rep = state_representation(final_state)
        # print(np.shape(rep))
        reward = -1 if final_state.getScore() < 0 else -0.25 if final_state.getScore() == 0 else 1
        print(f"Final score: {final_state.getScore()}")  # Print the final score
        print(f"Reward: {reward}")  # Print the reward

        for state, action in zip(agent.episode_states, agent.episode_actions):
            agent.update(state, action, reward, final_state)

        agent.episode_states = []
        agent.episode_actions = []

        # Print the Q-table after each iteration
        # print(f"Q-table after episode {episode + 1}:")
        # agent.printQTable()
        print(f"Length of Q-table: {len(agent.q_table)}")
        

    agent.saveQTable('q_table.pkl')

if __name__ == '__main__':
    # Initialize your agent
    agent = QLearningAgent(2)

    # if os.path.exists('q_table.pkl'):
    #     agent.loadQTable('q_table.pkl')
    #     agent.printQTable()

    # Train your agent
    trainAgent(agent, 200)  # Train for 1000 episodes

    # Save the Q-table to a file
    agent.saveQTable('q_table.pkl')
