using UnityEngine;

public class TriggerColliders : MonoBehaviour
{
    public ControlCockroaches barata;
    public TitleCardController titleCard;
    public Sprite subtitleSprite;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
         return;

        if (barata != null)
        {
            barata.ComecarMovimento();
        }

        if (titleCard != null && subtitleSprite != null)
        {
            titleCard.ShowTitleWithSubtitle(subtitleSprite);
        }
        gameObject.SetActive(false);
    }
}