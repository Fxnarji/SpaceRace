using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RingCounter : MonoBehaviour , IRingCounter
{
    [SerializeField] private TMP_Text text;
    private int ringCount = 0;
    
    public void CollectRing()
    {
        ringCount++;
        text.text = ringCount.ToString();
    }
}

public interface IRingCounter
{
    void CollectRing();
}