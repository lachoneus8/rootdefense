using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDroplet : AInteractable
{
    public GameObject dropletObject;

    public override void Interact(LadybugController ladybugController, out bool destroyInteractable)
    {
        destroyInteractable = ladybugController.SetCarriedItem(dropletObject, true);
    }
}
