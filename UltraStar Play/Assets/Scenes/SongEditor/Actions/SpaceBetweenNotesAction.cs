﻿using System.Collections.Generic;
using System.Linq;
using UniInject;

#pragma warning disable CS0649

public class SpaceBetweenNotesAction : INeedInjection
{
    [Inject]
    private SongMetaChangeEventStream songMetaChangeEventStream;

    [Inject]
    private UiManager uiManager;

    public void Execute(IReadOnlyCollection<Note> selectedNotes, int spaceInBeats)
    {
        if (spaceInBeats <= 0)
        {
            UiManager.CreateNotification("Minimum amount of space (in beats) must be greater than 0.");
            return;
        }

        // Sort notes
        List<Note> sortedNotes = selectedNotes.OrderBy(note => note.StartBeat).ToList();
        // Check if distance to following note satisfies the desired space. If not, then shorten note.
        for (int i = 0; i < sortedNotes.Count - 1; i++)
        {
            Note note = sortedNotes[i];
            Note followingNote = sortedNotes[i + 1];
            int distance = followingNote.StartBeat - note.EndBeat;
            if (distance < spaceInBeats)
            {
                int newEndBeat = followingNote.StartBeat - spaceInBeats;
                if (newEndBeat > note.StartBeat)
                {
                    note.SetEndBeat(newEndBeat);
                }
                else
                {
                    // Shorten as much as possible without removing the note
                    note.SetLength(1);
                }
            }
        }
    }

    public void ExecuteAndNotify(IReadOnlyCollection<Note> selectedNotes, int spaceInBeats)
    {
        Execute(selectedNotes, spaceInBeats);
        songMetaChangeEventStream.OnNext(new NotesChangedEvent());
    }
}
