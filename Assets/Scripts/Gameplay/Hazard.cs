using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            GameController.Instance?.DamagePlayer();
        }
    }
}

