using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float scaleFactor = 1.2f; // ���� ������� ��� �����
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = originalScale * scaleFactor; // ���� ���� ��� �����
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = originalScale; // ���� ����� ������� ��� ��� ������
    }
}
