using Michsky.MUIP;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Map : MonoBehaviour
{
    public CustomDropdown fromDw;
    public CustomDropdown toDw;
    public TMP_Text hoverStateTxt;
    public ButtonManager cleanSelectionsBtn;
    public ButtonManager findBtn;

    public Dictionary<string, State> states = new();

    private State from;
    public State From
    {
        get { return from; }
        set
        {
            if (From == value)
                return;

            if (To != null && value == To)
            {
                if (To == null)
                {
                    From = null;
                }
                else
                {
                    from = To;
                    To = null;
                }
            }
            else
            {
                from = value;

                if (From != null)
                {
                    fromDw.ChangeDropdownInfo(states.Keys.ToList().IndexOf(From.Name));
                }
                else
                {
                    fromDw.ChangeDropdownInfo(0);
                }
            }
        }
    }
    private State to;
    public State To
    {
        get { return to; }
        set
        {
            if (To == value)
                return;

            if (From != null && value == From)
            {
                if (From == null)
                {
                    To = null;
                }
                else
                {
                    to = From;
                    From = null;
                }
            }
            else
            {
                to = value;

                if (To != null)
                {
                    toDw.ChangeDropdownInfo(states.Keys.ToList().IndexOf(To.Name));
                }
                else
                {
                    toDw.ChangeDropdownInfo(0);
                }
                toDw.UpdateItemLayout();
            }
        }
    }

    private State current;
    public State Current
    {
        get { return current; }
        set
        {
            if (current == value)
                return;

            current = value;

            if (From == null)
            {
                From = current;
            }
            else
            {
                To = current;
            }
        }
    }

    [HideInInspector] public State hover;
    [HideInInspector] public bool selectable;
    [HideInInspector] public bool finding;

    void Start()
    {
        selectable = true;

        cleanSelectionsBtn.onClick.AddListener(CleanSelections);
        findBtn.onClick.AddListener(Find);

        var getStates = GameObject.FindObjectsByType<State>(FindObjectsSortMode.None).OrderBy(state => state.Name).ToList();

        for (int i = 0; i <= getStates.Count; i++)
        {
            if (i == 0)
            {
                fromDw.CreateNewItem("Ninguno", false);
                fromDw.items[i].OnItemSelection.AddListener(() => SetFrom("Ninguno"));
                toDw.CreateNewItem("Ninguno");
                toDw.items[i].OnItemSelection.AddListener(() => SetTo("Ninguno"));
                states.Add("Ninguno", null);
            }
            else
            {
                var currentState = getStates[i - 1];
                fromDw.CreateNewItem(currentState.Name, false);
                fromDw.items[i].OnItemSelection.AddListener(() => SetFrom(currentState.Name));
                toDw.CreateNewItem(currentState.Name, false);
                toDw.items[i].OnItemSelection.AddListener(() => SetTo(currentState.Name));
                states.Add(currentState.Name, currentState);
            }
        }

        fromDw.SetupDropdown();
        toDw.SetupDropdown();
    }

    void Update()
    {
        selectable = From == null || To == null;
        hoverStateTxt.text = hover != null && selectable ? hover.Name : "";
        findBtn.Interactable(From != null && To != null);
    }

    public void CleanSelections()
    {
        From = null;
        To = null;
        toDw.ChangeDropdownInfo(0);
        fromDw.ChangeDropdownInfo(0);
    }

    public void SetFrom(string input)
    {
        if (states.ContainsKey(input))
        {
            From = states[input];
        }
    }

    public void SetTo(string input)
    {
        if (states.ContainsKey(input))
        {
            To = states[input];
        }
    }

    public void Find()
    {
        finding = true;
    }
}
