using System.Collections.Generic;
using UnityEngine;
namespace TCS.PhysicsDropper {
    internal class ColliderData {
        public GameObject GameObject;
        public Collider Collider;
        public bool HadCollider;
        public bool OriginalIsConvex;
        public bool OriginalIsTrigger;
    }

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
        public bool OriginalIsTrigger;
        public Vector3 OriginalPosition;
        public Quaternion OriginalRotation;
        public float TotalSimulationTime;
        public List<ColliderData> ColliderDataList;
    }
}