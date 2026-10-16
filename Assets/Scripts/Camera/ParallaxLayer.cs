using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Параллакс")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f;     

    [Header("Ограничения движения слоя (в мировых координатах)")]
    public float minX = -50f;        
    public float maxX = 150f;              

    private Transform camTransform;
    private Vector3 startLocalPos;

    void Awake()
    {
        camTransform = Camera.main.transform;
        startLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        float camX = camTransform.position.x;

        // Вычисляем желаемую позицию с параллаксом
        float parallaxX = camX * parallaxFactor;
        float desiredX = startLocalPos.x + parallaxX;

        // Ограничиваем позицию, чтобы слой не уходил за границы
        desiredX = Mathf.Clamp(desiredX, minX, maxX);

        // Применяем позицию
        transform.localPosition = new Vector3(desiredX, startLocalPos.y, startLocalPos.z);
    }
}