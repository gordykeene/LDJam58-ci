using Game.Messages;
using UnityEngine;

// TODO: Juice the Change!
public class PickThreeController : OnMessage<BeginPickThree, ClosePickMenu>
{
    [SerializeField] private ExhibitPickerView _one;
    [SerializeField] private ExhibitPickerView _two;
    [SerializeField] private ExhibitPickerView _three;

    protected override void Execute(ClosePickMenu msg)
    {
        _one.gameObject.SetActive(false);
        _two.gameObject.SetActive(false);
        _three.gameObject.SetActive(false);
    }

    protected override void AfterEnable()
    {
        _one.gameObject.SetActive(false);
        _two.gameObject.SetActive(false);
        _three.gameObject.SetActive(false);
    }
    
    

    protected override void Execute(BeginPickThree msg)
    {
        _one.Init(msg.Exhibits[0]);
        _one.gameObject.SetActive(true);
        _two.Init(msg.Exhibits[1]);
        _two.gameObject.SetActive(true);
        _three.Init(msg.Exhibits[2]);
        _three.gameObject.SetActive(true);
    }
}
