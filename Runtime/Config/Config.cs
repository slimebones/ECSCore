using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;
using Scellecs.Morpeh;
using Slimebones.ECSCore.Base;
using Slimebones.ECSCore.Config.InternalSettingListeners;
using Slimebones.ECSCore.Controller;
using Slimebones.ECSCore.File;
using Slimebones.ECSCore.Key;
using Slimebones.ECSCore.Logging;
using Slimebones.ECSCore.React;
using Slimebones.ECSCore.UI;
using Slimebones.ECSCore.Utils;
using Slimebones.ECSCore.Utils.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Slimebones.ECSCore.Config
{
    /// <summary>
    /// Operates with game configuration file.
    /// </summary>
    public static partial class Config
    {
        public static World World { get; set; }

        private static Dictionary<string, IConfigSpec> specByKey =
            new Dictionary<string, IConfigSpec>();
        private static Dictionary<
            string, List<Action<string>>
        > settingUpdateActionsByKey =
            new Dictionary<string, List<Action<string>>>();

        private static readonly string Filename = "config.ini";
        private static readonly string DefaultSectionName = "Game";
        private static readonly int MaxOnChangeRecursion = 5;
        private static readonly IniFile file =
            new IniFile(
                Application.persistentDataPath + "/" + Filename
            );

        public static void Register(
            IConfigSpec[] specs
        )
        {
            foreach (var spec in specs)
            {
                if (specByKey.ContainsKey(spec.Key))
                {
                    Log.Error(
                        "spec with key {0} already exists => skip",
                        spec.Key
                    );
                }

                spec.World = World;
                specByKey[spec.Key] = spec;

                SetSpecValue(GetInitialValueForSpec(spec), spec);
            }
        }

        public static void SubscribeSetting(Entity e, UIInputType uiInputType)
        {
            var key = e.GetComponent<Key.Key>().key;
            CheckContainsKey(key);

            if (!settingUpdateActionsByKey.ContainsKey(key))
            {
                settingUpdateActionsByKey[key] = new List<Action<string>>();
            }
            var settingAction = specByKey[key].OnSettingInit(e);
            settingUpdateActionsByKey[key].Add(settingAction);
            // setup initial value
            settingAction(Get(key));

            SubscribeByInputType(
                key,
                GameObjectUtils.GetUnityOrError(e),
                uiInputType
            );
        }

        private static void SubscribeByInputType(
            string key,
            GameObject go,
            UIInputType uiInputType
        )
        {
            IKeyedGOListener listener;
            switch (uiInputType)
            {
                case UIInputType.Dropdown:
                    listener = new DropdownSettingKeyedGOListener();
                    break;
                case UIInputType.Toggle:
                    listener = new ToggleSettingKeyedGOListener();
                    break;
                case UIInputType.FloatSlider:
                    listener = new FloatSliderSettingKeyedGOListener();
                    break;
                case UIInputType.IntSlider:
                    listener = new IntSliderSettingKeyedGOListener();
                    break;
                default:
                    throw new NotFoundException(
                        "input type", uiInputType.ToString()
                    );
            }

            listener.Subscribe(key, go, World);
        }

        public static string Get(
            string key
        )
        {
            if (!file.KeyExists(key, DefaultSectionName))
            {
                throw new NotFoundException("key", key);
            }
            return file.Read(key, DefaultSectionName);
        }

        public static IConfigSpec GetSpec(string key)
        {
            CheckContainsKey(key);
            return specByKey[key];
        }

        public static void Set(
            string key,
            string value
        )
        {
            SetSilent(key, value);
            SetSpecValue(value, specByKey[key]);
        }

        private static void SetSpecValue(
            string value,
            IConfigSpec spec,
            int recursionNum = 0
        )
        {
            if (recursionNum > MaxOnChangeRecursion)
            {
                throw new RecursionException(MaxOnChangeRecursion);
            }

            string newNewValue;
            bool isValueChanged = spec.OnChange(value, out newNewValue);
            if (isValueChanged)
            {
                SetSpecValue(newNewValue, spec, recursionNum + 1);
                return;
            }

            // update settings only if it is a final value
            UpdateSettingsByKey(spec.Key, value);
        }

        private static void UpdateSettingsByKey(string key, string value)
        {
            if (!settingUpdateActionsByKey.ContainsKey(key))
            {
                // just skip if there are no such key, in case of initiali
                // disable settings
                return;
            }

            foreach (var action in settingUpdateActionsByKey[key])
            {
                action(value);
            }
        }

        private static void SetSilent(
            string key,
            string value
        )
        {
            CheckContainsKey(key);
            file.Write(key, value, DefaultSectionName);
        }

        private static void CheckContainsKey(string key)
        {
            if (!specByKey.ContainsKey(key))
            {
                throw new NotFoundException(
                    "config spec with key",
                    key
                );
            }
        }

        private static string GetInitialValueForSpec(
            IConfigSpec spec
        )
        {
            if (!file.KeyExists(
                spec.Key, DefaultSectionName
            ))
            {
                file.Write(
                    spec.Key,
                    spec.DefaultValueStr,
                    DefaultSectionName
                );
                return spec.DefaultValueStr;
            }
            else
            {
                return file.Read(spec.Key, DefaultSectionName);
            }
        }
    }
}
