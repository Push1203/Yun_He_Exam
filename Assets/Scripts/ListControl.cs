using UnityEngine;

public class ListControl : MonoBehaviour
{
    
    [Header("道具清單")]
    public GameObject ListPanel;
    [Header("道具清單預設展開or收起")]
    public bool startExpanded = false;

    private void Start()
    {
        if (ListPanel != null)
        {
            ListPanel.SetActive(startExpanded);
        }
    }
    public void Dropdown()
    {
        if (ListPanel != null)
        {
            bool nextState = !ListPanel.activeSelf;
            ListPanel.SetActive(nextState);
        }
    }
    
}
