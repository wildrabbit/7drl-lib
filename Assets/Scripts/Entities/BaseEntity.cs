using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseEntityDependencies
{
    public Transform ParentNode;
    public IEntityController EntityController;
    public IMapController MapController;
    public Vector2Int Coords;
    public BaseGameEvents GameEvents;
}

public class TileBasedEffect
{
    public int Elapsed;
    public int Duration;
}

public delegate void EntityMovedDelegate(Vector2Int nextCoords, Vector2 worldPos, BaseEntity entity);

public abstract class BaseEntity : MonoBehaviour, IScheduledEntity
{
    public string Name => _entityData.DisplayName;
    public virtual Vector2Int Coords
    {
        get => _coords;
        set
        {
            _coords = value;
            _mapController.ConstrainCoords(ref _coords);
            Vector2 targetPos = _mapController.WorldFromCoords(_coords);
            transform.position = targetPos;
            OnEntityMoved?.Invoke(_coords, targetPos, this);
        }
    }

    public bool Active
    {
        get => _active;
        set => _active = value;
    }

    protected Transform _view;
    protected BaseEntityData _entityData;
    protected IEntityController _entityController;
    protected IMapController _mapController;
    protected Vector2Int _coords;
    protected bool _active;

    public bool Frozen;

    protected TileBasedEffect _tileEffect = null;

    public event EntityMovedDelegate OnEntityMoved;

    public void Init(BaseEntityData entityData, BaseEntityDependencies deps)
    {
        _entityData = entityData;
        _entityController = deps.EntityController;
        _mapController = deps.MapController;
      
        Frozen = false;
        DoInit(deps);
        Coords = deps.Coords;
        CreateView();
    }

    protected abstract void DoInit(BaseEntityDependencies dependencies);

    public virtual void CreateView()
    {
        _view = Instantiate(_entityData.DefaultViewPrefab, transform, false);
        _view.localPosition = Vector3.zero;
        _view.localScale = Vector3.one;
    }

    public virtual void Cleanup()
    {
        _active = false;
    }

    public void RefreshCoords()
    {
        _coords = _mapController.CoordsFromWorld(transform.position);
    }


    #region ScheduledEntity
        public abstract void AddTime(float timeUnits, ref int playContext);
    #endregion

    public virtual void OnCreated()
    {

    }

    public virtual void OnAdded()
    {
        _active = true;
    }

    public virtual void OnDestroyed()
    {
        Cleanup();
    }

    public virtual void SetSpeedRate(float speedRate)
    {
    }

    public virtual void ResetSpeedRate()
    {
    }

    public abstract float DistanceFromPlayer();
    public abstract bool IsHostileTo(IBattleEntity2 other);
}