using System;
using System.Collections.Generic;
using TCS.StudioUtils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace TCS.PhysicsDropper {
    
    internal enum StoppingCriteria {
        TimeAfterImpact,
        VelocityThreshold
    }
    internal sealed class PhysicsDropper {
        public float TimeAfterImpact = 1.0f;
        public float VelocityThreshold = 0.001f;
        public float MaxSimulationTime = 60f;
        readonly List<PhysicsDropperObject> m_droppingObjects = new();
        bool m_isDropping;
        SimulationMode m_prevSimulationMode;
        public StoppingCriteria StoppingCriteria = StoppingCriteria.TimeAfterImpact;

        public Action<bool> SendFalseBool;

        public void DropSelectedObjects() {
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0) {
                Debug.LogError("Physics Dropper: No GameObjects selected!");
                return;
            }

            m_droppingObjects.Clear();

            // Iterate through each selected GameObject
            foreach (var obj in selectedObjects) {
                if (!obj) {
                    Debug.LogError("Physics Dropper: One of the selected GameObjects is null!");
                    continue;
                }

                var transform = obj.transform;
                var originalPosition = transform.position;
                var originalRotation = transform.rotation;

                // Register Undo for the transform (captures position and rotation)
                Undo.RegisterCompleteObjectUndo(transform, "Drop Objects");

                // Handle Collider
                var collider = obj.GetComponent<Collider>();
                bool hadCollider = collider != null;
                bool originalIsConvex = true;

                if (!collider) {
                    // Add MeshCollider without registering with Undo
                    collider = obj.AddComponent<MeshCollider>();
                    ((MeshCollider)collider).convex = true;
                }
                else if (collider is MeshCollider meshCollider) {
                    originalIsConvex = meshCollider.convex;
                    if (!meshCollider.convex) {
                        meshCollider.convex = true;
                    }
                }

                // Handle Rigidbody
                var rb = obj.GetComponent<Rigidbody>();
                bool hadRigidbody = rb != null;
                CollisionDetectionMode originalCollisionDetectionMode = CollisionDetectionMode.Discrete;
                bool originalIsKinematic = true;

                if (rb) {
                    originalCollisionDetectionMode = rb.collisionDetectionMode;
                    originalIsKinematic = rb.isKinematic;
                }
                else {
                    // Add Rigidbody without registering with Undo
                    rb = obj.AddComponent<Rigidbody>();
                }

                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.isKinematic = false;

                // Handle PhysicsDropperComponent
                var dropperComponent = obj.GetComponent<PhysicsDropperComponent>();
                if (!dropperComponent) {
                    // Add PhysicsDropperComponent without registering with Undo
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
                        OriginalPosition = originalPosition,
                        OriginalRotation = originalRotation,
                        TotalSimulationTime = 0f
                    }
                );
            }

            if (m_droppingObjects.Count > 0) {
                m_prevSimulationMode = Physics.simulationMode;
                Physics.simulationMode = SimulationMode.Script;

                m_isDropping = true;
                EditorApplication.update += UpdatePhysics;

                // Subscribe to Undo event
                Undo.undoRedoPerformed += OnUndoRedoPerformed;
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
            SendFalseBool?.Invoke(false);
        }

        void OnUndoRedoPerformed() {
            if (m_isDropping) {
                StopDropping();
                RestoreTransforms();
                SendFalseBool?.Invoke(false);
                Debug.Log("Physics Dropper: Undo operation detected, restored transforms.");
            }
        }

        void RestoreTransforms() {
            foreach (var obj in m_droppingObjects) {
                if (!obj.GameObject) continue;
                var transform = obj.GameObject.transform;
                Undo.RecordObject(transform, "Restore Transform");
                transform.position = obj.OriginalPosition;
                transform.rotation = obj.OriginalRotation;

                // Optionally, reset Rigidbody velocities to prevent further movement
                if (obj.Rigidbody) {
                    obj.Rigidbody.linearVelocity = Vector3.zero;
                    obj.Rigidbody.angularVelocity = Vector3.zero;
                }
            }
        }

        public void StopDropping() {
            Physics.simulationMode = m_prevSimulationMode;

            foreach (var obj in m_droppingObjects) {
                if (obj.Rigidbody) {
                    obj.Rigidbody.isKinematic = obj.OriginalIsKinematic;
                    obj.Rigidbody.collisionDetectionMode = obj.OriginalCollisionDetectionMode;

                    if (!obj.HadRigidbody) {
                        // Remove Rigidbody without registering with Undo
                        Object.DestroyImmediate(obj.Rigidbody);
                    }
                }

                if (obj.Collider) {
                    if (obj.Collider is MeshCollider meshCollider) {
                        meshCollider.convex = obj.OriginalIsConvex;
                    }

                    if (!obj.HadCollider) {
                        // Remove Collider without registering with Undo
                        Object.DestroyImmediate(obj.Collider);
                    }
                }

                if (obj.DropperComponent) {
                    // Remove PhysicsDropperComponent without registering with Undo
                    Object.DestroyImmediate(obj.DropperComponent);
                }
            }

            m_droppingObjects.Clear();
            m_isDropping = false;
            EditorApplication.update -= UpdatePhysics;

            // Unsubscribe from Undo event
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }
    }
}