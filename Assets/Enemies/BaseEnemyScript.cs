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
    public float ReNavDist = 2f;
    public GameObject NavDestinationPoint;

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

        NavDestinationPoint.transform.parent = null;
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
                NavDestinationPoint.SetActive(false);
                break;
            case EnemyState.Active:
                NavMeshAgentRef.isStopped = false;
                Vector3 destination = PlayerRef.transform.position;
                if (Vector3.Distance(NavMeshAgentRef.destination, destination) > ReNavDist)
                {
                    NavMeshAgentRef.destination = destination;
                }
                NavDestinationPoint.SetActive(true);
                NavDestinationPoint.transform.position = NavMeshAgentRef.destination;
                transform.LookAt(NavMeshAgentRef.destination);
                break;
        }
    }
    
    public void SetEnemyState(EnemyState state)
    {
        CurrentEnemyState = state;
    }
}
