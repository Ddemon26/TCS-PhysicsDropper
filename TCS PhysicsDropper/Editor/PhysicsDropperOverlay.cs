using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;
namespace TCS.StudioUtils {
    [Overlay(typeof(SceneView), null)]
    internal class PhysicsDropperOverlay : Overlay, ICreateToolbar {
        public override VisualElement CreatePanelContent() {
            //set the label for this panel to wrap the text
            return new PhysicsDropToolbarButton();
        }

        public override void OnCreated() {
            base.OnCreated();
            collapsedIcon = Resources.Load<Sprite>("d_ConstantForceRed").texture;
        }

        public override void OnWillBeDestroyed() {
            base.OnWillBeDestroyed();
            collapsedIcon = null;
        }

        public IEnumerable<string> toolbarElements { get { yield return PhysicsDropToolbarButton.ID; } }

        public enum StoppingCriteria {
            TimeAfterImpact,
            VelocityThreshold
        }

        class PhysicsDropper {
            public float TimeAfterImpact = 1.0f;
            public float VelocityThreshold = 0.001f;
            public float MaxSimulationTime = 10f;
            readonly List<PhysicsDropperObject> m_droppingObjects = new();
            bool m_isDropping;
            SimulationMode m_prevSimulationMode;
            public StoppingCriteria StoppingCriteria = StoppingCriteria.TimeAfterImpact;

            //public bool m_sendBool = false;
            public Action<bool> SendBool;
            public void DropSelectedObjects() {
                GameObject[] selectedObjects = Selection.gameObjects;

                if (selectedObjects.Length == 0) {
                    Debug.LogError("Physics Dropper: No GameObjects selected!");
                    return;
                }

                Undo.RegisterCompleteObjectUndo
                (
                    Array.ConvertAll
                    (
                        selectedObjects,
                        item => (Object)item
                    ), "Drop Objects"
                );

                m_droppingObjects.Clear();

                foreach (var obj in selectedObjects) {
                    if (!obj) {
                        Debug.LogError("Physics Dropper: One of the selected GameObjects is null!");
                        continue;
                    }

                    var collider = obj.GetComponent<Collider>();
                    bool hadCollider = collider;
                    var originalIsConvex = true;

                    if (!collider) {
                        collider = obj.AddComponent<MeshCollider>();
                        ((MeshCollider)collider).convex = true;
                    }
                    else if (collider is MeshCollider meshCollider) {
                        originalIsConvex = meshCollider.convex;
                        if (!meshCollider.convex) {
                            meshCollider.convex = true;
                        }
                    }

                    var rb = obj.GetComponent<Rigidbody>();
                    bool hadRigidbody = rb;
                    var originalCollisionDetectionMode = CollisionDetectionMode.Discrete;
                    var originalIsKinematic = true;
                    if (rb) {
                        originalCollisionDetectionMode = rb.collisionDetectionMode;
                        originalIsKinematic = rb.isKinematic;
                    }
                    else {
                        rb = obj.AddComponent<Rigidbody>();
                    }

                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rb.isKinematic = false;

                    var dropperComponent = obj.GetComponent<PhysicsDropperComponent>();
                    if (!dropperComponent) {
                        dropperComponent = obj.AddComponent<PhysicsDropperComponent>();
                    }

                    m_droppingObjects.Add
                    (
                        new PhysicsDropperObject {
                            GameObject = obj,
                            Rigidbody = rb,
                            HadRigidbody = hadRigidbody,
                            Collider = collider,
                            HadCollider = hadCollider,
                            DropperComponent = dropperComponent,
                            Timer = 0f,
                            HasLanded = false,
                            IsDroppingComplete = false,
                            OriginalCollisionDetectionMode = originalCollisionDetectionMode,
                            OriginalIsKinematic = originalIsKinematic,
                            OriginalIsConvex = originalIsConvex,
                            TotalSimulationTime = 0f
                        }
                    );
                }

                if (m_droppingObjects.Count > 0) {
                    m_prevSimulationMode = Physics.simulationMode;
                    Physics.simulationMode = SimulationMode.Script;

                    m_isDropping = true;
                    EditorApplication.update += UpdatePhysics;
                }
            }

            void UpdatePhysics() {
                if (!m_isDropping) {
                    EditorApplication.update -= UpdatePhysics;
                    return;
                }

                // Simulate physics step
                float simulationStep = Time.fixedDeltaTime;
                Physics.Simulate(simulationStep);

                // Refresh the scene view
                SceneView.RepaintAll();

                // Check if all objects have finished dropping
                var allDone = true;
                foreach (var obj in m_droppingObjects) {
                    if (obj.IsDroppingComplete) continue;

                    // Check if the GameObject or Rigidbody has been destroyed
                    if (!obj.GameObject || !obj.Rigidbody || !obj.DropperComponent) {
                        obj.IsDroppingComplete = true;
                        continue;
                    }

                    allDone = false; // At least one object is still dropping

                    obj.TotalSimulationTime += simulationStep;

                    if (obj.TotalSimulationTime >= MaxSimulationTime) {
                        obj.IsDroppingComplete = true;
                        continue;
                    }

                    if (StoppingCriteria == StoppingCriteria.TimeAfterImpact) {
                        if (!obj.HasLanded) {
                            if (obj.DropperComponent.m_hasCollided) {
                                obj.HasLanded = true;
                            }
                        }
                        else {
                            obj.Timer += simulationStep;
                            if (obj.Timer >= TimeAfterImpact) {
                                obj.IsDroppingComplete = true;
                            }
                        }
                    }
                    else if (StoppingCriteria == StoppingCriteria.VelocityThreshold) {
                        if (obj.Rigidbody.linearVelocity.magnitude <= VelocityThreshold) {
                            obj.IsDroppingComplete = true;
                        }
                    }
                }

                if (!allDone) return;
                StopDropping();
                SendBool?.Invoke(false);
                //Debug.Log("Physics Dropper: Drop complete.");
            }

            public void StopDropping() {
                Physics.simulationMode = m_prevSimulationMode;

                foreach (var obj in m_droppingObjects) {
                    if (obj.Rigidbody) {
                        obj.Rigidbody.isKinematic = obj.OriginalIsKinematic;
                        obj.Rigidbody.collisionDetectionMode = obj.OriginalCollisionDetectionMode;
                        if (!obj.HadRigidbody) {
                            Object.DestroyImmediate(obj.Rigidbody);
                        }
                    }

                    if (obj.Collider) {
                        if (obj.Collider is MeshCollider meshCollider) {
                            meshCollider.convex = obj.OriginalIsConvex;
                        }

                        if (!obj.HadCollider) {
                            Object.DestroyImmediate(obj.Collider);
                        }
                    }

                    if (obj.DropperComponent) {
                        Object.DestroyImmediate(obj.DropperComponent);
                    }
                }

                m_droppingObjects.Clear();
                m_isDropping = false;
                EditorApplication.update -= UpdatePhysics;
                //Debug.Log("Physics Dropper: Dropping stopped.");
            }
        }

        class PhysicsDropperWindow : PopupWindowContent {
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
                EditorGUI.LabelField(rect1, "Time After Impact");
                rect1.y += 18f;
                m_physicsDropper.TimeAfterImpact = EditorGUI.FloatField(rect1, m_physicsDropper.TimeAfterImpact);
                rect1.y += 18f;
                EditorGUI.LabelField(rect1, "Velocity Threshold");
                rect1.y += 18f;
                m_physicsDropper.VelocityThreshold = EditorGUI.FloatField(rect1, m_physicsDropper.VelocityThreshold);
                rect1.y += 18f;
                EditorGUI.LabelField(rect1, "Maximum Simulation Time");
                rect1.y += 18f;
                m_physicsDropper.MaxSimulationTime = EditorGUI.FloatField(rect1, m_physicsDropper.MaxSimulationTime);
                rect1.y += 18f;
            }
        }

        [EditorToolbarElement(ID, typeof(SceneView))]
        class PhysicsDropToolbarButton : EditorToolbarDropdownToggle, IAccessContainerWindow {
            public const string ID = "PhysicsDropToolbarButton";
            readonly PhysicsDropper m_physicsDropper = new();
            public EditorWindow containerWindow { get; set; }

            // Parameterless constructor
            public PhysicsDropToolbarButton() {
                icon = Resources.Load<Sprite>("d_ConstantForceRed").texture;
                name = "PhysicsDropToolbarButton";
                tooltip = L10n.Tr("Toggle skybox, fog, and various other effects.");
                dropdownClicked += () => PopupWindow.Show(worldBound, new PhysicsDropperWindow(m_physicsDropper));
                this.RegisterValueChangedCallback(evt => OnToggleChanged(evt.newValue));
                RegisterCallback(new EventCallback<AttachToPanelEvent>(OnAttachedToPanel));
                RegisterCallback(new EventCallback<DetachFromPanelEvent>(OnDetachFromPanel));

                m_physicsDropper.SendBool += OnToggleChanged;
            }
            void OnToggleChanged(bool evtNewValue) {
                value = evtNewValue;
                if (evtNewValue) {
                    m_physicsDropper.DropSelectedObjects();
                }
                else {
                    m_physicsDropper.StopDropping();
                }
            }
            void OnAttachedToPanel(AttachToPanelEvent evt) {
                Debug.Log("Element attached to panel");
            }
            void OnDetachFromPanel(DetachFromPanelEvent evt) {
                Debug.Log("Element detached from panel");
            }
        }

        class PhysicsDropperObject {
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
        }
    }
}