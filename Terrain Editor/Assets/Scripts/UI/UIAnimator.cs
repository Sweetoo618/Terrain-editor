using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI动画组件 - 提供丰富的交互动画效果
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIAnimator : MonoBehaviour
{
    [Header("动画配置")]
    public bool enableAnimations = true;
    public float animationSpeed = 1f;
    
    [Header("缩放动画")]
    public bool enableScaleAnimation = true;
    public float scaleOnHover = 1.05f;
    public float scaleOnClick = 0.95f;
    public float scaleDuration = 0.15f;
    
    [Header("颜色动画")]
    public bool enableColorAnimation = true;
    public Color hoverColor = new Color(1, 1, 1, 0.1f);
    public float colorTransitionDuration = 0.2f;
    
    [Header("位置动画")]
    public bool enablePositionAnimation = false;
    public Vector2 hoverOffset = new Vector2(0, 2);
    public float positionDuration = 0.2f;
    
    [Header("阴影动画")]
    public bool enableShadowAnimation = true;
    public float shadowOnHover = 12f;
    public float shadowOnNormal = 4f;
    public float shadowDuration = 0.2f;
    
    private RectTransform _rectTransform;
    private Image _image;
    private Shadow _shadow;
    private Vector3 _originalScale;
    private Vector2 _originalPosition;
    private Color _originalColor;
    private Coroutine _currentAnimation;
    
    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        _shadow = GetComponent<Shadow>();
        
        _originalScale = _rectTransform.localScale;
        _originalPosition = _rectTransform.anchoredPosition;
        
        if (_image != null)
        {
            _originalColor = _image.color;
        }
    }
    
    void OnEnable()
    {
        // 添加事件监听
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    void OnDisable()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }
    
    void OnClick()
    {
        if (!enableAnimations) return;
        PlayClickAnimation();
    }
    
    /// <summary>
    /// 播放点击动画
    /// </summary>
    public void PlayClickAnimation()
    {
        if (!enableAnimations) return;
        
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        
        _currentAnimation = StartCoroutine(ClickAnimationCoroutine());
    }
    
    IEnumerator ClickAnimationCoroutine()
    {
        // 按下效果
        float elapsed = 0f;
        Vector3 targetScale = _originalScale * scaleOnClick;
        
        while (elapsed < scaleDuration * 0.5f)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = elapsed / (scaleDuration * 0.5f);
            _rectTransform.localScale = Vector3.Lerp(_originalScale, targetScale, t);
            yield return null;
        }
        
        // 弹起效果
        elapsed = 0f;
        while (elapsed < scaleDuration * 0.5f)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = elapsed / (scaleDuration * 0.5f);
            _rectTransform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
            yield return null;
        }
        
        _rectTransform.localScale = _originalScale;
    }
    
    /// <summary>
    /// 播放悬停进入动画
    /// </summary>
    public void PlayHoverEnterAnimation()
    {
        if (!enableAnimations) return;
        
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        
        _currentAnimation = StartCoroutine(HoverEnterAnimationCoroutine());
    }
    
    IEnumerator HoverEnterAnimationCoroutine()
    {
        float elapsed = 0f;
        
        // 缩放动画
        if (enableScaleAnimation)
        {
            Vector3 targetScale = _originalScale * scaleOnHover;
            
            while (elapsed < scaleDuration)
            {
                elapsed += Time.deltaTime * animationSpeed;
                float t = elapsed / scaleDuration;
                t = EaseOutQuad(t);
                
                _rectTransform.localScale = Vector3.Lerp(_originalScale, targetScale, t);
                yield return null;
            }
            
            _rectTransform.localScale = targetScale;
        }
        
        // 颜色动画
        if (enableColorAnimation && _image != null)
        {
            elapsed = 0f;
            Color targetColor = _originalColor + hoverColor;
            
            while (elapsed < colorTransitionDuration)
            {
                elapsed += Time.deltaTime * animationSpeed;
                float t = elapsed / colorTransitionDuration;
                t = EaseOutQuad(t);
                
                _image.color = Color.Lerp(_originalColor, targetColor, t);
                yield return null;
            }
            
            _image.color = targetColor;
        }
        
        // 阴影动画
        if (enableShadowAnimation && _shadow != null)
        {
            elapsed = 0f;
            
            while (elapsed < shadowDuration)
            {
                elapsed += Time.deltaTime * animationSpeed;
                float t = elapsed / shadowDuration;
                t = EaseOutQuad(t);
                
                // 这里简化处理，实际项目中可能需要自定义阴影组件
                yield return null;
            }
        }
    }
    
    /// <summary>
    /// 播放悬停退出动画
    /// </summary>
    public void PlayHoverExitAnimation()
    {
        if (!enableAnimations) return;
        
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        
        _currentAnimation = StartCoroutine(HoverExitAnimationCoroutine());
    }
    
    IEnumerator HoverExitAnimationCoroutine()
    {
        float elapsed = 0f;
        
        // 缩放恢复
        if (enableScaleAnimation)
        {
            Vector3 currentScale = _rectTransform.localScale;
            
            while (elapsed < scaleDuration)
            {
                elapsed += Time.deltaTime * animationSpeed;
                float t = elapsed / scaleDuration;
                t = EaseOutQuad(t);
                
                _rectTransform.localScale = Vector3.Lerp(currentScale, _originalScale, t);
                yield return null;
            }
            
            _rectTransform.localScale = _originalScale;
        }
        
        // 颜色恢复
        if (enableColorAnimation && _image != null)
        {
            elapsed = 0f;
            Color currentColor = _image.color;
            
            while (elapsed < colorTransitionDuration)
            {
                elapsed += Time.deltaTime * animationSpeed;
                float t = elapsed / colorTransitionDuration;
                t = EaseOutQuad(t);
                
                _image.color = Color.Lerp(currentColor, _originalColor, t);
                yield return null;
            }
            
            _image.color = _originalColor;
        }
    }
    
    /// <summary>
    /// 播放显示动画
    /// </summary>
    public void PlayShowAnimation()
    {
        if (!enableAnimations) return;
        
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        
        _currentAnimation = StartCoroutine(ShowAnimationCoroutine());
    }
    
    IEnumerator ShowAnimationCoroutine()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        
        // 从透明和缩放状态开始
        if (_image != null)
        {
            Color startColor = _image.color;
            startColor.a = 0;
            _image.color = startColor;
        }
        
        _rectTransform.localScale = _originalScale * 0.9f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = elapsed / duration;
            t = EaseOutBack(t);
            
            // 缩放动画
            _rectTransform.localScale = Vector3.Lerp(_originalScale * 0.9f, _originalScale, t);
            
            // 透明度动画
            if (_image != null)
            {
                Color color = _image.color;
                color.a = Mathf.Lerp(0, _originalColor.a, t);
                _image.color = color;
            }
            
            yield return null;
        }
        
        _rectTransform.localScale = _originalScale;
        
        if (_image != null)
        {
            _image.color = _originalColor;
        }
    }
    
    /// <summary>
    /// 播放隐藏动画
    /// </summary>
    public void PlayHideAnimation()
    {
        if (!enableAnimations) return;
        
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        
        _currentAnimation = StartCoroutine(HideAnimationCoroutine());
    }
    
    IEnumerator HideAnimationCoroutine()
    {
        float elapsed = 0f;
        float duration = 0.2f;
        
        Vector3 startScale = _rectTransform.localScale;
        Color startColor = _image != null ? _image.color : Color.white;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = elapsed / duration;
            t = EaseInQuad(t);
            
            // 缩放动画
            _rectTransform.localScale = Vector3.Lerp(startScale, _originalScale * 0.9f, t);
            
            // 透明度动画
            if (_image != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0, t);
                _image.color = color;
            }
            
            yield return null;
        }
        
        _rectTransform.localScale = _originalScale * 0.9f;
        
        if (_image != null)
        {
            Color color = _image.color;
            color.a = 0;
            _image.color = color;
        }
    }
    
    /// <summary>
    /// 播放脉冲动画（用于强调效果）
    /// </summary>
    public void PlayPulseAnimation()
    {
        if (!enableAnimations) return;
        
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        
        _currentAnimation = StartCoroutine(PulseAnimationCoroutine());
    }
    
    IEnumerator PulseAnimationCoroutine()
    {
        float elapsed = 0f;
        float duration = 0.6f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = elapsed / duration;
            
            // 使用正弦波创建脉冲效果
            float scaleMultiplier = 1 + Mathf.Sin(t * Mathf.PI * 2) * 0.05f;
            _rectTransform.localScale = _originalScale * scaleMultiplier;
            
            yield return null;
        }
        
        _rectTransform.localScale = _originalScale;
    }
    
    /// <summary>
    /// 播放抖动动画（用于错误提示）
    /// </summary>
    public void PlayShakeAnimation()
    {
        if (!enableAnimations) return;
        
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        
        _currentAnimation = StartCoroutine(ShakeAnimationCoroutine());
    }
    
    IEnumerator ShakeAnimationCoroutine()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        float intensity = 10f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = elapsed / duration;
            
            // 衰减的抖动
            float currentIntensity = intensity * (1 - t);
            float offsetX = Mathf.Sin(t * Mathf.PI * 8) * currentIntensity;
            
            _rectTransform.anchoredPosition = _originalPosition + new Vector2(offsetX, 0);
            
            yield return null;
        }
        
        _rectTransform.anchoredPosition = _originalPosition;
    }
    
    #region Easing Functions
    float EaseOutQuad(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }
    
    float EaseInQuad(float t)
    {
        return t * t;
    }
    
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
    
    float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
    }
    #endregion
}
