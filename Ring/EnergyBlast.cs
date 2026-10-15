using GameNetcodeStuff;
using SnowyLib;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static float EnergyBlastEnemyKnockbackForce => PluginInstance.Config.Bind("Energy Blast", "Enemy Knockback Force", 0f, "How much knockback force the energy blast applies to enemies.").Value;
    public static float EnergyBlastPlayerKnockbackForce => PluginInstance.Config.Bind("Energy Blast", "Player Knockback Force", 0f, "How much knockback force the energy blast applies to players.").Value;
    public static int EnergyBlastEnemyDamage => PluginInstance.Config.Bind("Energy Blast", "Enemy Damage", 0, "How much damage the energy blast deals to enemies.").Value;
    public static int EnergyBlastPlayerDamage => PluginInstance.Config.Bind("Energy Blast", "Player Damage", 0, "How much damage the energy blast deals to players.").Value;
    
    const float energyBlastDistance = 5f;

    private void EnergyBlastKeyPressed(InputAction.CallbackContext context) // Energy Blast
    {
        if (context.canceled) { return; }
        if (playerWornBy == null || playerWornBy != localPlayer) { return; }

        if (!CanCastAbility(Ability.EnergyBlast))
        {
            DoCastFailedEffectsRpc();
            return;
        }

        energyBlastParticleSystem.Play();

        List<ulong> netObjIds = [];
        RaycastHit[] hits = Physics.SphereCastAll(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, 1f, localPlayer.gameplayCamera.transform.forward, energyBlastDistance, enemiesAndPlayersMask);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Enemies"))
            {
                EnemyAI enemy = hit.collider.gameObject.GetComponent<EnemyAICollisionDetect>().mainScript;
                netObjIds.Add(enemy.NetworkObjectId);
            }
            else if (hit.collider.CompareTag("Player"))
            {
                PlayerControllerB player = hit.collider.gameObject.GetComponent<PlayerControllerB>();
                netObjIds.Add(player.NetworkObjectId);
            }
        }

        EnergyBlastRpc(netObjIds.ToArray(), localPlayer.gameplayCamera.transform.forward);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void EnergyBlastRpc(ulong[] netObjIds, Vector3 direction)
    {
        energyBlastParticleSystem.Play();
        audioSource.PlayOneShot(energyBlastSFX);
        UsePower(EnergyBlastPowerUsageOnCast);
        SetCooldown(Ability.EnergyBlast, EnergyBlastCooldown);

        if (netObjIds == null || netObjIds.Length <= 0) { return; }

        foreach (var netObjId in netObjIds)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out NetworkObject netObj))
                if (netObj.TryGetComponent(out PlayerControllerB player))
                {
                    GameObject energyHitObj = Instantiate(energyHitPrefab, player.bodyParts[5]);
                    Destroy(energyHitObj, 1f);

                    player.DamagePlayer(EnergyBlastPlayerDamage, callRPC: false, causeOfDeath: CauseOfDeath.Blast, force: direction * EnergyBlastPlayerKnockbackForce);

                    if (!player.isPlayerDead && !player.isPlayerControlled)
                        player.AddForce(direction, EnergyBlastPlayerKnockbackForce);
                }
                else if (netObj.TryGetComponent(out EnemyAI enemy))
                {
                    GameObject energyHitObj = Instantiate(energyHitPrefab, enemy.eye);
                    Destroy(energyHitObj, 1f);

                    enemy.HitEnemy(EnergyBlastEnemyDamage, lastPlayerWornBy);

                    if (!enemy.isEnemyDead)
                        enemy.AddForce(direction, EnergyBlastEnemyKnockbackForce);
                }
        }
    }
}