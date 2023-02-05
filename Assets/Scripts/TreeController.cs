using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    public GameController gameController;

    public List<RootParent> rootParents;

    public float waterDrain;
    public float nutrientsDrain;
    public float healthDrainWhenEmpty;
    public float healthRecoveryWhenFull;

    public float waterAppliedPerItem;
    public float nutrientsAppliedPerItem;

    public Canvas canvas;

    public MeterBehaviour waterMeter;
    public MeterBehaviour nutrientsMeter;
    public MeterBehaviour healthMeter;

    private float curWater = 50f;
    private float curNutrients = 50f;
    private float curHealth = 100f;

    public float GetHealth()
    {
        return curHealth;
    }

    public RootParent GetClosestRootParent(GameObject gameObject, float maxDistanceToCheckRoots)
    {
        RootParent closestRootParent = null;
        float closestDistance = float.MaxValue;

        foreach (var rootParent in rootParents)
        {
            var diff = gameObject.transform.position - rootParent.GetCenterPos();
            var dist = diff.magnitude;
            if (dist > maxDistanceToCheckRoots)
            {
                continue;
            }

            if (dist < closestDistance)
            {
                closestRootParent = rootParent;
                closestDistance = dist;
            }
        }

        return closestRootParent;
    }

    public void DoAttack(float attackDamage)
    {
        curHealth -= attackDamage;
    }

    void Start()
    {
        waterMeter.Init(canvas.scaleFactor);
        nutrientsMeter.Init(canvas.scaleFactor);
        healthMeter.Init(canvas.scaleFactor);
    }

    void Update()
    {
        if (gameController.IsGameOver())
        {
            return;
        }

        curWater -= waterDrain * Time.deltaTime;
        curWater = Mathf.Clamp(curWater, 0f, 100f);
        curNutrients -= nutrientsDrain * Time.deltaTime;
        curNutrients = Mathf.Clamp(curNutrients, 0f, 100f);

        if (curWater == 100f)
        {
            curHealth += healthRecoveryWhenFull * Time.deltaTime;
        }

        if (curNutrients == 100f)
        {
            curHealth += healthRecoveryWhenFull * Time.deltaTime;
        }

        if (curWater == 0f)
        {
            curHealth -= healthDrainWhenEmpty * Time.deltaTime;
        }

        if (curNutrients == 0f)
        {
            curHealth -= healthDrainWhenEmpty * Time.deltaTime;
        }

        curHealth = Mathf.Clamp(curHealth, 0f, 100f);

        waterMeter.SetValue(curWater);
        nutrientsMeter.SetValue(curNutrients);
        healthMeter.SetValue(curHealth);
    }

    public void ApplyWater()
    {
        curWater += waterAppliedPerItem;
    }

    public void ApplyNutrients()
    {
        curNutrients += nutrientsAppliedPerItem;
    }
}
