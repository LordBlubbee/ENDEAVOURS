using UnityEngine;

public class ModuleDoorControls : Module
{
    public float GetDoorBuff()
    {
        return 150f + ModuleLevel.Value * 150;
    }

    bool isBuffActive = false;
    protected override void ActivateModule()
    {
        //
        if (!isBuffActive)
        {
            isBuffActive = true;
            foreach (Module mod in Space.GetModules())
            {
                if (mod is DoorSystem)
                {
                    mod.ExtraMaxHealth.Value += GetDoorBuff();
                    mod.Heal(GetDoorBuff());
                }
            }
        }
    }
    protected override void DeactivateModule()
    {
        //
        if (isBuffActive)
        {
            isBuffActive = false;
            foreach (Module mod in Space.GetModules())
            {
                if (mod is DoorSystem)
                {
                    mod.ExtraMaxHealth.Value -= GetDoorBuff();
                }
            }
        }
       
    }
}
