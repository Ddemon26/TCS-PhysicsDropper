using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.PhysicsDropper {
    [Overlay(typeof(SceneView), null)]
    internal sealed class PhysicsDropperOverlay : Overlay, ICreateToolbar {
        public override VisualElement CreatePanelContent() => new PhysicsDropToolbarButton();

        static readonly Texture2D PreloadedIcon = EditorGUIUtility.IconContent("ConstantForce Icon").image as Texture2D;

        public override void OnCreated() {
            base.OnCreated();
            displayName = "Physics Dropper";
            rootVisualElement.viewDataKey = "PhysicsDropperOverlay";
            var sprite = PreloadedIcon;
            if (sprite) {
                collapsedIcon = sprite;
            } else {
                Logger.LogError("Sprite 'ConstantForceRed' not found.");
            }
        }

        public override void OnWillBeDestroyed() {
            base.OnWillBeDestroyed();
            collapsedIcon = null;
        }

        public IEnumerable<string> toolbarElements { get { yield return PhysicsDropToolbarButton.ID; } }
    }
}