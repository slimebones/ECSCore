using Scellecs.Morpeh;
using Slimebones.ECSCore.Object;
using Slimebones.ECSCore.UI.Canvas;
using Slimebones.ECSCore.UI.Panel;

namespace Slimebones.ECSCore.UI
{
    public class UILateSystem: ILateSystem
    {
        private Filter mainCanvasF;
        private Filter enabledGOIDSF;

        private bool isFired = false;

        public World World
        {
            get; set;
        }

        public void OnAwake()
        {
            mainCanvasF = World.Filter.With<MainCanvas>().Build();
            enabledGOIDSF =
                World.Filter.With<UIInitiallyEnabledGOIDS>().Build();
        }

        public void OnUpdate(float deltaTime)
        {
            // TODO(ryzhovalex): maybe disposing is better here
            if (!isFired)
            {
                DisablePanels();
                isFired = true;
            }
        }

        public void Dispose()
        {
        }

        private void DisablePanels()
        {
            var mainCanvasGO = GoUtils.GetUnity(
                mainCanvasF.First()
            );
            var enabledGOIDSE =
                enabledGOIDSF
                .First();
            ref var enabledGOIDSC =
                ref enabledGOIDSE.GetComponent<UIInitiallyEnabledGOIDS>();

            foreach (
                var provider
                in mainCanvasGO.GetComponentsInChildren<PanelComponent>()
            )
            {
                if (!enabledGOIDSC.value.Contains(
                    provider.gameObject.GetInstanceID()
                ))
                {
                    provider.gameObject.SetActive(false); 
                }
            }

            World.RemoveEntity(enabledGOIDSE);
        }
    }
}
