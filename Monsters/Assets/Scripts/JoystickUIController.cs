using UnityEngine;
using UnityEngine.UI;

public class JoystickUIController : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement player;
    public Image anchorCircle;
    public Image fingerCircle;
    public Image lineImage; // линия теперь Image

    [Header("Settings")]
    public float maxDistance = 200f;
    public float lineThickness = 8f;

    private Vector2 anchorPos;
    private bool isDragging;

    void Start()
    {
        HideUI();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            StartDrag(Input.mousePosition);

        if (Input.GetMouseButton(0) && isDragging)
            Drag(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            EndDrag();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;
        var t = Input.GetTouch(0);

        switch (t.phase)
        {
            case TouchPhase.Began: StartDrag(t.position); break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary: if (isDragging) Drag(t.position); break;
            case TouchPhase.Ended: EndDrag(); break;
        }
    }

    void StartDrag(Vector2 startPos)
    {
        isDragging = true;
        anchorPos = startPos;
        ShowUI(startPos);
    }

    void Drag(Vector2 currentPos)
    {
        Vector2 delta = currentPos - anchorPos;
        delta = Vector2.ClampMagnitude(delta, maxDistance);
        Vector2 clampedPos = anchorPos + delta;

        // передаём нормализованное направление движения
        player.SetInput(delta / maxDistance);

        // визуализация
        anchorCircle.rectTransform.position = anchorPos;
        fingerCircle.rectTransform.position = clampedPos;

        UpdateLine(anchorPos, clampedPos);
    }

    void EndDrag()
    {
        isDragging = false;
        player.SetInput(Vector2.zero);
        HideUI();
    }

    void UpdateLine(Vector2 start, Vector2 end)
    {
        Vector2 dir = end - start;
        float dist = dir.magnitude;

        RectTransform lineRect = lineImage.rectTransform;
        lineRect.sizeDelta = new Vector2(dist, lineThickness);
        lineRect.position = start;
        lineRect.rotation = Quaternion.FromToRotation(Vector3.right, dir.normalized);
    }

    void ShowUI(Vector2 pos)
    {
        anchorCircle.enabled = true;
        fingerCircle.enabled = true;
        lineImage.enabled = true;

        anchorCircle.rectTransform.position = pos;
        fingerCircle.rectTransform.position = pos;
        UpdateLine(pos, pos);
    }

    void HideUI()
    {
        anchorCircle.enabled = false;
        fingerCircle.enabled = false;
        lineImage.enabled = false;
    }
}
