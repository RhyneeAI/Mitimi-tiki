using UnityEngine;

public class FloatingUpDown : MonoBehaviour
{
    public float amplitude = 10f; // jarak naik turun
    public float speed = 2f;      // kecepatan
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);
    }
}