using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeTrajectory : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int resolution = 30; // 轨迹的分辨率
    public Transform throwPoint; // 投掷点
    public float throwForce = 10f; // 投掷力度
    public float timeStep = 0.1f; // 轨迹点之间的时间步长
    public LayerMask collisionMask; // 用于检测轨迹碰撞

    private void Start()
    {
        lineRenderer.positionCount = resolution;
    }

    void Update()
    {
        SimulateTrajectory();
    }

    void SimulateTrajectory()
    {
        Vector3[] trajectoryPoints = new Vector3[resolution];
        Vector3 startPoint = throwPoint.position;
        Vector3 startVelocity = throwPoint.forward * throwForce;

        for (int i = 0; i < resolution; i++)
        {
            float time = i * timeStep;
            Vector3 point = startPoint + startVelocity * time + 0.5f * Physics.gravity * time * time;
            trajectoryPoints[i] = point;

            // 检测碰撞
            if (i > 0)
            {
                if (Physics.Linecast(trajectoryPoints[i - 1], trajectoryPoints[i], out RaycastHit hit, collisionMask))
                {
                    // 如果检测到碰撞，调整轨迹并停止计算
                    trajectoryPoints[i] = hit.point;
                    Array.Resize(ref trajectoryPoints, i + 1);
                    break;
                }
            }
        }

        // 更新LineRenderer的位置
        lineRenderer.positionCount = trajectoryPoints.Length;
        lineRenderer.SetPositions(trajectoryPoints);
    }
}
