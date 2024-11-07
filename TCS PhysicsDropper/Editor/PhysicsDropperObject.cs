using TCS.StudioUtils;
using UnityEngine;
namespace TCS.PhysicsDropper {
    internal sealed class PhysicsDropperObject {
        public GameObject GameObject;
        public Rigidbody Rigidbody;
        public bool HadRigidbody;
        public Collider Collider;
        public bool HadCollider;
        public PhysicsDropperComponent DropperComponent;
        public float Timer;
        public bool HasLanded;
        public bool IsDroppingComplete;
        public CollisionDetectionMode OriginalCollisionDetectionMode;
        public bool OriginalIsKinematic;
        public bool OriginalIsConvex;
        public float TotalSimulationTime;
        public Vector3 OriginalPosition;
        public Quaternion OriginalRotation;
    }
}