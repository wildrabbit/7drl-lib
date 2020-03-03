using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameResult
{
    None,
    Running,
    Won,
    Lost
}

public abstract class GameController : MonoBehaviour
{
    public class PlayStates
    {
        public const int Action = 0;
        public const int GameOver = 1;
        public const int Last = PlayStates.GameOver;
    }

    [SerializeField] protected GameData _gameData;
    [SerializeField] protected CameraController _cameraController;
    
    [SerializeField] protected HUD _hudPrefab;

    protected BaseGameEvents GameEvents;

    protected IMapController _mapController;
    protected TimeController _timeController;
    protected IEntityController _entityController;

    protected MonsterCreator _monsterCreator;

    public TimeController TimeController => _timeController;

    float _inputDelay;
    
    int _playContext;
    bool _loading;
    public bool Loading => _loading;
    
    // :thinking: Does the behaviour/data make sense for this? Should context also include the data?
    Dictionary<int, IPlayState> _playStates;
    Dictionary<int, PlayStateContext> _playStatesData;

    BaseInputController _input;
    GameResult _result;
    
    protected virtual void Awake()
    {
        _input = CreateInputController();
        _timeController = new TimeController();
        _entityController = CreateEntityController();
        _mapController = CreateMapController();
        _monsterCreator = new MonsterCreator();

        _result = GameResult.None;
        _loading = false;

        InitPlayStates();
        InitPlayStateData();

        ExtendedInit();
    }

    void Start()
    {
        StartGame();
    }

    protected virtual BaseInputController CreateInputController()
    {
        return new BaseInputController();
    }

    protected abstract IMapController CreateMapController();
    protected abstract BaseGameEvents CreateGameEvents();

    protected virtual IEntityController CreateEntityController()
    {
        return new EntityController();
    }

    private void InitPlayStates()
    {
        _playStates = new Dictionary<int, IPlayState>
        {
            [PlayStates.Action] = CreatePlayerActionState(),
            [PlayStates.GameOver] = new GameOverState()
        };
        InitExtendedPlayStates();
    }

    protected virtual void InitExtendedPlayStates()
    {}

    protected virtual void ExtendedInit()
    {

    }

    private void InitPlayStateData()
    {
        _playStatesData = new Dictionary<int, PlayStateContext>
        {
            [PlayStates.Action] = CreatePlayerActionStateContext(),
            [PlayStates.GameOver] = new GameOverStateContext
            {
                Input = _input,
                Controller = this
            }
        };
        InitExtendedPlayStates();
    }


    void ResetGame()
    {
        UnRegisterEntityEvents();

        GameEvents = null;
        _cameraController.Cleanup();
        _monsterCreator.Cleanup();
        _mapController.Cleanup();

        _entityController.Cleanup();
        _timeController.Cleanup();

    }

    IEnumerator DelayedPurge(float delay)
    {
        yield return new WaitForSeconds(delay);
        _entityController.PurgeEntities();
    }

    void StartGame()
    {
        GameEvents = CreateGameEvents();

        // Init game control / time vars
        _timeController.Init(_entityController, GameEvents, _gameData.DefaultTimescale);
        _timeController.Start();

        _inputDelay = _gameData.InputDelay;
        _input.Init(_gameData.InputData, _inputDelay);

        _mapController.Init(_gameData.MapData);

        _entityController.Init(_mapController, _gameData.EntityCreationData, GameEvents);

        _monsterCreator.Init(_entityController, _mapController, _timeController, GameEvents.Monsters, _gameData.EntityCreationData.MonsterData);

        
        // populate the level
        PopulateLevel();


        _playContext = PlayStates.Action;
        _playStatesData[_playContext].Refresh(this);

        _result = GameResult.Running;

        GameEvents.Time.NewTurn += (val) => Debug.Log($"New turn: {_timeController.Turns}. Time elapsed: {_timeController.TimeUnits}");
        
        RegisterEntityEvents();
        GameEvents.Flow.SendStarted();
    }

    void PopulateLevel()
    {
        _mapController.BuildMap();
        _entityController.StartGame();

        _cameraController.Init(_mapController.WorldBounds, _entityController.Player.transform, GameEvents);

        _monsterCreator.RegisterSpawnPoints(FetchMonsterSpawnPoints());
        _monsterCreator.ProcessInitialSpawns();

        ExtendedPopulate();

        _entityController.AddNewEntities();
        _entityController.RemovePendingEntities();
    }

    protected virtual void ExtendedPopulate()
    { }

    protected virtual List<MonsterSpawnData> FetchMonsterSpawnPoints()
    {
        return new List<MonsterSpawnData>();
    }

    void RegisterEntityEvents()
    {
        GameEvents.Player.Died += OnPlayerDied;
    }

    void UnRegisterEntityEvents()
    {
        GameEvents.Player.Died -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        _result = GameResult.Lost;
        _playContext = PlayStates.GameOver;

        GameEvents.Flow.SendGameOver(_result);
    }

    public void Restart()
    {
        StartCoroutine(RestartGame());
    }
    
    // Update is called once per frame
    void Update()
    {
        if(_loading)
        {
            return;
        }

        _input.Read();
        _playStatesData[_playContext].Refresh(this);
        int nextPlayState = _playStates[_playContext].Update(_playStatesData[_playContext], out var timeWillPass);
        _playContext = nextPlayState;

        if (timeWillPass)
        {
            _timeController.Update(ref _playContext);            
        }

        _entityController.RemovePendingEntities();
        _entityController.AddNewEntities();

        if(_result == GameResult.Running)
        {
            _result = EvaluateVictory();
        }
    }

    protected virtual GameResult EvaluateVictory()
    {
        return GameResult.Running;
    }

    private IEnumerator RestartGame()
    {
        _loading = true;
        ResetGame();
        yield return new WaitForSeconds(0.1f);
        StartGame(); // TODO: Change level or smthing.
        _loading = false;
    }

    protected virtual PlayerActionState CreatePlayerActionState()
    {
        return new PlayerActionState();
    }

    protected virtual PlayerActionStateContext CreatePlayerActionStateContext()
    {
        return new PlayerActionStateContext
        {
            Input = _input,
            BumpingWallsWillSpendTurn = _gameData.BumpingWallsWillSpendTurn,
            EntityController = _entityController,
            Map = _mapController
        };
    }
}
