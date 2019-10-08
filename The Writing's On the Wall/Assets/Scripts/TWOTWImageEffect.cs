using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TWOTWImageEffect : MonoBehaviour
{
    [SerializeField]
    private Shader imageEffectShader;
    
    private bool initialized = false;
    private RenderTexture colorTex;
    private RenderTexture objectDepthTex;
    private RenderTexture backgroundDepthTex;

    private Camera cam;
    private Camera Cam
    {
        get
        {
            if (!cam)
            {
                cam = GetComponent<Camera>();
            }
            return cam;
        }
    }


    private Material mat;
    private Material Mat
    {
        get
        {
            if (!mat)
            {
                mat = new Material(imageEffectShader);
            }
            return mat;
        }
    }


    public void Initialize(RenderTexture _colorTex, RenderTexture _objectDepthTex, RenderTexture _backgroundDepthTex, Matrix4x4 worldToView, Vector3 cameraPosition, Vector3 cameraForwardVector)
    {
        initialized = true;
        colorTex = _colorTex;
        objectDepthTex = _objectDepthTex;
        backgroundDepthTex = _backgroundDepthTex;
        Mat.SetTexture("_OverlayColorTex", colorTex);
        Mat.SetTexture("_OverlayObjectDepthTex", objectDepthTex);
        Mat.SetTexture("_OverlayBackgroundDepthTex", backgroundDepthTex);
        Mat.SetMatrix("_OldWorldToView", worldToView);
        Mat.SetVector("_OldCameraPosition", cameraPosition);
        Mat.SetVector("_OldCameraForwardVector", cameraForwardVector);
    }


    public void Uninitialize()
    {
        initialized = false;
        if (colorTex) { colorTex.Release(); }
        if (objectDepthTex) { objectDepthTex.Release(); }
        if (backgroundDepthTex) { backgroundDepthTex.Release(); }
    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!initialized)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Get the view frustum dimensions with a depth of 1
        float frustumHeight = 2 * Mathf.Tan(Cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * Cam.aspect;
        mat.SetFloat("_FrustumWidth", frustumWidth);
        mat.SetFloat("_FrustumHeight", frustumHeight);

        Graphics.Blit(source, destination, mat);
    }
}
