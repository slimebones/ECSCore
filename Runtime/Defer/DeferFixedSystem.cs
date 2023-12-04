using Scellecs.Morpeh;
using Slimebones.ECSCore.Base.CoreSystem;
using Slimebones.ECSCore.Base.Request;
using Unity.VisualScripting.YamlDotNet.Serialization;

namespace Slimebones.ECSCore.Defer
{
    public class DeferFixedSystem: IFixedSystem
    {
        private const UpdateType RequiredUpdateType = UpdateType.FixedUpdate;

        private SystemCallOrder callOrder;
        private Filter deferRequestF;

        public DeferFixedSystem(
            SystemCallOrder callOrder
        )
        {
            this.callOrder = callOrder;
        }

        public World World
        {
            get; set;
        }

        public void OnAwake()
        {
            deferRequestF = RequestUtils.FB.With<DeferRequest>().Build();
        }

        public void OnUpdate(float deltaTime)
        {
            SystemUtils.IterateEntities(deferRequestF, OnEntity);
        }

        public void Dispose()
        {
        }

        private void OnEntity(Entity e)
        {
            ref var c = ref e.GetComponent<DeferRequest>();
            if (
                c.updateType != RequiredUpdateType
                || c.callOrder != callOrder
            )
            {
                return;
            }
            if (c.skippedFrames < c.framesToSkip)
            {
                c.skippedFrames++;
                return;
            }
            c.action();
            RequestUtils.Complete(e);
        }
    }
}

