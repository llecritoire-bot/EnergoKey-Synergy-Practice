using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public sealed class ExitGate : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isOpen;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetOpen(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            GameController.Instance?.TryUseExit();
        }
    }

    public void SetOpen(bool value)
    {
        isOpen = value;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = ThemeAssets.Sprite(value ? "exit_open" : "exit_closed");
        spriteRenderer.color = Color.white;
    }

    public void LockedFeedback()
    {
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(Pulse());
    }

    private IEnumerator Pulse()
    {
        Vector3 original = transform.localScale;
        for (int i = 0; i < 2; i++)
        {
            transform.localScale = original * 1.18f;
            yield return new WaitForSeconds(0.08f);
            transform.localScale = original;
            yield return new WaitForSeconds(0.08f);
        }
        pulseRoutine = null;
    }
}

