using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class Hazard : MonoBehaviour
{
    private void Awake()
    {
        Collider2D hazardCollider = GetComponent<Collider2D>();
        hazardCollider.isTrigger = true;

        // Prefabs may be created before their sprite is imported. Unity then gives a
        // BoxCollider2D an effectively zero size, so enforce a usable gameplay shape.
        if (hazardCollider is BoxCollider2D box && (box.size.x < 0.1f || box.size.y < 0.1f))
        {
            box.size = new Vector2(0.82f, 0.82f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            GameController.Instance?.DamagePlayer();
        }
    }
}

