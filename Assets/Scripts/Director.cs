using Michsky.MUIP;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Director : MonoBehaviour
{
    public static Director Instance { get; private set; }
    [HideInInspector] public WindowManager.WindowItem currentWindow;
    public PostProcessVolume volume;
    public Map map;
    public Settings settings;
    public Menu menu;

    [Header("UI Elements")]
    public WindowManager wm;

    private int lastIndex;

    [Header("Global Settings")]
    public bool invertZoom = true;
    public bool invertPanX = true;
    public bool invertPanY = true;
    public bool useEffects
    {
        get { return volume.enabled; }
        set { volume.enabled = value; }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        wm.OpenWindow("Menu");
    }

    void Update()
    {
        currentWindow = wm.windows[wm.currentWindowIndex];
    }

    public void onSettings(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        if (currentWindow.windowName != "Settings")
        {
            lastIndex = wm.currentWindowIndex;
            wm.OpenWindow("Settings");
        }
        else
        {
            wm.OpenWindowByIndex(lastIndex);
        }
    }
}