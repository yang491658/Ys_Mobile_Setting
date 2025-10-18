using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private GameObject basePrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPos;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (basePrefab == null)
            basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Base.prefab");

        if (spawnPos == null)
            spawnPos = transform.Find("SpawnPos");
    }
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

    #region 소환
    #endregion

    #region 제거
    #endregion

    #region 동작
    #endregion

    #region SET
    #endregion

    #region GET
    #endregion
}
