using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtensionTerrain : MonoBehaviour
{
    public GameObject terrainPrefab;
    public Material TestMaterial;
    public Material originalMaterial;
    private Dictionary<Vector2Int, GameObject> terrainChunks = new Dictionary<Vector2Int, GameObject>();
    private bool canExtendTerrain = true; 

    public void ClearTerrain()
    {
        
        foreach (var chunk in terrainChunks.Values)
        {
            Destroy(chunk);
        }
        terrainChunks.Clear();
        canExtendTerrain = true; 
    }

    public Dictionary<Vector2Int, GameObject> GetTerrainChunks()
    {
        return terrainChunks;
    }

    void Start()
    {
        Vector2Int initialPosition = Vector2Int.zero;
        terrainChunks[initialPosition] = terrainPrefab;
    }

    void Update()
    {
        //Les coroutine permettront d'attendre 0,5 seconde entre chaque extension, pour ne pas se retrouver avec 10 chunk d'un coup
        if (Input.GetKey(KeyCode.F5) && canExtendTerrain)
        {
            Vector2Int vec = Vector2Int.zero;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                StartCoroutine(ExtendTerrainWithDelay(Vector2Int.up));
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                StartCoroutine(ExtendTerrainWithDelay(Vector2Int.down));
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                StartCoroutine(ExtendTerrainWithDelay(Vector2Int.left));
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                StartCoroutine(ExtendTerrainWithDelay(Vector2Int.right));
            }
        }
        if (Input.GetKey(KeyCode.F4))
        {


            StartCoroutine(HighlightNewChunk());
        }

    }



    public void AddTerrain(Vector2Int position)
    {
        if (!terrainChunks.ContainsKey(position))
        {
            Vector3 worldPosition = new Vector3(position.x * terrainPrefab.GetComponent<CreerTerrain>().dimension, 0, position.y * terrainPrefab.GetComponent<CreerTerrain>().dimension);
            GameObject newChunk = Instantiate(terrainPrefab, worldPosition, Quaternion.identity);
            terrainChunks[position] = newChunk;
        }
    }

    IEnumerator ExtendTerrainWithDelay(Vector2Int direction)
    {
        canExtendTerrain = false; // Bloque l'extension
        ExtendTerrain(direction); // Code principal
        yield return new WaitForSeconds(0.5f); // Attend 0,5 seconde avant de réactiver l'extension
        canExtendTerrain = true; // Réactive l'extension
    }
    void ExtendTerrain(Vector2Int direction)
    {
        List<Vector2Int> newChunkPositions = new List<Vector2Int>();

        foreach (Vector2Int chunkPosition in terrainChunks.Keys)
        {
            Vector2Int newPosition = chunkPosition + direction;

            if (!terrainChunks.ContainsKey(newPosition))
            {
                newChunkPositions.Add(newPosition);
            }
        }

        foreach (Vector2Int position in newChunkPositions)
        {
            AddTerrain(position);
        }
    }

    IEnumerator HighlightNewChunk()
    {

        foreach (var chunk in terrainChunks)
        {
            chunk.Value.GetComponent<Renderer>().material = TestMaterial; //change le matériaux
        }

        yield return new WaitForSeconds(3f);// attend 3 seconde

        foreach (var chunk in terrainChunks)
        {
            chunk.Value.GetComponent<Renderer>().material = originalMaterial; //remet le matériaux de base
        }
    }
}
