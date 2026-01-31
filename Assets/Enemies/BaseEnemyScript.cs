using System.Collections;
using System.Collections.Generic;
using GGJ;
using UnityEngine;

public class BaseEnemyScript : MonoBehaviour
{
    [SerializeField]
    public float MoveSpeed = 5;
    [SerializeField]
    public EnemyState CurrentEnemyState;
    [SerializeField]
    public GameObject PlayerRef;

    void Start()
    {
        SetEnemyState(EnemyState.Active);
    }
    
    void FixedUpdate()
    {
        switch(CurrentEnemyState)
        {
            case EnemyState.NotActive:
                break;
            case EnemyState.Active:
                Vector3 dirToPlayer = (PlayerRef.transform.position - transform.position).normalized;
                transform.position += new Vector3(dirToPlayer.x, 0, dirToPlayer.z) * Time.fixedDeltaTime * MoveSpeed;
                transform.LookAt(PlayerRef.transform);
                break;
        }
    }
    
    public void SetEnemyState(GGJ.EnemyState state)
    {
        CurrentEnemyState = state;
    }
}
