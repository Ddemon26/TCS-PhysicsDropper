using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;
namespace TCS.PhysicsDropper {
    [EditorToolbarElement(ID, typeof(SceneView))]
    internal sealed class PhysicsDropToolbarButton : EditorToolbarDropdownToggle, IAccessContainerWindow {
        public const string ID = "PhysicsDropToolbarButton";
        readonly PhysicsDropper m_physicsDropper = new();
        public EditorWindow containerWindow { get; set; }

        public PhysicsDropToolbarButton() {
            EditorApplication.delayCall += () => {
                icon = Resources.Load<Texture2D>("D_ConstantForceRed");
                if (!icon) {
                    Debug.LogError("Sprite 'D_ConstantForceRed' not found in Resources.");
                }
            };
            name = "PhysicsDropToolbarButton";
            tooltip = L10n.Tr("Physics Dropper");
            dropdownClicked += () => PopupWindow.Show(worldBound, new PhysicsDropperWindow(m_physicsDropper));
            this.RegisterValueChangedCallback(evt => OnToggleChanged(evt.newValue));
            RegisterCallback(new EventCallback<AttachToPanelEvent>(OnAttachedToPanel));
            RegisterCallback(new EventCallback<DetachFromPanelEvent>(OnDetachFromPanel));

            m_physicsDropper.SendFalseBool += OnToggleChanged;
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
            //NO-OP
            //Debug.Log("Element attached to panel");
        }
        void OnDetachFromPanel(DetachFromPanelEvent evt) {
            //NO-OP
            //Debug.Log("Element detached from panel");
        }
    }
}