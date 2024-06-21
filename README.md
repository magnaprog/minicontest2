# Mini-Contest 2: Multi-Agent Adversarial Pacman

This repository contains the code for the Mini-Contest 2: Multi-Agent Adversarial Pacman, a project for CS 275: Artificial Life for Computer Graphics and Vision. The project involves developing multi-agent systems using Q-learning and evolutionary algorithms to create competitive Pacman agents.

## Project Overview

The goal of this project is to implement a dynamic and competitive multi-agent environment where agents utilize Q-learning and evolutionary strategies to optimize their gameplay. The agents will demonstrate advanced strategic thinking, adaptability, and competitive behaviors.

## Getting Started
1. Clone the repository and cd into the src directory. 
2. To run the Python simulation with the various algorithms, use the following command:
    ```bash
    python capture.py <team1> <team2>
    ```
    You can find the team names from the team file names. For example, to pit greedy agents against approximate Q-learning agents for 100 games on random layout boards, use the following command:
    ```bash
    python capture.py -r baselineTeam.py -b myTeamApproxQLearningAgent.py -n 100 -l RANDOM
    ```
3. You can open the `CFT-Pacman` folder in Unity to experience the game alternatively, but the final simulation with all available algorithms is simply in Python.

## Project Structure

- **src/**: Contains the main source code for running the simulations.
- **CFT-Pacman/**: Unity project folder for visualizing the game environment.
- **docs/**: Documentation of the project.

## Evaluation and Results

To test the efficacy of our developed agents, we conducted a series of games comparing our learning-based agents against baseline greedy agents. Our Approximate Q-Learning agents demonstrated superior performance with an 84% win rate, showcasing their ability to adapt and strategize effectively.

## Future Work

- **Hyperparameter Optimization**: Further experiments with PPO and Q-learning hyperparameters to enhance agent performance.
- **Advanced Learning Techniques**: Integration of advanced methods such as deep Q-networks (DQN) and hierarchical reinforcement learning.
- **Team-Based Decision Making**: Development of more sophisticated team strategies leveraging multi-agent reinforcement learning.
- **Real-Time Adaptation**: Implementation of real-time learning mechanisms for dynamic strategy adjustments.

## References

For a comprehensive list of references and related works, please refer to the `references.bib` file in the repository.

## Contributors

- Kevin Lee
- Yu-Hsin Weng
- Varun Kumar
- Siddarth Chalasani

## Acknowledgments

We would like to thank Professor Terzopoulos for his guidance and support throughout this project.
