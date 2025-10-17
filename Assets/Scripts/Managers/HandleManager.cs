using UnityEngine;

public class HandleManager : MonoBehaviour
{
    public static HandleManager Instance { get; private set; }

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

    private void Start()
    {
    }

    private void Update()
    {
    }
}
