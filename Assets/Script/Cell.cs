using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cell : MonoBehaviour
{
    public Vector2Int cellPos;        
    internal TicTacPlayer player = TicTacPlayer.None;

    public Image cellImage;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        GameManager.Instance.InitGridCells(this);
        button.onClick.AddListener(OnClickCell);
       // gridPos = new Vector2Int(index / 3, index % 3);
    }

    private void OnClickCell()
    {
        if (player != TicTacPlayer.None)
            return;

        player = GameManager.Instance.GetCurrentPlayer();

        cellImage.sprite = (player == TicTacPlayer.Player1) ? GameManager.Instance.xSprite : GameManager.Instance.oSprite;

        GameManager.Instance.NextMove();

    }
    //AI click
    public void AI_Click()
    {
        OnClickCell(); 
    }

    public void ResetCell()
    {
        player = TicTacPlayer.None;
        cellImage.sprite = null;
    }
}
