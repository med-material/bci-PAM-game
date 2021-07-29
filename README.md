# BCI PAM fishing game
![PAM fishing game screenshot](https://github.com/med-material/bci-PAM-game/blob/main/PAM-fishing-game.png)

## About
This game was made as a master project on the Medialogy MSc in Spring ’20. The game is intended for motivating rehabilitation of stroke patients through a BCI game with various performance-accommodating mechanisms (PAMs).

## Contributors
The game was created at Aalborg University by Ingeborg Goll Rossau, Rasmus Bugge Skammelsen and Jedrzej Jacek Czapla with additional scripts for managing input and logging created by Bastian Ilsø Hougaard. All sound effects are licensed under Creative Commons.

## Game
In the game, a fisherman attempts to catch fish. The player controls lowering/raising the hook via the up/down arrow keys to get a bite. To reel in a fish, the player must perform motor imagery within the input window (signified by the white/green progress bar). If successful, the fish will be reeled in by one lane, if not it will swim away. Three failures will result in the fish escaping completely.
To mitigate the frustration of BCI’s low accuracy, the game has three different mechanisms which help the user:
*	Augmented success: on a successful trial, the fisherman becomes stronger and reels the fish in by two lanes.
*	Mitigated failure: on a failed trial, the fisherman applies a clamp to the reel, preventing the fish from swimming away.
*	Override input: on a failed trial, a runner will appear and reel the fish in by one lane.

## Input
The project has three options for input: blinking, key sequence and BCI. To switch between them, activate the appropriate script on the InputManager object in the editor. Input will only be registered when the line is in the green part of progress bar.
*	BCI: the player player reels the fish in by performing motor imagery. Requires BCI hardware or a simulated signal.
*	Blinking: the player reels the fish in by blinking. Input is registered when the player closes their eyes within the input window. Requires a Tobii eye tracker.
*	KeySequence: the player reels the fish in by by typing in a keysequence in the correct order within a time limit. The time limit can be adjusted (Sequence Time Limit), as can the sequence (Keyboard Sequence). Useful for internal testing.

## Game settings
To set the game up for experiments, go to the GameManager in the Unity editor. Here you can adjust:
*	Experiment settings
    *	Condition: The type of PAM to apply. The ‘Control’ condition has no PAM.
    *	Trials: How many trials should be in the experiment.
    *	Positive rate: How high a percentage of the trials should result in a successful output
    *	PAM rate: How high a percentage of the trials should result in a PAM being employed. Note that if the condition is augmented success, PAM trials will replace successful trials, while for the other two PAMs they will replace unsuccessful trials.  
    *	(NOTE: For both successful and PAM trials, the number will always be rounded down (if applicable), e.g. if Trials = 15 with Positive rate = 0.5, the number of successful trials will be 7.)
*	InputWindow settings
    *	Inter-trial interval seconds: How long the time between input windows should be, i.e. the white part of the progress bar.
    *	Input window seconds: How long the player has to perform an input, i.e. the green part of the progress bar.

## Logging
When the game ends (or OnApplicationQuit), the game saves two logs in the /documents folder (unless otherwise specified): Game and Meta. The Meta log saves information about the settings that can be adjusted via the GameManager. The Game log saves information about events throughout the game:
*	Input window status (closed/open)
*	Inter-trial interval length
*	Input window length
*	Game state (running/stopped)
*	No. of trials left + rate of acc/rej/PAM
*	Trial result
*	Trial goal
*	Fish events (caught/escaped)
*	Arrowkey input
For more information about the LoggingManager, see: https://github.com/med-material/LoggingManager.
