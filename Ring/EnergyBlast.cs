using GameNetcodeStuff;
using SnowyLib;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static UnityEngine.LightAnchor;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static float EnergyBlastEnemyKnockbackForce => PluginInstance.Config.Bind("Energy Blast", "Enemy Knockback Force", 50f, "How much knockback force the energy blast applies to enemies.").Value;
    public static float EnergyBlastPlayerKnockbackForce => PluginInstance.Config.Bind("Energy Blast", "Player Knockback Force", 50f, "How much knockback force the energy blast applies to players.").Value;
    public static int EnergyBlastEnemyDamage => PluginInstance.Config.Bind("Energy Blast", "Enemy Damage", 1, "How much damage the energy blast deals to enemies.").Value;
    public static int EnergyBlastPlayerDamage => PluginInstance.Config.Bind("Energy Blast", "Player Damage", 10, "How much damage the energy blast deals to players.").Value;
    
    const float energyBlastDistance = 5f;

    private void EnergyBlastKeyPressed(InputAction.CallbackContext context) // Energy Blast
    {
        if (!CanUseRing() || AbilityEnabled[(int)Ability.EnergyBlast] || context.canceled) { return; }

        QuickCastAbilitySynced(() =>
        {
            if (!CanCastAbility(Ability.EnergyBlast))
            {
                CastAbilityFailedRpc();
                return;
            }

            energyBlastParticleSystem.Play();

            List<ulong> netObjIds = [];
            RaycastHit[] hits = Physics.SphereCastAll(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, 2f, localPlayer.gameplayCamera.transform.forward, energyBlastDistance, enemiesAndPlayersMask);

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    var collisionDetect = hit.collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                    if (collisionDetect == null) { continue; }
                    EnemyAI enemy = collisionDetect.mainScript;
                    logger.LogDebug($"{enemy.enemyType.enemyName} hit");
                    netObjIds.Add(enemy.NetworkObjectId);
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    PlayerControllerB player = hit.collider.gameObject.GetComponent<PlayerControllerB>();
                    if (player == playerWornBy) { continue; }
                    logger.LogDebug($"{player.playerUsername} hit");
                    netObjIds.Add(player.NetworkObjectId);
                }
            }

            Vector3 forwardDirection = localPlayer.transform.TransformDirection(Vector3.forward).normalized;
            Vector3 upDirection = localPlayer.transform.TransformDirection(Vector3.up).normalized * 2;
            Vector3 throwDirection = (forwardDirection + upDirection).normalized;

            EnergyBlastRpc(netObjIds.ToArray(), throwDirection); // TODO: Test this
        });
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void EnergyBlastRpc(ulong[] netObjIds, Vector3 direction)
    {
        logger.LogDebug("EnergyBlastRpc");
        energyBlastParticleSystem.Play();
        audioSource.PlayOneShot(energyBlastSFX);
        UsePower(AbilityPowerUsageOnCast[(int)Ability.EnergyBlast]);
        SetCooldown(Ability.EnergyBlast);

        if (netObjIds == null || netObjIds.Length <= 0) { return; }

        foreach (var netObjId in netObjIds)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out NetworkObject netObj)) { continue; }

            if (netObj.TryGetComponent(out PlayerControllerB player))
            {
                GameObject energyHitObj = Instantiate(energyHitPrefab, player.bodyParts[5]);
                Destroy(energyHitObj, 1f);

                player.DamagePlayer(EnergyBlastPlayerDamage, callRPC: false, causeOfDeath: CauseOfDeath.Blast, force: direction * EnergyBlastPlayerKnockbackForce);

                if (!player.isPlayerDead && !player.isPlayerControlled)
                {
                    logger.LogDebug($"Adding force to {player.playerUsername}");
                    player.AddForce(direction, EnergyBlastPlayerKnockbackForce);
                }
            }
            else if (netObj.TryGetComponent(out EnemyAI enemy))
            {
                GameObject energyHitObj = Instantiate(energyHitPrefab, enemy.eye);
                Destroy(energyHitObj, 1f);

                enemy.HitEnemy(EnergyBlastEnemyDamage, lastPlayerWornBy);

                if (!enemy.isEnemyDead)
                {
                    logger.LogDebug($"Adding force to {enemy.enemyType.enemyName}");
                    enemy.AddForce(direction, EnergyBlastEnemyKnockbackForce);
                }
            }
        }
    }
}