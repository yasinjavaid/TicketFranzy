
using UnityEngine;

public class PageEvents : MonoBehaviour
{
    public GameObject ColorHolder;
    public GameObject OutfitsHolder;
    public Animator animator;

    public void EnablePageOne()
    {
        animator.SetBool("PageOne", true);
        animator.SetBool("PageTwo", false);
    }

    public void EnablePageTwo()
    {
        animator.SetBool("PageOne", false);
        animator.SetBool("PageTwo", true);
    }

    public void EnableColorHolder()
    {
        ColorHolder.SetActive(true);
        OutfitsHolder.SetActive(false);
    }

    public void EnableOutfitsHolder()
    {
        ColorHolder.SetActive(false);
        OutfitsHolder.SetActive(true);
    }


}
