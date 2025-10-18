using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public event System.Action<bool> OnOpenUI;

    [Header("InGame UI")]
    [SerializeField] private GameObject inGameUI;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Setting UI")]
    [SerializeField] private GameObject settingUI;
    [SerializeField] private TextMeshProUGUI settingScoreText;

    [Header("Sound UI")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Image bgmIcon;
    [SerializeField] private Image sfxIcon;
    [SerializeField] private List<Sprite> bgmIcons = new List<Sprite>();
    [SerializeField] private List<Sprite> sfxIcons = new List<Sprite>();

    [Header("Confirm UI")]
    [SerializeField] private GameObject confirmUI;
    [SerializeField] private TextMeshProUGUI confirmText;
    private System.Action confirmAction;

    [Header("Game Over UI")]
    [SerializeField] private GameObject resultUI;
    [SerializeField] private TextMeshProUGUI resultScoreText;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (inGameUI == null)
            inGameUI = GameObject.Find("InGameUI");
        if (scoreText == null)
            scoreText = GameObject.Find("InGameUI/Score/ScoreText").GetComponent<TextMeshProUGUI>();

        if (settingUI == null)
            settingUI = GameObject.Find("SettingUI");
        if (settingScoreText == null)
            settingScoreText = GameObject.Find("SettingUI/Box/Score/ScoreText").GetComponent<TextMeshProUGUI>();

        if (bgmSlider == null)
            bgmSlider = GameObject.Find("BGM/BgmSlider").GetComponent<Slider>();
        if (sfxSlider == null)
            sfxSlider = GameObject.Find("SFX/SfxSlider").GetComponent<Slider>();
        if (bgmIcon == null)
            bgmIcon = GameObject.Find("BGM/BgmBtn/BgmIcon").GetComponent<Image>();
        if (sfxIcon == null)
            sfxIcon = GameObject.Find("SFX/SfxBtn/SfxIcon").GetComponent<Image>();

        bgmIcons.Clear();
        LoadSprite(bgmIcons, "White Music");
        LoadSprite(bgmIcons, "White Music Off");
        sfxIcons.Clear();
        LoadSprite(sfxIcons, "White Sound On");
        LoadSprite(sfxIcons, "White Sound Icon");
        LoadSprite(sfxIcons, "White Sound Off 2");

        if (confirmUI == null)
            confirmUI = GameObject.Find("ConfirmUI");
        if (confirmText == null)
            confirmText = GameObject.Find("ConfirmUI/Box/ConfirmText").GetComponent<TextMeshProUGUI>();

        if (resultUI == null)
            resultUI = GameObject.Find("ResultUI");
        if (resultScoreText == null)
            resultScoreText = GameObject.Find("ResultUI/Score/ScoreText").GetComponent<TextMeshProUGUI>();
    }

    private static void LoadSprite(List<Sprite> _list, string _sprite)
    {
        if (string.IsNullOrEmpty(_sprite)) return;
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Imports/Dark UI/Icons" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in assets)
            {
                var s = obj as Sprite;
                if (s != null && s.name == _sprite)
                {
                    _list.Add(s);
                    return;
                }
            }
        }
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

    private void Start()
    {
        UpdateScore(GameManager.Instance.GetTotalScore());
    }

    private void OnEnable()
    {
        GameManager.Instance.OnChangeScore += UpdateScore;

        SoundManager.Instance.OnChangeVolume += UpdateVolume;
        bgmSlider.value = SoundManager.Instance.GetBGMVolume();
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
        sfxSlider.value = SoundManager.Instance.GetSFXVolume();
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);

        OnOpenUI += GameManager.Instance.Pause;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnChangeScore -= UpdateScore;

        SoundManager.Instance.OnChangeVolume -= UpdateVolume;
        bgmSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetBGMVolume);
        sfxSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetSFXVolume);

        OnOpenUI -= GameManager.Instance.Pause;
    }

    #region 오픈
    public void OpenUI(bool _on)
    {
        OpenResult(_on);
        OpenConfirm(_on);
        OpenSetting(_on);
    }

    public void OpenSetting(bool _on)
    {
        if (settingUI == null) return;

        OnOpenUI?.Invoke(_on);

        inGameUI.SetActive(!_on);
        settingUI.SetActive(_on);
    }

    public void OpenConfirm(bool _on, string _text = null, System.Action _action = null, bool _pass = false)
    {
        if (confirmUI == null) return;

        if (!_pass)
        {
            confirmUI.SetActive(_on);
            confirmText.text = $"{_text}하시겠습니까?";
            confirmAction = _action;
        }

        if (!_on) confirmAction = null;

        if (_pass) _action?.Invoke();
    }

    public void OpenResult(bool _on)
    {
        if (resultUI == null) return;

        OnOpenUI?.Invoke(_on);

        inGameUI.SetActive(!_on);
        settingUI.SetActive(!_on);
        confirmUI.SetActive(!_on);

        resultUI.SetActive(_on);
    }
    #endregion

    #region 업데이트
    public void UpdateScore(int _score)
    {
        scoreText.text = _score.ToString("0000");
        settingScoreText.text = _score.ToString("0000");
        resultScoreText.text = _score.ToString("0000");
    }

    private void UpdateVolume(SoundType _type, float _volume)
    {
        switch (_type)
        {
            case SoundType.BGM:
                if (!Mathf.Approximately(bgmSlider.value, _volume))
                    bgmSlider.value = _volume;
                break;

            case SoundType.SFX:
                if (!Mathf.Approximately(sfxSlider.value, _volume))
                    sfxSlider.value = _volume;
                break;

            default:
                return;
        }
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (bgmIcons.Count >= 2)
            bgmIcon.sprite = SoundManager.Instance.IsBGMMuted() ? bgmIcons[1] : bgmIcons[0];

        if (sfxIcons.Count >= 3)
        {
            if (SoundManager.Instance.IsSFXMuted())
                sfxIcon.sprite = sfxIcons[2];
            else if (SoundManager.Instance.GetSFXVolume() < 0.2f)
                sfxIcon.sprite = sfxIcons[1];
            else
                sfxIcon.sprite = sfxIcons[0];
        }
    }
    #endregion

    #region 버튼
    public void OnClickSetting() => OpenSetting(true);
    public void OnClickClose() => OpenUI(false);

    public void OnClickBGM() => SoundManager.Instance.ToggleBGM();
    public void OnClickSFX() => SoundManager.Instance.ToggleSFX();

    public void OnClickReplay() => OpenConfirm(true, "다시", GameManager.Instance.Replay);
    public void OnClickQuit() => OpenConfirm(true, "종료", GameManager.Instance.Quit);

    public void OnClickReplayByPass() => OpenConfirm(true, "다시", GameManager.Instance.Replay, true);
    public void OnClickQuitByPass() => OpenConfirm(true, "종료", GameManager.Instance.Quit, true);

    public void OnClickOkay() => confirmAction?.Invoke();
    public void OnClickCancel() => OpenConfirm(false);
    #endregion

    private IEnumerator PlayClickThen(System.Action _action)
    {
        SoundManager.Instance.Button();
        float len = SoundManager.Instance.GetSFXLength("Button");
        if (len > 0f) yield return new WaitForSecondsRealtime(len);
        _action?.Invoke();
    }

    #region GET
    public bool GetOnSetting() => settingUI.activeSelf;
    public bool GetOnConfirm() => confirmUI.activeSelf;
    public bool GetOnResult() => resultUI.activeSelf;
    #endregion
}
