using UnityEngine;

public class PieceDrag : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // ������ �� �� ����� ����� �� ���� ����
        Collider2D hit = Physics2D.OverlapPoint(transform.position);
        if (hit != null && hit.CompareTag("BoardSlot"))
        {
            transform.position = hit.transform.position;
        }
        else
        {
            // ���� ������ ��� ������ ��� �� ��� ����� �� ���� ����
            transform.position -= offset;
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 10f; // ������ ������ ���� ��������
        return cam.ScreenToWorldPoint(mousePoint);
    }
}

