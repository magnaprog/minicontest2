# Mini-Contest 2: Multi-Agent Adversarial Pacman

This repository contains the code for the Mini-Contest 2: Multi-Agent Adversarial Pacman, a project for CS 275: Artificial Life for Computer Graphics and Vision. The project involves developing multi-agent systems using Q-learning and evolutionary algorithms to create competitive Pacman agents.

## Project Overview

The goal of this project is to implement a dynamic and competitive multi-agent environment where agents utilize Q-learning and evolutionary strategies to optimize their gameplay. The agents will demonstrate advanced strategic thinking, adaptability, and competitive behaviors.

## Getting Started
1. Clone the repository.
2. To run the Python simulation with the various algorithms, use the following command:
    ```bash
    python capture.py <team1> <team2>
    ```
    You can find the team names from the team file names. For example, to pit greedy agents against approximate Q-learning agents for 100 games on random layout boards, use the following command:
    ```bash
    python capture.py -r baselineTeam.py -b myTeamApproxQLearningAgent.py -n 100 -l RANDOM
    ```
3. You can open the `CFT-Pacman` folder in Unity to experience the game alternatively, but the final simulation with all available algorithms is simply in Python.
