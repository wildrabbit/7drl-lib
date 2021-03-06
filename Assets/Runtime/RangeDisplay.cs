using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeDisplay : MonoBehaviour
{
    const string Valid = "valid_target";
    const string Invalid = "invalid_target";

    [SerializeField] Animator TilePrefab;
    List<Animator> _tileInstances;

    IMapController _mapController;
    Animator _animator;
    // Start is called before the first frame update
    void Awake()
    {
        _tileInstances = new List<Animator>();
    }

    public void Setup(IMapController controller)
    {
        _mapController = controller;
    }

    // Update is called once per frame
    public void RefreshLine(List<Vector3> coords, List<bool> activation)
    {
        int idx = 0;
        int last = Mathf.Min(_tileInstances.Count, activation.Count);
        for(idx = 0; idx < last; ++idx)
        {
            if (!_tileInstances[idx].gameObject.activeInHierarchy)
            {
                _tileInstances[idx].gameObject.SetActive(true);
            }
            _tileInstances[idx].transform.position = coords[idx];
            _tileInstances[idx].Play(activation[idx] ? Valid : Invalid);
        }

        if(idx < activation.Count)
        {
            for (idx = last; idx < activation.Count; ++idx)
            {
                var instance = Instantiate(TilePrefab, transform);
                instance.transform.position = coords[idx];
                instance.Play(activation[idx] ? Valid : Invalid);
                _tileInstances.Add(instance);
            }
        }
        else
        {
            for (idx = last; idx < _tileInstances.Count; ++idx)
            {
                if (_tileInstances[idx].gameObject.activeInHierarchy)
                {
                    _tileInstances[idx].gameObject.SetActive(false);
                }
            }
        }
        
    }
}
