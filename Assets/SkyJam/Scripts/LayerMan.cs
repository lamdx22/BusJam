using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class LayerMan
    {
        public static readonly int QueueMask = LayerMask.GetMask("Queue");
        public static readonly int PickableMask = LayerMask.GetMask("Vehicle", "Stone");
        public static readonly int VehicleMask = LayerMask.GetMask("Vehicle");
        public static readonly int EditorPickMask = LayerMask.GetMask("Queue", "Vehicle", "Stone");

        public static readonly int DefaultLayer = LayerMask.NameToLayer("Default");
        public static readonly int NonCollideLayer = LayerMask.NameToLayer("NonCollide");
    }

    public class AnimatorHash
    {
        public static readonly int getOutTrigger = Animator.StringToHash("getOut");
    }
}
