using UnityEngine;
using TMPro;

public class KeyValueLabel : MonoBehaviour

{
    [SerializeField] private TextMeshProUGUI _keyLabel;
    [SerializeField] private TextMeshProUGUI _valueLabel;

    public void Init(string key, string value)
    {
        _keyLabel.text = key;
        _valueLabel.text = value;
    }
}
