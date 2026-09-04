using UnityEngine;
using UnityEngine.XR;

namespace InkRidge.Movement
{
    /// <summary>
    /// Drives the camera transform from the HMD pose via the legacy XR Input
    /// Devices API (InputDevices.GetDeviceAtXRNode). Used because the scene's
    /// TrackedPoseDriver has empty InputAction bindings (Input System only
    /// project, actions were never wired), leaving the camera frozen — the
    /// player sees a static flat view instead of head-tracked VR.
    ///
    /// Reads the head device pose directly each frame; no input actions
    /// required. Attach to the same GameObject as MainCamera (child of
    /// XROrigin's camera offset).
    /// </summary>
    public class HeadPoseDriver : MonoBehaviour
    {
        void Update()
        {
            var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (!device.isValid) return;

            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out var pos))
                transform.localPosition = pos;

            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out var rot))
                transform.localRotation = rot;
        }
    }
}
