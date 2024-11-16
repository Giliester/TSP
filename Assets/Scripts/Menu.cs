using Michsky.MUIP;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public ButtonManager mapBtn;
    public ButtonManager creditsBtn;
    public ButtonManager settingsBtn;

    void Start()
    {
        mapBtn.onClick.AddListener(() => Director.Instance.wm.OpenWindow("Map"));
        creditsBtn.onClick.AddListener(() => Director.Instance.wm.OpenWindow("Credits"));
        settingsBtn.onClick.AddListener(() => Director.Instance.wm.OpenWindow("Settings"));
    }

    void Update()
    {
        
    }
}
