using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class RetroCameraJitter : MonoBehaviour
{
    [Header("Snap de posición")]
    [SerializeField] private bool enablePositionSnap = true;
    [SerializeField] private float positionSnapStep = 0.01f;

    [Header("Snap de rotación")]
    [SerializeField] private bool enableRotationSnap = true;
    [SerializeField] private float rotationSnapStep = 0.15f;

    [Header("Wobble sutil")]
    [SerializeField] private bool enableWobble = true;
    [SerializeField] private float wobblePositionAmount = 0.001f;
    [SerializeField] private float wobbleRotationAmount = 0.04f;
    [SerializeField] private float wobbleSpeed = 1.3f;

    [Header("Opciones")]
    [SerializeField] private bool affectXPosition = true;
    [SerializeField] private bool affectYPosition = true;
    [SerializeField] private bool affectZPosition = false;

    private Camera targetCamera;

    private Vector3 cleanPosition;
    private Quaternion cleanRotation;

    private bool transformWasModifiedForRender;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering +=
            OnBeginCameraRendering;

        RenderPipelineManager.endCameraRendering +=
            OnEndCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -=
            OnBeginCameraRendering;

        RenderPipelineManager.endCameraRendering -=
            OnEndCameraRendering;

        RestoreCleanTransform();
    }

    public void ApplyPreset(int presetIndex)
    {
        presetIndex = Mathf.Clamp(presetIndex, 0, 2);

        switch (presetIndex)
        {
            case 0:
                enablePositionSnap = false;
                enableRotationSnap = false;
                enableWobble = false;
                break;

            case 1:
                enablePositionSnap = true;
                positionSnapStep = 0.01f;

                enableRotationSnap = true;
                rotationSnapStep = 0.15f;

                enableWobble = true;
                wobblePositionAmount = 0.001f;
                wobbleRotationAmount = 0.04f;
                wobbleSpeed = 1.3f;
                break;

            case 2:
                enablePositionSnap = true;
                positionSnapStep = 0.02f;

                enableRotationSnap = true;
                rotationSnapStep = 0.25f;

                enableWobble = true;
                wobblePositionAmount = 0.002f;
                wobbleRotationAmount = 0.08f;
                wobbleSpeed = 1.6f;
                break;
        }

        RestoreCleanTransform();
    }

    private void OnBeginCameraRendering(
        ScriptableRenderContext context,
        Camera camera
    )
    {
        if (camera != targetCamera)
            return;

        if (transformWasModifiedForRender)
        {
            RestoreCleanTransform();
        }

        cleanPosition = transform.position;
        cleanRotation = transform.rotation;

        Vector3 finalPosition = cleanPosition;
        Vector3 finalEuler = cleanRotation.eulerAngles;

        if (enableWobble)
        {
            float timeValue =
                Time.unscaledTime * wobbleSpeed;

            float wobbleX =
                Mathf.Sin(timeValue * 1.37f) *
                wobblePositionAmount;

            float wobbleY =
                Mathf.Cos(timeValue * 1.11f) *
                wobblePositionAmount;

            float wobbleZRotation =
                Mathf.Sin(timeValue * 1.83f) *
                wobbleRotationAmount;

            if (affectXPosition)
            {
                finalPosition.x += wobbleX;
            }

            if (affectYPosition)
            {
                finalPosition.y += wobbleY;
            }

            if (affectZPosition)
            {
                finalPosition.z += wobbleX * 0.5f;
            }

            finalEuler.z += wobbleZRotation;
        }

        if (enablePositionSnap &&
            positionSnapStep > 0f)
        {
            if (affectXPosition)
            {
                finalPosition.x = SnapValue(
                    finalPosition.x,
                    positionSnapStep
                );
            }

            if (affectYPosition)
            {
                finalPosition.y = SnapValue(
                    finalPosition.y,
                    positionSnapStep
                );
            }

            if (affectZPosition)
            {
                finalPosition.z = SnapValue(
                    finalPosition.z,
                    positionSnapStep
                );
            }
        }

        if (enableRotationSnap &&
            rotationSnapStep > 0f)
        {
            finalEuler.x = SnapAngle(
                finalEuler.x,
                rotationSnapStep
            );

            finalEuler.y = SnapAngle(
                finalEuler.y,
                rotationSnapStep
            );

            finalEuler.z = SnapAngle(
                finalEuler.z,
                rotationSnapStep
            );
        }

        transform.SetPositionAndRotation(
            finalPosition,
            Quaternion.Euler(finalEuler)
        );

        transformWasModifiedForRender = true;
    }

    private void OnEndCameraRendering(
        ScriptableRenderContext context,
        Camera camera
    )
    {
        if (camera != targetCamera)
            return;

        RestoreCleanTransform();
    }

    private void RestoreCleanTransform()
    {
        if (!transformWasModifiedForRender)
            return;

        transform.SetPositionAndRotation(
            cleanPosition,
            cleanRotation
        );

        transformWasModifiedForRender = false;
    }

    private float SnapValue(float value, float step)
    {
        return Mathf.Round(value / step) * step;
    }

    private float SnapAngle(float angle, float step)
    {
        return Mathf.Round(angle / step) * step;
    }
}