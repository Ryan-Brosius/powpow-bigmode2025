using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroy : MonoBehaviour
{
    public GameObject Drop;
    public int Chance;



    private void OnDestroy()
    {
        if (gameObject.scene.isLoaded)
        {
            if (Random.Range(1, 100) <= Chance) Instantiate(Drop, transform.position, Quaternion.identity);
        }
    }
}
