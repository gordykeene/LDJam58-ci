using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts;
using System.Linq;

public class ExhibitPickerView : MonoBehaviour
{
    [SerializeField] private Image _exhibitImage;
    [SerializeField] private KeyValueLabel _exhibitNameLabel;
    [SerializeField] private KeyValueLabel _sizeLabel;
    [SerializeField] private KeyValueLabel _rarityLabel;
    [SerializeField] private KeyValueLabel _enjoymentLabel;
    [SerializeField] private KeyValueLabel _popularityLabel;
    [SerializeField] private TextMeshProUGUI _tagsLabel;

    public void Init(ExhibitTileType exhibits)
    {
        //_exhibitImage.sprite = exhibits.Sprite;
        _exhibitNameLabel.Init("Name", exhibits.DisplayName);
        _sizeLabel.Init("Size", exhibits.Size.x + "x" + exhibits.Size.y);
        _rarityLabel.Init("Rarity", exhibits.Rarity.ToString());
        _enjoymentLabel.Init("Enjoyment", exhibits.Enjoyment.ToString());
        _popularityLabel.Init("Popularity", exhibits.Popularity.ToString());
        _tagsLabel.text = string.Join(", ", exhibits.Tags.Select(t => {
            var s = t.ToString();
            var noUnderscore = s.Contains("_") ? s.Substring(s.IndexOf('_') + 1) : s;
            var withSpaces = System.Text.RegularExpressions.Regex.Replace(noUnderscore, "(\\B[A-Z])", " $1");
            return withSpaces;
        }));
    }
}
