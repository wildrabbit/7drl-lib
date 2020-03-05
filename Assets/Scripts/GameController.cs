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

    [SerializeField] float _gameOverDelay;

    public BaseGameEvents GameEvents => _gameEvents;

    protected BaseGameEvents _gameEvents;
    protected GameEventLog _eventLogger;

    protected IMapController _mapController;
    protected TimeController _timeController;
    protected IEntityController _entityController;
    protected HUD _hud;

    protected MonsterCreator _monsterCreator;

    public TimeController TimeController => _timeController;

    float _inputDelay;
    
    int _playStateID;
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
        _eventLogger = CreateGameLogger();

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
                Controller = this,
                Delay = _gameOverDelay,
                Elapsed = -1.0f
            }
        };
        InitExtendedPlayStates();
    }


    void ResetGame()
    {
        UnRegisterEntityEvents();

        _gameEvents = null;
        _cameraController.Cleanup();
        _monsterCreator.Cleanup();
        _mapController.Cleanup();

        _entityController.Cleanup();
        _timeController.Cleanup();

        _input.OnLayoutChanged += _hud.OnInputLayoutChanged;
        _hud.Cleanup();
        Destroy(_hud.gameObject);
    }

    IEnumerator DelayedPurge(float delay)
    {
        yield return new WaitForSeconds(delay);
        _entityController.PurgeEntities();
    }

    void StartGame()
    {
        _gameEvents = CreateGameEvents();

        // Init game control / time vars
        _timeController.Init(_entityController, _gameEvents, _gameData.DefaultTimescale);
        _timeController.Start();

        _inputDelay = _gameData.InputDelay;
        _input.Init(_gameData.InputData, _inputDelay);

        _mapController.Init(_gameData.MapData);

        _entityController.Init(_mapController, _gameData.EntityCreationData, _gameEvents);

        _monsterCreator.Init(_entityController, _mapController, _timeController, _gameEvents.Monsters, _gameData.EntityCreationData.MonsterData);

        _eventLogger.Init(_timeController, _gameEvents);

        _hud = CreateGameHUD();
        
        _input.OnLayoutChanged += _hud.OnInputLayoutChanged;

        
        // populate the level
        PopulateLevel();

        foreach(var stateContext in _playStatesData.Values)
        {
            stateContext.Init(this);
        };

        _playStateID = PlayStates.Action;
        _result = GameResult.Running;

        _hud.Init(_eventLogger, _entityController.Player, _timeController, _cameraController.UICamera);
        RegisterEntityEvents();
        _gameEvents.Flow.SendStarted();
    }

    void PopulateLevel()
    {
        _mapController.BuildMap();
        _entityController.StartGame();

        _cameraController.Init(_mapController.WorldBounds, _entityController.Player.transform, _gameEvents);

        _monsterCreator.RegisterSpawnPoints(FetchMonsterSpawnPoints());
        _monsterCreator.ProcessInitialSpawns();

        BuildTraps();
        BuildBlocks();

        ExtendedPopulate();

        _entityController.AddNewEntities();
        _entityController.RemovePendingEntities();
    }

    protected virtual void ExtendedPopulate()
    { }

    protected virtual void BuildTraps()
    {
        
    }

    protected virtual void BuildBlocks()
    {
        
    }

    protected virtual List<MonsterSpawnData> FetchMonsterSpawnPoints()
    {
        return new List<MonsterSpawnData>();
    }

    void RegisterEntityEvents()
    {
        _gameEvents.Player.Died += OnPlayerDied;
    }

    void UnRegisterEntityEvents()
    {
        _gameEvents.Player.Died -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        _result = GameResult.Lost;
        _playStateID = PlayStates.GameOver;
        ((GameOverStateContext)_playStatesData[_playStateID]).Elapsed = 0.0f;

        _gameEvents.Flow.SendGameOver(_result);
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
        _playStatesData[_playStateID].Refresh(this);
        int nextPlayState = _playStates[_playStateID].Update(_playStatesData[_playStateID], out var timeWillPass);
        _playStateID = nextPlayState;

        if (timeWillPass)
        {
            _timeController.Update(ref _playStateID);            
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
            Map = _mapController,
            Events = _gameEvents
        };
    }

    protected virtual HUD CreateGameHUD()
    {
        var hud = Instantiate<HUD>(_hudPrefab);
        return hud;
    }

    protected virtual GameEventLog CreateGameLogger()
    {
        var logger = new GameEventLog();
        return logger;
    }
}
