using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootParent : MonoBehaviour
{
    private Vector3 centerPos = Vector3.zero;

    public GameObject GetClosestRoot(GameObject objectToCheck)
    {
        GameObject closestRoot = null;
        float closestDist = float.MaxValue;

        for (int ch = 0; ch < transform.childCount; ++ch)
        {
            var child = transform.GetChild(ch).gameObject;

            var diff = child.transform.position - objectToCheck.transform.position;
            var dist = diff.magnitude;

            if (dist < closestDist)
            {
                closestRoot = child;
                closestDist = dist;
            }
        }

        return closestRoot;
    }

    public Vector3 GetCenterPos()
    {
        return centerPos;
    }

    private void Start()
    {
        Bounds bounds = new Bounds();
        bool boundsInit = false;

        for (int ch = 0; ch < transform.childCount; ++ch)
        {
            var child = transform.GetChild(ch).gameObject;

            if (!boundsInit)
            {
                boundsInit = true;
                bounds = new Bounds(child.transform.position, Vector3.zero);
            }
            else
            {
                bounds.Encapsulate(child.transform.position);
            }
        }

        centerPos = bounds.center;
    }
}
