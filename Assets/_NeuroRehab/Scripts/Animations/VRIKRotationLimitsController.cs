using UnityEngine;
using RootMotion.FinalIK;

public class VRIKRotationLimitsController : MonoBehaviour
{
    public VRIK ik;
    public RotationLimit[] rotationLimits;
    void Start()
    {
        // Disable all rotation limits initially to avoid conflicts with VRIK solving process
        foreach (RotationLimit limit in rotationLimits)
        {
            limit.enabled = false;
        }

        // Register the AfterVRIK method to be called after VRIK has updated
        ik.solver.OnPostUpdate += AfterVRIK;
    }

    private void OnDestroy()
    {
        ik.solver.OnPostUpdate -= AfterVRIK;
    }

    private void AfterVRIK()
    {
        // Apply each rotation limit after VRIK has solved
        foreach (RotationLimit limit in rotationLimits)
        {
            limit.Apply();
        }
    }
}
