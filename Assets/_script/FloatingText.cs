using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public Animator animator;
    private Text damageText;
    void OnEnable()
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        Debug.Log("length  :: " + clipInfo[0].clip.length.ToString());
        Destroy(gameObject, clipInfo[0].clip.length);
        Debug.Log("length  :: " + clipInfo[0].clip.length.ToString());
        damageText = animator.GetComponent<Text>();
    }

    public void SetText(string text)
    {
        damageText.text = text;
    }
}
