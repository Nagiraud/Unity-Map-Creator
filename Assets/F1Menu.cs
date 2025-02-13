using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class F1Menu : MonoBehaviour
{
    public GameObject modalWindow;
    bool IsActive = true;

    private ExtensionTerrain extTerr;

    public TMP_Text nbTriangles;
    public TMP_Text nb_Vertices;
    public TMP_Text nb_Chunks;

    public TMP_Text NamePattern;
    public TMP_Text Intensity;
    public TMP_Text Radius;

    public TMP_Text nameCurve;

    public CreerTerrain TerrainPrefab;

    
    void Start()
    {
        extTerr = FindObjectOfType<ExtensionTerrain>();
        UpdateTerrainInfo();
    }


    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            IsActive = !IsActive;
            modalWindow.SetActive(IsActive);
            if (IsActive)
            {
                UpdateTerrainInfo();
            }
        }
    }

    private void UpdateTerrainInfo()//à chaque appel dans Update() cette methode Update les infos du Terrain
    {
        if (extTerr == null) return;

        int totalTriangles = 0;
        int totalVertices = 0;
        int totalChunks = extTerr.GetTerrainChunks().Count;

        foreach (GameObject chunk in extTerr.GetTerrainChunks().Values)
        {
            MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                totalTriangles += meshFilter.sharedMesh.triangles.Length / 3;
                totalVertices += meshFilter.sharedMesh.vertexCount;
            }
        }

        nbTriangles.text = totalTriangles.ToString();
        nb_Vertices.text = totalVertices.ToString();
        nb_Chunks.text = totalChunks.ToString();
        NamePattern.text = TerrainPrefab.GetNameBrush();
        Intensity.text = TerrainPrefab.GetIntensity().ToString();
        Radius.text = TerrainPrefab.GetRadius().ToString();
        nameCurve.text = TerrainPrefab.GetCurve();
    }
}