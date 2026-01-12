using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animations : MonoBehaviour
{
    public static Animations Instance;

    private Dictionary<RectTransform, Vector2> initialPositions = new Dictionary<RectTransform, Vector2>();
    private Dictionary<RectTransform, Coroutine> activePopupLoops = new Dictionary<RectTransform, Coroutine>();

    public RectTransform uiObject;
    [Header("Popup Settings")]
    [SerializeField] private float cycleDuration = 1.0f;
    [SerializeField] private float moveAmount = 15f;

    [Header("Glow Settings")]
    [SerializeField] private float glowSpeed = 2f;
    [SerializeField] private Color glowColor = new Color(1f, 1f, 0.3f);
    private readonly List<Image> targetGlows = new List<Image>();
    private bool glowActive;
    private float glowTimer;

    [Header("Win Cell Scale Settings")]
    public float scaleAmount = 1.1f;
    public float scaleDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        UpdateDoGlow(); // Glow stays in Update for smooth oscillation
    }

    #region Continuous Popup Logic

    public void StartContinuousPopup(RectTransform uiObject)
    {
        if (uiObject == null) return;

        // Save the very first position as the "Home" position
        if (!initialPositions.ContainsKey(uiObject))
            initialPositions.Add(uiObject, uiObject.anchoredPosition);

        // Stop existing loop if any
        StopContinuousPopup(uiObject);

        // Start new infinite loop
        activePopupLoops[uiObject] = StartCoroutine(LoopingPopupRoutine(uiObject));
    }

    public void StopContinuousPopup(RectTransform uiObject)
    {
        if (uiObject == null) return;

        if (activePopupLoops.ContainsKey(uiObject) && activePopupLoops[uiObject] != null)
        {
            StopCoroutine(activePopupLoops[uiObject]);
            activePopupLoops.Remove(uiObject);
        }

        // Snap back to the TRUE initial position
        if (initialPositions.ContainsKey(uiObject))
            uiObject.anchoredPosition = initialPositions[uiObject];
    }

    private IEnumerator LoopingPopupRoutine(RectTransform ui)
    {
        Vector2 startPos = initialPositions[ui];

        while (true) // Infinite loop
        {
            float elapsed = 0f;
            while (elapsed < cycleDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / cycleDuration;

                // Using Sine wave for a smoother continuous "bounce"
                float curve = Mathf.Sin(normalizedTime * Mathf.PI);

                ui.anchoredPosition = startPos + new Vector2(0, moveAmount * curve);
                yield return null;
            }
            ui.anchoredPosition = startPos;
        }
    }
    #endregion

    #region Glow Logic (Unchanged but cleaned)
    public void StartGlow(Image target)
    {
        glowActive = true;
        if (!targetGlows.Contains(target))
        {
            targetGlows.Add(target);
            target.color = Color.white;
        }
    }

    public void StopGlow()
    {
        glowActive = false;
        foreach (var img in targetGlows)
        {
            if (img != null) img.color = Color.white;
        }
        targetGlows.Clear();

    }

    private void UpdateDoGlow()
    {
        if (!glowActive) return;

        glowTimer += Time.deltaTime * glowSpeed;
        float t = Mathf.PingPong(glowTimer, 1f);

        foreach (var img in targetGlows)
        {
            if (img != null)
                img.color = Color.Lerp(Color.white, glowColor, t);
        }
    }
    #endregion

    #region Win Effects
    public void PlayWinAnimation(List<Cell> winCells)
    {
        foreach (Cell cell in winCells)
        {
            StartCoroutine(ScaleWinCell(cell.GetComponent<RectTransform>()));
            StartGlow(cell.cellImage);
        }
    }

    private IEnumerator ScaleWinCell(RectTransform rect)
    {
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * scaleAmount;
        float t = 0;
        while (t < scaleDuration)
        {
            t += Time.deltaTime;
            float step = Mathf.SmoothStep(0, 1, t / scaleDuration);
            rect.localScale = Vector3.Lerp(startScale, endScale, step);
            yield return null;
        }
    }
    #endregion
    public void PlayBounce(RectTransform target)
    {
        StartCoroutine(DoBounceClickCell(target));
    }
    private IEnumerator DoBounceClickCell(RectTransform target)
    {
        Vector3 startScale = Vector3.one;
        Vector3 upScale = Vector3.one * 1.2f;
        float Duration = 0.15f;

        float tDelta = 0f;
        while (tDelta < 1f)
        {
            tDelta += Time.deltaTime / Duration;
            target.localScale = Vector3.Lerp(startScale, upScale, tDelta);
            yield return null;
        }

        tDelta = 0f;
        while (tDelta < 1f)
        {
            tDelta += Time.deltaTime / Duration;
            target.localScale = Vector3.Lerp(upScale, startScale, tDelta);
            yield return null;
        }

        target.localScale = startScale;
    }
}
//public class Animations : MonoBehaviour
//{
//    public static Animations Instance;
//    private RectTransform targetUI;
//    private Dictionary<RectTransform, Vector2> initialPositions = new Dictionary<RectTransform, Vector2>();
//    private Dictionary<RectTransform, Coroutine> activeLoops = new Dictionary<RectTransform, Coroutine>();

//    [Header("Glow Settings")]
//    [SerializeField] private float glowSpeed = 2f;
//    [SerializeField] private Color glowColor = new Color(1f, 1f, 0.3f);

//    [Header("Continuous Popup Settings")]
//    [SerializeField] private float cycleDuration = 1.0f; 
//    [SerializeField] private float moveAmount = 20f;

//    private readonly List<Image> targetGlows = new List<Image>();
//    private bool glowActive;
//    private float glowTimer;

//    private float durationTime = 1f;
//    private float moveAmount = 10f;

//    private float tDelta;
//    private float tickFactor;
//    private float smoothStep;

//    private Vector2 originalPos;
//    internal bool PopupActive;

//    [Header("Win Cell Scale Settings")]
//    public float scaleAmount = 1.05f;
//    public float scaleDuration = 0.5f;
//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }

//    }
//    void Update()
//    {
//        UpdatePopupAnimation();
//        UpdateDoGlow();
//    }
//    public void PlayPopup(RectTransform uiObject)
//    {
//        //targetUI = uiObject;
//        //originalPos = targetUI.anchoredPosition;

//        //tDelta = 0f;
//        //tickFactor = 1f / durationTime;
//        if (!initialPositions.ContainsKey(uiObject))
//        {
//            initialPositions.Add(uiObject, uiObject.anchoredPosition);
//        }

//        // 2. Stop any movement currently happening on this specific UI
//        StopCoroutine("DoPopupRoutine");
//        StartCoroutine(DoPopupRoutine(uiObject));

//        //animate = true;
//    }
//    private IEnumerator DoPopupRoutine(RectTransform ui)
//    {
//        // Always use the saved Initial Position as the base
//        Vector2 startPos = initialPositions[ui];
//        float elapsed = 0f;

//        while (elapsed < durationTime)
//        {
//            elapsed += Time.deltaTime;
//            float normalizedTime = elapsed / durationTime;

//            // Parabolas math: 4 * x * (1 - x)
//            float curve = 4 * normalizedTime * (1 - normalizedTime);

//            ui.anchoredPosition = new Vector2(startPos.x, startPos.y + (moveAmount * curve));
//            yield return null;
//        }

//        // 3. FORCE return to the exact saved start position
//        ui.anchoredPosition = startPos;
//    }
//    public void PlayBounce(RectTransform target)
//    {
//        StartCoroutine(DoBounceClickCell(target));
//    }
//    public void PlayWinAnimation(List<Cell> winCells)
//    {
//        foreach (Cell cell in winCells)
//        {
//            StartCoroutine(ScaleWinCell(cell.GetComponent<RectTransform>()));
//            StartGlow(cell.cellImage);
//        }
//    }
//    private void UpdatePopupAnimation()
//    {
//        if (!PopupActive || targetUI == null) return;

//        tDelta += Time.deltaTime;
//        //smoothStep = tDelta * tickFactor;
//        smoothStep = Mathf.Clamp01(tDelta * tickFactor);

//        smoothStep = 4 * smoothStep - 4 * smoothStep * smoothStep;

//        Vector2 newPos = originalPos;
//        newPos.y = originalPos.y + moveAmount * smoothStep;

//        targetUI.anchoredPosition = newPos;

//        if (tDelta >= durationTime)
//        {
//            tDelta = 0f;
//            //animate = false; 
//            targetUI.anchoredPosition = originalPos;
//        }
//    }
//    private IEnumerator ScaleWinCell(RectTransform rect)
//    {
//        Vector3 startScale = Vector3.one;
//        Vector3 endScale = Vector3.one * scaleAmount;

//        float t = 0;

//        while (t < scaleDuration)
//        {
//            t += Time.deltaTime;
//            float smoothStep = t / scaleDuration;


//            smoothStep = smoothStep * smoothStep * (3 - 2 * smoothStep);

//            rect.localScale = Vector3.Lerp(startScale, endScale, smoothStep);
//            yield return null;
//        }

//        rect.localScale = endScale;
//    }
//    // another method for scaleup
//    private IEnumerator DoBounceClickCell(RectTransform target)
//    {
//        Vector3 startScale = Vector3.one;
//        Vector3 upScale = Vector3.one * 1.2f;
//        float Duration = 0.15f;

//        float tDelta = 0f;
//        while (tDelta < 1f)
//        {
//            tDelta += Time.deltaTime / Duration;
//            target.localScale = Vector3.Lerp(startScale, upScale, tDelta);
//            yield return null;
//        }

//        tDelta = 0f;
//        while (tDelta < 1f)
//        {
//            tDelta += Time.deltaTime / Duration;
//            target.localScale = Vector3.Lerp(upScale, startScale, tDelta);
//            yield return null;
//        }

//        target.localScale = startScale;
//    }
//    #region DoGlow
//    public void StartGlow(Image target)
//    {
//        glowActive = true;
//        if (!targetGlows.Contains(target))
//        {
//            targetGlows.Add(target);
//            target.color = Color.white;
//        }
//    }
//    public void StopGlow()
//    {
//        glowActive = false;

//        foreach (var img in targetGlows)
//            img.color = Color.white;

//        targetGlows.Clear();
//    }

//    private void UpdateDoGlow()
//    {
//        if (!glowActive) return;

//        glowTimer += Time.deltaTime * glowSpeed;
//        /*oscillates it between a minimum of 0 and a specified length (or maximum value).*/
//        float t = Mathf.PingPong(glowTimer, 1f);

//        foreach (var img in targetGlows)
//        {
//            img.color = Color.Lerp(Color.white, glowColor, t);
//        }
//    }
//    public void ResetUIPosition(RectTransform ui)
//    {
//        foreach (var entry in initialPositions)
//        {
//            entry.Key.anchoredPosition = entry.Value;
//        }
//    }
//    #endregion
//}

//void Update()
//{
//    UpdatePopupAnimation();
//    UpdateDoGlow();
//    //if (!animate) return;

//    //tDelta += Time.deltaTime;
//    //smoothStep = tDelta * tickFactor;

//    //smoothStep = 4 * smoothStep - 4 * smoothStep * smoothStep;

//    //Vector2 newPos = originalPos;
//    //newPos.y = originalPos.y + moveAmount * smoothStep;

//    //targetUI.anchoredPosition = newPos;

//    //if (tDelta >= durationTime)
//    //{
//    //   // newPos.y = originalPos.y;
//    //    tDelta = 0;
//    //    //animate = false;
//    //    targetUI.anchoredPosition = originalPos;

//    //}
//    ////targetUI.anchoredPosition = originalPos;
//}

//private void UpdateDoGlow()
//{
//    float glowSpeed = 2f;
//    Color normal = Color.white;
//    Color glow = new Color(1f, 1f, 0.3f);

//    while (isGlowing)
//    {
//        float t = 0;
//        while (t < 1)
//        {
//            t += Time.deltaTime * glowSpeed;
//            targetGlow.color = Color.Lerp(normal, glow, t);
//            //yield return null;
//        }

//        t = 0;
//        while (t < 1)
//        {
//            t += Time.deltaTime * glowSpeed;
//            targetGlow.color = Color.Lerp(glow, normal, t);
//            //yield return null;
//        }
//        //target.color = normal;

//    }
//}


//public void ScaleWinCells(Cell[] winningCells)
//{
//    foreach (Cell cell in winningCells)
//    {
//        RectTransform rect = cell.GetComponent<RectTransform>();
//        if (rect != null)
//        {
//            // Start a repeating scale up/down animation
//            StartCoroutine(ScaleCellRoutine(rect));
//        }
//    }
//}

//private System.Collections.IEnumerator ScaleCellRoutine(RectTransform rect)
//{
//    Vector3 originalScale = rect.localScale;
//    Vector3 targetScale = originalScale * winScaleAmount;

//    while (true)
//    {
//        // Scale up
//        float t = 0;
//        while (t < winScaleDuration)
//        {
//            rect.localScale = Vector3.Lerp(originalScale, targetScale, t / winScaleDuration);
//            t += Time.deltaTime;
//            yield return null;
//        }

//        // Scale down
//        t = 0;
//        while (t < winScaleDuration)
//        {
//            rect.localScale = Vector3.Lerp(targetScale, originalScale, t / winScaleDuration);
//            t += Time.deltaTime;
//            yield return null;
//        }
//    }
//}

