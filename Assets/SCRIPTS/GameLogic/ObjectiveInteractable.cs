using Unity.Netcode;
using UnityEngine;

public class ObjectiveInteractable : NetworkBehaviour, iInteractable
{
    public Module.ModuleTypes GetInteractableType()
    {
        return Module.ModuleTypes.VAULT;
    }
    public bool IsDisabled()
    {
        return false;
    }
}
