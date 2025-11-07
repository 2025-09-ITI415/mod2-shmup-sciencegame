using UnityEngine;

public class ProjectileEnemy : MonoBehaviour
{
    public Vector3 vel;
    public float damage = 1f;
    private BoundsCheck bndCheck;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
    }

    void Update()
    {
        transform.position += vel * Time.deltaTime;

        if (bndCheck != null && !bndCheck.isOnScreen)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject go = other.gameObject;
        if (go.CompareTag("Player"))
        {
            Hero hero = go.GetComponent<Hero>();
            if (hero != null)
            {
                hero.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
