using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyEvent : MonoBehaviour
{

    public TreeFromSkeleton treeFromSkeleton;

    // Start is called before the first frame update
    void Start()
    {
        // 查找TreeFromSkeleton脚本所在的GameObject
        treeFromSkeleton = FindObjectOfType<TreeFromSkeleton>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            treeFromSkeleton.AddLeafEvent();
        }
    }
}
