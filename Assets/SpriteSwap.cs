using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwap : MonoBehaviour
{
    [SerializeField] bool isAutomatic = true;
    [SerializeField] List<Sprite> spritesList;
    private int currentSpriteIndex;
    private SpriteRenderer spriteComp;
    [SerializeField] float swapTimer;
    [SerializeField] float swapDuration = 0.5f;

    private void Awake()
    {
        spriteComp = this.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isAutomatic)
        {
            if (swapTimer > swapDuration)
            {
                swapTimer = 0f;
                if (currentSpriteIndex < spritesList.Count - 1) currentSpriteIndex++;
                else currentSpriteIndex = 0;
                spriteComp.sprite = spritesList[currentSpriteIndex];
            }
            swapTimer += Time.deltaTime;
        }
    }
        

    public IEnumerator ManualSpriteSwap()
    {
        for (int i = 1; i < spritesList.Count; i++)
        {
            spriteComp.sprite = spritesList[i];
            yield return new WaitForSeconds(swapDuration);
        }
        spriteComp.sprite = spritesList[0];
    }
}
