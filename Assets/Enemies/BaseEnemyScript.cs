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
    public Transform AttackStart;
    public float AttackRadius = 2f;
    public float AttackDist = 5f;
    public float AttackRate = 2f;
    public float LastAttack = 0f;

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
        switch (CurrentEnemyState)
        {
            case EnemyState.NotActive:
                NavMeshAgentRef.isStopped = true;
                break;
            case EnemyState.Active:
                if ((LastAttack + AttackRate) <= Time.time)
                {
                    Attack();
                    LastAttack = Time.time;
                }

                NavMeshAgentRef.isStopped = false;
                Vector3 destination = PlayerRef.transform.position;
                if (Vector3.Distance(NavMeshAgentRef.destination, destination) > ReNavDist)
                {
                    NavMeshAgentRef.destination = destination;
                }
                transform.LookAt(NavMeshAgentRef.destination);
                break;
        }
    }
    
    public void Attack()
    {
        RaycastHit hit;
        {
            // Top
            Debug.DrawRay(AttackStart.position + new Vector3(0, AttackRadius / 2, 0), transform.forward * AttackDist, Color.red, 1f);
            // Bottom
            Debug.DrawRay(AttackStart.position - new Vector3(0, AttackRadius / 2, 0), transform.forward * AttackDist, Color.red, 1f);
            // Right
            Debug.DrawRay(AttackStart.position + new Vector3(AttackRadius / 2, 0, 0), transform.forward * AttackDist, Color.red, 1f);
            // Left
            Debug.DrawRay(AttackStart.position - new Vector3(AttackRadius / 2, 0, 0), transform.forward * AttackDist, Color.red, 1f);
        }
        if (Physics.SphereCast(AttackStart.position, AttackRadius, transform.forward, out hit, AttackDist))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                Debug.Log("Doing Damage");
                PlayerStats.Instance.currentHealth -= 10;
            }
        }
    }
    
    public void SetEnemyState(EnemyState state)
    {
        CurrentEnemyState = state;
    }
}
