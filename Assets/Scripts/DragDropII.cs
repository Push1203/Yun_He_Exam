using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropII : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("對應道具")]
    public GameObject itemPrefab;

    [Header("拋出力道")]
    public float launchForce = 14f;

    [Header("拋物線仰角")]
    [Range(0.1f, 1f)]
    public float launchUpAngle = 0.45f;

    [Header("指引線")]
    public LineRenderer guideLine;

    //從清單拖曳時產生的道具
    private GameObject previewObject;
    //道具上的 Rigidbody
    private Rigidbody previewRb;
    //主攝影機，追蹤滑鼠位置
    private Camera mainCamera;
    //判斷物體是否可以掉落在地面上
    private bool hasValidGroundHit = false;
    //紀錄指引線掉落在地面末端點
    private Vector3 currentHitPoint;
    //拋出速度
    private Vector3 launchVelocity;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (guideLine == null)
        {
            // 建立一個新的GameObject當作指引線
            GameObject lineObj = new GameObject("DragParabolaLine");
            guideLine = lineObj.AddComponent<LineRenderer>();
            
            
            guideLine.startWidth = 0.25f;
            guideLine.endWidth = 0.18f;
            guideLine.useWorldSpace = true;

            Shader shader = Shader.Find("Hidden/Internal-Colored");
            
            guideLine.material = new Material(shader);
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

        UpdateParabolaPreview(eventData.position);
    }

    //拖出道具ing
    public void OnDrag(PointerEventData eventData)
    {
        UpdateParabolaPreview(eventData.position);
    }

    //放掉道具，結束拖曳
    public void OnEndDrag(PointerEventData eventData)
    {
        // 關閉指引線
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

            if (previewRb != null)
            {
                previewRb.isKinematic = false;

                // 給予初速度與微小旋轉力矩
                previewRb.linearVelocity = launchVelocity;
                previewRb.AddTorque(Random.insideUnitSphere * 1f, ForceMode.Impulse);
            }
        }

        previewObject = null;
        previewRb = null;
        hasValidGroundHit = false;
    }

    // 更新指引線預覽
    private void UpdateParabolaPreview(Vector2 screenPosition)
    {
        if (previewObject == null || mainCamera == null) return;

        Ray mouseRay = mainCamera.ScreenPointToRay(screenPosition);
        Vector3 spawnPos = mouseRay.origin + mouseRay.direction * 5.5f;
        previewObject.transform.position = spawnPos;

        Vector3 forwardDir = mouseRay.direction;
        Vector3 throwDir = (forwardDir + Vector3.up * launchUpAngle).normalized;
        launchVelocity = throwDir * launchForce;

        int maxSteps = 35;
        float timeStep = 0.05f;
        List<Vector3> arcPoints = new List<Vector3>();
        arcPoints.Add(spawnPos);

        Vector3 currentPos = spawnPos;
        Vector3 currentVel = launchVelocity;
        Vector3 gravity = Physics.gravity;

        hasValidGroundHit = false;

        for (int i = 0; i < maxSteps; i++)
        {
            Vector3 nextPos = currentPos + currentVel * timeStep + 0.5f * gravity * timeStep * timeStep;
            currentVel += gravity * timeStep;

            RaycastHit hit;
            if (Physics.Linecast(currentPos, nextPos, out hit))
            {
                if (hit.collider.GetComponent<Terrain>() != null || hit.collider.gameObject.name.ToLower().Contains("terrain"))
                {
                    hasValidGroundHit = true;
                    currentHitPoint = hit.point;
                    arcPoints.Add(hit.point);
                    break;
                }
            }

            arcPoints.Add(nextPos);
            currentPos = nextPos;
        }

        if (guideLine != null)
        {
            guideLine.enabled = true;
            guideLine.positionCount = arcPoints.Count;
            guideLine.SetPositions(arcPoints.ToArray());

            //亮藍 ➔ 鮮亮純綠色
            Gradient gradient = new Gradient();
            if (hasValidGroundHit)
            {
                gradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(new Color(0f, 0.8f, 1f), 0.0f),  // 0% 起點：鮮亮青藍色 (Electric Cyan)
                        new GradientColorKey(new Color(0f, 1f, 0.2f), 1.0f)   // 100% 終點：鮮明翡翠綠 (Bright Green)
                    },
                    new GradientAlphaKey[] { 
                        new GradientAlphaKey(1.0f, 0.0f), 
                        new GradientAlphaKey(1.0f, 1.0f) 
                    }
                );
            }
            else
            {
                // 未命中地面時半透明紅色
                gradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(new Color(1f, 0.2f, 0.2f), 0.0f), 
                        new GradientColorKey(new Color(1f, 0.5f, 0.1f), 1.0f) 
                    },
                    new GradientAlphaKey[] { 
                        new GradientAlphaKey(0.8f, 0.0f), 
                        new GradientAlphaKey(0.3f, 1.0f) 
                    }
                );
            }

            guideLine.colorGradient = gradient;
        }
    }
}