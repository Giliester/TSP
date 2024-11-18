using UnityEngine;
using System.Collections.Generic;

public enum StateView
{
    Normal,
    Hover,
    Selected,
    Visited,
    Disabled,
    Visiting,
    Path
}

public class State : MonoBehaviour
{
    public string Name
    {
        get
        {
            return gameObject.name;
        }
    }
    public string LowerName
    {
        get
        {
            return gameObject.name.ToLower();
        }
    }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    private MeshRenderer meshRenderer;
    private Material normalMaterial;
    private Material hoverMaterial;
    private Material selectedMaterial;
    private Material disabledMaterial;
    private Material visitedMaterial;
    private Material pathMaterial;
    private Material visitingMaterial;

    private StateView view;
    private const string normalMaterialPath = "Materials/";

    private Dictionary<Collider, GameObject> childColliders;

    public StateView View
    {
        get
        {
            return view;
        }
        set
        {
            if (view == value) return;
            view = value;

            switch (view)
            {
                case StateView.Normal:
                    GetComponent<Renderer>().material = normalMaterial;
                    break;
                case StateView.Hover:
                    GetComponent<Renderer>().material = hoverMaterial;
                    break;
                case StateView.Selected:
                    GetComponent<Renderer>().material = selectedMaterial;
                    break;
                case StateView.Disabled:
                    GetComponent<Renderer>().material = disabledMaterial;
                    break;
                case StateView.Visited:
                    GetComponent<Renderer>().material = visitedMaterial;
                    break;
                case StateView.Visiting:
                    GetComponent<Renderer>().material = visitingMaterial;
                    break;
                case StateView.Path:
                    GetComponent<Renderer>().material = pathMaterial;
                    break;
            }
        }
    }

    void Start()
    {
        childColliders = new Dictionary<Collider, GameObject>();

        meshRenderer = GetComponent<MeshRenderer>();
        normalMaterial = Resources.Load<Material>(normalMaterialPath + "USA/" + Name);
        selectedMaterial = Resources.Load<Material>(normalMaterialPath + "Selected");
        hoverMaterial = Resources.Load<Material>(normalMaterialPath + "Hover");
        disabledMaterial = Resources.Load<Material>(normalMaterialPath + "Disabled");
        visitedMaterial = Resources.Load<Material>(normalMaterialPath + "Visited");
        visitingMaterial = Resources.Load<Material>(normalMaterialPath + "Visiting");
        pathMaterial = Resources.Load<Material>(normalMaterialPath + "Path");

        GetComponent<Renderer>().material = normalMaterial;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            if (!childColliders.ContainsKey(collider))
            {
                childColliders.Add(collider, collider.gameObject);
            }
        }
    }

    void Update()
    {
        // Si el estado es parte del camino o está siendo visitado, no cambiar su visualización
        if (View == StateView.Path || View == StateView.Visited || View == StateView.Visiting)
        {
            return;
        }

        if (Director.Instance.map.finding)
        {
            View = StateView.Disabled;
            return;
        }

        if (Director.Instance.map.From == this || Director.Instance.map.To == this)
        {
            View = StateView.Selected;
            return;
        }

        if (Director.Instance.map.From != null && Director.Instance.map.To != null)
        {
            View = StateView.Disabled;
            return;
        }

        if (Director.Instance.map.selectable && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Inputs.Instance.Point);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (childColliders.ContainsKey(hit.collider))
                {
                    View = StateView.Hover;
                    Director.Instance.map.hover = this;

                    if (Inputs.Instance.LeftClick)
                    {
                        Director.Instance.map.Current = this;
                    }
                    return;
                }
            }
        }
        View = StateView.Normal;
    }
}