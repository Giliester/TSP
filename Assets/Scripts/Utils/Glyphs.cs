using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Threading.Tasks;

public static class Glyphs
{
    public static Dictionary<string, StateNode> StateNodes { get; private set; } = new Dictionary<string, StateNode>();
    public static Dictionary<string, Dictionary<string, double>> AdjacencyMatrix { get; private set; } = new Dictionary<string, Dictionary<string, double>>();

    public static async Task InitializeAsync(Dictionary<string, State> states)
    {
        try
        {
            ResourceRequest jsonRequest = Resources.LoadAsync<TextAsset>("us-states");
            await jsonRequest;

            TextAsset jsonText = jsonRequest.asset as TextAsset;
            if (jsonText == null)
            {
                Director.Instance.Notify("Error", "No se pudo cargar el archivo JSON de estados", 1);
                return;
            }

            string jsonContent = jsonText.text;
            RootObject root = JsonConvert.DeserializeObject<RootObject>(jsonContent);

            foreach (var feature in root.features)
            {
                string stateName = feature.properties.name;
                JToken coordinatesToken = feature.geometry.coordinates;
                string geometryType = feature.geometry.type;

                List<Vector2> coordinates = new List<Vector2>();

                if (geometryType == "Polygon")
                {
                    foreach (var polygon in coordinatesToken)
                    {
                        foreach (var coordinatePair in polygon)
                        {
                            double lon = (double)coordinatePair[0];
                            double lat = (double)coordinatePair[1];
                            coordinates.Add(new Vector2((float)lat, (float)lon));
                        }
                    }
                }
                else if (geometryType == "MultiPolygon")
                {
                    foreach (var multiPolygon in coordinatesToken)
                    {
                        foreach (var polygon in multiPolygon)
                        {
                            foreach (var coordinatePair in polygon)
                            {
                                double lon = (double)coordinatePair[0];
                                double lat = (double)coordinatePair[1];
                                coordinates.Add(new Vector2((float)lat, (float)lon));
                            }
                        }
                    }
                }

                Vector2 centroid = CalculateCentroid(coordinates);

                if (states.TryGetValue(stateName, out State state))
                {
                    state.Latitude = centroid.x;
                    state.Longitude = centroid.y;

                    StateNode stateNode = new StateNode
                    {
                        Name = stateName,
                        Latitude = centroid.x,
                        Longitude = centroid.y
                    };
                    StateNodes[stateName] = stateNode;
                }
                else
                {
                    Director.Instance.Notify("Advertencia", $"Estado '{stateName}' no encontrado en el diccionario de estados.", 1);
                }
            }

            await BuildAdjacencyMatrixFromCSVAsync(states);
        }
        catch
        {
            Director.Instance.Notify("Error", "Error al inicializar el grafo", 1);
        }
    }

    private static async Task BuildAdjacencyMatrixFromCSVAsync(Dictionary<string, State> states)
    {
        ResourceRequest csvRequest = Resources.LoadAsync<TextAsset>("adjacency_matrix");
        await csvRequest;

        TextAsset csvData = csvRequest.asset as TextAsset;

        if (csvData == null)
        {
            Director.Instance.Notify("Error", "Archivo CSV de matriz de adyacencia no encontrado en Recursos.", 1);
            return;
        }

        using (StringReader reader = new StringReader(csvData.text))
        {
            string headerLine = await reader.ReadLineAsync();

            if (string.IsNullOrEmpty(headerLine))
            {
                Director.Instance.Notify("Error", "Archivo CSV vacío o mal formado", 1);
                return;
            }

            string[] headerFields = headerLine.Split(',');
            List<string> stateNames = new List<string>();

            for (int i = 1; i < headerFields.Length; i++)
            {
                string stateName = headerFields[i].Trim().Replace("\"", "");
                stateNames.Add(stateName);
            }

            AdjacencyMatrix = new Dictionary<string, Dictionary<string, double>>();

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] fields = line.Split(',');

                if (fields.Length != headerFields.Length)
                {
                    Director.Instance.Notify("Advertencia", "La longitud de la fila no coincide con la longitud del encabezado. Saltando fila.", 1);
                    continue;
                }

                string stateName = fields[0].Trim().Replace("\"", "");

                if (!states.ContainsKey(stateName))
                {
                    Director.Instance.Notify("Advertencia", $"Estado '{stateName}' no encontrado en el diccionario de estados. Saltando fila.", 1);
                    continue;
                }

                if (!StateNodes.TryGetValue(stateName, out StateNode fromState))
                {
                    Director.Instance.Notify("Advertencia", $"StateNode para '{stateName}' no encontrado. Saltando fila.", 1);
                    continue;
                }

                AdjacencyMatrix[stateName] = new Dictionary<string, double>();

                for (int i = 1; i < fields.Length; i++)
                {
                    string neighborName = stateNames[i - 1];
                    string adjacencyValue = fields[i].Trim().Replace("\"", "");

                    if (!int.TryParse(adjacencyValue, out int adjacencyFlag))
                    {
                        Director.Instance.Notify("Advertencia", $"Valor de adyacencia inválido en estado '{stateName}', vecino '{neighborName}'.", 1);
                        continue;
                    }

                    if (adjacencyFlag == 1)
                    {
                        if (!states.ContainsKey(neighborName))
                        {
                            Director.Instance.Notify("Advertencia", $"Estado vecino '{neighborName}' no encontrado en el diccionario de estados.", 1);
                            continue;
                        }

                        if (!StateNodes.TryGetValue(neighborName, out StateNode toState))
                        {
                            Director.Instance.Notify("Advertencia", $"StateNode para vecino '{neighborName}' no encontrado.", 1);
                            continue;
                        }

                        double distance = CalculateHaversineDistance(
                            fromState.Latitude, fromState.Longitude,
                            toState.Latitude, toState.Longitude);

                        AdjacencyMatrix[stateName][neighborName] = distance;
                    }
                }
            }
        }

        Director.Instance.Notify("Éxito", "Matriz de adyacencia generada", 1);
    }

    private static Vector2 CalculateCentroid(List<Vector2> points)
    {
        if (points.Count == 0)
            return Vector2.zero;

        float x = 0;
        float y = 0;

        foreach (var point in points)
        {
            x += point.x;
            y += point.y;
        }

        x /= points.Count;
        y /= points.Count;

        return new Vector2(x, y);
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const int R = 6371;
        const double Deg2Rad = Math.PI / 180.0;

        double dLat = (lat2 - lat1) * Deg2Rad;
        double dLon = (lon2 - lon1) * Deg2Rad;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Deg2Rad) * Math.Cos(lat2 * Deg2Rad) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}

public class RootObject
{
    public string type { get; set; }
    public List<Feature> features { get; set; }
}

public class Feature
{
    public string type { get; set; }
    public string id { get; set; }
    public Properties properties { get; set; }
    public Geometry geometry { get; set; }
}

public class Properties
{
    public string name { get; set; }
    public double density { get; set; }
}

public class Geometry
{
    public string type { get; set; }
    public JToken coordinates { get; set; }
}

public class StateNode
{
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}