using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeplacerCapsule : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private MeshRenderer capsuleRenderer;

    private GameObject sphere;//tête du perso

    // Déplacements via ZQSD
    public float moveSpeedZQSD = 5f;
    private Vector3 moveDirection;

    // Référence au script ExtensionTerrain
    private ExtensionTerrain extensionTerrain;

    void Start()
    {
        targetPosition = transform.position;

        capsuleRenderer = GetComponent<MeshRenderer>();
        capsuleRenderer.enabled = false;

        sphere = transform.Find("Sphere")?.gameObject;

        if (sphere != null)
        {
            sphere.SetActive(false);
        }

        
        extensionTerrain = FindObjectOfType<ExtensionTerrain>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.Escape))
        {
            // Bascule la visibilité de la capsule et de la sphère
            bool isVisible = capsuleRenderer.enabled;
            capsuleRenderer.enabled = !isVisible;

            // Change la visibilité de la sphère
            if (sphere != null)
            {
                sphere.SetActive(!isVisible);
            }
        }

        if (capsuleRenderer.enabled)
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                SetTargetPosition();
            }

            
            HandleZQSDMovement();

            MoveCapsule();
        }
    }

    private void HandleZQSDMovement()//Déplacement avec ZQSD
    {
        
        moveDirection = Vector3.zero;

        
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W)) 
        {
            moveDirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S)) 
        {
            moveDirection += Vector3.back;
        }
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A)) 
        {
            moveDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D)) 
        {
            moveDirection += Vector3.right;
        }

        
        moveDirection = moveDirection.normalized;

        
        targetPosition += moveDirection * moveSpeedZQSD * Time.deltaTime;

        
        AdjustHeightToTerrain();
    }

    private void AdjustHeightToTerrain()//Ajuste le déplacement de la capsule en hauteur
    {
        if (extensionTerrain == null) return;

        
        Ray ray = new Ray(new Vector3(targetPosition.x, 100f, targetPosition.z), Vector3.down);
        RaycastHit hit;

        
        foreach (var chunk in extensionTerrain.GetTerrainChunks().Values)
        {
            MeshCollider terrainCollider = chunk.GetComponent<MeshCollider>();

            
            if (terrainCollider != null && terrainCollider.Raycast(ray, out hit, Mathf.Infinity))
            {
                
                targetPosition.y = hit.point.y + 1f;
                break; 
            }
        }
    }

    private void MoveCapsule()//Déplace la capsule
    {
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void SetTargetPosition()//Regarde à quel point on a cliqué
    {
        if (extensionTerrain == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        
        foreach (var chunk in extensionTerrain.GetTerrainChunks().Values)
        {
            MeshCollider terrainCollider = chunk.GetComponent<MeshCollider>();

            if (terrainCollider != null && terrainCollider.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 hitPoint = hit.point;
                hitPoint.y += 1;
                targetPosition = hitPoint;
                break; 
            }
        }
    }
}