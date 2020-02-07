using UnityEngine;

public class RandomSetter : StateMachineBehaviour
{
    public string IntParameterName;
    public int StateCount;

    private int test;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        test = Random.Range(0, StateCount);
        animator.SetInteger(IntParameterName, test);
        Debug.Log("test:" + test.ToString());
    }
}