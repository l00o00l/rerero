using UnityEngine;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseDragDropTarget : MonoBehaviour
    {
        public ReleaseDropTargetDescriptor Descriptor { get; private set; }

        public void Configure(ReleaseDropTargetKind targetKind, int slotId)
        {
            Descriptor = new ReleaseDropTargetDescriptor(targetKind, slotId);
        }
    }
}
