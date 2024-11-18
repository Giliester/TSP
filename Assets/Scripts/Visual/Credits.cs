using Michsky.MUIP;
using UnityEngine;

public class Credits : MonoBehaviour
{
    public ButtonManager menuBtn;
    void Start()
    {
        menuBtn.onClick.AddListener(() => Director.Instance.wm.OpenWindow("Menu"));
    }
}
