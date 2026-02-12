using Models.Dialogue;
using UnityEngine;

public class DialogueController: MonoBehaviour
{
    private DialogueModel _activeDialogue;
    private DialogueStepModel _currentStep;

    public void StartDialogue(DialogueModel dialogue)
    {
        _activeDialogue = dialogue;
        GoToStep(dialogue.StartStepId);
    }

    public void ChooseOption(int choiceIndex)
    {
        var choice = _currentStep.Choices[choiceIndex];
        GoToStep(choice.NextStepId);
    }

    public void Continue()
    {
        if (_currentStep.NextStepId.HasValue)
        {
            GoToStep(_currentStep.NextStepId.Value);
        }
        else
        {
            // EndDialogue();
        }
    }

    private void GoToStep(int? stepId)
    {
        if (stepId is null) return;
        _currentStep = _activeDialogue.Steps[(int)stepId];
        // Notify UI
    }
}
