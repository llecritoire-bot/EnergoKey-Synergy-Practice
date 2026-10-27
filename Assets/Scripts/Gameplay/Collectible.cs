using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class Collectible : MonoBehaviour
{
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collected && other.GetComponent<PlayerController>() != null)
        {
            GameController.Instance?.Collect(this);
        }
    }

    public void Consume()
    {
        collected = true;
        gameObject.SetActive(false);
    }
}

