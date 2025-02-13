using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera FlyCamera;
    public Camera FlyCamera2;

    private bool isFirstPersonActive = false;

    void Start()
    {
        FlyCamera.gameObject.SetActive(true); //active par default la caméra 3eme personne
        FlyCamera2.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            isFirstPersonActive = !isFirstPersonActive;//si on appui plusieurs fois sur f3 cela change tout le temps de caméra
            SwitchCamera();
        }
    }

    void SwitchCamera()//change la caméra active
    {
        if (isFirstPersonActive)
        {
            FlyCamera.gameObject.SetActive(false);
            FlyCamera2.gameObject.SetActive(true);
        }
        else
        {
            FlyCamera.gameObject.SetActive(true);
            FlyCamera2.gameObject.SetActive(false);
        }
    }
}
