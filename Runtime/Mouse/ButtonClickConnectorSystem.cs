using Scellecs.Morpeh.Systems;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using Scellecs.Morpeh;
using Slimebones.ECSCore.Mouse;
using Slimebones.ECSCore;
using UnityEngine.UI;

/// <summary>
/// Connects mouse interactables with Unity UI Buttons OnClick events.
/// </summary>
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(ButtonClickConnectorSystem))]
public sealed class ButtonClickConnectorSystem : UpdateSystem {
    private Filter mouseInteractableF;

    public override void OnAwake() {
        mouseInteractableF = World
            .Filter.With<MouseInteractable>().With<ECSGameObject>().Build();

        // selects every mouse interactable and assigns it's mouse bridge to
        // it's button. If there is no button Unity component, just skip.

        foreach (var e in mouseInteractableF) {

            ref GameObject unityGO = ref ECSGameObjectUtils.GetUnityOrError(e);

            // get button and mouse bridge unity components and setup a
            // romantic date for them

            bool hasButton = unityGO.TryGetComponent(
                out Button button
            );
            bool hasMouseBridge = unityGO.TryGetComponent(
                out MouseBridge mouseBridge
            );

            if (!hasButton) {
                // for non-button entities, just skip
                continue;
            }
            if (!hasMouseBridge) {
                throw new MissingUnityComponentException<MouseBridge>(
                    unityGO
                );
            }

            button.onClick.AddListener(mouseBridge.OnMouseDown); 
        }
    }

    public override void OnUpdate(float deltaTime) {
    }
}