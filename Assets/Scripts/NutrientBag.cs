using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutrientBag : AInteractable
{
    public GameObject bagObject;

    public override void Interact(LadybugController ladybugController, out bool destroyInteractable)
    {
        destroyInteractable = ladybugController.SetCarriedItem(bagObject, false);
    }
}
