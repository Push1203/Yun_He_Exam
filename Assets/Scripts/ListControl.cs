using UnityEngine;

public class ListControll : MonoBehaviour
{
    
    [Header("要展開/收合的清單面板")]
    public GameObject ListPanel;
    [Header("清單預設展開/收合")]
    public bool startExpanded = false;

    private void Start()
    {
        if (ListPanel != null)
        {
            ListPanel.SetActive(startExpanded);
        }
    }
    public void ToggleDropdown()
    {
        if (ListPanel != null)
        {
            bool nextState = !ListPanel.activeSelf;
            ListPanel.SetActive(nextState);
        }
    }
    
}
