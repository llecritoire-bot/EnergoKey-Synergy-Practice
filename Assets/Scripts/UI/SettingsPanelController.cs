using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SettingsPanelController : MonoBehaviour
{
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text volumeValueText;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private bool startHidden = true;

    private bool hasStarted;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        BindControls();
    }

    private void Start()
    {
        hasStarted = true;
        PrepareSlider();
        SyncFromSave();
        SetPanelVisible(!startHidden);
    }

    private void OnEnable()
    {
        if (hasStarted)
        {
            SyncFromSave();
        }
    }

    public void Configure(
        Slider slider,
        CanvasGroup panel = null,
        Button openTrigger = null,
        Button closeTrigger = null,
        Text valueLabel = null,
        bool hideInitially = true)
    {
        UnbindControls();
        volumeSlider = slider;
        panelGroup = panel;
        openButton = openTrigger;
        closeButton = closeTrigger;
        volumeValueText = valueLabel;
        startHidden = hideInitially;
        PrepareSlider();
        BindControls();
        SyncFromSave();
        SetPanelVisible(!startHidden);
    }

    public void OpenPanel()
    {
        if (panelGroup != null)
        {
            panelGroup.gameObject.SetActive(true);
        }

        SyncFromSave();
        SetPanelVisible(true);
    }

    public void ClosePanel()
    {
        SetPanelVisible(false);
    }

    public void SyncFromSave()
    {
        float savedVolume = SaveService.MasterVolume;
        AudioListener.volume = savedVolume;

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(savedVolume);
        }

        UpdateValueLabel(savedVolume);
    }

    public void SetVolume(float value)
    {
        float safeValue = Mathf.Clamp01(value);
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(safeValue);
        }

        SaveService.SetMasterVolume(safeValue);
        UpdateValueLabel(safeValue);
    }

    private void OnVolumeChanged(float value)
    {
        SetVolume(value);
    }

    private void PrepareSlider()
    {
        if (volumeSlider == null)
        {
            return;
        }

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;
    }

    private void SetPanelVisible(bool visible)
    {
        IsOpen = visible;
        if (panelGroup == null)
        {
            return;
        }

        panelGroup.alpha = visible ? 1f : 0f;
        panelGroup.interactable = visible;
        panelGroup.blocksRaycasts = visible;
    }

    private void UpdateValueLabel(float value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
        }
    }

    private void BindControls()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (openButton != null)
        {
            openButton.onClick.RemoveListener(OpenPanel);
            openButton.onClick.AddListener(OpenPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void UnbindControls()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        if (openButton != null)
        {
            openButton.onClick.RemoveListener(OpenPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }
    }

    private void OnDestroy()
    {
        UnbindControls();
    }
}
