using Scellecs.Morpeh;
using Slimebones.ECSCore.Object;
using System;
using UnityEngine.UI;

namespace Slimebones.ECSCore.Config.Specs
{
    public static class ConfigSpecUtils
    {
        public static Action<string> OnBasicToggleSettingInit(Entity e)
        {
            var go = GoUtils.GetUnity(e);
            Toggle toggleUnity = go.GetComponent<Toggle>();
            return (string value) =>
                toggleUnity.SetIsOnWithoutNotify(value == "1");
        }
    }
}