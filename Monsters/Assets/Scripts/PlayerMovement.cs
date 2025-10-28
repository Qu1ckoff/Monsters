using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float smooth = 10f;
    public float rotationSpeed = 10f; // �������� ��������

    private Vector3 moveInput;
    private Vector3 moveDir;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // ����� �� ����������� ���������
    }

    public void SetInput(Vector2 input)
    {
        moveInput = new Vector3(input.x, 0, input.y);
    }

    void FixedUpdate()
    {
        // ������ ��������� � ������ ����������� ��������
        moveDir = Vector3.Lerp(moveDir, moveInput, Time.fixedDeltaTime * smooth);

        // ������� ������
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // ���� ���� �������� � ������������ ������ � ���� �����������
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            rb.MoveRotation(smoothedRotation);
        }
    }
}
