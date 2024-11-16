using Michsky.MUIP;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public ButtonManager menuBtn;
    public ButtonManager backBtn;
    public Toggle invertZoom;
    public Toggle invertPanX;
    public Toggle invertPanY;
    public Toggle useEffects;

    void Start()
    {
        menuBtn.onClick.AddListener(() => Director.Instance.wm.OpenWindow("Menu"));
        backBtn.onClick.AddListener(Director.Instance.ToggleSettings);
        invertZoom.onValueChanged.AddListener((value) => Director.Instance.invertZoom = value);
        invertPanX.onValueChanged.AddListener((value) => Director.Instance.invertPanX = value);
        invertPanY.onValueChanged.AddListener((value) => Director.Instance.invertPanY = value);
        useEffects.onValueChanged.AddListener((value) => Director.Instance.useEffects = value);
    }

    void Update()
    {
        
    }
}
