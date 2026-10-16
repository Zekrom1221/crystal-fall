using UnityEngine;
using Cinemachine;

public class CinemachineClamp : CinemachineExtension
{
    [Header("Ограничения по X")]
    public float minX = -50f;
    public float maxX = 150f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize)
        {
            var pos = state.RawPosition;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            state.RawPosition = pos;
        }
    }
}