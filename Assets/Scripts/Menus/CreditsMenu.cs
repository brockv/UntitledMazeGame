using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
    /**
     * Opens the asset store page for the FirstPersonAIO asset.
     */
    public void OpenFirstPersonLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/tools/input-management/first-person-all-in-one-135316");
    }

    /**
     * Opens the asset store page for the ENTITY music asset.
     */
    public void OpenEntityLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/audio/ambient/entity-129293#description");
    }

    /**
     * Opens the asset store page for the Flashlight 2.0 Tactical asset.
     */
    public void OpenFlashlightLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/3d/props/electronics/flashlight-2-0-tactical-42301#description");
    }

    /**
     * Opens the asset store page for the Dungeon Ground Texture asset.
     */
    public void OpenGroundTextureLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/floors/dungeon-ground-texture-33296");
    }

    /**
     * Opens the asset store page for the Tileable Bricks Wall asset.
     */
    public void OpenBrickWallLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/brick/tileable-bricks-wall-24530#reviews");
    }

    /**
     * Opens the asset store page for the Zombie model asset.
     */
    public void OpenZombieLink()
    {
        Application.OpenURL("https://assetstore.unity.com/packages/3d/characters/humanoids/zombie-30232");
    }
}
