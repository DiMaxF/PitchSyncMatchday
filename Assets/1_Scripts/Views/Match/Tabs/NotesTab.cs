using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class NotesTab : UIView
{
    [SerializeField] private ListContainer keyMoments;
    [SerializeField] private ListContainer notesList;
    [SerializeField] private InputField notesInput;
    [SerializeField] private Button addNoteButton;

    private MatchCenterDataManager MatchCenter => DataManager.MatchCenter;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (keyMoments != null)
        {
            keyMoments.Init(MatchCenter.EventsAsObject);
        }

        if (notesList != null)
        {
            notesList.Init(MatchCenter.NotesItemsAsObject);
        }

        if (notesInput != null)
        {
            AddToDispose(notesInput.OnValueChangedAsObservable()
                .Subscribe(_ => UpdateAddButtonState())
                .AddTo(this));
        }

        if (addNoteButton != null)
        {
            addNoteButton.OnClickAsObservable()
                .Subscribe(_ => AddNote())
                .AddTo(this);
        }

        AddToDispose(MatchCenter.NotesItems.ObserveCountChanged()
            .Subscribe(_ => UpdateNotesListVisibility())
            .AddTo(this));
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        UpdateNotesListVisibility();
        UpdateAddButtonState();
    }

    private void AddNote()
    {
        if (notesInput == null) return;
        var text = notesInput.text;
        if (string.IsNullOrWhiteSpace(text)) return;

        MatchCenter.AddNote(text);
        notesInput.text = "";
        UpdateAddButtonState();
    }

    private void UpdateNotesListVisibility()
    {
        if (notesList == null) return;

        if (MatchCenter.NotesItems.Count == 0)
        {
            notesList.Hide();
        }
        else
        {
            notesList.Show();
        }
    }

    private void UpdateAddButtonState()
    {
        if (addNoteButton == null) return;
        bool hasText = notesInput != null && !string.IsNullOrWhiteSpace(notesInput.text);
        addNoteButton.interactable = hasText;
    }
}

