using System.Collections;
using System.Collections.Generic;
using GGJ;
using UnityEngine;

public class EnemyManagerScript : MonoBehaviour
{
    public GameObject PlayerRef;
    public playerController PlayerControllerRef;

    public float MaskTime = 0f;

    public GameObject GasEnemyTemplate;

    public float SpawnDist = 5f;

    public float MaskTimeToSpawnGasEnemy = 3f;

    // Update is called once per frame
    void Update()
    {
        if (PlayerControllerRef.CurrentMaskState == MaskState.On)
        {
            MaskTime += Time.deltaTime;
        }

        if (MaskTime >= MaskTimeToSpawnGasEnemy)
        {
            GameObject spawnedEnemy = Instantiate(GasEnemyTemplate);
            spawnedEnemy.transform.position = GetSpawnLocationAroundTransform(PlayerRef.transform);
            spawnedEnemy.transform.parent = transform;
            MaskTime = 0f;
        }
    }

    private Vector3 GetSpawnLocationAroundTransform(Transform AroundTransform)
    {
        Vector3 xzTransformDir = new Vector3(AroundTransform.forward.x, 0, AroundTransform.forward.z).normalized;
        return AroundTransform.position + xzTransformDir * SpawnDist;
    }
}
