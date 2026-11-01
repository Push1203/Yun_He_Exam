using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("對應道具")]
    public GameObject itemPrefab;

    [Header("指引線")]
    public LineRenderer guideLine;

    //從清單拖曳時產生的道具
    private GameObject previewObject;
    //道具上的 Rigidbody
    private Rigidbody previewRb;
    //主攝影機，用來追蹤滑鼠位置
    private Camera mainCamera;
    //判斷物體是否可以掉落在地面上
    private bool hasValidGroundHit = false;
    //紀錄指引線掉落在地面末端點
    private Vector3 currentHitPoint;
    //道具在空中的位置
    private Vector3 currentHoverPos;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (guideLine == null)
        {
            // 建立一個新的GameObject當作指引線
            GameObject lineObj = new GameObject("DragGuideLine");
            guideLine = lineObj.AddComponent<LineRenderer>();
            guideLine.startWidth = 0.5f;
            guideLine.endWidth = 0.2f;
            guideLine.positionCount = 2;
            guideLine.useWorldSpace = true;

            //無光照
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

            //指引線外觀顏色
            guideLine.material = new Material(shader);
            guideLine.startColor = Color.yellow;
            guideLine.endColor = Color.green;
            guideLine.enabled = false;
        }
    }

    
    //開始拖出道具
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemPrefab == null) return;

        // 生成道具
        previewObject = Instantiate(itemPrefab);
        previewRb = previewObject.GetComponent<Rigidbody>();

        // 拖曳期間開啟Kinematic，讓道具可以跟著滑鼠走
        if (previewRb != null) previewRb.isKinematic = true;

        // 暫時關閉碰撞，避免射線打中自身
        Collider col = previewObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        UpdatePreviewPosition(eventData.position);
    }

    //拖出道具ing
    public void OnDrag(PointerEventData eventData)
    {
        UpdatePreviewPosition(eventData.position);
    }

    //放掉道具，結束拖曳
    public void OnEndDrag(PointerEventData eventData)
    {
        //先隱藏指引線
        if (guideLine != null) guideLine.enabled = false;

        if (previewObject == null) return;

        //判斷目前座標是否位於螢幕UI中       
        bool isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

        // 若座標在UI上或落在空處，道具直接銷毀
        if (isPointerOverUI || !hasValidGroundHit)
        {
            Destroy(previewObject);
        }
        else
        {
            // 啟用 Collider
            Collider col = previewObject.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            // 確保有互動點擊腳本
            if (previewObject.GetComponent<PlacedObject>() == null)
            {
                previewObject.AddComponent<PlacedObject>();
            }

            // 從懸浮處自然釋放，給予微幅力道！
            if (previewRb != null)
            {
                previewRb.isKinematic = false; // 恢復重力

                // 計算從相機向前微拋的方向向量
                Vector3 throwDirection = (currentHitPoint - currentHoverPos).normalized + mainCamera.transform.forward * 0.3f;
                
                // 給予初速度與微小旋轉力矩，讓落體翻滾更自然
                previewRb.linearVelocity = throwDirection * 2f;
                previewRb.AddTorque(Random.insideUnitSphere * 1f, ForceMode.Impulse);
            }
        }

        previewObject = null;
        previewRb = null;
        hasValidGroundHit = false;
    }

    //更新滑鼠的座標位置
    private void UpdatePreviewPosition(Vector2 mousePosition)
    {
        if (previewObject == null || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500f))
        {
            //判斷是否打中地面
            if (hit.collider.GetComponent<Terrain>() != null || hit.collider.gameObject.name.ToLower().Contains("terrain"))
            {
                hasValidGroundHit = true;
                currentHitPoint = hit.point;

                // 保留足夠的掉落空間
                currentHoverPos = hit.point + Vector3.up * 3.5f;
                previewObject.transform.position = currentHoverPos;

                if (guideLine != null)
                {
                    guideLine.enabled = true;
                    guideLine.SetPosition(0, currentHoverPos);
                    guideLine.SetPosition(1, hit.point);
                }
                return;
            }
        }

        hasValidGroundHit = false;
        previewObject.transform.position = ray.origin + ray.direction * 15f;
        if (guideLine != null) guideLine.enabled = false;
    }
}