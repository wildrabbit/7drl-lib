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

    [SerializeField] GameData _gameData;
    [SerializeField] CameraController _cameraController;
    
    [SerializeField] HUD _hudPrefab;

    public BaseGameEvents GameEvents;

    protected IMapController _mapController;
    protected TimeController _timeController;
    protected IEntityController _entityController;

    public TimeController TimeController => _timeController;

    float _inputDelay;
    
    int _playContext;
    bool _loading;
    public bool Loading => _loading;
    
    // :thinking: Does the behaviour/data make sense for this? Should context also include the data?
    Dictionary<int, IPlayState> _playStates;
    Dictionary<int, PlayStateContext> _playContextData;

    BaseInputController _input;
    GameResult _result;
    
    protected virtual void Awake()
    {
        _input = CreateInputController();
        _timeController = new TimeController();
        _entityController = CreateEntityController();
        _mapController = CreateMapController();

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
        _playStates = new Dictionary<int, IPlayState>();
        _playStates[PlayStates.Action] = CreatePlayerActionState();
        _playStates[PlayStates.GameOver] = new GameOverState();
        InitExtendedPlayStates();
    }

    protected virtual void InitExtendedPlayStates()
    {}

    protected virtual void ExtendedInit()
    {

    }

    private void InitPlayStateData()
    {
        _playContextData = new Dictionary<int, PlayStateContext>();

        _playContextData[PlayStates.Action] = CreatePlayerActionStateContext();
        _playContextData[PlayStates.GameOver] = new GameOverStateContext
        {
            Input = _input,
            Controller = this
        };
        InitExtendedPlayStates();
    }


    void ResetGame()
    {
        UnRegisterEntityEvents();

        GameEvents = null;
        _cameraController.Cleanup();
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

        _inputDelay = _gameData.InputDelay;
        _input.Init(_gameData.InputData, _inputDelay);

        _mapController.Init(_gameData.MapData);

        _entityController.Init(_mapController, _gameData.EntityCreationData, GameEvents);        

        _cameraController.Init(_mapController.WorldBounds, _entityController.Player.transform, GameEvents);


        // populate the level
        PopulateLevel();

        // Init game control / time vars
        _timeController.Init(_entityController, GameEvents.Time, _gameData.DefaultTimescale);
        _timeController.Start();

        _playContext = PlayStates.Action;
        _playContextData[_playContext].Refresh(this);

        _result = GameResult.Running;

        GameEvents.Time.NewTurn += (val) => Debug.Log($"New turn: {_timeController.Turns}. Time elapsed: {_timeController.TimeUnits}");
        
        RegisterEntityEvents();
    }

    protected virtual void PopulateLevel()
    {
        // Game specific map logic?
    }

    void RegisterEntityEvents()
    {

    }

    void UnRegisterEntityEvents()
    {
 
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
        _playContextData[_playContext].Refresh(this);
        int nextPlayState = _playStates[_playContext].Update(_playContextData[_playContext], out var timeWillPass);
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
