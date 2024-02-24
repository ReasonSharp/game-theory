# Game Theory

A tournament simulator for a simple game from game theory:

1. The game is for two players and turn-based, and each player decides whether to "Cooperate" (C) or "Deflect" (D) each turn.
2. If both players choose C, they each get 3 points that round.
3. If one player chooses C and the other D, the one that chose D gets 5 points and the other zero.
4. If both players choose D, they each get 1 point.

## Running the simulator

    sudo docker build -t game-theory .
    sudo docker run game-theory
