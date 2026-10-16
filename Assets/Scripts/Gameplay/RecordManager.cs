using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }

    private const string Key = "BestTime";

    public float BestTime { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BestTime = PlayerPrefs.GetFloat(Key, 0f);
    }

    public bool TrySetRecord(float time)
    {
        if (time > BestTime)
        {
            BestTime = time;
            PlayerPrefs.SetFloat(Key, BestTime);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
}
