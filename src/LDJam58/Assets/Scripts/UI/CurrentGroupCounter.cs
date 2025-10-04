using UnityEngine;
using TMPro;

// TODO: Juice the Change!
public class TargetGroupCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counterText;

    private void OnEnable()
    {
        CurrentGameState.Subscribe(OnGameStateChanged, this);
    }

    private void OnDisable()
    {
        CurrentGameState.Unsubscribe(this);
    }

    private void Start()
    {
        UpdateCounter(CurrentGameState.ReadOnly.currentNumVisitingGroups);
    }

    private void OnGameStateChanged(GameStateChanged msg)
    {
        UpdateCounter(msg.State.currentNumVisitingGroups);
    }

    private void UpdateCounter(int newValue)
    {
        _counterText.text = "Visitor Groups: " + newValue.ToString();
    }
}

