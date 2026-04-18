using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour

{
    [Header("Grid Settings")]
    public float cellSize = 1.0f;               // Размер одной ячейки в метрах
    public Vector2Int startGridPos = Vector2Int.zero;
    public float moveDuration = 0.25f;           // Время перемещения между ячейками
    public float turnDuration = 0.2f;            // Время поворота

    [Header("Collision")]
    public LayerMask obstacleMask = ~0;          // Слои, блокирующие движение

    private Vector2Int currentGridPos;
    private bool isMoving = false;
    private CharacterController controller;      // Можно заменить на простой Transform

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentGridPos = startGridPos;
        // Принудительно ставим персонажа в центр стартовой ячейки
        transform.position = GridToWorld(currentGridPos);
    }

    void Update()
    {
        if (isMoving) return;

        // Поворот налево (A)
        if (Input.GetKeyDown(KeyCode.A))
            StartCoroutine(RotateByAngle(-90f));
        // Поворот направо (D)
        else if (Input.GetKeyDown(KeyCode.D))
            StartCoroutine(RotateByAngle(90f));
        // Шаг вперёд (W)
        else if (Input.GetKeyDown(KeyCode.W))
            TryMove(GetForwardGridDirection());
        // Шаг назад (S)
        else if (Input.GetKeyDown(KeyCode.S))
            TryMove(GetBackwardGridDirection());
    }

    // Преобразование координат сетки в мировые (центр ячейки)
    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, transform.position.y, gridPos.y * cellSize);
    }

    // Получить направление вперёд в координатах сетки (учитывает поворот персонажа)
    Vector2Int GetForwardGridDirection()
    {
        float angle = transform.eulerAngles.y;
        // Округляем угол до ближайшего направления (0, 90, 180, 270)
        int dirIndex = Mathf.RoundToInt(angle / 90f) % 4;
        return dirIndex switch
        {
            0 => new Vector2Int(0, 1),   // Север
            1 => new Vector2Int(1, 0),   // Восток
            2 => new Vector2Int(0, -1),  // Юг
            3 => new Vector2Int(-1, 0),  // Запад
            _ => Vector2Int.zero
        };
    }

    Vector2Int GetBackwardGridDirection()
    {
        return -GetForwardGridDirection();
    }

    void TryMove(Vector2Int direction)
    {
        Vector2Int targetGridPos = currentGridPos + direction;
        Vector3 targetWorldPos = GridToWorld(targetGridPos);

        if (IsCellWalkable(targetWorldPos))
            StartCoroutine(MoveToCell(targetGridPos, targetWorldPos));
    }

    bool IsCellWalkable(Vector3 worldCenter)
    {
        // Проверяем сферой радиусом чуть меньше половины клетки
        float checkRadius = cellSize * 0.4f;
        return !Physics.CheckSphere(worldCenter, checkRadius, obstacleMask);
    }

    IEnumerator MoveToCell(Vector2Int targetGridPos, Vector3 targetWorldPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            Vector3 newPos = Vector3.Lerp(startPos, targetWorldPos, t);
            
            if (controller != null)
                controller.Move(newPos - transform.position);
            else
                transform.position = newPos;
            
            yield return null;
        }

        // Финальная установка позиции и обновление координат в сетке
        if (controller != null)
            controller.Move(targetWorldPos - transform.position);
        else
            transform.position = targetWorldPos;

        currentGridPos = targetGridPos;
        isMoving = false;
    }

    IEnumerator RotateByAngle(float angle)
    {
        isMoving = true;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, angle, 0);
        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / turnDuration);
            yield return null;
        }

        transform.rotation = targetRot;
        isMoving = false;
    }
}