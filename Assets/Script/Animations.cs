using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animations : MonoBehaviour
{
    public static Animations Instance;
    private RectTransform targetUI;

    [Header("Glow Settings")]
    [SerializeField] private float glowSpeed = 2f;
    [SerializeField] private Color glowColor = new Color(1f, 1f, 0.3f);

    private readonly List<Image> targetGlows = new List<Image>();
    private bool glowActive;
    private float glowTimer;

    private float durationTime = 1f;
    private float moveAmount = 10f;

    private float tDelta;
    private float tickFactor;
    private float smoothStep;

    private Vector2 originalPos;
    internal bool PopupActive;

    [Header("Win Cell Scale Settings")]
    public float scaleAmount = 1.05f;
    public float scaleDuration = 0.5f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    void Update()
    {
        UpdatePopupAnimation();
        UpdateDoGlow();
    }
    public void PlayPopup(RectTransform uiObject)
    {
        targetUI = uiObject;
        originalPos = targetUI.anchoredPosition;

        tDelta = 0f;
        tickFactor = 1f / durationTime;

        //animate = true;
    }
    public void PlayBounce(RectTransform target)
    {
        StartCoroutine(DoBounceClickCell(target));
    }
    public void PlayWinAnimation(List<Cell> winCells)
    {
        foreach (Cell cell in winCells)
        {
            StartCoroutine(ScaleWinCell(cell.GetComponent<RectTransform>()));
            StartGlow(cell.cellImage);
        }
    }
    private void UpdatePopupAnimation()
    {
        if (!PopupActive) return;

        tDelta += Time.deltaTime;
        smoothStep = tDelta * tickFactor;

        smoothStep = 4 * smoothStep - 4 * smoothStep * smoothStep;

        Vector2 newPos = originalPos;
        newPos.y = originalPos.y + moveAmount * smoothStep;

        targetUI.anchoredPosition = newPos;

        if (tDelta >= durationTime)
        {
            tDelta = 0f;
            //animate = false; 
            targetUI.anchoredPosition = originalPos;
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
            float smoothStep = t / scaleDuration;


            smoothStep = smoothStep * smoothStep * (3 - 2 * smoothStep);

            rect.localScale = Vector3.Lerp(startScale, endScale, smoothStep);
            yield return null;
        }

        rect.localScale = endScale;
    }
    // another method for scaleup
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
    #region DoGlow
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
            img.color = Color.white;

        targetGlows.Clear();
    }

    private void UpdateDoGlow()
    {
        if (!glowActive) return;

        glowTimer += Time.deltaTime * glowSpeed;
        /*oscillates it between a minimum of 0 and a specified length (or maximum value).*/
        float t = Mathf.PingPong(glowTimer, 1f);

        foreach (var img in targetGlows)
        {
            img.color = Color.Lerp(Color.white, glowColor, t);
        }
    }
    #endregion
}

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

