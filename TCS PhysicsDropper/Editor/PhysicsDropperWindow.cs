using UnityEditor;
using UnityEngine;
namespace TCS.PhysicsDropper {
    internal sealed class PhysicsDropperWindow : PopupWindowContent {
        readonly PhysicsDropper m_physicsDropper;
        public PhysicsDropperWindow(PhysicsDropper physicsDropper) => m_physicsDropper = physicsDropper;
        public override Vector2 GetWindowSize() {
            return new Vector2(200, 200);
        }

        public override void OnGUI(Rect rect) {
            if (m_physicsDropper == null)
                return;
            if (Event.current.type == EventType.Layout)
                return;
            Draw(rect);
            if (Event.current.type == EventType.MouseMove)
                Event.current.Use();
            if (Event.current.type != EventType.KeyDown || Event.current.keyCode != KeyCode.Escape)
                return;
            editorWindow.Close();
            GUIUtility.ExitGUI();
        }

        void Draw(Rect rect) {
            if (m_physicsDropper == null)
                return;
            var rect1 = new Rect(1f, 1f, rect.width - 2f, 18f);
            EditorGUI.LabelField(rect1, "Stopping Criteria");
            rect1.y += 18f;
            m_physicsDropper.StoppingCriteria = (StoppingCriteria)EditorGUI.EnumPopup(rect1, m_physicsDropper.StoppingCriteria);
            rect1.y += 18f;

            if (m_physicsDropper.StoppingCriteria == StoppingCriteria.TimeAfterImpact) {
                EditorGUI.LabelField(rect1, "Time After Impact");
                rect1.y += 18f;
                m_physicsDropper.TimeAfterImpact = EditorGUI.FloatField(rect1, m_physicsDropper.TimeAfterImpact);
            }
            else if (m_physicsDropper.StoppingCriteria == StoppingCriteria.VelocityThreshold) {
                EditorGUI.LabelField(rect1, "Velocity Threshold");
                rect1.y += 18f;
                m_physicsDropper.VelocityThreshold = EditorGUI.FloatField(rect1, m_physicsDropper.VelocityThreshold);
            }

            rect1.y += 18f;
            EditorGUI.LabelField(rect1, "Maximum Simulation Time");
            rect1.y += 18f;
            m_physicsDropper.MaxSimulationTime = EditorGUI.FloatField(rect1, m_physicsDropper.MaxSimulationTime);
            rect1.y += 18f;
        }
    }
}