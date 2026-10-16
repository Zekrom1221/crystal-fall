using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    [Header("По умолчанию (если вызвали Shake() без параметров)")]
    public float defaultDuration = 0.2f;
    public float defaultMagnitude = 0.15f;

    private Coroutine _shakeRoutine;
    private Vector3 _originalLocalPos;
    private bool _capturedOriginal;

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (!_capturedOriginal)
        {
            _originalLocalPos = transform.localPosition;
            _capturedOriginal = true;
        }

        if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
        _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = _originalLocalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _originalLocalPos;
        _shakeRoutine = null;
    }
}
