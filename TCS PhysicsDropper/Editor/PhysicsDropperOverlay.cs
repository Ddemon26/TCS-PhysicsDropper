using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.PhysicsDropper {
    [Overlay(typeof(SceneView), null)]
    internal sealed class PhysicsDropperOverlay : Overlay, ICreateToolbar {
        public override VisualElement CreatePanelContent() => new PhysicsDropToolbarButton();

        static readonly Texture2D PreloadedIcon = Resources.Load<Texture2D>("D_ConstantForceRed");

        public override void OnCreated() {
            base.OnCreated();
            var sprite = PreloadedIcon ? PreloadedIcon : DebugIconFallback();
            if (sprite) {
                collapsedIcon = sprite;
            }
            else {
                Logger.LogError("Sprite 'D_ConstantForceRed' not found in Resources.");
            }
        }

        public override void OnWillBeDestroyed() {
            base.OnWillBeDestroyed();
            collapsedIcon = null;
        }

        public IEnumerable<string> toolbarElements { get { yield return PhysicsDropToolbarButton.ID; } }

        Texture2D DebugIconFallback() {
            Logger.Log("Refresh Scene View to load the icon.");
            return EditorGUIUtility.IconContent("BuildSettings.Broadcom").image as Texture2D;
        }
    }
}