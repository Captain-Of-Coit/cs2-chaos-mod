using Colossal.Entities;
using Colossal.Mathematics;
using Game;
using Game.Audio;
using Game.Common;
using Game.Events;
using Game.Objects;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace ChaosMod.Systems
{
    public class ChaosModSystem : GameSystemBase
    {
        // private SimulationSystem simulation;
        private EntityQuery m_VehicleQuery;
        private EntityArchetype m_ImpactEventArchetype;

        protected override void OnCreate()
        {
            base.OnCreate();
            CreateKeyBinding();
            // Example on how to get a existing ECS System from the ECS World
            // this.simulation = World.GetExistingSystemManaged<SimulationSystem>();
            m_VehicleQuery = GetEntityQuery(new EntityQueryDesc() {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<Vehicle>(),
                    ComponentType.ReadOnly<Moving>(),
                },
                None = new ComponentType[] {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>()
                }
            });

            m_ImpactEventArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<Impact>());
        }


        private void CreateKeyBinding()
        {
            var inputAction = new InputAction("MyModHotkeyPress");
            inputAction.AddBinding("<Keyboard>/n");
            inputAction.performed += OnHotkeyPress;
            inputAction.Enable();
        }

        private void OnHotkeyPress(InputAction.CallbackContext obj)
        {
            // Query for currently selected entity
            // Make entity lose control
            UnityEngine.Debug.Log("Adding OutOfControl to everything!");
            AddOutOfControlComponent();
        }

        public void AddOutOfControlComponent() {

            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            //
            //component2.m_VelocityDelta.xz = component2.m_Severity * MathUtils.Left(math.normalizesafe(component1.m_Velocity.xz));

            //Entity entity2 = commandBuffer.CreateEntity(this.m_ImpactEventArchetype);
            //commandBuffer.SetComponent<Impact>(entity2, component2);


            NativeArray<Entity> entityArray = m_VehicleQuery.ToEntityArray(Allocator.TempJob);

            

            foreach (var entity in entityArray) {
                Moving movingComponent;
                EntityManager.TryGetComponent<Moving>(entity, out movingComponent);

                //Moving newMovingComponent = new Moving{
                //    m_Velocity = movingComponent.m_Velocity,
                //    m_AngularVelocity = movingComponent.m_AngularVelocity,
                //};

                //newMovingComponent.m_Velocity.y = 10f;
                

                var impactComponent = new Impact() {
                    m_Event = entity,
                    m_Target = entity,
                    m_Severity = 5f
                };

                var random = new Unity.Mathematics.Random {};


                float yDelta = random.NextFloat(-2f, 2f);
                impactComponent.m_AngularVelocityDelta.y = yDelta;
                
                // Want this to be front flipped first
                float xDelta = random.NextFloat(-1f, -3f);
                impactComponent.m_AngularVelocityDelta.x = xDelta;

                impactComponent.m_VelocityDelta.xz = impactComponent.m_Severity * MathUtils.Left(math.normalizesafe(movingComponent.m_Velocity.xz));
                impactComponent.m_VelocityDelta.y = 10f;

                Entity impactEntity = commandBuffer.CreateEntity(m_ImpactEventArchetype);
                commandBuffer.SetComponent<Impact>(impactEntity, impactComponent);

                commandBuffer.AddComponent<OutOfControl>(entity);
                //commandBuffer.SetComponent<Moving>(entity, newMovingComponent);



            }
            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
            entityArray.Dispose();
        }

        protected override void OnUpdate() {}
    }
}
