using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace InkRidge.Movement
{
    /// <summary>
    /// XR Grab interactable for climbing. When player grabs this,
    /// the XR Origin moves opposite to the hand pull direction.
    /// Uses XRIT's built-in climb provider.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ClimbInteractable : MonoBehaviour
    {
        private XRGrabInteractable _grabInteractable;

        void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            _grabInteractable.trackRotation = false;
            _grabInteractable.transform.SetParent(transform);
        }
    }
}
