using UnityEngine;

public interface iInteractable
{
    public Transform transform { get; }

    public Module.ModuleTypes GetInteractableType();
    public bool IsDisabled();
}
