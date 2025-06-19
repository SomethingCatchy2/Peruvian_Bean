using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public static class GlobalVolumeManager
{
    private static Volume globalVolume;
    private static VolumeProfile defaultProfileAsset;
    private static VolumeProfile sylodasticProfileAsset;

    // For blending
    private static Volume blendVolume;
    private static Coroutine currentBlendCoroutine;
    private static GameObject blendVolumeGO;
    private static float blendDuration = 1.0f; // seconds

    // For temporary profile switching
    private static Coroutine tempProfileCoroutine;
    // Remove lastProfileName, use a bool instead
    private static bool isSylodasticActive = false;
    private static float tempProfileEndTime = 0f;

    static GlobalVolumeManager()
    {
        Initialize();
    }

    private static void Initialize()
    {
        // Find the global volume in the scene
        globalVolume = Object.FindFirstObjectByType<Volume>();
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

        // Find or create the blend volume (hidden in hierarchy)
        if (blendVolumeGO == null)
        {
            blendVolumeGO = GameObject.Find("GlobalVolumeBlendHelper");
            if (blendVolumeGO == null)
            {
                blendVolumeGO = new GameObject("GlobalVolumeBlendHelper");
                blendVolumeGO.hideFlags = HideFlags.HideAndDontSave;
                blendVolume = blendVolumeGO.AddComponent<Volume>();
                blendVolume.isGlobal = true;
                blendVolume.priority = 1001; // Higher than main global volume
                blendVolume.weight = 0f;
                blendVolume.enabled = false;
            }
            else
            {
                blendVolume = blendVolumeGO.GetComponent<Volume>();
            }
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

        VolumeProfile targetProfile = null;
        switch (profileName.ToLower())
        {
            case "sylodastic":
                targetProfile = sylodasticProfileAsset;
                break;
            case "default":
                targetProfile = defaultProfileAsset;
                break;
            default:
                Debug.LogWarning($"Unknown profile: {profileName}");
                return;
        }

        if (targetProfile == null)
        {
            Debug.LogWarning($"{profileName} profile asset is null.");
            return;
        }

        // Start smooth transition
        if (globalVolume.profile == targetProfile)
        {
            Debug.Log("GlobalVolumeManager: Already using target profile.");
            return;
        }

        // If running in play mode, use coroutine for smooth blend
        if (Application.isPlaying)
        {
            MonoBehaviour runner = GetRunner();
            if (runner != null)
            {
                if (currentBlendCoroutine != null)
                {
                    runner.StopCoroutine(currentBlendCoroutine);
                }
                currentBlendCoroutine = runner.StartCoroutine(BlendToProfile(targetProfile, blendDuration));
            }
            else
            {
                // Fallback: instant switch
                globalVolume.profile = targetProfile;
            }
        }
        else
        {
            // In edit mode, just switch instantly
            globalVolume.profile = targetProfile;
        }
    }

    // Helper to get a MonoBehaviour to run coroutines
    private static MonoBehaviour GetRunner()
    {
        // Try to find any active MonoBehaviour in the scene
        var go = Object.FindAnyObjectByType<MonoBehaviour>();
        return go;
    }

    private static IEnumerator BlendToProfile(VolumeProfile targetProfile, float duration)
    {
        if (blendVolume == null)
            Initialize();

        // Setup blend volume
        blendVolume.profile = targetProfile;
        blendVolume.weight = 0f;
        blendVolume.enabled = true;

        float startTime = Time.time;
        float endTime = startTime + duration;

        // Store original profile for reference
        VolumeProfile originalProfile = globalVolume.profile;

        // Blend weight from 0 to 1
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            blendVolume.weight = t;
            globalVolume.weight = 1f - t;
            yield return null;
        }

        // Finish blend
        blendVolume.weight = 0f;
        blendVolume.enabled = false;
        globalVolume.weight = 1f;
        globalVolume.profile = targetProfile;
        Debug.Log("GlobalVolumeManager: Smooth transition complete.");
    }

    public static void SetProfileTemporary(string profileName, float duration)
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

        MonoBehaviour runner = GetRunner();
        if (runner == null)
        {
            Debug.LogError("No MonoBehaviour found to run coroutine for temporary profile switch.");
            return;
        }

        // Only allow Sylodastic as a temporary profile for now
        bool isSylodasticRequest = profileName.ToLower() == "sylodastic";

        // If already running Sylodastic, extend the timer
        if (isSylodasticRequest && isSylodasticActive)
        {
            tempProfileEndTime = Mathf.Max(tempProfileEndTime, Time.time + duration);
            Debug.Log($"GlobalVolumeManager: Extended temporary profile 'Sylodastic' for {duration} more seconds.");
            return;
        }

        // If a temp profile coroutine is running, stop it (to reset timer and logic)
        if (tempProfileCoroutine != null)
            runner.StopCoroutine(tempProfileCoroutine);

        // Set state and start coroutine
        isSylodasticActive = isSylodasticRequest;
        tempProfileEndTime = Time.time + duration;
        tempProfileCoroutine = runner.StartCoroutine(TempProfileRoutine(profileName, duration));
    }

    private static IEnumerator TempProfileRoutine(string profileName, float duration)
    {
        // Only handle Sylodastic as a temp profile
        SetProfile(profileName);
        Debug.Log($"GlobalVolumeManager: Switched to '{profileName}' profile for {duration} seconds.");

        // Wait until timer expires (can be extended)
        while (Time.time < tempProfileEndTime)
        {
            yield return null;
        }

        // Always revert to default after Sylodastic
        if (profileName.ToLower() == "sylodastic")
        {
            Debug.Log($"GlobalVolumeManager: Reverting to 'default' profile.");
            SetProfile("default");
            isSylodasticActive = false;
        }
        tempProfileCoroutine = null;
    }
}
