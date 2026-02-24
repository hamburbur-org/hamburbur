using UnityEngine;

namespace hamburbur.Components;

public class LookAtCamera : MonoBehaviour
{
    private void Update()
    {
        if (Camera.main == null)
            return;

        transform.LookAt(Camera.main.transform);
        transform.Rotate(0f, 180f, 0f);
    }
}