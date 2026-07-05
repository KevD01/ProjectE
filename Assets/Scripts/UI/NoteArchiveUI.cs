using TMPro;
using UnityEngine;

public class NoteArchiveUI : MonoBehaviour
{
    public static NoteArchiveUI Instance;

    [Header("Referencias")]
    [SerializeField] private GameObject archivePanel;
    [SerializeField] private TMP_Text archiveTitleText;
    [SerializeField] private TMP_Text archiveBodyText;
    [SerializeField] private TMP_Text archiveHelpText;

    [Header("Input")]
    [SerializeField] private KeyCode openArchiveKey = KeyCode.J;

    private PlayerTankController playerMovement;
    private int currentNoteIndex;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        Instance = this;

        if (archivePanel != null)
        {
            archivePanel.SetActive(false);
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerTankController>();
        }
    }

    private void Update()
    {
        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return;

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return;

        if (!isOpen)
        {
            if (Input.GetKeyDown(openArchiveKey))
            {
                OpenArchive();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseArchive();
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            PreviousNote();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            NextNote();
        }
    }

    private void OpenArchive()
    {
        if (NoteArchive.Instance == null)
            return;

        if (NoteArchive.Instance.NoteCount <= 0)
        {
            Debug.Log("No hay notas guardadas todavía.");
            return;
        }

        isOpen = true;
        currentNoteIndex = Mathf.Clamp(currentNoteIndex, 0, NoteArchive.Instance.NoteCount - 1);

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        InteractionPromptUI.Instance?.Hide();

        if (archivePanel != null)
        {
            archivePanel.SetActive(true);
        }

        RefreshArchive();
    }

    private void CloseArchive()
    {
        isOpen = false;

        if (archivePanel != null)
        {
            archivePanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    public void ForceClose()
    {
        isOpen = false;

        if (archivePanel != null)
        {
            archivePanel.SetActive(false);
        }
    }

    private void NextNote()
    {
        if (NoteArchive.Instance == null || NoteArchive.Instance.NoteCount <= 0)
            return;

        currentNoteIndex++;

        if (currentNoteIndex >= NoteArchive.Instance.NoteCount)
        {
            currentNoteIndex = 0;
        }

        RefreshArchive();
    }

    private void PreviousNote()
    {
        if (NoteArchive.Instance == null || NoteArchive.Instance.NoteCount <= 0)
            return;

        currentNoteIndex--;

        if (currentNoteIndex < 0)
        {
            currentNoteIndex = NoteArchive.Instance.NoteCount - 1;
        }

        RefreshArchive();
    }

    private void RefreshArchive()
    {
        NoteData note = NoteArchive.Instance.GetNote(currentNoteIndex);

        if (note == null)
            return;

        if (archiveTitleText != null)
        {
            archiveTitleText.text = note.noteTitle;
        }

        if (archiveBodyText != null)
        {
            archiveBodyText.text = note.noteBody;
        }

        if (archiveHelpText != null)
        {
            archiveHelpText.text =
                "Nota " + (currentNoteIndex + 1) + "/" + NoteArchive.Instance.NoteCount +
                "     A / D para cambiar     Escape para cerrar";
        }
    }
}