using UnityEngine;

public class Cheats : MonoBehaviour
{

#if UNITY_EDITOR
    private void Update()
    {
        var isHoldingLeftShift = Input.GetKey(KeyCode.LeftShift);
        if (isHoldingLeftShift && Input.GetKeyDown(KeyCode.I))
            EventPublisher.FadeInScene();

        if (isHoldingLeftShift && Input.GetKeyDown(KeyCode.F))
            EventPublisher.FadeOutScene();
    }
#endif
}

