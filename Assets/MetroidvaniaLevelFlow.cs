using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Six school rooms, a movement lesson, and a future boss-completion hook.</summary>
public sealed class MetroidvaniaLevelFlow : MonoBehaviour
{
    [SerializeField] private SchoolDoor tutorialEntrance;
    [SerializeField] private SchoolDoor bossExit;
    [SerializeField] private Tilemap dawnBackground;
    [SerializeField] private Tilemap dawnFloor;
    [SerializeField] private float tutorialStartX = 26f;
    [SerializeField] private float bossStartX = 58f;
    [Tooltip("Enable after adding a boss, then call CompleteBossFight on its death.")]
    [SerializeField] private bool bossEncounterEnabled;

    private enum Step { Arrival, Move, Jump, Explore, Boss, Finished }
    private Step step;
    private Transform player;
    private PlayerMovement2D movement;
    private float tutorialStartPositionX;
    private int initialJumpCount;
    private bool bossCompleted;
    private float dawnProgress;

    private void Start()
    {
        if (bossExit != null) bossExit.SetLocked(bossEncounterEnabled);
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found == null) return;
            player = found.transform;
            movement = found.GetComponent<PlayerMovement2D>();
        }

        // Recover light while crossing room five; retain it when returning.
        dawnProgress = Mathf.Max(dawnProgress, Mathf.InverseLerp(129f, 157f, player.position.x));
        float shade = Mathf.Lerp(0.62f, 1f, dawnProgress);
        if (dawnBackground != null) dawnBackground.color = new Color(shade, shade, shade, 1f);
        if (dawnFloor != null) dawnFloor.color = new Color(shade, shade, shade, 1f);

        switch (step)
        {
            case Step.Arrival:
                if (player.position.x >= tutorialStartX)
                {
                    if (tutorialEntrance != null) tutorialEntrance.SetLocked(true);
                    tutorialStartPositionX = player.position.x;
                    step = Step.Move;
                }
                break;
            case Step.Move:
                if (Mathf.Abs(player.position.x - tutorialStartPositionX) >= 2f)
                {
                    initialJumpCount = movement != null ? movement.JumpCount : 0;
                    step = Step.Jump;
                }
                break;
            case Step.Jump:
                if (movement != null && movement.JumpCount > initialJumpCount && movement.IsGrounded)
                    CompleteTutorial();
                break;
            case Step.Explore:
                if (bossEncounterEnabled && !bossCompleted && player.position.x >= bossStartX)
                {
                    if (bossExit != null) bossExit.SetLocked(true);
                    step = Step.Boss;
                }
                else if (player.position.x >= 163f) step = Step.Finished;
                break;
        }
    }

    public void CompleteTutorial()
    {
        if (tutorialEntrance != null) tutorialEntrance.SetLocked(false);
        step = Step.Explore;
    }

    public void CompleteBossFight()
    {
        bossCompleted = true;
        if (bossExit != null)
        {
            bossExit.SetLocked(false);
            bossExit.SetOpen(true);
        }
        step = Step.Explore;
    }

    public void OnPlayerRespawn()
    {
        if (tutorialEntrance != null) tutorialEntrance.SetLocked(false);
        if (step == Step.Move || step == Step.Jump) step = Step.Arrival;
        if (step == Step.Boss) step = Step.Explore;
    }

    private void OnGUI()
    {
        string message = null;
        if (step == Step.Move) message = "A / D   -   WALK";
        if (step == Step.Jump) message = "K   -   JUMP AND LAND";
        if (step == Step.Boss) message = "BOSS ROOM";
        if (message != null) GUI.Box(new Rect(Screen.width * 0.5f - 140f, 24f, 280f, 45f), message);
    }
}
