using Scellecs.Morpeh;
using Slimebones.ECSCore.Object;
using Slimebones.ECSCore.UI.Canvas;
using Slimebones.ECSCore.UI.Panel;
using System.Collections.Generic;

namespace Slimebones.ECSCore.UI
{
    public class UIInitSystem: IInitializer
    {
        private Filter mainCanvasF;
        private Filter enabledGOIDSF;

        public World World
        {
            get; set;
        }

        public void OnAwake()
        {
            mainCanvasF = World.Filter.With<MainCanvas>().Build();
            enabledGOIDSF =
                World.Filter.With<UIInitiallyEnabledGOIDS>().Build();

            EnablePanels();
        }

        public void Dispose()
        {
        }

        private void EnablePanels()
        {
            var mainCanvasGO = GoUtils.GetUnity(
                mainCanvasF.First()
            );

            var goidsE = World.CreateEntity();
            ref var enabledGOIDSC =
                ref goidsE.AddComponent<UIInitiallyEnabledGOIDS>();
            enabledGOIDSC.value = new List<int>();

            foreach (
                var provider
                in mainCanvasGO.GetComponentsInChildren<PanelComponent>(true)
            )
            {
                if (provider.gameObject.activeSelf)
                {
                    enabledGOIDSC.value.Add(
                        provider.gameObject.GetInstanceID()
                    );
                    continue;
                }
                provider.gameObject.SetActive(true);
            }
        }
    }
}
