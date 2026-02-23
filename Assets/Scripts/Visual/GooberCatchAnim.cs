using System.Collections;
using UnityEngine;

public class GooberCatchAnim : MonoBehaviour
{
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite catchSprite;
    
    public void GooberAnim()
    {
        StartCoroutine(GooberAnimCoroutine());
    }

    IEnumerator GooberAnimCoroutine()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = catchSprite;
        yield return new WaitForSeconds(0.8f);
        this.gameObject.GetComponent<SpriteRenderer>().sprite = originalSprite;
    }
}
