﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TCS.PhysicsDropper {
    internal enum StoppingCriteria {
        TimeAfterImpact,
        VelocityThreshold,
    }

    internal sealed class PhysicsDropper {
        public float TimeAfterImpact = 1.0f;
        public float VelocityThreshold = 0.001f;
        public float MaxSimulationTime = 20f;
        public StoppingCriteria StoppingCriteria = StoppingCriteria.TimeAfterImpact;
        public Action<bool> SendFalseBool;

        readonly List<PhysicsDropperObject> m_droppingObjects = new();
        readonly List<RigidbodyState> m_nonSelectedRigidbodies = new();
        bool m_isDropping;
        SimulationMode m_prevSimulationMode;

        public void DropSelectedObjects() {
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0) {
                Logger.LogError("No GameObjects selected!");
                SendFalseBool?.Invoke(false);
                return;
            }

            // Check for at least one MeshRenderer in selected objects or their children
            var hasAtLeastOneMeshRenderer = false;
            foreach (var obj in selectedObjects) {
                
                if (!HasMeshRenderer(obj)) continue;
                hasAtLeastOneMeshRenderer = true;
                break;
            }

            if (!hasAtLeastOneMeshRenderer) {
                Logger.LogError("At least one selected GameObject or its children must have a MeshRenderer component.");
                return;
            }

            m_droppingObjects.Clear();
            m_nonSelectedRigidbodies.Clear();

            // Collect all rigidbodies in the scene that are not selected
            CollectNonSelectedRigidbodies(selectedObjects);

            // Disable non-selected rigidbodies
            DisableNonSelectedRigidbodies();

            foreach (var obj in selectedObjects) {
                if (!obj) {
                    Logger.LogError("One of the selected GameObjects is null!");
                    continue;
                }

                var transform = obj.transform;
                var originalPosition = transform.position;
                var originalRotation = transform.rotation;

                // Register Undo for the transform (captures position and rotation)
                Undo.RegisterCompleteObjectUndo(transform, "Drop Objects");

                bool hadRigidbody;
                var rb = obj.GetComponent<Rigidbody>();
                var originalCollisionDetectionMode = CollisionDetectionMode.Discrete;
                var originalIsKinematic = true;

                if (rb) {
                    hadRigidbody = true;
                    originalCollisionDetectionMode = rb.collisionDetectionMode;
                    originalIsKinematic = rb.isKinematic;
                }
                else {
                    hadRigidbody = false;
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

                // Prepare data for PhysicsDropperObject
                var physicsDropperObject = new PhysicsDropperObject {
                    GameObject = obj,
                    Rigidbody = rb,
                    HadRigidbody = hadRigidbody,
                    DropperComponent = dropperComponent,
                    Timer = 0f,
                    HasLanded = false,
                    IsDroppingComplete = false,
                    OriginalCollisionDetectionMode = originalCollisionDetectionMode,
                    OriginalIsKinematic = originalIsKinematic,
                    OriginalPosition = originalPosition,
                    OriginalRotation = originalRotation,
                    TotalSimulationTime = 0f,
                    ColliderDataList = new List<ColliderData>(),
                };

                SetupColliders(obj, physicsDropperObject);

                m_droppingObjects.Add(physicsDropperObject);
            }

            if (m_droppingObjects.Count <= 0) return;
            m_prevSimulationMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;

            m_isDropping = true;
            EditorApplication.update += UpdatePhysics;

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        void CollectNonSelectedRigidbodies(GameObject[] selectedObjects) {
            Rigidbody[] allRigidbodies = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            HashSet<GameObject> selectedSet = new(selectedObjects);

            foreach (var rb in allRigidbodies) {
                if (!rb || !rb.gameObject) continue;

                if (!selectedSet.Contains(rb.gameObject)) {
                    m_nonSelectedRigidbodies.Add
                    (
                        new RigidbodyState {
                            Rigidbody = rb,
                            OriginalIsKinematic = rb.isKinematic,
                        }
                    );
                }
            }
        }

        void DisableNonSelectedRigidbodies() {
            foreach (var rbState in m_nonSelectedRigidbodies) {
                rbState.Rigidbody.isKinematic = true;
            }
        }

        void RestoreNonSelectedRigidbodies() {
            foreach (var rbState in m_nonSelectedRigidbodies) {
                rbState.Rigidbody.isKinematic = rbState.OriginalIsKinematic;
            }

            m_nonSelectedRigidbodies.Clear();
        }

        static void SetupColliders(GameObject obj, PhysicsDropperObject physicsDropperObject) {
            if (obj.transform.childCount > 0) {
                // Object has children
                // Add colliders to all descendants
                foreach (var descendant in obj.GetComponentsInChildren<Transform>(includeInactive: true)) {
                    if (descendant == obj.transform) continue; // Skip the parent itself

                    var collider = descendant.GetComponent<Collider>();
                    bool hadCollider = collider;
                    var originalIsConvex = true;
                    var originalIsTrigger = false; // Initialize the variable

                    if (!collider) {
                        // Add MeshCollider without registering with Undo
                        collider = descendant.gameObject.AddComponent<MeshCollider>();
                        ((MeshCollider)collider).convex = true;
                        collider.isTrigger = false; // Ensure it's not a trigger
                    }
                    else {
                        originalIsTrigger = collider.isTrigger; // Store original isTrigger
                        if (originalIsTrigger) {
                            collider.isTrigger = false; // Set to false for simulation
                        }

                        if (collider is MeshCollider meshCollider) {
                            originalIsConvex = meshCollider.convex;
                            if (!meshCollider.convex) {
                                meshCollider.convex = true;
                            }
                        }
                    }

                    // Add collider data to ColliderDataList
                    physicsDropperObject.ColliderDataList.Add
                    (
                        new ColliderData {
                            GameObject = descendant.gameObject,
                            Collider = collider,
                            HadCollider = hadCollider,
                            OriginalIsConvex = originalIsConvex,
                            OriginalIsTrigger = originalIsTrigger, // Store the original isTrigger
                        }
                    );
                }
            }
            else {
                // Object has no children
                // Handle Collider for obj

                var collider = obj.GetComponent<Collider>();
                bool hadCollider = collider;
                var originalIsConvex = true;
                var originalIsTrigger = false; // Initialize the variable

                if (!collider) {
                    // Add MeshCollider without registering with Undo
                    collider = obj.AddComponent<MeshCollider>();
                    ((MeshCollider)collider).convex = true;
                    collider.isTrigger = false; // Ensure it's not a trigger
                }
                else {
                    originalIsTrigger = collider.isTrigger; // Store original isTrigger
                    if (originalIsTrigger) {
                        collider.isTrigger = false; // Set to false for simulation
                    }

                    if (collider is MeshCollider meshCollider) {
                        originalIsConvex = meshCollider.convex;
                        if (!meshCollider.convex) {
                            meshCollider.convex = true;
                        }
                    }
                }

                // Set collider data in physicsDropperObject
                physicsDropperObject.Collider = collider;
                physicsDropperObject.HadCollider = hadCollider;
                physicsDropperObject.OriginalIsConvex = originalIsConvex;
                physicsDropperObject.OriginalIsTrigger = originalIsTrigger; // Store the original isTrigger
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

                switch (StoppingCriteria) {
                    case StoppingCriteria.TimeAfterImpact when !obj.HasLanded:
                        if (obj.DropperComponent.m_hasCollided) {
                            obj.HasLanded = true;
                        }

                        break;
                    case StoppingCriteria.TimeAfterImpact:
                        obj.Timer += simulationStep;
                        if (obj.Timer >= TimeAfterImpact) {
                            obj.IsDroppingComplete = true;
                        }

                        break;
                    case StoppingCriteria.VelocityThreshold:
                        if (obj.Rigidbody.linearVelocity.magnitude <= VelocityThreshold) {
                            obj.IsDroppingComplete = true;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (!allDone) return;
            StopDropping();
            SendFalseBool?.Invoke(false);
        }

        void OnUndoRedoPerformed() {
            if (!m_isDropping) return;
            StopDropping();
            RestoreTransforms();
            SendFalseBool?.Invoke(false);
        }

        void RestoreTransforms() {
            foreach (var obj in m_droppingObjects) {
                if (!obj.GameObject) continue;
                var transform = obj.GameObject.transform;
                Undo.RecordObject(transform, "Restore Transform");
                transform.position = obj.OriginalPosition;
                transform.rotation = obj.OriginalRotation;

                // Optionally, reset Rigidbody velocities to prevent further movement
                if (!obj.Rigidbody) continue;
                obj.Rigidbody.linearVelocity = Vector3.zero;
                obj.Rigidbody.angularVelocity = Vector3.zero;
            }
        }

        public void StopDropping() {
            Physics.simulationMode = m_prevSimulationMode;

            // Restore non-selected rigidbodies
            RestoreNonSelectedRigidbodies();

            foreach (var obj in m_droppingObjects) {
                if (obj.Rigidbody) {
                    obj.Rigidbody.isKinematic = obj.OriginalIsKinematic;
                    obj.Rigidbody.collisionDetectionMode = obj.OriginalCollisionDetectionMode;

                    if (!obj.HadRigidbody) {
                        // Remove Rigidbody without registering with Undo
                        Object.DestroyImmediate(obj.Rigidbody);
                    }
                }

                if (obj.ColliderDataList is { Count: > 0 }) {
                    foreach (var colliderData in obj.ColliderDataList) {
                        if (colliderData.Collider is MeshCollider meshCollider) {
                            meshCollider.convex = colliderData.OriginalIsConvex;
                        }

                        // Restore original isTrigger value
                        colliderData.Collider.isTrigger = colliderData.OriginalIsTrigger;

                        if (!colliderData.HadCollider) {
                            // Remove Collider without registering with Undo
                            Object.DestroyImmediate(colliderData.Collider);
                        }
                    }
                }

                if (obj.Collider) {
                    if (obj.Collider is MeshCollider meshCollider) {
                        meshCollider.convex = obj.OriginalIsConvex;
                    }

                    // Restore original isTrigger value
                    obj.Collider.isTrigger = obj.OriginalIsTrigger;

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

        // Helper method to check for MeshRenderer
        static bool HasMeshRenderer(GameObject obj) {
            if (obj.TryGetComponent<MeshRenderer>(out _)) {
                return true;
            }

            foreach (var child in obj.GetComponentsInChildren<Transform>(true)) {
                if (child.TryGetComponent<MeshRenderer>(out _)) {
                    return true;
                }
            }

            return false;
        }
    }
}