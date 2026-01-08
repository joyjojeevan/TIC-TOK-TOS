using TMPro;
using UnityEngine;

public class MobileKeyboardAdjuster : MonoBehaviour
{
    public static MobileKeyboardAdjuster Instance;

    public TMP_InputField chatInput;
    private RectTransform rectTransform;
    private Vector2 initialPosition;
    //private bool isMovingUp = false;
    private bool initialPosCaptured = false;

    // Swipe Detection Variables
    private Vector2 fingerStartPos;
    private float minSwipeDistance = 50f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        //initialPosition = rectTransform.anchoredPosition;
        Invoke("CaptureInitialPosition", 0.1f);
    }
    void CaptureInitialPosition()
    {
        initialPosition = rectTransform.anchoredPosition;
        initialPosCaptured = true;
    }

    void Update()
    {
        if (!initialPosCaptured) return;

        HandleSwipeDown();

        float keyboardHeight = GetKeyboardHeight();
        Canvas canvas = GetComponentInParent<Canvas>();
        float scaleFactor = (canvas != null) ? canvas.scaleFactor : 1.0f;

        Vector2 targetPos;
        // Condition: Move up ONLY if keyboard is visible AND the input field is selected
        if (keyboardHeight > 0 && chatInput.isFocused)
        {
            //isMovingUp = true;
            float adjustedHeight = keyboardHeight / scaleFactor;
            targetPos = new Vector2(initialPosition.x, initialPosition.y + adjustedHeight);

            // Smoothly move up
            //rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * 15f);
        }
        else
        {
            //isMovingUp = false;
            // Smoothly move back to starting position
            //rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, initialPosition, Time.deltaTime * 15f);
            targetPos = initialPosition;
        }
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * 20f);
    }
    private void HandleSwipeDown()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved && touch.deltaPosition.y < -20f)
            {
                ForceCloseKeyboard();
            }
        }
    }
    public void ForceCloseKeyboard()
    {
        chatInput.DeactivateInputField();
        if (TouchScreenKeyboard.visible)
        {
            TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default).active = false;
        }
    }

    private float GetKeyboardHeight()
    {
#if UNITY_EDITOR
        return Input.GetKey(KeyCode.Space) ? Screen.height / 3f : 0;
#elif UNITY_ANDROID
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var view = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
                var rect = new AndroidJavaObject("android.graphics.Rect");
                view.Call("getWindowVisibleDisplayFrame", rect);
                return Screen.height - rect.Call<int>("height");
            }
#else
            return TouchScreenKeyboard.area.height;
#endif
    }
}