using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int lineToChange;
    public TextController target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        target.currentLine = lineToChange;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target.currentLine = 0;
    }
}
