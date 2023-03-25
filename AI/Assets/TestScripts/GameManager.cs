using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameState state;
    public static event Action<GameState> OnStateChange;
    [SerializeField] private PlantManager plantManager;
    [SerializeField] private MouseController mouseController;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateGameState(GameState.SetUp);
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.SetUp:
                HandleSetUp();
                break;
            case GameState.MouseControl:
                HandleMouseControl();
                break;
            case GameState.PlantSeeds:
                HandlePlantSeeds();
                break;
            case GameState.HarvestSeeds:
                HandleHarvestSeeds();
                break;
        }

        OnStateChange?.Invoke(newState);

    }

    private void HandleSetUp()
    {
        Debug.Log("GM: Setting Up");
    }

    private void HandleMouseControl()
    {
        Debug.Log("GM: Mouse Control");
    }

    private void HandlePlantSeeds()
    {
        Debug.Log("GM: Planting Seeds");
        mouseController.gameObject.SetActive(false);
        plantManager.gameObject.SetActive(true);

    }

    private void HandleHarvestSeeds()
    {
        Debug.Log("GM: Harvesting Seeds");
    }

    public enum GameState
{
    SetUp,
    MouseControl,
    PlantSeeds,
    HarvestSeeds
}
}