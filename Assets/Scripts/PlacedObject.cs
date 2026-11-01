using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    private float fallDestroyY = -10f;

    private GameObject actionMenuInstance;
    private static GameObject currentActiveMenu;

    private void Update()
    {
        // 低於邊界自動銷毀
        if (transform.position.y < fallDestroyY)
        {
            Destroy(gameObject);
        }
    }

    private void OnMouseDown()
    {
        if (currentActiveMenu != null)
        {
            Destroy(currentActiveMenu);
        }

        CreateActionMenu();
    }

    private void CreateActionMenu()
    {
        // 建立刪除選單，道具上方偏右
        actionMenuInstance = new GameObject("ActionMenu_" + gameObject.name);
        currentActiveMenu = actionMenuInstance;
        actionMenuInstance.transform.position = transform.position + Vector3.up * 2.0f + Vector3.right * 1.2f;

        // 建立紅色按鈕
        GameObject deleteBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deleteBtn.name = "DeleteButton";
        deleteBtn.transform.parent = actionMenuInstance.transform;
        deleteBtn.transform.localPosition = Vector3.zero;
        deleteBtn.transform.localScale = new Vector3(5.5f, 2.0f, 0.1f);

        // 紅底材質
        Renderer r = deleteBtn.GetComponent<Renderer>();
        r.material.color = new Color(0.92f, 0.2f, 0.2f);

        // 建立文字
        GameObject textObj = new GameObject("Label");
        textObj.transform.parent = actionMenuInstance.transform;
        textObj.transform.localPosition = new Vector3(0, 0, -0.1f);
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = "Delete";
        textMesh.fontSize = 50;         
        textMesh.characterSize = 0.33f;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.white;
        textMesh.fontStyle = FontStyle.Bold;

        // 刪除觸發
        DeleteTrigger trigger = deleteBtn.AddComponent<DeleteTrigger>();
        trigger.targetObject = this.gameObject;
        trigger.menuRoot = actionMenuInstance;

        if (Camera.main != null)
        {
            actionMenuInstance.transform.rotation = Camera.main.transform.rotation;
        }
    }

    private void OnDestroy()
    {
        if (actionMenuInstance != null)
        {
            Destroy(actionMenuInstance);
        }
    }
}

// 刪除道具
public class DeleteTrigger : MonoBehaviour
{
    public GameObject targetObject;
    public GameObject menuRoot;

    private void OnMouseDown()
    {
        if (menuRoot != null) Destroy(menuRoot);
        if (targetObject != null) Destroy(targetObject);
    }
}