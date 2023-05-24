using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Vector3 targetPosition = Vector3.zero; // 要聚焦的目标位置
    public Vector3 offset = new Vector3(0.0f, 0.0f, 0.0f); // 相机与目标点的偏移向量
    public Vector3 returnPosition = Vector3.zero; // 相机返回的位置
    public float moveSpeed = 1f; // 相机移动的速度
    public float rotationSpeed = 1f; // 相机旋转的速度

    private bool isMoving = false; // 控制相机移动的开关

    private int moveCount = 0;

    public TreeFromSkeleton treeFromSkeleton;

    private void Start()
    {
        treeFromSkeleton = FindObjectOfType<TreeFromSkeleton>();
    }

    public System.Collections.IEnumerator MoveCamera(Vector3 targetPos, float distance = 1.0f, Vector3 offset = default(Vector3))
    {
        isMoving = true; // 开始相机移动

        // 计算相机的初始位置和目标位置
        Vector3 startPosition = transform.position;

        // targetPos += offset; // 考虑偏移向量

        while (isMoving)
        {
            // 使用Lerp计算相机的新位置
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
            transform.position = newPosition;


            // 判断是否到达目标位置
            if (Vector3.Distance(transform.position, targetPos) < distance)
            {
                isMoving = false;
            }

            yield return null;
        }
    }

    public System.Collections.IEnumerator MoveCameraAndReturn(Vector3 targetPos, Vector3 returnPos, float delay = 0f)
    {
        isMoving = true; // 开始相机移动

        // 计算相机的初始位置和目标位置
        Vector3 startPosition = transform.position;
        targetPos += offset; // 考虑偏移向量

        while (isMoving)
        {
            // 使用Lerp计算相机的新位置
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
            transform.position = newPosition;

            // 判断是否到达目标位置
            if (Vector3.Distance(transform.position, targetPos) < 1.0f)
            {
                isMoving = false;
                if (delay > 0f)
                {
                    yield return new WaitForSeconds(delay);
                }
                yield return StartCoroutine(MoveCamera(returnPos, 0.02f));
            }

            yield return null;
        }
    }

    // 停止相机移动的方法
    public void StopCameraMovement()
    {
        if (moveCount == 2)
        {
            moveCount = 0;
            isMoving = false;
        }
        else
        {
            treeFromSkeleton.leavesNumber++;
            treeFromSkeleton.treeUpdated = true;
            isMoving = false;

            targetPosition = new Vector3(0, 3, -6);
            StartCoroutine(DelayedMoveCamera(2f));
        }
    }

    private IEnumerator DelayedMoveCamera(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(MoveCamera(targetPosition));
    }
}
