using Michsky.MUIP;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public class Director : MonoBehaviour
{
    public static Director Instance { get; private set; }
    [HideInInspector] public WindowManager.WindowItem currentWindow;
    public PostProcessVolume volume;
    public Map map;
    public Settings settings;
    public Menu menu;
    public Credits credits;

    [Header("UI Elements")]
    public WindowManager wm;
    [SerializeField] private ButtonManager closeAppBtn;
    [SerializeField] private ModalWindowManager resultsMW;
    [SerializeField] private ModalWindowManager progressVM;
    [SerializeField] private TMP_Text progressTxt;
    [SerializeField] private NotificationManager notificationNF;

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
    public float searchSpeed = 0.5f;

    public bool ShowResults
    {
        get { return resultsMW.isActiveAndEnabled; }
        set
        {
            if (value)
                resultsMW.Open();
            else
                resultsMW.Close();
        }
    }
    public bool ShowProgress
    {
        get { return progressVM.isActiveAndEnabled; }
        set
        {
            if (value)
                progressVM.Open();
            else
                progressVM.Close();
        }
    }

    public string ProgressText
    {
        get { return progressTxt.text; }
        set { progressTxt.text = value; }
    }

    public bool ShowNotification
    {
        get { return notificationNF.isActiveAndEnabled; }
        set
        {
            if (value)
                notificationNF.Open();
            else
                notificationNF.Close();
        }
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

    public void Result(string title, string content)
    {
        resultsMW.titleText = title;
        resultsMW.descriptionText = content;
        resultsMW.UpdateUI();
        resultsMW.Open();
    }

    public void Notify(string title, string content, float time = 3)
    {
        notificationNF.title = title;
        notificationNF.description = content;
        notificationNF.timer = time;
        notificationNF.UpdateUI();
        notificationNF.Open();
    }

    public async Task Load(Func<IProgress<string>, Task> action)
    {
        ShowProgress = true;
        var progress = new Progress<string>(content =>
        {
            ProgressText = content;
        });

        try
        {
            await action(progress);
        }
        catch {}
        finally
        {
            ShowProgress = false;
        }
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