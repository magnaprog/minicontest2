# User Guide: Mini-Contest 2 - Multi-Agent Adversarial Pacman

This user guide provides detailed instructions on how to set up, run, and modify the Mini-Contest 2: Multi-Agent Adversarial Pacman project.

## Table of Contents

1. [Introduction](#introduction)
2. [Prerequisites](#prerequisites)
3. [Installation](#installation)
4. [Running the Game](#running-the-game)
5. [Project Structure](#project-structure)
6. [Modifying the Agents](#modifying-the-agents)
7. [Q-Learning Agent Implementation](#q-learning-agent-implementation)
8. [Using Evolutionary Algorithms](#using-evolutionary-algorithms)
9. [Contributing](#contributing)
10. [License](#license)
11. [Acknowledgments](#acknowledgments)

## Introduction
This project is part of the CS 275: Artificial Life for Computer Graphics and Vision course. It involves developing a competitive Pacman game using multi-agent systems that utilize Q-learning and evolutionary algorithms to optimize their gameplay strategies.

## Prerequisites
- Python 3.x
- Pygame library

## Installation
1. **Clone the repository:**
   \`\`\`sh
   git clone https://github.com/your-username/minicontest2.git
   cd minicontest2
   \`\`\`
2. **Install the required libraries:**
   \`\`\`sh
   pip install pygame numpy
   \`\`\`

## Running the Game
To run the game, use the following command:
\`\`\`sh
python capture.py
\`\`\`
You can specify different teams and layouts using the command-line options:
\`\`\`sh
python capture.py -r baselineTeam -b baselineTeam -l testCapture
\`\`\`

## Project Structure
- \`capture.py\`: Main file to run the game.
- \`captureAgents.py\`: Specification and helper methods for capture agents.
- \`myTeam.py\`: Your custom agents implementing Q-learning and evolutionary strategies.
- \`layouts/\`: Directory containing various layout files for the game.
- Other supporting files for graphics, utilities, and game logic.

## Modifying the Agents

### Q-Learning Agent Implementation
The \`QLearningAgent\` class in \`myTeam.py\` implements a basic Q-learning agent. Here is a brief overview of the key components:

1. **Initialization:**
   - The Q-table is initialized in the \`registerInitialState\` method.
   - Learning rate (\`alpha\`), discount factor (\`gamma\`), and exploration rate (\`epsilon\`) are set.

2. **Action Selection:**
   - The \`chooseAction\` method selects actions using an epsilon-greedy policy.
   - Q-values are used to determine the best action.

3. **State Representation:**
   - The \`getState\` method extracts relevant state information from the game state, including the agent's position, food, walls, and ghost positions.

4. **Q-Table Update:**
   - The \`updateQTable\` method updates Q-values based on the reward received and the maximum Q-value of the next state.

5. **Reward Structure:**
   - The \`getReward\` method defines the reward structure, assigning rewards for actions such as eating food and penalties for being eaten.

### Using Evolutionary Algorithms
Evolutionary algorithms can be used to enhance the Q-learning agent by evolving the Q-learning parameters or policies. The key steps include:

1. **Fitness Function:**
   - Define a fitness function to evaluate agent performance based on game outcomes.

2. **Selection:**
   - Select the best-performing agents to propagate their traits.

3. **Crossover and Mutation:**
   - Implement crossover and mutation operations to generate new strategies.

## Contributing
Contributions are welcome! Here are some ways you can contribute:

1. **Report Bugs:**
   - If you find any bugs, please report them using the GitHub issues.

2. **Suggest Features:**
   - Suggest new features or enhancements by opening a GitHub issue.

3. **Submit Pull Requests:**
   - Fork the repository, make your changes, and submit a pull request.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments
- UC Berkeley for the original Pacman AI projects.
- CS 275: Artificial Life for Computer Graphics and Vision course materials." > USER_GUIDE.md

# Add, commit, and push the USER_GUIDE.md file
git add USER_GUIDE.md
git commit -m "Add user guide"
git push
