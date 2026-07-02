using System.Collections.Generic;
using UnityEngine;

public class NoteArchive : MonoBehaviour
{
    public static NoteArchive Instance;

    private readonly List<NoteData> collectedNotes = new List<NoteData>();

    public int NoteCount => collectedNotes.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddNote(NoteData noteData)
    {
        if (noteData == null)
            return;

        if (collectedNotes.Contains(noteData))
            return;

        collectedNotes.Add(noteData);
        Debug.Log("Nota agregada al archivo: " + noteData.noteTitle);
    }

    public NoteData GetNote(int index)
    {
        if (index < 0 || index >= collectedNotes.Count)
            return null;

        return collectedNotes[index];
    }
}