using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class GlobalVolumeManager
{
    private static Volume globalVolume;
    private static VolumeProfile defaultProfileAsset;
    private static VolumeProfile sylodasticProfileAsset;

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

        // Load the Sylodastic profile from Resources
        sylodasticProfileAsset = Resources.Load<VolumeProfile>("SylodasticProfile");
        if (sylodasticProfileAsset == null)
        {
            Debug.LogWarning("Sylodastic VolumeProfile not found in Resources folder");
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

        switch (profileName.ToLower())
        {
            case "sylodastic":
                if (sylodasticProfileAsset != null)
                {
                    globalVolume.profile = sylodasticProfileAsset;
                    Debug.Log("GlobalVolumeManager: Switched to Sylodastic profile asset.");
                }
                else
                {
                    Debug.LogWarning("Sylodastic profile asset is null.");
                }
                break;
            case "default":
                if (defaultProfileAsset != null)
                {
                    globalVolume.profile = defaultProfileAsset;
                    Debug.Log("GlobalVolumeManager: Restored default profile asset.");
                }
                else
                {
                    Debug.LogWarning("Default profile asset is null.");
                }
                break;
            default:
                Debug.LogWarning($"Unknown profile: {profileName}");
                break;
        }
    }
}
