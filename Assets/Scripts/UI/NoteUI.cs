using TMPro;
using UnityEngine;

public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance;

    [Header("Referencias")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    private PlayerTankController currentPlayerMovement;
    private bool noteIsOpen;

    public bool IsOpen => noteIsOpen;

    private void Awake()
    {
        Instance = this;

        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!noteIsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseNote();
        }
    }

    public void OpenNote(string title, string body, PlayerTankController playerMovement)
    {
        if (notePanel == null)
            return;

        currentPlayerMovement = playerMovement;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        if (currentPlayerMovement != null)
        {
            currentPlayerMovement.enabled = false;
        }

        InteractionPromptUI.Instance?.Hide();

        notePanel.SetActive(true);
        noteIsOpen = true;
    }

    public void CloseNote()
    {
        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }

        if (currentPlayerMovement != null)
        {
            currentPlayerMovement.enabled = true;
        }

        currentPlayerMovement = null;
        noteIsOpen = false;
    }
}