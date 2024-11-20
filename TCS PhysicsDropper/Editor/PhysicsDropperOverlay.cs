using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.PhysicsDropper {
    [Overlay(typeof(SceneView), null)]
    internal sealed class PhysicsDropperOverlay : Overlay, ICreateToolbar {
        public override VisualElement CreatePanelContent() => new PhysicsDropToolbarButton();

        public override void OnCreated() {
            base.OnCreated();
            var sprite = Resources.Load<Texture2D>("D_ConstantForceRed");
            if (sprite) {
                collapsedIcon = sprite;
            } else {
                Debug.LogError("Sprite 'D_ConstantForceRed' not found in Resources.");
            }
        }

        public override void OnWillBeDestroyed() {
            base.OnWillBeDestroyed();
            collapsedIcon = null;
        }

        public IEnumerable<string> toolbarElements { get { yield return PhysicsDropToolbarButton.ID; } }
    }
}