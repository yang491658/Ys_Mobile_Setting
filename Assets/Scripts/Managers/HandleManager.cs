using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class HandleManager : MonoBehaviour
{
    public static HandleManager Instance { get; private set; }

    private Camera cam => Camera.main;
    private LayerMask layer;
    private float time;

    [Header("Click")]
    private const float doubleClick = 0.25f;
    private bool isDoubleClick;

    [Header("Drag")]
    private const float drag = 0.15f;
    private bool isDragging;
    private Vector2 dragStart;
    private Vector2 dragCurrent;

#if UNITY_EDITOR
    [Header("Mark")]
    private readonly List<Vector2> marks = new();
    private readonly List<float> markTimes = new();
    private readonly List<Color> markColors = new();
    [SerializeField] private float markDuration = 1f;
    [SerializeField] private float markRadius = 0.5f;
    [SerializeField] private int markSegment = 24;

    private readonly List<Vector2> dragPath = new();
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused) return;

#if UNITY_EDITOR
        HandleMouse();
        DrawDebug();
#else
        HandleTouch();
#endif
    }

#if UNITY_EDITOR
    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0)) HandleBegin(Input.mousePosition);
        else if (Input.GetMouseButton(0)) HandleMove(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) HandleEnd(Input.mousePosition);

        if (Input.GetMouseButtonDown(1)) OnRightClick(ScreenToWorld(Input.mousePosition));
        if (Input.GetMouseButtonDown(2)) OnMiddleClick(ScreenToWorld(Input.mousePosition));
    }
#endif

    private void HandleTouch()
    {
        if (Input.touchCount == 0) return;
        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began && !IsOverUI(t.fingerId))
            HandleBegin(t.position, t.fingerId);
        else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            HandleMove(t.position);
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            HandleEnd(t.position);
    }

    private bool IsOverUI(int fingerID = -1)
        => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerID);

    private Vector2 ScreenToWorld(Vector2 screenPos) => cam.ScreenToWorldPoint(screenPos);

    private bool CanSelect(RaycastHit2D _go)
    {
        if (layer == 0) return true;

        bool con1 = _go.collider != null;
        bool con2 = true; // TODO : 추가 조건

        return con1 && con2;
    }

    #region 구분
    private void HandleBegin(Vector2 _pos, int _fingerID = -1)
    {
        if (IsOverUI(_fingerID)) return;

        Vector2 worldPos = ScreenToWorld(_pos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, layer);

        if (CanSelect(hit))
        {
            isDragging = false;
            dragStart = worldPos;
            dragCurrent = dragStart;
#if UNITY_EDITOR
            dragPath.Clear();
            dragPath.Add(dragStart);
#endif
        }
    }

    private void HandleMove(Vector2 _pos)
    {
        Vector2 worldPos = ScreenToWorld(_pos);
        float distance = Vector2.Distance(dragStart, worldPos);

        if (!isDragging && distance >= drag)
        {
            isDragging = true;
            OnDragBegin(dragStart);
        }

        if (isDragging)
        {
            dragCurrent = worldPos;
            OnDragMove(dragStart, dragCurrent);
#if UNITY_EDITOR
            dragPath.Add(dragCurrent);
#endif
        }
    }

    private void HandleEnd(Vector2 _pos)
    {
        Vector2 worldPos = ScreenToWorld(_pos);

        if (isDragging)
        {
            float distance = Vector2.Distance(dragStart, worldPos);
            if (distance >= drag)
            {
                isDragging = false;
                OnDragEnd(dragStart, worldPos);
#if UNITY_EDITOR
                dragPath.Add(worldPos);
                dragPath.Clear();
#endif
                return;
            }
        }

        if (Time.time - time < doubleClick)
        {
            isDoubleClick = false;
            time = 0;
            OnDouble(worldPos);
        }
        else
        {
            isDoubleClick = true;
            time = Time.time;
            StartCoroutine(SingleCoroutine(worldPos));
        }

        isDragging = false;
#if UNITY_EDITOR
        dragPath.Clear();
#endif
    }

    private IEnumerator SingleCoroutine(Vector2 _pos)
    {
        yield return new WaitForSeconds(doubleClick);
        if (isDoubleClick)
        {
            isDoubleClick = false;
            OnSingle(_pos);
        }
    }
    #endregion

    #region 동작
    private void OnSingle(Vector2 _pos)
    {
        Debug.Log($"단순 터치 : {_pos}"); // TODO : 단순 터치 동작
#if UNITY_EDITOR
        AddClick(_pos, Color.cyan);
#endif
    }

    private void OnDouble(Vector2 _pos)
    {
        Debug.Log($"더블 터치 : {_pos}"); // TODO : 더블 터치 동작
#if UNITY_EDITOR
        AddClick(_pos, Color.blue);
#endif
    }

    private void OnDragBegin(Vector2 _pos)
    {
        Debug.Log($"드래그 시작 : {_pos}"); // TODO : 드래그 시작 동작
    }

    private void OnDragMove(Vector2 _start, Vector2 _current)
    {
        Debug.Log($"드래그 진행"); // TODO : 드래그 진행 동작
    }

    private void OnDragEnd(Vector2 _start, Vector2 _end)
    {
        Debug.Log($"드래그 종료 : {_start} → {_end}"); // TODO : 드래그 종료 동작
    }

#if UNITY_EDITOR
    private void OnRightClick(Vector2 _pos)
    {
        Debug.Log($"우클릭 : {_pos}"); // TODO : 우클릭 동작
        AddClick(_pos, Color.yellow);
    }

    private void OnMiddleClick(Vector2 _pos)
    {
        Debug.Log($"휠클릭 : {_pos}"); // TODO : 휠클릭 동작
        AddClick(_pos, Color.red);
    }

    private void AddClick(Vector2 _pos, Color _color)
    {
        marks.Add(_pos);
        markTimes.Add(Time.time + markDuration);
        markColors.Add(_color);
    }

    private void DrawDebug()
    {
        for (int i = markTimes.Count - 1; i >= 0; i--)
        {
            if (Time.time > markTimes[i])
            {
                int last = markTimes.Count - 1;
                (markTimes[i], markTimes[last]) = (markTimes[last], markTimes[i]);
                (marks[i], marks[last]) = (marks[last], marks[i]);
                (markColors[i], markColors[last]) = (markColors[last], markColors[i]);
                markTimes.RemoveAt(last);
                marks.RemoveAt(last);
                markColors.RemoveAt(last);
                continue;
            }

            Vector2 center = marks[i];
            Color c = markColors[i];
            for (int s = 0; s < markSegment; s++)
            {
                float a0 = (Mathf.PI * 2f) * s / markSegment;
                float a1 = (Mathf.PI * 2f) * (s + 1) / markSegment;
                Vector2 p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * markRadius;
                Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * markRadius;
                Debug.DrawLine(p0, p1, c);
            }
        }

        if (isDragging)
        {
            Debug.DrawLine(dragStart, dragCurrent, Color.green);

            for (int i = 1; i < dragPath.Count; i++)
                Debug.DrawLine(dragPath[i - 1], dragPath[i], Color.magenta);
        }
    }
#endif
    #endregion
}
