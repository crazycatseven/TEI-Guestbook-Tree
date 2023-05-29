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

        if (Input.GetKeyUp(KeyCode.B))
        {
            StartCoroutine(AnimateGrowthFactor());
        }
        
    }

    IEnumerator AnimateGrowthFactor()
    {
        float duration = 4.0f; // Duration in seconds
        float endValue = 0.999f;

        float elapsedTime = 0.0f;

        // Assuming that treeFromSkeleton.growthFactor starts at 0
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime; // Update time passed
            float newValue = Mathf.Lerp(0, endValue, elapsedTime / duration);
            treeFromSkeleton.growthFactor = newValue;
            treeFromSkeleton.treeUpdated = true;
            yield return null;
        }
        
        // Ensure that growthFactor reaches the endValue
        treeFromSkeleton.growthFactor = endValue;
        
    }


}
