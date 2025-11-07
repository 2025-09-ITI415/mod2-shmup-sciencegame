using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public float baseFireInterval = 1.5f;
    public float randomOffset = 0.5f;
    public string playerTag = "Player";

    private Weapon weapon;
    private Transform shotPoint;
    private Transform player;

    void Start()
    {
        weapon = GetComponentInChildren<Weapon>();
        if (weapon == null)
        {
            enabled = false;
            return;
        }

        if (weapon.transform.childCount > 0)
            shotPoint = weapon.transform.GetChild(0);
        else
            shotPoint = weapon.transform;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;

        StartCoroutine(ShootLoop());
    }

    IEnumerator ShootLoop()
    {
        float firstDelay = Random.Range(0f, baseFireInterval);
        yield return new WaitForSeconds(firstDelay);

        while (true)
        {
            float interval = baseFireInterval + Random.Range(-randomOffset, randomOffset);
            if (interval < 0.1f) interval = 0.1f;
            yield return new WaitForSeconds(interval);
            FireAtPlayer();
        }
    }

    void FireAtPlayer()
    {
        if (Time.time < weapon.nextShotTime) return;
        if (weapon.def == null || weapon.def.projectilePrefab == null) return;
        if (player == null) return;

        if (Weapon.PROJECTILE_ANCHOR == null)
        {
            GameObject anchor = new GameObject("_ProjectileAnchor");
            Weapon.PROJECTILE_ANCHOR = anchor.transform;
        }

        Vector3 spawnPos = shotPoint.position;
        spawnPos.z = 0f;
        Vector3 dir = (player.position - spawnPos).normalized;

        GameObject projGO = Instantiate(weapon.def.projectilePrefab, Weapon.PROJECTILE_ANCHOR);
        projGO.transform.position = spawnPos;


        ProjectileEnemy proj = projGO.GetComponent<ProjectileEnemy>();
        if (proj != null)
        {
            proj.vel = dir * weapon.def.velocity;
            proj.damage = 1f; // you can adjust damage value
        }


        weapon.nextShotTime = Time.time + weapon.def.delayBetweenShots;
    }
}
