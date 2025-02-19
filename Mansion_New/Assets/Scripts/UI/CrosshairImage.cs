using UnityEngine;
using UnityEngine.UI;

public class CrosshairImage : Image
{
    public void Enter()
        => rectTransform.sizeDelta = new(50, 50);
    public void Exit()
        => rectTransform.sizeDelta = new(15, 15);

    public void StartHold()
    {
        transform.GetChild(0).GetComponent<Animator>().ResetTrigger("End");
        transform.GetChild(0).GetComponent<Animator>().SetTrigger("Start");
    }
    public void EndHold()
        => transform.GetChild(0).GetComponent<Animator>().SetTrigger("End");
}
