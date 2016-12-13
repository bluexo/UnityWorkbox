﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraParallax : MonoBehaviour {

    public Camera mainCamera;
    public Camera farCamera;
    public Camera nearCamera;

    public float mainDepth = 2f;
    public float parallaxDepth = 1f;

    void OnEnable()
    {
        InitCameras();
    }

    void LateUpdate()
    {
        UpdateCameras();
    }

    public void InitCameras()
    {
        if(farCamera != null)
        {
            farCamera.transform.localPosition = Vector3.zero;
            farCamera.transform.rotation = Quaternion.identity;
            farCamera.transform.localScale = Vector3.one;
            farCamera.orthographic = false;
            farCamera.clearFlags = CameraClearFlags.Depth;
            farCamera.depth = parallaxDepth;
            farCamera.transparencySortMode = TransparencySortMode.Orthographic;
        }

        if(mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.clearFlags = CameraClearFlags.Nothing;
            mainCamera.depth = mainDepth;
        }

        if(nearCamera != null)
        {
            nearCamera.transform.localPosition = Vector3.zero;
            nearCamera.transform.rotation = Quaternion.identity;
            nearCamera.transform.localScale = Vector3.one;
            nearCamera.orthographic = false;
            nearCamera.clearFlags = CameraClearFlags.Depth;
            nearCamera.transparencySortMode = TransparencySortMode.Orthographic;
            nearCamera.depth = 0;
        }
    }

    public void UpdateCameras()
    {
        if(mainCamera == null || farCamera == null || nearCamera == null) return;

        // orthoSize
        float a = mainCamera.orthographicSize;
        // distanceFromOrigin
        float b = Mathf.Abs(mainCamera.transform.position.z);

        //change clipping planes based on main camera z-position
        farCamera.nearClipPlane = b;
        farCamera.farClipPlane = mainCamera.farClipPlane;
        nearCamera.farClipPlane = b;
        nearCamera.nearClipPlane = mainCamera.nearClipPlane;

        //update field fo view for parallax cameras
        float fieldOfView = Mathf.Atan(a / b)  * Mathf.Rad2Deg * 2f;
        nearCamera.fieldOfView = farCamera.fieldOfView = fieldOfView;
    }
}
