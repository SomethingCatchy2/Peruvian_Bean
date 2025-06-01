using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class GlobalVolumeManager
{
    private static Volume globalVolume;
    private static VolumeProfile defaultProfileAsset;

    static GlobalVolumeManager()
    {
        Initialize();
    }

    private static void Initialize()
    {
        // Find the global volume in the scene
        globalVolume = Object.FindObjectOfType<Volume>();
        if (globalVolume != null && globalVolume.isGlobal)
        {
            defaultProfileAsset = globalVolume.profile;
        }
        else
        {
            Debug.LogWarning("No global Volume with 'isGlobal=true' found in scene. Default profile cannot be cached.");
        }
    }

    public static void SetProfile(string profileName)
    {
        if (globalVolume == null)
        {
            Initialize();
            if (globalVolume == null)
            {
                Debug.LogError("No global Volume found in scene");
                return;
            }
        }

        if (string.IsNullOrEmpty(profileName))
        {
            Debug.LogWarning("Profile name cannot be null or empty.");
            return;
        }

        if (profileName.ToLower() == "default")
        {
            if (defaultProfileAsset != null)
            {
                globalVolume.profile = defaultProfileAsset;
                Debug.Log("GlobalVolumeManager: Restored default profile asset.");
            }
            else
            {
                Debug.LogWarning("Default profile asset is null. Cannot restore.");
            }
        }
        else
        {
            VolumeProfile loadedProfile = Resources.Load<VolumeProfile>(profileName);
            if (loadedProfile != null)
            {
                globalVolume.profile = loadedProfile;
                Debug.Log($"GlobalVolumeManager: Switched to '{profileName}' profile asset.");
            }
            else
            {
                Debug.LogWarning($"VolumeProfile '{profileName}' not found in Resources folder.");
            }
        }
    }
}
