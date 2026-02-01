using System.Collections;
using System.Collections.Generic;
using GGJ;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemyScript : MonoBehaviour
{
    float health = 100;
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
    public float AttackDamage = 10f;
    public ParticleSystem ParticleSystemRef;
    public Animator enemyMeshAnimator;

    [Header("Audio")]
    public AudioClip ambientSound;
    private AudioSource audioSource;

    public void Damage(float inputHealth)
    {
        AnimatorStateInfo enemyInfo = enemyMeshAnimator.GetCurrentAnimatorStateInfo(0);
        if (enemyInfo.IsName("MonsterWalk"))
        {
            health -= inputHealth;
            enemyMeshAnimator.Play("TakeDamage");
        }
    }

    void Start()
    {
        health = 100;
        if (!PlayerRef)
        {
            PlayerRef = GameObject.FindGameObjectWithTag("Player");
        }

        playerController controller = PlayerRef?.GetComponent<playerController>();

        // Initialize spatial audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = ambientSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Set to 3D spatial audio
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
    }

    void Update()
    {
        if (health < 0)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            enemyMeshAnimator.Play("Die");
            Destroy(this);
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(NavMeshAgentRef);
        }
        if (IsGoalReachable(PlayerRef.transform.position) && Input.GetMouseButtonDown(1))
        {
            CurrentEnemyState = EnemyState.Active;
        }
        AnimatorStateInfo enemyInfo = enemyMeshAnimator.GetCurrentAnimatorStateInfo(0);
        switch (CurrentEnemyState)
        {
            case EnemyState.NotActive:
                NavMeshAgentRef.isStopped = true;
                if (!enemyInfo.IsName("MonsterSleep"))
                    enemyMeshAnimator.Play("MonsterSleep");
                
                if (audioSource.isPlaying)
                    audioSource.Stop();

                break;
            case EnemyState.Active:
                if (enemyInfo.IsName("MonsterSleep"))
                    enemyMeshAnimator.Play("MonsterWakeUp");

                if (ambientSound != null && !audioSource.isPlaying)
                    audioSource.Play();

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
                break;
        }
    }

    bool IsGoalReachable(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        // Calculate the path. NavMesh.CalculatePath is synchronous.
        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);

        // Check the path status
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            return true;
        }
        else
        {
            // The path is incomplete, meaning the destination is unreachable.
            // Other statuses can be PathPartial or PathInvalid.
            return false;
        }
    }

    public void Attack()
    {
        if (ParticleSystemRef)
            ParticleSystemRef.Play();

        RaycastHit hit;
        if (Physics.Raycast(AttackStart.position, transform.forward, out hit, AttackDist) && hit.collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("Doing Damage");
            PlayerStats.Instance.currentHealth -= AttackDamage;
        }
    }
    
    public void SetEnemyState(EnemyState state)
    {
        CurrentEnemyState = state;
    }
}
