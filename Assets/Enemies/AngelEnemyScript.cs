using System.Collections;
using System.Collections.Generic;
using GGJ;
using UnityEngine;
using UnityEngine.AI;

public class AngelEnemyScript : MonoBehaviour
{
    float health = 100;
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

    public float LookAngleThreshold = 20f;

    [Header("Audio")]
    public AudioClip crySound;
    private AudioSource audioSource;

    public void Damage(float inputHealth)
    {
        health -= inputHealth;
    }

    void Start()
    {
        if (!PlayerRef)
        {
            PlayerRef = GameObject.FindGameObjectWithTag("Player");
        }

        // Initialize audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = crySound;
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
            
            Destroy(this);
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(NavMeshAgentRef);
            return;
        }

        Vector3 dirFromPlayer = (transform.position - PlayerRef.transform.position).normalized;
        bool playerLookingAtThis = Vector3.Angle(dirFromPlayer, PlayerRef.transform.forward) <= LookAngleThreshold;
        SetEnemyState(playerLookingAtThis ? EnemyState.NotActive : EnemyState.Active);

        // Cry logic: play sound if player is looking at the angel
        if (playerLookingAtThis && crySound != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

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
