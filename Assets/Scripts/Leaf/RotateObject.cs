using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 72f;  // 每秒旋转的角度数
    private bool rotationEnabled = true; // 新增变量，控制是否允许旋转

    private void Update()
    {
        // 按下小键盘上的0键，切换旋转开关
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            rotationEnabled = !rotationEnabled;
        }

        // 按下左方向键，手动向左旋转
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RotateLeft();
        }

        // 按下右方向键，手动向右旋转
        if (Input.GetKey(KeyCode.RightArrow))
        {
            RotateRight();
        }

        // 当旋转开关打开时，自动旋转
        if (rotationEnabled)
        {
            // 计算这一帧需要旋转的角度
            float angle = rotationSpeed * Time.deltaTime;

            // 在Y轴上旋转物体
            transform.Rotate(0, angle, 0);
        }
    }

    // 左旋转函数
    private void RotateLeft()
    {
        float angle = rotationSpeed * Time.deltaTime;
        transform.Rotate(0, -angle, 0); // 注意这里的旋转角度是负值，表示向左旋转
    }

    // 右旋转函数
    private void RotateRight()
    {
        float angle = rotationSpeed * Time.deltaTime;
        transform.Rotate(0, angle, 0);
    }
}
