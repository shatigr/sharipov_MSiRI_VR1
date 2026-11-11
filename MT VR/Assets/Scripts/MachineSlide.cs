using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
public class MachineSlide : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    private Interactable interactable;
    private Rigidbody rb;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Если мы схватили объект рукой
        if (interactable.attachedToHand != null)
        {
            Hand hand = interactable.attachedToHand;
            
            // Вычисляем направление от Start до End
            Vector3 direction = (endPoint.position - startPoint.position).normalized;
            float maxDistance = Vector3.Distance(startPoint.position, endPoint.position);

            // Берем позицию руки
            Vector3 handPos = hand.transform.position;

            // Проецируем позицию руки на нашу линию движения
            Vector3 projectedPos = startPoint.position + Vector3.Project(handPos - startPoint.position, direction);

            // Считаем расстояние от старта
            float distanceFromStart = Vector3.Dot(projectedPos - startPoint.position, direction);

            // Ограничиваем расстояние, чтобы не уехать за Start или End
            distanceFromStart = Mathf.Clamp(distanceFromStart, 0f, maxDistance);

            // Новая позиция
            Vector3 newPosition = startPoint.position + direction * distanceFromStart;

            // Двигаем Rigidbody (так как Is Kinematic включен, это сработает отлично)
            rb.MovePosition(newPosition);
        }
    }
}