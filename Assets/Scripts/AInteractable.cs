using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AInteractable : MonoBehaviour
{
    public GameController gameController;

    public abstract void Interact(LadybugController ladybugController, out bool destroyInteractable);
}
