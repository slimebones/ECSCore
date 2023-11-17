using Scellecs.Morpeh;
using Slimebones.ECSCore.UI.Button;
using Slimebones.ECSCore.UI.Settings;

namespace Slimebones.ECSCore
{
    public class CoreSystem: ISystem
    {
        public World World
        {
            get; set;
        }

        public void OnAwake()
        {
            World.GetStash<Button>().AsDisposable();
            World.GetStash<DropdownSetting>().AsDisposable();
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Dispose()
        {
        }
    }
}
