import numpy as np
import torch
from torch import nn
from torch.distributions import Categorical
from capture import readCommand, runGames
from captureAgents import CaptureAgent
import sys
from utils import convert_onnx
'''
def state_representation(state):
#     """
#     Convert the game state to a state representation tensor.
#     """
    features = []

    # Add agent position
    agent_pos = state.getAgentPosition(2)
    if agent_pos is not None:
        features.append(agent_pos[0])
        features.append(agent_pos[1])
        features.append(1 if state.getAgentState(2).isPacman else 0)
    else:
        features.append(-1)
        features.append(-1)
        features.append(-1)
    # Add other agent positions
    for i in range(4):
        if i != 2:
            pos = state.getAgentPosition(i)
            if pos is not None:
                features.append(pos[0])
                features.append(pos[1])
                features.append(1 if state.getAgentState(i).isPacman else 0)
            else:
                features.append(-1)
                features.append(-1)
                features.append(-1)



    # Add wall information
    walls = state.getWalls().data
    for x in range(len(walls)):
        for y in range(len(walls[0])):
            features.append(1 if walls[x][y] else 0)

    # Add food information
    red_food = state.getRedFood().data
#     # blue_food = state.getBlueFood().data
    for x in range(len(red_food)):
        for y in range(len(red_food[0])):
            features.append(1 if red_food[x][y] else 0)
            # features.append(1 if blue_food[x][y] else 0)

    # Add capsule information
    # red_capsules = state.getRedCapsules()
    # blue_capsules = state.getBlueCapsules()
    # capsule_positions = [(x, y) for x in range(len(walls)) for y in range(len(walls[0]))]
    # for x, y in capsule_positions:
    #     features.append(1 if (x, y) in red_capsules else 0)
        # features.append(1 if (x, y) in blue_capsules else 0)

    # Add agent positions
    # num_agents = state.getNumAgents()
    # for i in range(num_agents):
    #     pos = state.getAgentPosition(i)
    #     if pos is not None:
    #         features.append(pos[0])
    #         features.append(pos[1])
        # else:
        #     features.append(-1)
        #     features.append(-1)

    # Add agent states (whether they are Pacman or Ghost)
    # for i in range(num_agents):
    #     agent_state = state.getAgentState(i)
    #     features.append(1 if agent_state.isPacman else 0)

    # Add score
    features.append(state.getScore())

    return np.array(features)
'''

char_to_int = {
    '%': 1,
    ' ': 0,
    'X': 2,
    'x': 3,
    'T': 4,
    't': 5,
    'P': 6,
    'G': 7,
    '.': 8,
    'o': 9
}

def state_representation(state):
    """
    Convert the game state to a state representation tensor.
    """
    state_str = str(state)  # Get the string representation of the state
    features = [char_to_int[char] for char in state_str if char in char_to_int]
    return np.array(features)


class Buffer:
    def __init__(self):
        self.states = []
        self.actions = []
        self.log_probs = []
        self.rewards = []

class NeuralNetwork(nn.Module):
    def __init__(self, in_dim, out_dim):
        super(NeuralNetwork, self).__init__()
        self.fc1 = nn.Linear(in_dim, 256)
        self.fc2 = nn.Linear(256, 256)
        self.fc3 = nn.Linear(256, out_dim)

    def forward(self, x):
        x = torch.nn.functional.relu(self.fc1(x))
        x = torch.nn.functional.relu(self.fc2(x))
        return self.fc3(x)

class PPOAgent(CaptureAgent):
    ACTION_MAPPING = {
        "North": 0,
        "South": 1,
        "East": 2,
        "West": 3,
        "Stop": 4
    }

    def __init__(self, index):
        super().__init__(index)
        observation_dimension = 514  # Define the dimension based on your state representation
        action_dimension = 5  # Define the action dimension
        self.policy_network = NeuralNetwork(observation_dimension, action_dimension)
        self.value_network = NeuralNetwork(observation_dimension, 1)
        self.buffer = Buffer()
        self.policy_optimizer = torch.optim.Adam(self.policy_network.parameters(), lr=1e-4)
        self.value_optimizer = torch.optim.Adam(self.value_network.parameters(), lr=1e-4)
        self.loss_func = nn.MSELoss()
        self.step_count = 1
        self.prev_score = 0
        self.prev_reward = 0
        self.episode_step_count = 0
        self.visited_positions = set()
        self.prev_position = None
        self.prev_prev_position = None
        self.epsilon = 0.1
        self.was_pacman = False
        self.batch_size = 32
        self.buffer_size = 32
        self.empty_buffer = True
        self.prev_action = ""

    def registerInitialState(self, gameState):
        """
        This method handles the initial setup of the agent to populate useful fields.
        """
        super().registerInitialState(gameState)

    def chooseAction(self, gameState):
        """
        Choose the best action based on the current gameState.
        """
        state_tensor = torch.tensor(state_representation(gameState), dtype=torch.float)
        action_probs, log_prob = self.act(state_tensor)

        # Epsilon-greedy action selection -- explore with probability epsilon
        if np.random.rand() < self.epsilon:
            legal_actions = gameState.getLegalActions(self.index)
            selected_action = np.random.choice(legal_actions)
            selected_action_index = self.ACTION_MAPPING[selected_action]
        else:
            # Get legal actions from the game state
            legal_actions = gameState.getLegalActions(self.index)

            # Map the action_probs to legal actions
            legal_action_indices = [i for i, action in enumerate(["North", "South", "East", "West", "Stop"]) if action in legal_actions]
            # Choose the action with the highest probability
            legal_action_probs = action_probs[legal_action_indices]
            selected_action_index = legal_action_indices[np.argmax(legal_action_probs)]

            # Save the chosen action and log probability to the buffer
            selected_action = ["North", "South", "East", "West", "Stop"][selected_action_index]
            # print(f"Reward: {self.prev_reward}")
            # print(f"Selected action: {selected_action}")
        

        if not self.empty_buffer:
            self.step(gameState)
            # print(f"Reward: {self.prev_reward}")
        else:
            self.empty_buffer = False
            self.step_count -= 1
        

        if len(self.buffer.rewards) == self.buffer_size:
            print(f"Updating network after {self.step_count} steps.")
            self.update_network()
            self.step_count = 0
            self.episode_step_count = 0
            self.buffer.states = []
            self.buffer.actions = []
            self.buffer.log_probs = []
            self.buffer.rewards = []
            self.prev_score = 0
            self.prev_reward = 0
            self.buffer_size += 32
            if self.buffer_size > 2048:
                self.buffer_size = 2048


        self.buffer.states.append(state_representation(gameState))
        self.buffer.actions.append(selected_action_index)
        self.buffer.log_probs.append(log_prob)
        self.prev_action = selected_action
        # print(f"Selected action: {selected_action}")

        # if len(self.buffer.states) == 2048:
        #     self.step(gameState)
        
        self.step_count += 1

        # if self.step_count == self.buffer_size:
        #     print(f"Updating network after {self.step_count} steps.")
        #     self.update_network()
        #     self.step_count = 0
        #     self.episode_step_count = 0
        #     self.buffer.states = []
        #     self.buffer.actions = []
        #     self.buffer.log_probs = []
        #     self.buffer.rewards = []
        #     self.prev_score = 0
        #     self.prev_reward = 0
            
        self.episode_step_count += 1
        return selected_action

    def act(self, state):
        # Get the action logits from the policy network
        action_logits = self.policy_network(state)
        action_probs = torch.softmax(action_logits.detach(), dim=-1)
        dist = Categorical(action_probs)
        # Sample an action from the action probabilities
        action = dist.sample()
        # Calculate the log probability of the action
        log_prob = dist.log_prob(action)
        return action_probs, log_prob

    # TODO: Improve the reward calculation
    def calculate_reward(self, gameState, prev_score, agent_index):
        """
        Calculate reward based on the game state changes.
        """
        reward = 0
        current_score = gameState.getScore()
        # reward += current_score - prev_score

        agent_state = gameState.getAgentState(agent_index)

        # Reward for collecting food
        reward += 0.25 * agent_state.numCarrying + 0.5 * agent_state.numReturned # Small reward for carrying food

        if agent_state.isPacman:
            reward += 3 # Reward for being a Pacman
            self.was_pacman = True
        
        if agent_state.isPacman and agent_state.numCarrying == 0:
            reward -= 1  # Small penalty for not carrying food
    
        # if not agent_state.isPacman and agent_state.scaredTimer > 0:
        #     reward -= 2  # Penalty for being scared

        # Reward for ghost eating an opposing Pacman
        # if not agent_state.isPacman:  # If the agent is a ghost
        #     for i in range(gameState.getNumAgents()):
        #         if i == agent_index or i == 0:
        #             continue
        #         other_agent_state = gameState.getAgentState(i)
        #         if other_agent_state.isPacman and agent_state.getPosition() == other_agent_state.getPosition():
        #             reward += 5  # Reward for eating an opposing Pacman
        
        current_position = agent_state.getPosition()
        # TODO: Might not need this
        x, _ = current_position
        
        if x < 4:
            reward += x * 0.25
        elif x < 8:
            reward += 1
        elif x < 16:
            reward += 2

        

        if self.prev_action == "Stop":
            if x < 16:
                reward -= x * 0.5
            else:
                reward -= 8 + x * 0.25
        
        # if self.prev_prev_position is not None and self.prev_prev_position == current_position:
        #     reward -= 5
        # if self.prev_position is not None and self.prev_position == current_position:
        #     reward -= 1

        # if self.prev_position is not None:
        #     self.prev_prev_position = self.prev_position

        # self.prev_position = current_position

        if current_position not in self.visited_positions:
            reward += 2.5  # Reward for exploring new locations
            self.visited_positions.add(current_position)
        else:
            reward -= 2.5 # Penalty for revisiting the same location

        # reward -= 3

        return reward, current_score


    def step(self, gameState):
        self.prev_reward, self.prev_score = self.calculate_reward(gameState, self.prev_score, self.index)
        self.buffer.rewards.append(self.prev_reward)
        # print(f"Reward: {self.prev_reward}")

        # if self.step_count > 1200:
        #     print(f"Updating network after {self.step_count} steps.")
        #     self.step_count = 0
        #     self.update_network()

    def set_training_mode(self):
        """
        set training mode
        """
        self.policy_network.train()
        self.value_network.train()


    def update_network(self):
        indices = np.random.permutation(len(self.buffer.states))
        for start in range(0, len(indices), self.batch_size):
            end = start + self.batch_size
            batch_indices = indices[start:end]

            states = torch.tensor(np.array([self.buffer.states[i] for i in batch_indices]), dtype=torch.float)
            actions = torch.tensor(np.array([self.buffer.actions[i] for i in batch_indices]), dtype=torch.long)
            log_probs = torch.tensor([self.buffer.log_probs[i] for i in batch_indices], dtype=torch.float)
            rewards = torch.tensor([self.buffer.rewards[i] for i in batch_indices], dtype=torch.float)
            
        # # only take 64 entries from the buffer for stability
        # states = torch.tensor(np.array(self.buffer.states[-64:]), dtype=torch.float)
        # actions = torch.tensor(np.array(self.buffer.actions[-64:]), dtype=torch.long)  # Use long for indices
        # log_probs = torch.tensor(self.buffer.log_probs[-64:], dtype=torch.float)

            rewards_to_go = []
            discounted_reward = 0
            for reward in reversed(rewards):
                discounted_reward = reward + discounted_reward * 0.99
                rewards_to_go.insert(0, discounted_reward)
            rewards_to_go = torch.tensor(rewards_to_go, dtype=torch.float)

            V = self.value_network(states).squeeze()
            advantages = rewards_to_go - V.detach()
            advantages = (advantages - advantages.mean()) / (advantages.std() + 1e-10)

            for _ in range(3):
                V = self.value_network(states).squeeze()

                pred_action_logits = self.policy_network(states)
                # Calculate the log probabilities of the actions
                action_probs = torch.softmax(pred_action_logits, dim=-1)
                dist = Categorical(action_probs)
                cur_log_prob = dist.log_prob(actions)

                # Calculate the entropy bonus and ratios
                entropy = dist.entropy().mean()
                ratios = torch.exp(cur_log_prob - log_probs)

                # Calculate the surrogate losses
                surr1 = ratios * advantages
                surr2 = torch.clamp(ratios, 1 - 0.2, 1 + 0.2) * advantages

                # Calculate the actor and critic losses
                actor_loss = -torch.min(surr1, surr2).mean()
                critic_loss = self.loss_func(V, rewards_to_go)

                # Update the policy network
                self.policy_optimizer.zero_grad()
                actor_loss.backward(retain_graph=True)
                self.policy_optimizer.step()

                # Update the value network
                self.value_optimizer.zero_grad()
                critic_loss.backward()
                self.value_optimizer.step()

        self.buffer = Buffer()


    def save_model(self, policy_path, value_path):
        torch.save(self.policy_network.state_dict(), policy_path)
        torch.save(self.value_network.state_dict(), value_path)

def train_ppo_agent(num_episodes, update_interval, agent_index=2):
    agent = PPOAgent(agent_index)
    # agent.set_training_mode()
    for episode in range(num_episodes):
        print(f"----- EPISODE {episode + 1}/{num_episodes} -----")

        # Initialize the game
        options = readCommand(sys.argv[1:], red_agent=agent)
        games = runGames(**options)
        # Remove the last element from the buffer.state, buffer.actions, buffer.log_probs lists
        agent.buffer.states = agent.buffer.states[:-1]
        agent.buffer.actions = agent.buffer.actions[:-1]
        agent.buffer.log_probs = agent.buffer.log_probs[:-1]
        if len(agent.buffer.rewards) != len(agent.buffer.states):
            print("Rewards and states do not match!!!")
        agent.empty_buffer = True
        final_state = games[0].state
        # print(str(final_state))
        # print(state_representation(final_state))

        # Get the final score and if it is a win or loss. If it is a win, reward the previous agent.episode_step_count steps. If it is a loss, penalize the previous agent.episode_step_count steps. If it is a tie, penalize the previous agent.episode_step_count steps a little bit. The rewards from the previous steps are stored in the agent.buffer.rewards list.
        final_score = final_state.getScore()
        # for i in range(len(agent.buffer.rewards) - agent.episode_step_count, len(agent.buffer.rewards)):
        #     if final_score > 0:
        #         agent.buffer.rewards[i] += 25
        #     if agent.was_pacman:
        #         agent.buffer.rewards[i] += 10
            # elif final_score <= 0:
            #     agent.buffer.rewards[i] -= 5
                
        # print(agent.buffer.rewards)

        agent.was_pacman = False
        agent.episode_step_count = 0
        agent.visited_positions = set()

        # if (episode + 1) % update_interval == 0:
            # print(f"Updating network after {agent.step_count} steps.")
            # agent.step_count = 0
            # agent.update_network()
            # agent.buffer.states = []
            # agent.buffer.actions = []
            # agent.buffer.log_probs = []
            # agent.buffer.rewards = []

        # if (episode + 1) == 15:
        #     update_interval = 5
        # Save the model weights after each episode (optional)
        agent.save_model('policy_network.pth', 'value_network.pth')
    convert_onnx(agent, 514)


if __name__ == "__main__":
    # Number of episodes to train the agent
    num_episodes = 250
    # Update the network after this many steps
    update_interval = 5

    train_ppo_agent(num_episodes, update_interval)
