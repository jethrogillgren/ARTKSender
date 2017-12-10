using UnityEngine;
using UnityEngine.VR;

namespace SKStudios.Portals
{
    
    public class PKProVRTracking : MonoBehaviour
    {
        public static Vector3 EyeOffset = Vector3.zero;

        public void Update()
        {
            EyeOffset = (Quaternion.Inverse(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head))
                              * Vector3.Scale(UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.RightEye) -
                                 UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.LeftEye), transform.lossyScale)) / 2f;
        }

    }


}
