using UnityEngine;

public class PinchScaler : MonoBehaviour
{
    private float factor = 1;
    private float initialDistance;
    private Vector3 initialScale;
    private float currentDistance;

    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;
    [SerializeField] private float sensibility;
    [SerializeField] private Transform targetTransform;

    void Start()
    {
        targetTransform = GetComponent<Transform>();
        var localScale = PlayerPrefs.GetFloat("LocalScale", -1);
        if (localScale != -1)
        {
            targetTransform.localScale = Vector3.one * localScale;
        }
    }

    private void Update() => CalculatePinch();

    // Based on CREDIT: https://www.youtube.com/watch?v=ISBIu6Jzfk8&ab_channel=MohdHamza
    public void CalculatePinch()
    {
        if (Input.touchCount != 2) return;
        var touchZero = Input.GetTouch(0);
        var touchOne = Input.GetTouch(1);

        if (touchZero.phase is TouchPhase.Ended or TouchPhase.Canceled ||
            touchOne.phase is TouchPhase.Ended or TouchPhase.Canceled)
        {
            PlayerPrefs.SetFloat("LocalScale", targetTransform.localScale.x);
            return;
        }

        if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
        {
            initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
            initialScale = targetTransform.localScale;
            // print($"Pantalla pulsada, initial scale: {initialScale} (touchZ {touchZero.position}, touchO {touchOne.position}");
        }
        else // fingers moved
        {
            currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
            if (Mathf.Abs(initialDistance - currentDistance) < 0.01f || Mathf.Approximately(initialDistance, 0)) return;
            factor = currentDistance / initialDistance;
            targetTransform.localScale = initialScale * IntensifyBySensibility(factor);
            targetTransform.localScale = ClampVector3(targetTransform.localScale);
        }
    }

    public Vector3 ClampVector3(Vector3 v)
    {
        if (v.x < minScale) return Vector3.one * minScale;
        if (v.x > maxScale) return Vector3.one * maxScale;
        return v;
    }

    public float IntensifyBySensibility(float value) => (((value - 1) * sensibility) + 1);
}
