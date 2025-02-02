using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    [Header("Director Spawn Settings")]
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public int cost;
    [SerializeField] public OutpostType type;
}
