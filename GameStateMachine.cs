using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team3.Events;

public class GameStateMachine 
{
    public static GameState DefaultState = new DefaultState(),
                            LoadState = new LoadState(),
                            MainMenuState = new MainMenuState(),
                            RunningState = new RunningState(),
                            ExitingState = new ExitingState(),
                            PauseState = new PauseState();

    private static GameStateMachine instance = null;

    public static GameStateMachine Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameStateMachine();
            }
            return instance;
        }
    }

    private GameState currentState;

    public GameState CurrentState
    {
        get
        {
            return currentState;
        }
    }

    private GameStateMachine()
    {
        currentState = DefaultState;
        currentState.stateMachine = this;
    }

    public void SwitchState(GameState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.stateMachine = this;
        newState.Enter();
    }
}

public class DefaultState : GameState
{

}
