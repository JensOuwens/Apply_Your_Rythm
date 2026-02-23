using System.Collections;
using UnityEngine;

public class GooberCatchAnim : MonoBehaviour
{
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite catchSprite;

    private Coroutine currentAnim;
    
    public void GooberAnim()
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(GooberAnimCoroutine(0.8f));
    }
    
    public void GooberHoldAnim(float duration)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(GooberAnimCoroutine(duration));
    }

    private IEnumerator GooberAnimCoroutine(float duration)
    {
        GetComponent<SpriteRenderer>().sprite = catchSprite;
        yield return new WaitForSeconds(duration);
        GetComponent<SpriteRenderer>().sprite = originalSprite;
        currentAnim = null;
    }
}