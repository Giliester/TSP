using UnityEngine;

public class Inputs : MonoBehaviour
{
    private Input inputActions;
    public static Inputs Instance;

    public Vector2 Point;
    public Vector2 Scroll;
    public bool LeftClick;
    public bool RightClick;
    public bool MiddleClick;
    public bool Tab;
    public bool Settings;

    void Awake()
    {
        inputActions = new Input();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.UI.Point.performed += ctx => Point = ctx.ReadValue<Vector2>();
        inputActions.UI.ScrollWheel.performed += ctx => Scroll = ctx.ReadValue<Vector2>();
        inputActions.UI.Click.performed += ctx => LeftClick = ctx.action.WasPressedThisFrame();
        inputActions.UI.Click.canceled += ctx => LeftClick = false;
        inputActions.UI.RightClick.performed += ctx => RightClick = ctx.action.WasPressedThisFrame();
        inputActions.UI.RightClick.canceled += ctx => RightClick = false;
        inputActions.UI.MiddleClick.performed += ctx => MiddleClick = ctx.action.WasPressedThisFrame();
        inputActions.UI.MiddleClick.canceled += ctx => MiddleClick = false;
        inputActions.UI.AutoFill.performed += ctx => Tab = ctx.action.WasPressedThisFrame();
        inputActions.UI.AutoFill.canceled += ctx => Tab = false;
        inputActions.UI.Menu.started += ctx => Settings = ctx.action.WasPressedThisFrame();
        inputActions.UI.Menu.canceled += ctx => Settings = false;
    }

    void OnDisable()
    {
        inputActions.Disable();
        inputActions.UI.Point.performed -= ctx => Point = ctx.ReadValue<Vector2>();
        inputActions.UI.ScrollWheel.performed -= ctx => Scroll = ctx.ReadValue<Vector2>();
        inputActions.UI.Click.performed -= ctx => LeftClick = ctx.action.WasPressedThisFrame();
        inputActions.UI.Click.canceled -= ctx => LeftClick = false;
        inputActions.UI.RightClick.performed -= ctx => RightClick = ctx.action.WasPressedThisFrame();
        inputActions.UI.RightClick.canceled -= ctx => RightClick = false;
        inputActions.UI.MiddleClick.performed -= ctx => MiddleClick = ctx.action.WasPressedThisFrame();
        inputActions.UI.MiddleClick.canceled -= ctx => MiddleClick = false;
        inputActions.UI.AutoFill.performed -= ctx => Tab = ctx.action.WasPressedThisFrame();
        inputActions.UI.AutoFill.canceled -= ctx => Tab = false;
        inputActions.UI.Menu.started -= ctx => Settings = ctx.action.WasPressedThisFrame();
        inputActions.UI.Menu.canceled -= ctx => Settings = false;
    }
}
