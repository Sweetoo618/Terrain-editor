using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIControlManager))]
public class TerrainUIManagerCombined : MonoBehaviour
{
    [Header("=== Core Dependencies ===")]
    [Tooltip("地形编辑器核心组件，负责实际的地形修改操作")]
    public TerrainEditor terrainEditor;
    
    [Tooltip("相机控制器，负责视角移动和旋转")]
    public CameraController cameraController;

    [Space(10)]
    [Header("=== Mode Buttons ===")]
    [Tooltip("升高/降低模式按钮")]
    public Button btnRaiseLower;
    
    [Tooltip("粗糙化模式按钮")]
    public Button btnRoughen;
    
    [Tooltip("平整模式按钮")]
    public Button btnFlatten;
    
    [Tooltip("平滑模式按钮")]
    public Button btnSmooth;

    [Space(5)]
    [Header("Button Colors")]
    [Tooltip("按钮激活状态颜色")]
    public Color activeButtonColor = new Color(0.2f, 0.6f, 1.0f);
    
    [Tooltip("按钮激活状态悬停颜色")]
    public Color activeButtonHover = new Color(0.3f, 0.7f, 1.0f);
    
    [Tooltip("按钮激活状态按下颜色")]
    public Color activeButtonPressed = new Color(0.1f, 0.5f, 0.9f);
    
    [Tooltip("按钮非激活状态颜色")]
    public Color inactiveButtonColor = new Color(0.5f, 0.5f, 0.5f);
    
    [Tooltip("按钮非激活状态悬停颜色")]
    public Color inactiveButtonHover = new Color(0.6f, 0.6f, 0.6f);
    
    [Tooltip("按钮非激活状态按下颜色")]
    public Color inactiveButtonPressed = new Color(0.4f, 0.4f, 0.4f);

    [Space(10)]
    [Header("=== Parameter Panels ===")]
    [Tooltip("升高/降低面板")]
    public GameObject panelRaiseLower;
    
    [Tooltip("粗糙化面板")]
    public GameObject panelRoughen;
    
    [Tooltip("平整面板")]
    public GameObject panelFlatten;
    
    [Tooltip("平滑面板")]
    public GameObject panelSmooth;
    
    [Tooltip("相机设置面板（始终显示）")]
    public GameObject panelCamera;

    [Space(10)]
    [Header("=== Brush Controls ===")]
    [Tooltip("笔刷半径滑块")]
    public Slider radiusSlider;
    
    [Tooltip("笔刷半径标签")]
    public TextMeshProUGUI radiusLabel;
    
    [Tooltip("笔刷强度滑块")]
    public Slider strengthSlider;
    
    [Tooltip("笔刷强度标签")]
    public TextMeshProUGUI strengthLabel;

    [Space(5)]
    [Header("Brush Settings")]
    [Tooltip("笔刷半径范围最小值")]
    public float minBrushRadius = 0.1f;
    
    [Tooltip("笔刷半径范围最大值")]
    public float maxBrushRadius = 10.0f;
    
    [Tooltip("笔刷强度范围最小值")]
    public float minBrushStrength = 0.1f;
    
    [Tooltip("笔刷强度范围最大值")]
    public float maxBrushStrength = 5.0f;

    [Space(10)]
    [Header("=== Roughen Controls ===")]
    [Tooltip("粗糙化强度滑块")]
    public Slider roughenSlider;
    
    [Tooltip("粗糙化强度标签")]
    public TextMeshProUGUI roughenLabel;
    
    [Tooltip("粗糙化面板专用半径滑块（可选）")]
    public Slider radiusSliderRoughen;

    [Space(5)]
    [Header("Roughen Settings")]
    [Tooltip("粗糙化范围最小值")]
    public float minRoughenAmount = 0f;
    
    [Tooltip("粗糙化范围最大值")]
    public float maxRoughenAmount = 1f;

    [Space(10)]
    [Header("=== Flatten Controls ===")]
    [Tooltip("平整强度滑块")]
    public Slider strengthSliderFlatten;
    
    [Tooltip("平整强度标签")]
    public TextMeshProUGUI strengthLabelFlatten;

    [Space(10)]
    [Header("=== Smooth Controls ===")]
    [Tooltip("平滑强度滑块")]
    public Slider strengthSliderSmooth;
    
    [Tooltip("平滑强度标签")]
    public TextMeshProUGUI strengthLabelSmooth;

    [Space(10)]
    [Header("=== Camera Controls ===")]
    [Tooltip("移动速度滑块")]
    public Slider moveSpeedSlider;
    
    [Tooltip("移动速度标签")]
    public TextMeshProUGUI moveSpeedLabel;
    
    [Tooltip("旋转速度滑块")]
    public Slider rotationSpeedSlider;
    
    [Tooltip("旋转速度标签")]
    public TextMeshProUGUI rotationSpeedLabel;
    
    [Tooltip("垂直速度滑块")]
    public Slider verticalSpeedSlider;
    
    [Tooltip("垂直速度标签")]
    public TextMeshProUGUI verticalSpeedLabel;

    [Space(5)]
    [Header("Camera Settings")]
    [Tooltip("移动速度范围最小值")]
    public float minMoveSpeed = 10f;
    
    [Tooltip("移动速度范围最大值")]
    public float maxMoveSpeed = 100f;
    
    [Tooltip("旋转速度范围最小值")]
    public float minRotationSpeed = 50f;
    
    [Tooltip("旋转速度范围最大值")]
    public float maxRotationSpeed = 1000f;
    
    [Tooltip("垂直速度范围最小值")]
    public float minVerticalSpeed = 10f;
    
    [Tooltip("垂直速度范围最大值")]
    public float maxVerticalSpeed = 100f;

    [Space(10)]
    [Header("=== Status Display ===")]
    [Tooltip("当前模式显示标签")]
    public TextMeshProUGUI modeLabel;

    [Space(10)]
    [Header("=== Undo/Redo ===")]
    [Tooltip("撤销按钮")]
    public Button btnUndo;
    
    [Tooltip("重做按钮")]
    public Button btnRedo;

    [Space(10)]
    [Header("=== Accessibility ===")]
    [Tooltip("启用键盘导航")]
    public bool enableKeyboardNavigation = true;
    
    [Tooltip("启用实时同步到UIControlManager")]
    public bool syncWithControlManager = true;

    private UIControlManager _controlManager;

    void Start()
    {
        InitializeDependencies();
        StartCoroutine(DelayedInitialize());
    }
    
    IEnumerator DelayedInitialize()
    {
        yield return null;
        
        SetupSliders();
        SetupButtons();
        SetupCameraSliders();
        SetupUndoRedoButtons();
        ApplyStyling();
        
        SetMode(TerrainEditor.EditMode.RaiseLower);
        UpdateAllLabels();
        
        VerifySetup();
    }
    
    void VerifySetup()
    {
        Debug.Log("[TerrainUIManagerCombined] Setup verification:");
        Debug.Log($"  TerrainEditor: {(terrainEditor != null ? "OK" : "MISSING")}");
        Debug.Log($"  CameraController: {(cameraController != null ? "OK" : "MISSING")}");
        Debug.Log($"  UIControlManager: {(_controlManager != null ? "OK" : "MISSING")}");
        Debug.Log($"  Mode buttons: {btnRaiseLower != null}, {btnRoughen != null}, {btnFlatten != null}, {btnSmooth != null}");
        Debug.Log($"  Sliders: {radiusSlider != null}, {strengthSlider != null}, {moveSpeedSlider != null}");
    }

    void InitializeDependencies()
    {
        if (terrainEditor == null)
        {
            terrainEditor = FindObjectOfType<TerrainEditor>();
            if (terrainEditor == null)
            {
                Debug.LogError("[TerrainUIManagerCombined] TerrainEditor not found!");
            }
        }

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogError("[TerrainUIManagerCombined] CameraController not found!");
            }
        }

        _controlManager = GetComponent<UIControlManager>();
        if (_controlManager == null)
        {
            Debug.LogWarning("[TerrainUIManagerCombined] UIControlManager not found, creating one...");
            _controlManager = gameObject.AddComponent<UIControlManager>();
        }
    }

    void SetupSliders()
    {
        SetupSlider(radiusSlider, minBrushRadius, maxBrushRadius, terrainEditor?.brushRadius ?? 1f, HandleRadiusChanged);
        SetupSlider(strengthSlider, minBrushStrength, maxBrushStrength, terrainEditor?.brushStrength ?? 1f, HandleStrengthChanged);
        SetupSlider(roughenSlider, minRoughenAmount, maxRoughenAmount, terrainEditor?.roughenAmount ?? 0.5f, HandleRoughenChanged);
        
        if (radiusSliderRoughen != null)
        {
            SetupSlider(radiusSliderRoughen, minBrushRadius, maxBrushRadius, terrainEditor?.brushRadius ?? 1f, HandleRadiusChanged);
        }
        
        SetupSlider(strengthSliderFlatten, minBrushStrength, maxBrushStrength, terrainEditor?.brushStrength ?? 1f, HandleStrengthChanged);
        SetupSlider(strengthSliderSmooth, minBrushStrength, maxBrushStrength, terrainEditor?.brushStrength ?? 1f, HandleStrengthChanged);
    }

    void SetupSlider(Slider slider, float min, float max, float initial, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;
        
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = initial;
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(callback);
        
        CanvasGroup canvasGroup = slider.GetComponentInParent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    void SetupButtons()
    {
        AddButtonListener(btnRaiseLower, () => SetMode(TerrainEditor.EditMode.RaiseLower));
        AddButtonListener(btnRoughen, () => SetMode(TerrainEditor.EditMode.Roughen));
        AddButtonListener(btnFlatten, () => SetMode(TerrainEditor.EditMode.Flatten));
        AddButtonListener(btnSmooth, () => SetMode(TerrainEditor.EditMode.Smooth));
    }

    void AddButtonListener(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    void SetupCameraSliders()
    {
        if (cameraController == null) return;
        
        SetupSlider(moveSpeedSlider, minMoveSpeed, maxMoveSpeed, cameraController.GetBaseMoveSpeed(), HandleMoveSpeedChanged);
        SetupSlider(rotationSpeedSlider, minRotationSpeed, maxRotationSpeed, cameraController.rotationSpeed, HandleRotationSpeedChanged);
        SetupSlider(verticalSpeedSlider, minVerticalSpeed, maxVerticalSpeed, cameraController.verticalSpeed, HandleVerticalSpeedChanged);
    }

    void SetupUndoRedoButtons()
    {
        AddButtonListener(btnUndo, () => 
        {
            if (terrainEditor != null)
            {
                terrainEditor.Undo();
            }
        });
        
        AddButtonListener(btnRedo, () => 
        {
            if (terrainEditor != null)
            {
                terrainEditor.Redo();
            }
        });
    }

    void ApplyStyling()
    {
        StyleSlider(radiusSlider);
        StyleSlider(strengthSlider);
        StyleSlider(roughenSlider);
        StyleSlider(strengthSliderFlatten);
        StyleSlider(strengthSliderSmooth);
        StyleSlider(moveSpeedSlider);
        StyleSlider(rotationSpeedSlider);
        StyleSlider(verticalSpeedSlider);
    }

    void StyleSlider(Slider slider)
    {
        if (slider == null) return;
        
        var fillRect = slider.fillRect?.GetComponent<Image>();
        if (fillRect != null)
        {
            fillRect.color = activeButtonColor;
        }
        
        var handleRect = slider.handleRect?.GetComponent<Image>();
        if (handleRect != null)
        {
            handleRect.color = Color.white;
            
            if (handleRect.GetComponent<Mask>() == null)
            {
                handleRect.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            }
        }
    }

    void Update()
    {
        UpdateModeLabel();
        
        if (enableKeyboardNavigation)
        {
            HandleKeyboardInput();
        }
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetMode(TerrainEditor.EditMode.RaiseLower);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetMode(TerrainEditor.EditMode.Roughen);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SetMode(TerrainEditor.EditMode.Flatten);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            SetMode(TerrainEditor.EditMode.Smooth);

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            terrainEditor?.Undo();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            terrainEditor?.Redo();
    }

    public void SetMode(TerrainEditor.EditMode mode)
    {
        if (terrainEditor == null) return;

        terrainEditor.currentMode = mode;

        bool isRaiseLower = mode == TerrainEditor.EditMode.RaiseLower;
        bool isRoughen = mode == TerrainEditor.EditMode.Roughen;
        bool isFlatten = mode == TerrainEditor.EditMode.Flatten;
        bool isSmooth = mode == TerrainEditor.EditMode.Smooth;

        SetPanelActive(panelRaiseLower, isRaiseLower);
        SetPanelActive(panelRoughen, isRoughen);
        SetPanelActive(panelFlatten, isFlatten);
        SetPanelActive(panelSmooth, isSmooth);
        SetPanelActive(panelCamera, true);

        UpdateButtonStyles(mode);

        if (syncWithControlManager && _controlManager != null)
        {
            _controlManager.SetEditMode(mode);
        }
    }

    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    void UpdateButtonStyles(TerrainEditor.EditMode mode)
    {
        SetButtonActive(btnRaiseLower, mode == TerrainEditor.EditMode.RaiseLower);
        SetButtonActive(btnRoughen, mode == TerrainEditor.EditMode.Roughen);
        SetButtonActive(btnFlatten, mode == TerrainEditor.EditMode.Flatten);
        SetButtonActive(btnSmooth, mode == TerrainEditor.EditMode.Smooth);
    }

    void SetButtonActive(Button button, bool isActive)
    {
        if (button == null) return;

        var colors = button.colors;
        if (isActive)
        {
            colors.normalColor = activeButtonColor;
            colors.highlightedColor = activeButtonHover;
            colors.pressedColor = activeButtonPressed;
        }
        else
        {
            colors.normalColor = inactiveButtonColor;
            colors.highlightedColor = inactiveButtonHover;
            colors.pressedColor = inactiveButtonPressed;
        }
        button.colors = colors;
    }

    void UpdateModeLabel()
    {
        if (modeLabel == null || terrainEditor == null) return;

        string modeText = terrainEditor.currentMode switch
        {
            TerrainEditor.EditMode.RaiseLower => Input.GetKey(KeyCode.LeftShift) ? "Mode: Lower" : "Mode: Raise",
            TerrainEditor.EditMode.Roughen => "Mode: Roughen",
            TerrainEditor.EditMode.Flatten => "Mode: Flatten",
            TerrainEditor.EditMode.Smooth => "Mode: Smooth",
            _ => "Mode: Unknown"
        };

        modeLabel.text = modeText;
    }

    void UpdateAllLabels()
    {
        UpdateRadiusLabel(radiusSlider?.value ?? terrainEditor?.brushRadius ?? 0);
        UpdateStrengthLabel(strengthSlider?.value ?? terrainEditor?.brushStrength ?? 0);
        UpdateRoughenLabel(roughenSlider?.value ?? terrainEditor?.roughenAmount ?? 0);
        UpdateFlattenStrengthLabel(strengthSliderFlatten?.value ?? terrainEditor?.brushStrength ?? 0);
        UpdateSmoothStrengthLabel(strengthSliderSmooth?.value ?? terrainEditor?.brushStrength ?? 0);
        
        if (cameraController != null)
        {
            UpdateMoveSpeedLabel(moveSpeedSlider?.value ?? cameraController.GetBaseMoveSpeed());
            UpdateRotationSpeedLabel(rotationSpeedSlider?.value ?? cameraController.rotationSpeed);
            UpdateVerticalSpeedLabel(verticalSpeedSlider?.value ?? cameraController.verticalSpeed);
        }
    }

    #region Event Handlers
    void HandleRadiusChanged(float value)
    {
        if (terrainEditor != null)
        {
            terrainEditor.brushRadius = value;
        }
        UpdateRadiusLabel(value);
        
        if (syncWithControlManager && _controlManager != null)
        {
            _controlManager.SetSliderValue("BrushRadius", value);
        }
    }

    void HandleStrengthChanged(float value)
    {
        if (terrainEditor != null)
        {
            terrainEditor.brushStrength = value;
        }
        UpdateStrengthLabel(value);
        UpdateFlattenStrengthLabel(value);
        UpdateSmoothStrengthLabel(value);
        
        if (syncWithControlManager && _controlManager != null)
        {
            _controlManager.SetSliderValue("BrushStrength", value);
        }
    }

    void HandleRoughenChanged(float value)
    {
        if (terrainEditor != null)
        {
            terrainEditor.roughenAmount = value;
        }
        UpdateRoughenLabel(value);
        
        if (syncWithControlManager && _controlManager != null)
        {
            _controlManager.SetSliderValue("RoughenAmount", value);
        }
    }

    void HandleMoveSpeedChanged(float value)
    {
        if (cameraController != null)
        {
            cameraController.SetMoveSpeed(value);
        }
        UpdateMoveSpeedLabel(value);
        
        if (syncWithControlManager && _controlManager != null)
        {
            _controlManager.SetSliderValue("MoveSpeed", value);
        }
    }

    void HandleRotationSpeedChanged(float value)
    {
        if (cameraController != null)
        {
            cameraController.SetRotationSpeed(value);
        }
        UpdateRotationSpeedLabel(value);
        
        if (syncWithControlManager && _controlManager != null)
        {
            _controlManager.SetSliderValue("RotationSpeed", value);
        }
    }

    void HandleVerticalSpeedChanged(float value)
    {
        if (cameraController != null)
        {
            cameraController.SetVerticalSpeed(value);
        }
        UpdateVerticalSpeedLabel(value);
        
        if (syncWithControlManager && _controlManager != null)
        {
            _controlManager.SetSliderValue("VerticalSpeed", value);
        }
    }
    #endregion

    #region Label Updates
    void UpdateRadiusLabel(float value)
    {
        if (radiusLabel != null) radiusLabel.text = $"Radius: {value:F1}";
    }

    void UpdateStrengthLabel(float value)
    {
        if (strengthLabel != null) strengthLabel.text = $"Strength: {value:F2}";
    }

    void UpdateRoughenLabel(float value)
    {
        if (roughenLabel != null) roughenLabel.text = $"Roughen: {(value * 100f):F0}%";
    }

    void UpdateFlattenStrengthLabel(float value)
    {
        if (strengthLabelFlatten != null) strengthLabelFlatten.text = $"Strength: {value:F2}";
    }

    void UpdateSmoothStrengthLabel(float value)
    {
        if (strengthLabelSmooth != null) strengthLabelSmooth.text = $"Strength: {value:F2}";
    }

    void UpdateMoveSpeedLabel(float value)
    {
        if (moveSpeedLabel != null) moveSpeedLabel.text = $"Move Speed: {value:F0}";
    }

    void UpdateRotationSpeedLabel(float value)
    {
        if (rotationSpeedLabel != null) rotationSpeedLabel.text = $"Rotation Speed: {value:F0}";
    }

    void UpdateVerticalSpeedLabel(float value)
    {
        if (verticalSpeedLabel != null) verticalSpeedLabel.text = $"Vertical Speed: {value:F0}";
    }
    #endregion
}