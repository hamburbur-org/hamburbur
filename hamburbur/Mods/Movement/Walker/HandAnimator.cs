using UnityEngine;

namespace hamburbur.Mods.Movement.Walker;

public class HandAnimator : MonoBehaviour
{
    private const float StepDistance = 0.3f;
    private const float StepHeight   = 0.3f;
    private const float StepLength   = 0.2f;
    private const float Speed        = 10f;
    public        bool  IsLeftHand;

    public LayerMask    TerrainLayer;
    public HandAnimator OtherHandAnimator;

    public Transform Body;
    public Vector3   FootOffset;

    public float FootSpacing;

    private float   lerp;
    private Vector3 oldNormal, newNormal, currentNormal;

    private Vector3 oldPosition, newPosition, currentPosition;

    private void Start()
    {
        oldPosition = newPosition = currentPosition = transform.position;
        oldNormal   = newNormal   = currentNormal   = transform.right;
        lerp        = 1f;
    }

    private void Update()
    {
        transform.position = currentPosition;
        transform.right    = IsLeftHand ? -currentNormal : currentNormal;

        Ray ray = new(Body.position + Body.right * FootSpacing, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, TerrainLayer.value) &&
            Vector3.Distance(newPosition, hit.point) > StepDistance && !OtherHandAnimator.IsMoving() && lerp >= 1f)
        {
            lerp = 0f;
            int direction = Body.InverseTransformPoint(hit.point).z > Body.InverseTransformPoint(newPosition).z
                                    ? 1
                                    : -1;

            newPosition = hit.point + Body.forward * (StepLength * direction) + FootOffset;
            newNormal   = hit.normal;
        }

        if (lerp < 1f)
        {
            Vector3 temporaryPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            temporaryPosition.y += Mathf.Sin(lerp * Mathf.PI) * StepHeight;
            currentPosition     =  temporaryPosition;
            currentNormal       =  Vector3.Lerp(oldNormal, newNormal, lerp);
            lerp                += Time.deltaTime * Speed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal   = newNormal;
        }
    }

    private bool IsMoving() => lerp < 1f;
}