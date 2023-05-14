using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Vector3 targetPosition = Vector3.zero; // 要聚焦的目标位置
    public Vector3 offset = new Vector3(0.0f, 0.0f, 0.5f); // 相机与目标点的偏移向量
    public float moveSpeed = 1f; // 相机移动的速度
    public float rotationSpeed = 1f; // 相机旋转的速度

    private bool isMoving = false; // 控制相机移动的开关

    private int moveCount = 0;

    public TreeFromSkeleton treeFromSkeleton;

    private void Start()
    {
        // treeFromSkeleton = GameObject.Find("GameObject").GetComponent<TreeFromSkeleton>();

        treeFromSkeleton = FindObjectOfType<TreeFromSkeleton>();

        Debug.Log(treeFromSkeleton);
    }

    public System.Collections.IEnumerator MoveCamera()
    {
        isMoving = true; // 开始相机移动
        moveCount++;

        // 计算相机的初始位置和目标位置
        Vector3 startPosition = transform.position;
        Vector3 targetPos = targetPosition + offset; // 考虑偏移向量

        while (isMoving)
        {
            // 使用Lerp计算相机的新位置
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
            transform.position = newPosition;

            // // 使用Slerp平滑旋转相机朝向目标点
            // Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
            // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 判断是否到达目标位置
            if (Vector3.Distance(transform.position, targetPos) < 1.0f)
            {
                StopCameraMovement();
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
        else{

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
        StartCoroutine(MoveCamera());
    }
}
