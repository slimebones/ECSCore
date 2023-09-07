using Scellecs.Morpeh;
using System;
using System.Linq;
using static Slimebones.ECSCore.Utils.Delegates;

namespace Slimebones.ECSCore.Collision {
    public static class CollisionUtils {
        /// <summary>
        /// Executes action for each collision event.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="world"></param>
        /// <param name="allowedTypes"></param>
        public static void ExecuteForEachCollisionEvent(
            ActionRef<CollisionEvent> action,
            World world,
            CollisionEventType[] allowedTypes = null
        ) {
            Filter collisionEventFilter = world.Filter.With<CollisionEvent>();

            foreach (Entity collisionEventEntity in collisionEventFilter) {
                ref CollisionEvent collisionEvent =
                    ref collisionEventEntity.GetComponent<CollisionEvent>();

                if (!isAllowedType(collisionEvent.type, allowedTypes)) {
                    return; 
                }

                action(ref collisionEvent);
            }
        }

        /// <summary>
        /// Executes action for each collision event where HostComponent is
        /// attached to host collider entity.
        /// </summary>
        /// <typeparam name="HostComponent"></typeparam>
        /// <param name="action"></param>
        /// <param name="world"></param>
        /// <param name="allowedTypes"></param>
        public static void ExecuteForEachCollisionEvent<HostComponent>(
            ActionRef<CollisionEvent, HostComponent> action,
            World world,
            CollisionEventType[] allowedTypes = null
        ) where HostComponent : struct, IComponent {
            // create action wrapper for less detailed overload of this method
            ActionRef<CollisionEvent> actionWrapper =
                (ref CollisionEvent collisionEvent) => {
                    Entity hostEntity = collisionEvent.hostEntity;
                    ref HostComponent hostComponent =
                        ref hostEntity.GetComponent<HostComponent>(
                            out bool isTargetHostComponent
                        );
                    if (isTargetHostComponent) {
                        action(ref collisionEvent, ref hostComponent);
                    }
                };
            ExecuteForEachCollisionEvent(actionWrapper, world, allowedTypes);
        }

        /// <summary>
        /// Executes action for each collision event where HostComponent and
        /// GuestComponent are attached to host/guest entities.
        /// </summary>
        /// <typeparam name="HostComponent"></typeparam>
        /// <typeparam name="GuestComponent"></typeparam>
        /// <param name="action"></param>
        /// <param name="world"></param>
        /// <param name="allowedTypes"></param>
        public static void ExecuteForEachCollisionEvent<
            HostComponent,
            GuestComponent
        >(
            ActionRef<CollisionEvent, HostComponent, GuestComponent> action,
            World world,
            CollisionEventType[] allowedTypes = null
        )
            where HostComponent: struct, IComponent
            where GuestComponent: struct, IComponent
        {

            // create action wrapper for less detailed overload of this method
            ActionRef<CollisionEvent, HostComponent> actionWrapper =
                (
                    ref CollisionEvent collisionEvent,
                    ref HostComponent hostComponent
                ) => {
                    Entity guestEntity = collisionEvent.guestEntity;

                    if (guestEntity == null) {
                        return; 
                    }

                    ref GuestComponent guestComponent =
                        ref guestEntity.GetComponent<GuestComponent>(
                            out bool isTargetGuestComponent
                        );

                    if (isTargetGuestComponent) {
                        action(
                            ref collisionEvent,
                            ref hostComponent,
                            ref guestComponent
                        );
                    }
                };

            ExecuteForEachCollisionEvent<HostComponent>(
                actionWrapper, world, allowedTypes
            );
        }

        private static bool isAllowedType(
            CollisionEventType type,
            CollisionEventType[] allowedTypes
        ) {
            if (allowedTypes == null) {
                return true;
            }
            return allowedTypes.Contains(type);
        }
    }
}