using Scellecs.Morpeh;
using Slimebones.ECSCore.Base;
using Slimebones.ECSCore.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace Slimebones.ECSCore.UI.Panel
{
    public class PanelSystem: ISystem
    {
        private Filter panelF;
        private Filter reqF;

        private bool isStorageInitialized;

        private Dictionary<string, GameObject> panelGOByKey =
            new Dictionary<string, GameObject>();

        public World World
        {
            get; set;
        }

        public void OnAwake()
        {
            reqF = World.Filter.With<SetPanelStateRequest>().Build();
            panelF = World.Filter.With<Panel>().Build();

            InitStorage();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var reqE in reqF)
            {
                if (!RequestComponentUtils.RegisterCall(reqE))
                {
                    continue;
                }

                ref var reqC = ref reqE.GetComponent<SetPanelStateRequest>();

                if (!panelGOByKey.ContainsKey(reqC.key))
                {
                    Log.Error(
                        "[Panel] unregistered request's key "
                        + reqC.key
                        + " => skip"
                    );
                    continue;
                }

                panelGOByKey[reqC.key].SetActive(GetFinalState(
                    ref reqC,
                    panelGOByKey[reqC.key]
                ));
            }
        }

        public void Dispose()
        {
        }

        private bool GetFinalState(
            ref SetPanelStateRequest reqC,
            GameObject reffedGO
        )
        {
            switch (reqC.state)
            {
                case PanelStateChange.Enable:
                    return true;
                case PanelStateChange.Disable:
                    return false;
                case PanelStateChange.Toggle:
                    // use exactly active self since we do want to
                    // know only the object's state, his parents don't
                    // matter here
                    return !reffedGO.activeSelf;
                default:
                    Log.Error(
                        "[Panel] undefined panel request's state "
                        + reqC.state
                        + " => set to false"
                    );
                    return false;
            }
        }

        private void InitStorage()
        {
            foreach (var panelE in panelF)
            {
                var key = panelE.GetComponent<Panel>().key;
                var panelGO = GameObjectUtils.GetUnityOrError(
                    panelE
                );

                if (key == "")
                {
                    Log.Error(
                        "[Panel] game object "
                        + panelGO
                        + " has an empty key => skip"
                    );
                    continue;
                }

                if (panelGOByKey.ContainsKey(key))
                {
                    Log.Error(
                        "[Panel] game object "
                        + panelGO
                        + "has a duplicate key "
                        + key
                        + " => skip"
                    );
                    continue;
                }

                panelGOByKey[key] = panelGO;
            }
        }
    }
}