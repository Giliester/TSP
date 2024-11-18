using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dijkstra : MonoBehaviour
{
    public float Offset = 1.0f;
    public TMP_Text infoText;

    private List<GameObject> lineObjects = new();
    private Material lineMaterial;

    void Start()
    {
        lineMaterial = Resources.Load<Material>("Materials/LineMaterial");

        if (infoText == null)
        {
            infoText = GetComponentInChildren<TMP_Text>();
        }
    }

    public IEnumerator FindShortestPath(Dictionary<string, State> states, string startStateName, string endStateName)
    {
        var graph = Glyphs.AdjacencyMatrix;

        if (!graph.ContainsKey(startStateName))
        {
            Debug.LogError($"Start state '{startStateName}' not found in the adjacency matrix.");
            yield break;
        }

        if (!graph.ContainsKey(endStateName))
        {
            yield break;
        }

        foreach (var state in states.Values)
        {
            if (state != null)
            {
                state.View = StateView.Disabled;
            }
        }

        var distances = new Dictionary<string, double>();
        var previous = new Dictionary<string, string>();
        var unvisited = new HashSet<string>();

        foreach (var state in graph.Keys)
        {
            distances[state] = double.MaxValue;
            previous[state] = null;
            unvisited.Add(state);
        }
        distances[startStateName] = 0;

        while (unvisited.Count > 0)
        {
            if (!this.gameObject.activeInHierarchy)
            {
                yield break;
            }

            string currentState = null;
            double smallestDistance = double.MaxValue;

            foreach (var state in unvisited)
            {
                if (distances[state] < smallestDistance)
                {
                    smallestDistance = distances[state];
                    currentState = state;
                }
            }

            if (currentState == null)
            {
                break;
            }

            if (states.TryGetValue(currentState, out State currentStateObj))
            {
                currentStateObj.View = StateView.Visiting;
            }

            if (infoText != null)
            {
                infoText.text = $"Visitando: {currentState}\nDistancia desde inicio: {distances[currentState]:F2} km";
            }

            yield return new WaitForSeconds(Director.Instance.searchSpeed);

            if (currentState == endStateName)
            {
                break;
            }

            unvisited.Remove(currentState);

            if (states.TryGetValue(currentState, out currentStateObj))
            {
                currentStateObj.View = StateView.Visited;
            }

            foreach (var neighbor in graph[currentState])
            {
                if (!unvisited.Contains(neighbor.Key))
                    continue;

                double altDistance = distances[currentState] + neighbor.Value;

                if (altDistance < distances[neighbor.Key])
                {
                    distances[neighbor.Key] = altDistance;
                    previous[neighbor.Key] = currentState;

                    if (states.TryGetValue(neighbor.Key, out State neighborState))
                    {
                        DrawLineBetweenStates(states[currentState], neighborState, Color.yellow);
                    }
                }

                if (states.TryGetValue(neighbor.Key, out State neighborObj) && neighborObj.View == StateView.Disabled)
                {
                    neighborObj.View = StateView.Visiting;
                }

                yield return new WaitForSeconds(Director.Instance.searchSpeed / 2);
            }
        }

        var path = new List<string>();
        string pathState = endStateName;

        if (previous[pathState] == null && pathState != startStateName)
        {
            ClearLines();

            if (infoText != null)
            {
                Director.Instance.Result("Resultado",$"No se encontró un camino entre {startStateName} y {endStateName}.");
            }

            yield break;
        }

        while (pathState != null)
        {
            path.Add(pathState);
            pathState = previous[pathState];
        }

        path.Reverse();

        ClearLines();

        for (int i = 0; i < path.Count; i++)
        {
            string stateName = path[i];
            if (states.TryGetValue(stateName, out State stateObj))
            {
                stateObj.View = StateView.Path;
            }

            if (i < path.Count - 1)
            {
                string nextStateName = path[i + 1];
                if (states.TryGetValue(nextStateName, out State nextStateObj))
                {
                    DrawLineBetweenStates(states[stateName], nextStateObj, Color.red);
                }
            }

            yield return new WaitForSeconds(Director.Instance.searchSpeed / 2);
        }

        double totalDistance = distances[endStateName];

        if (infoText != null)
        {
            Director.Instance.Result("Resultado", $"Camino más corto encontrado:\n{string.Join(" -> ", path)}\nDistancia total: {totalDistance:F2} km");
        }
    }

    private void DrawLineBetweenStates(State fromState, State toState, Color color)
    {
        GameObject lineObj = new("Line_" + fromState.Name + "_" + toState.Name);
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        Vector3 fromPosition = fromState.transform.position + new Vector3(0, Offset, 0);
        Vector3 toPosition = toState.transform.position + new Vector3(0, Offset, 0);

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, fromPosition);
        lineRenderer.SetPosition(1, toPosition);

        if (lineMaterial == null)
        {
            lineMaterial = Resources.Load<Material>("Materials/LineMaterial");
        }

        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineObjects.Add(lineObj);
    }

    public void ClearLines()
    {
        foreach (var lineObj in lineObjects)
        {
            Destroy(lineObj);
        }
        lineObjects.Clear();
    }

    void OnDisable()
    {
        ClearLines();

        if (Glyphs.StateNodes != null)
        {
            foreach (var stateNode in Glyphs.StateNodes.Values)
            {
                if (Director.Instance.map.states.TryGetValue(stateNode.Name, out State state))
                {
                    state.View = StateView.Normal;
                }
            }
        }

        if (infoText != null)
        {
            infoText.text = "";
        }
    }
}