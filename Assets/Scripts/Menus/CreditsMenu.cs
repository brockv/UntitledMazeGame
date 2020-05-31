using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
    public void OpenFirstPersonLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/tools/input-management/first-person-all-in-one-135316");
    }

    public void OpenEntityLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/audio/ambient/entity-129293#description");
    }

    public void OpenFlashlightLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/3d/props/electronics/flashlight-2-0-tactical-42301#description");
    }

    public void OpenGroundTextureLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/floors/dungeon-ground-texture-33296");
    }

    public void OpenBrickWallLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/brick/tileable-bricks-wall-24530#reviews");
    }

    public void OpenZombieLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/3d/characters/humanoids/zombie-30232");
    }
}
