using UnityEngine;
using UnityEngine.EventSystems;

public class UICardHoverEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationSpeed = 16f;

    private Vector3 targetScale = Vector3.one;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }
}
