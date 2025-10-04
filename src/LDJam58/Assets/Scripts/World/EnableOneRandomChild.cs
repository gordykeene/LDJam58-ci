using UnityEngine;

public class EnableOneRandomChild : MonoBehaviour
{
    private void Start()
    {
        var childCount = transform.childCount;
        if (childCount <= 0) return;

        var selectedIndex = Random.Range(0, childCount);
        for (var i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            child.SetActive(i == selectedIndex);
        }
    }
}
