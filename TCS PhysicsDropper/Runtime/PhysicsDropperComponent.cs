using UnityEngine;
namespace TCS.StudioUtils {
    [ExecuteAlways]
    public class PhysicsDropperComponent : MonoBehaviour {
        public bool m_hasCollided = false;

        void OnCollisionEnter(Collision collision) {
            m_hasCollided = true;
        }
    }
}