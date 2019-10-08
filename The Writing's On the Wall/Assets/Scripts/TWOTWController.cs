using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TWOTWController : MonoBehaviour
{
    [SerializeField]
    private string temporaryLayer;

    [SerializeField]
    private GameObject objectToFlatten;
    [SerializeField]
    private PlayerMovement playerMovement;

    private bool objectFlattened = false;

    bool resetInProgress = false;


    void Update()
    {
        if (resetInProgress) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!objectFlattened)
            {
                playerMovement.StoreState();
                FlattenObject(objectToFlatten);
            }
            else
            {
                StartCoroutine(ResetCoroutine());
            }
        }
    }


    private void FlattenObject(GameObject objectToFlatten)
    {
        Camera mainCamera = Camera.main;

        // Render an image of only the object and store it in targetTexture
        Camera tempCam = new GameObject().AddComponent<Camera>();
        tempCam.CopyFrom(mainCamera);

        // Render an image of the object to be flattened
        RenderTexture colorTex = new RenderTexture(tempCam.pixelWidth, tempCam.pixelHeight, 0);
        tempCam.targetTexture = colorTex;
        tempCam.clearFlags = CameraClearFlags.Nothing;
        tempCam.cullingMask = 1 << LayerMask.NameToLayer(temporaryLayer);
        objectToFlatten.layer = LayerMask.NameToLayer(temporaryLayer);
        tempCam.Render();

        // Render a depth texture of the object to be flattened
        RenderTexture objectDepthTex = new RenderTexture(tempCam.pixelWidth, tempCam.pixelHeight, 16, RenderTextureFormat.Depth);
        tempCam.targetTexture = objectDepthTex;
        tempCam.clearFlags = CameraClearFlags.Depth;
        tempCam.depthTextureMode = DepthTextureMode.Depth;
        tempCam.Render();

        // Render a depth texture of the object's surroundings
        RenderTexture backgroundDepthTex = new RenderTexture(tempCam.pixelWidth, tempCam.pixelHeight, 16, RenderTextureFormat.Depth);
        tempCam.targetTexture = backgroundDepthTex;
        tempCam.cullingMask = mainCamera.cullingMask & ~(1 << LayerMask.NameToLayer(temporaryLayer)); // ~ is bitwise NOT
        tempCam.Render();

        tempCam.targetTexture = null;
        Destroy(tempCam.gameObject);
        objectToFlatten.SetActive(false);

        mainCamera.GetComponent<TWOTWImageEffect>().Initialize(colorTex, objectDepthTex, backgroundDepthTex, mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix, mainCamera.transform.position, mainCamera.transform.rotation * Vector3.forward);

        objectFlattened = true;
    }

    private IEnumerator ResetCoroutine()
    {
        resetInProgress = true;

        playerMovement.MoveToStoredState(0.5f);
        yield return new WaitForSeconds(0.5f);

        objectFlattened = false;
        Camera.main.GetComponent<TWOTWImageEffect>().Uninitialize();
        objectToFlatten.SetActive(true);

        resetInProgress = false;
    }
}
