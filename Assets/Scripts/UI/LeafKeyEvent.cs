using UnityEngine;
using UnityEngine.SceneManagement;

public class LeafKeyEvent : MonoBehaviour
{
    public CustomLeafShape leafShape;
    public TreeFromSkeleton treeFromSkeleton;

    // 添加一个枚举类型以跟踪每个属性的当前状态
    enum Direction
    {
        Increase,
        Decrease
    }

    // 为每个属性创建一个变化方向的状态
    Direction[] directions = new Direction[9];

    // 每次属性变化的大小
    private const float increment = 0.001f;

    void Start()
    {

        // 查找TreeFromSkeleton脚本所在的GameObject
        treeFromSkeleton = FindObjectOfType<TreeFromSkeleton>();

        // 初始化所有方向为“增加”
        for (int i = 0; i < directions.Length; i++)
        {
            directions[i] = Direction.Increase;
        }

    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) // 如果回车键被按下
        {
            LeafData data = leafShape.SaveLeafData("Assets/Data/leafData.csv");
            treeFromSkeleton.AddLeafEventContinue(data);
            return;
        }

        // 根据键盘输入更新每个属性
        for (int i = 0; i < 9; i++)
        {


            KeyCode key = KeyCode.None;
            if (i >= 0 && i <= 8)
            {
                // 使用大键盘上的数字键
                key = KeyCode.Alpha1 + i;
            }
            else if (i == 9)
            {
                // 使用小键盘上的数字键0
                key = KeyCode.Keypad0;
            }
            

            if (Input.GetKey(key))
            {
                IncrementValue(i);
            }

            // 同时处理小键盘上的数字键
            KeyCode numpadKey = KeyCode.Keypad1 + i;
            if (Input.GetKey(numpadKey))
            {
                IncrementValue(i);
            }

            // KeyCode key = KeyCode.Keypad1 + i; // 对应小键盘的数字键
            // if (Input.GetKey(key))
            // {
            //     IncrementValue(i);
            // }


            
        }
    }

    // 函数：根据键盘输入更新属性值
    void IncrementValue(int attributeIndex)
    {
        float value = 0f;
        float min = 0f, max = 1f;

        switch (attributeIndex)
        {
            case 0:
                value = leafShape.controlPointX;
                break;
            case 1:
                value = leafShape.controlPointY;
                break;
            case 2:
                value = leafShape.controlPoint2X;
                break;
            case 3:
                value = leafShape.controlPoint2Y;
                break;
            case 4:
                value = leafShape.leafHeight;
                break;
            case 5:
                value = leafShape.leafThickness;
                break;
            case 6:
                value = leafShape.leafHue;
                break;
            case 7:
                value = leafShape.leafSaturation;
                break;
            case 8:
                value = leafShape.leafBrightness;
                break;
        }

        // 根据当前方向增加或减少属性值
        if (directions[attributeIndex] == Direction.Increase)
        {
            value += increment;
            if (value >= max) // 如果达到了上限，反转方向
            {
                directions[attributeIndex] = Direction.Decrease;
                value = max; // 保持值在上限
            }
        }
        else // 如果方向是“减少”
        {
            value -= increment;
            if (value <= min) // 如果达到了下限，反转方向
            {
                directions[attributeIndex] = Direction.Increase;
                value = min; // 保持值在下限
            }
        }

        // 将新值回写到属性中
        switch (attributeIndex)
        {
            case 0:
                leafShape.controlPointX = value;
                break;
            case 1:
                leafShape.controlPointY = value;
                break;
            case 2:
                leafShape.controlPoint2X = value;
                break;
            case 3:
                leafShape.controlPoint2Y = value;
                break;
            case 4:
                leafShape.leafHeight = value;
                break;
            case 5:
                leafShape.leafThickness = value;
                break;
            case 6:
                leafShape.leafHue = value;
                break;
            case 7:
                leafShape.leafSaturation = value;
                break;
            case 8:
                leafShape.leafBrightness = value;
                break;
        }
    }
}
