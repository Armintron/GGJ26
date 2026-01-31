using System.Collections;
using System.Collections.Generic;
using GGJ;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemyScript : MonoBehaviour
{
    [SerializeField]
    public float MoveSpeed = 5;
    [SerializeField]
    public EnemyState CurrentEnemyState;
    [SerializeField]
    public GameObject PlayerRef;
    public NavMeshAgent NavMeshAgentRef;

    void Start()
    {
        if (!PlayerRef)
        {
            PlayerRef = GameObject.FindGameObjectWithTag("Player");
        }

        playerController controller = PlayerRef?.GetComponent<playerController>();
        if (controller)
        {
            controller.EventMaskStateChanged.AddListener((MaskState state) =>
            {
                SetEnemyState(MaskToEnemyState(state));
            });

            SetEnemyState(MaskToEnemyState(controller.CurrentMaskState));
        }
    }

    private EnemyState MaskToEnemyState(MaskState state)
    {
        return state == MaskState.On ? EnemyState.Active : EnemyState.NotActive;
    }
    
    void Update()
    {
        switch(CurrentEnemyState)
        {
            case EnemyState.NotActive:
                NavMeshAgentRef.isStopped = true;
                break;
            case EnemyState.Active:
                NavMeshAgentRef.isStopped = false;
                NavMeshAgentRef.destination = PlayerRef.transform.position;
                transform.LookAt(PlayerRef.transform);
                break;
        }
    }
    
    public void SetEnemyState(EnemyState state)
    {
        CurrentEnemyState = state;
    }
}
