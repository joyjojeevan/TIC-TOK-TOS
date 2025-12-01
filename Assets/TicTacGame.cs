using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TicTacGame : MonoBehaviour
{
    private Button[] buttons;
    private TMP_Text[] texts;
    private Image[] images;

    private int turn = 0;

    void Start()
    {
        // Get all buttons from the children (9 buttons)
        buttons = GetComponentsInChildren<Button>();

        // Prepare arrays
        texts = new TMP_Text[buttons.Length];
        images = new Image[buttons.Length];

        // Set listener for each button
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; 

            texts[i] = buttons[i].GetComponentInChildren<TMP_Text>();
            images[i] = buttons[i].GetComponent<Image>();

            buttons[i].onClick.AddListener(() => OnCellClicked(index));
        }
    }

    void OnCellClicked(int index)
    {
        // check and continue
        if (texts[index].text != "") return;

        turn++;

        if (turn % 2 == 1)
        {
            texts[index].text = "X";
            images[index].color = Color.blue;
        }
        else
        {
            texts[index].text = "O";
            images[index].color = Color.red;
        }
    }
}
