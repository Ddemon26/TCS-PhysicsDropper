using System;
using UnityEditor;
using UnityEngine;
namespace TCS.PhysicsDropper {
    internal sealed class PhysicsDropperWindow : PopupWindowContent {
        readonly PhysicsDropper m_physicsDropper;
        public PhysicsDropperWindow(PhysicsDropper physicsDropper) => m_physicsDropper = physicsDropper;
        public override Vector2 GetWindowSize() {
            var height = 18f; // Initial height for the first label
            
            height += 18f; // Height for the Stopping Criteria dropdown
            
            height += 18f; // Height for the current showing label
            height += 18f; // Height for the current showing field

            height += 18f; // Height for the Maximum Simulation Time label
            height += 18f; // Height for the Maximum Simulation Time field

            return new Vector2(200, height + 9f);
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
            if (m_physicsDropper == null) return;

            var rect1 = new Rect(1f, 1f, rect.width - 2f, 18f);

            switch (m_physicsDropper.StoppingCriteria) {
                case StoppingCriteria.TimeAfterImpact:
                    EditorGUI.LabelField(rect1, "Time After Impact");
                    rect1.y += 18f;
                    m_physicsDropper.TimeAfterImpact = EditorGUI.Slider(rect1, m_physicsDropper.TimeAfterImpact, 0.001f, 60f);
                    break;
                case StoppingCriteria.VelocityThreshold:
                    EditorGUI.LabelField(rect1, "Velocity Threshold");
                    rect1.y += 18f;
                    m_physicsDropper.VelocityThreshold = EditorGUI.Slider(rect1, m_physicsDropper.VelocityThreshold, 0.001f, 0.1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            rect1.y += 18f;
            EditorGUI.LabelField(rect1, "Maximum Simulation Time");
            rect1.y += 18f;
            m_physicsDropper.MaxSimulationTime = EditorGUI.Slider(rect1, m_physicsDropper.MaxSimulationTime, 0.1f, 60f);
            rect1.y += 18f;
            EditorGUI.LabelField(rect1, "Stopping Criteria");
            rect1.y += 18f;
            m_physicsDropper.StoppingCriteria = (StoppingCriteria)EditorGUI.EnumPopup(rect1, m_physicsDropper.StoppingCriteria);
            rect1.y += 18f;
        }
    }
}