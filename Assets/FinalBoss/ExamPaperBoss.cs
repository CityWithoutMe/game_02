using UnityEngine;

// Dedicated hit receiver: each accepted sword swing ALWAYS removes one point.
public sealed class ExamPaperBoss : MonoBehaviour
{
    public FinalExamEncounter encounter;
    public void TakeHit() { if (encounter != null) encounter.TakeBossHit(); }
}
