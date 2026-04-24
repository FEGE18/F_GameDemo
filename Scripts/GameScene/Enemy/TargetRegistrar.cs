using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetRegistrar : MonoBehaviour
{
    public float weight = 1f;

    private void Awake()
    {
        EnemyTargetRegistry.Register(transform);
    }

    private void ODestroy()
    {
        EnemyTargetRegistry.Unregister(transform);
    }
}
