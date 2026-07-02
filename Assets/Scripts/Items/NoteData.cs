using UnityEngine;

[CreateAssetMenu(fileName = "NewNoteData", menuName = "Ecos del Silencio/Note Data")]
public class NoteData : ScriptableObject
{
    [Header("Identificación")]
    public string noteId = "note_001";

    [Header("Contenido")]
    public string noteTitle = "Título de la nota";

    [TextArea(6, 18)]
    public string noteBody = "Contenido de la nota.";
}