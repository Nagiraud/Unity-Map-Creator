using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class CreerTerrain : MonoBehaviour
{
    private Mesh p_mesh;
    private Vector3[] p_vertices;
    private Vector3[] p_normals;
    private int[] p_triangles;
    public int resolution = 100;
    public float dimension = 10f;
    public bool CentrerPivot = true;
    private MeshFilter p_meshFilter;
    private MeshCollider p_meshCollider;

    [Header("Deformation Settings")]
    public Texture2D[] brushes;
    private int currentBrushIndex = 0;
    public AnimationCurve deformationCurve;
    public float intensity = 1f;
    public float patternRadius = 2f;
    public float deformationSpeed = 1f;
    private bool useBrush = true;
 
    public ExtensionTerrain extensionTerrain; // Reference vers ExtensionTerrain

    [Header("Key Bindings")]
    public KeyCode increaseRadiusKey = KeyCode.KeypadPlus;
    public KeyCode decreaseRadiusKey = KeyCode.KeypadMinus;
    public KeyCode nextPatternKey = KeyCode.P;
    public KeyCode changerBrosse = KeyCode.B;

    public KeyCode toggleDistanceMethodKey = KeyCode.F;
    public KeyCode toggleVertexApproximationKey = KeyCode.V;
    public KeyCode toggleNormalsUpdateKey = KeyCode.N;
    public KeyCode toggleGridSpaceKey = KeyCode.R;

    private bool isMouseHeld = false;
    private bool isControlPressed = false;

    private bool useFirstVertexApproximation = false;
    private bool updateOnlyModifiedNormals = false;
    private bool useGridSpaceForNeighbors = true;

    private DistanceMethod currentDistanceMethod = DistanceMethod.Euclidean;
    private enum DistanceMethod { Euclidean, Manhattan, Chebyshev }

    private Vector2 cameraRotation;

    public float rotationSpeed = 133f;

    private bool canDeform = true;

    public GameObject modalPanel; 
    public InputField dimensionInputField;
    public InputField resolutionInputField;


    void Start()
    {
        p_meshFilter = GetComponent<MeshFilter>();
        p_meshCollider = GetComponent<MeshCollider>();
        extensionTerrain = FindObjectOfType<ExtensionTerrain>();
        CréerTerrain();
    }

    void Update()
    {
        RotateTerrain();
        ModifyIntensity();
        ZoomTerrain();

        isControlPressed = Input.GetKey(KeyCode.LeftControl);

        if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.Escape))
        {
            canDeform = !canDeform;
        }
        if (Input.GetKeyDown(increaseRadiusKey))
        {
            patternRadius += 0.5f;
            Debug.Log("Increase Radius");
        }
        if (Input.GetKeyDown(decreaseRadiusKey))
        {
            patternRadius = Mathf.Max(0.5f, patternRadius - 0.5f);
            Debug.Log("Decrease Radius");
        }

        if (Input.GetMouseButton(0))
        {
            isMouseHeld = true;
            ModifyTerrain();
        }
        else if (isMouseHeld)
        {
            UpdateMeshCollider();
            isMouseHeld = false;
        }

        if (Input.GetKeyDown(nextPatternKey))
        {
            deformationCurve = deformationCurve.Equals(AnimationCurve.Linear(0, 0, 1, 1)) ? AnimationCurve.EaseInOut(0, 0, 1, 1) : AnimationCurve.Linear(0, 0, 1, 1);
            Debug.Log("Pattern Changed");
        }

        if (Input.GetKeyDown(toggleDistanceMethodKey))
        {
            currentDistanceMethod = (DistanceMethod)(((int)currentDistanceMethod + 1) % 3);
            Debug.Log("Distance Method Changed: " + currentDistanceMethod);
        }

        if (Input.GetKeyDown(toggleVertexApproximationKey))
        {
            useFirstVertexApproximation = !useFirstVertexApproximation;
            Debug.Log("Vertex Approximation: " + useFirstVertexApproximation);
        }

        if (Input.GetKeyDown(toggleNormalsUpdateKey))
        {
            updateOnlyModifiedNormals = !updateOnlyModifiedNormals;
            Debug.Log("Normals Update Mode: " + updateOnlyModifiedNormals);
        }

        if (Input.GetKeyDown(toggleGridSpaceKey))
        {
            useGridSpaceForNeighbors = !useGridSpaceForNeighbors;
            Debug.Log("Grid Space Neighbors: " + useGridSpaceForNeighbors);
        }
        if (Input.GetKeyDown(changerBrosse))
        {
            if (brushes.Length > 0)
            {
                currentBrushIndex = (currentBrushIndex + 1) % brushes.Length;
                Debug.Log("Brush Changed to index: " + currentBrushIndex);
            }
            else
            {
                Debug.LogWarning("No brushes available to change.");
            }
        }

        if (Input.GetKeyDown(KeyCode.F10))//Pour le menu F10 ----ce menu ne fonctionne pas encore----
        {
            OpenModal();
        }
    }

    private void OpenModal()//Open la fenetre quand on appuye sur F10
    {
        modalPanel.SetActive(true); 
        dimensionInputField.text = dimension.ToString(); 
        resolutionInputField.text = resolution.ToString(); 
    }

    private void EffacerTerrain() //Pour le F10
    {
        if (p_mesh != null)
        {
            p_mesh.Clear();
            p_meshFilter.mesh = null;
            p_meshCollider.sharedMesh = null;
        }

        p_vertices = null;
        p_triangles = null;
    }
    public void ApplyNewTerrainParameters()//Fonction pour Appliquer les parametres donnés par l'utilisateur dans le modal
    {//----Ne fonctionne pas----
        if (float.TryParse(dimensionInputField.text, out float newDimension) &&
            int.TryParse(resolutionInputField.text, out int newResolution) && newResolution > 0)
        {
            Debug.Log($"Applying new parameters: dimension={newDimension}, resolution={newResolution}");
            dimension = newDimension;
            resolution = newResolution;

            
            extensionTerrain.ClearTerrain();

            extensionTerrain.AddTerrain(Vector2Int.zero);

            EffacerTerrain();
            CréerTerrain(); 
            modalPanel.SetActive(false); 
        }
        else
        {
            Debug.LogWarning("Entrées non valides pour la dimension ou la résolution.");
        }
    }


    private void ModifyIntensity()//Modifie l'intensité 
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            intensity += 0.1f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightAlt))
        {
            intensity = Mathf.Max(0, intensity - 0.1f * Time.deltaTime);
        }
    }

    private void ModifyTerrain()//Modifie le Terrain avec l'aide de Raycast
    {
        if (!canDeform) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (p_meshCollider.Raycast(ray, out hit, Mathf.Infinity))
        {
            int nearestVertexIndex = useFirstVertexApproximation ? GetFirstVertex(hit.point) : GetNearestVertex(hit.point);
            ApliquerBrosseDeformation(nearestVertexIndex);
            UpdateMeshCollider();
        }
    }

    private int GetNearestVertex(Vector3 hitPoint)//Avoir le vertex le plus proche de l'endroit où l'on clique
    {
        int nearestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < p_vertices.Length; i++)
        {
            float distance = CalculateDistance(transform.TransformPoint(p_vertices[i]), hitPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }

    private int GetFirstVertex(Vector3 hitPoint)
    {
        return 0;
    }

    private float CalculateDistance(Vector3 a, Vector3 b)//Calcul la Distance selon la method 
    {
        switch (currentDistanceMethod)
        {
            case DistanceMethod.Manhattan:
                return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
            case DistanceMethod.Chebyshev:
                return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y), Mathf.Abs(a.z - b.z));
            default:
                return Vector3.Distance(a, b);
        }
    }

    private void ApliquerBrosseDeformation(int vertexIndex)//change de brosse et l'applique
    {
        Texture2D brush = brushes[currentBrushIndex];
        Vector3 worldPosition = transform.TransformPoint(p_vertices[vertexIndex]);

        float pixelWorldSize = (2 * patternRadius) / brush.width;

        for (int i = 0; i < p_vertices.Length; i++)
        {
            Vector3 vertexWorldPos = transform.TransformPoint(p_vertices[i]);
            float distance = Vector3.Distance(worldPosition, vertexWorldPos);

            if (distance < patternRadius)
            {
                Vector2 brushUV = CalculUVBrosse(worldPosition, vertexWorldPos, brush);
                float uvWidthPerVertex = (brush.width * pixelWorldSize) / (2 * patternRadius);
                float pixelIntensity = GetAverageIntensityInArea(brush, brushUV, uvWidthPerVertex);

                float deformationAmount = pixelIntensity * intensity * (isControlPressed ? -1 : 1) * deformationSpeed * Time.deltaTime;
                p_vertices[i].y += deformationAmount;
            }
        }

        p_mesh.vertices = p_vertices;
        p_mesh.RecalculateNormals();
        p_mesh.RecalculateBounds();
        p_meshFilter.mesh = p_mesh;
    }

    private Vector2 CalculUVBrosse(Vector3 center, Vector3 point, Texture2D brush)
    {
        Vector2 uv = new Vector2((point.x - center.x) / patternRadius + 0.5f,
                                 (point.z - center.z) / patternRadius + 0.5f);
        uv.x *= brush.width;
        uv.y *= brush.height;
        return uv;
    }

    private float GetAverageIntensityInArea(Texture2D brush, Vector2 uv, float uvWidthPerVertex)
    {
        int startX = Mathf.Clamp((int)(uv.x - uvWidthPerVertex / 2), 0, brush.width - 1);
        int startY = Mathf.Clamp((int)(uv.y - uvWidthPerVertex / 2), 0, brush.height - 1);
        int endX = Mathf.Clamp((int)(uv.x + uvWidthPerVertex / 2), 0, brush.width - 1);
        int endY = Mathf.Clamp((int)(uv.y + uvWidthPerVertex / 2), 0, brush.height - 1);

        float totalIntensity = 0f;
        int pixelCount = 0;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Color pixelColor = brush.GetPixel(x, y);
                float pixelIntensity = (pixelColor.r + pixelColor.g + pixelColor.b) / 3f;
                totalIntensity += pixelIntensity;
                pixelCount++;
            }
        }

        return (pixelCount > 0) ? totalIntensity / pixelCount : 0f;
    }

    //private void ApplyDeformation(int vertexIndex) //Cette fonction est ici car elle nous a aidé pour les 4 premiers exercices.
    //{
    //    Vector3 selectedVertexPosition = p_vertices[vertexIndex];

    //    Vector3 worldPosition = transform.TransformPoint(selectedVertexPosition);

    //    for (int i = 0; i < p_vertices.Length; i++)
    //    {
    //        Vector3 vertexWorldPosition = transform.TransformPoint(p_vertices[i]);

    //        float distance = Vector3.Distance(worldPosition, vertexWorldPosition);

    //        if (distance < patternRadius)
    //        {
    //            float attenuation = deformationCurve.Evaluate(distance / patternRadius);

    //            float deformationAmount = attenuation * intensity * (isControlPressed ? -1 : 1) * deformationSpeed * Time.deltaTime;

    //            p_vertices[i].y += deformationAmount;
    //        }
    //    }

    //    p_mesh.vertices = p_vertices;
    //    p_mesh.RecalculateNormals();
    //    p_mesh.RecalculateBounds();
    //    p_meshFilter.mesh = p_mesh;
    //}

    private void UpdateMeshCollider()
    {
        p_meshCollider.sharedMesh = null;
        p_meshCollider.sharedMesh = p_mesh;
    }

    void SetTriangles()//Set les triangles pour le terrain
    {
        int indice_triangle = 0;
        for (int j = 0; j < resolution - 1; j++)
        {
            for (int i = 0; i < resolution - 1; i++)
            {
                int vertexIndex = j * resolution + i;
                p_triangles[indice_triangle + 0] = vertexIndex;
                p_triangles[indice_triangle + 1] = vertexIndex + resolution;
                p_triangles[indice_triangle + 2] = vertexIndex + 1;
                indice_triangle += 3;
                p_triangles[indice_triangle + 0] = vertexIndex + 1;
                p_triangles[indice_triangle + 1] = vertexIndex + resolution;
                p_triangles[indice_triangle + 2] = vertexIndex + resolution + 1;
                indice_triangle += 3;
            }
        }
    }

    public void CréerTerrain()//Fonction principale qui crée le terrain
    {
        p_mesh = new Mesh();
        p_mesh.name = "ProceduralTerrainMESH";
        p_vertices = new Vector3[resolution * resolution];
        p_normals = new Vector3[p_vertices.Length];
        p_triangles = new int[3 * 2 * (resolution - 1) * (resolution - 1)];

        
        int indice_vertex = 0;
        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                p_vertices[indice_vertex] = GetVertexPosition(i, j);
                p_normals[indice_vertex] = Vector3.up;
                indice_vertex++;
            }
        }

        
        if (CentrerPivot)
        {
            Vector3 decalCentrage = new Vector3(dimension / 2, 0, dimension / 2);
            for (int k = 0; k < p_vertices.Length; k++)
                p_vertices[k] -= decalCentrage;
        }

        
        SetTriangles();
        p_mesh.vertices = p_vertices;
        p_mesh.normals = p_normals;
        p_mesh.triangles = p_triangles;
        p_meshFilter.mesh = p_mesh;
        p_meshCollider.sharedMesh = p_mesh;
    }


    private Vector3 GetVertexPosition(int i, int j)
    {
        float x = i * dimension / (resolution - 1);
        float z = j * dimension / (resolution - 1);
        float y = 0f;
        return new Vector3(x, y, z);
    }

    void ZoomTerrain()//Fonction pour le zoom ou le dézoom
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            float zoomSpeed = 2f; // Vitesse de zoom


            // Déplacer le terrain le long de l'axe de la caméra (Z)
            Vector3 zoomDirection = Camera.main.transform.position;
            transform.position += zoomDirection * Input.GetAxis("Vertical") * zoomSpeed * Time.deltaTime;

        }
    }
    void RotateTerrain()//Rotate le terrain 
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            float horizontalInput = Input.GetAxis("Horizontal");


            if (horizontalInput != 0 && extensionTerrain.GetTerrainChunks().Count > 1)// Si plusieurs Chunks
            {
                Vector3 pivotPoint = Vector3.zero;

                // Calculer le centre commun des chunks
                foreach (var chunk in extensionTerrain.GetTerrainChunks().Values)
                {
                    pivotPoint += chunk.transform.position;
                }
                pivotPoint /= extensionTerrain.GetTerrainChunks().Count;

                // Tourne tous les chunks autour du mémé point
                foreach (var chunk in extensionTerrain.GetTerrainChunks().Values)
                {
                    // Calcule du point central de tous les chunk
                    Vector3 directionToChunk = chunk.transform.position - pivotPoint;
                    directionToChunk = Quaternion.Euler(0, horizontalInput * rotationSpeed * Time.deltaTime, 0) * directionToChunk;
                    chunk.transform.position = pivotPoint + directionToChunk;

                    // Rotation autour de ce points
                    chunk.transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime, Space.World);
                }
            }
            else if (horizontalInput != 0)
            {
                transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
            }
        }
    }

    //Ici tous les Getter afin d'avoir toutes le variable nécessaire au menu F1 ou à l'extension de Terrain
    public string GetNameBrush()
    {
        return brushes[currentBrushIndex].ToString(); 
    }

    public float GetIntensity()
    {
        return intensity;
    }

    public float GetRadius()
    {
        return patternRadius;
    }

    public string GetCurve()
    {
        
        if (deformationCurve.Equals(AnimationCurve.Linear(0, 0, 1, 1)))
        {
            return "Linear";
        }
        else if (deformationCurve.Equals(AnimationCurve.EaseInOut(0, 0, 1, 1)))
        {
            return "EaseInOut";
        }
        else
        {
            return "Custom";
        }
    }

}
