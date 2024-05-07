
using UnityEngine;
using UnityEngine.Serialization;

public class TooltipEvents : MonoBehaviour
{
    [FormerlySerializedAs("InfoBoxAnimator")]
    public Animator animator;

    public void EnableTooltip(string name) => animator.SetBool(name, true);
    public void DisableTooltip(string name) => animator.SetBool(name, false);
}
