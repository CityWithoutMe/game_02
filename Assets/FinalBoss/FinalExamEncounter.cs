using System.Collections;
using UnityEngine;

public sealed class FinalExamEncounter : MonoBehaviour
{
    [Header("Scene references")]
    public SpriteRenderer bossVisual;
    public SpriteRenderer mirror;
    public SpriteRenderer reflection;
    public Sprite paperSprite;
    public Transform hazardsRoot;
    public FinalExamVictoryPresentation victoryPresentation;
    [Header("Encounter")]
    public Vector2 triggerCenter = new Vector2(178f, 3f);
    public Vector2 triggerSize = new Vector2(3.6f, 8f);
    public Vector3 hoverPosition = new Vector3(178f, 8.8f, 0f);
    [Min(1)] public int maxHealth = 30;
    [Min(0.1f)] public float hitInvulnerability = 0.3f;
    [Header("Attacks")]
    [Min(4)] public int paperCount = 12;
    [Min(0.1f)] public float paperSpeed = 4.2f;
    [Min(0.2f)] public float beamWarning = 1.1f;
    [Min(0.1f)] public float beamDuration = 0.65f;
    [Min(0.1f)] public float attackRest = 1.5f;
    public int CurrentHealth { get; private set; }
    public bool IsFighting { get; private set; }
    public bool Defeated { get; private set; }
    private Transform player;
    private PlayerAttributes attributes;
    private SpriteRenderer playerSprite;
    private float nextHit;
    private float nextPlayerHit;
    private bool introducing;
    private Material inkMaterial;
    private Vector3 bossScale;
    private Color mirrorColor;

    private void Awake()
    {
        bossScale = bossVisual.transform.localScale;
        mirrorColor = mirror.color;
        CurrentHealth = maxHealth;
        bossVisual.gameObject.SetActive(false);
        inkMaterial = new Material(Shader.Find("Sprites/Default"));
    }
    private void Start()
    {
        var found = GameObject.FindGameObjectWithTag("Player");
        if (found == null) return;
        player = found.transform;
        attributes = found.GetComponent<PlayerAttributes>();
        playerSprite = found.GetComponentInChildren<SpriteRenderer>();
        if (attributes != null) attributes.HealthChanged += OnPlayerHealth;
    }
    private void OnPlayerHealth(int health, int maximum)
    {
        if (health <= 0 && !Defeated) ResetEncounter();
    }
    private void Update()
    {
        if (player == null) return;
        if ((IsFighting || introducing) && (player.position.x < 160f || player.position.x > 196f || player.position.y < -5f))
        { ResetEncounter(); return; }
        if (!Defeated && !IsFighting && !introducing &&
            Mathf.Abs(player.position.x - triggerCenter.x) <= triggerSize.x * 0.5f &&
            Mathf.Abs(player.position.y - triggerCenter.y) <= triggerSize.y * 0.5f)
            StartCoroutine(BeginEncounter());
        if (IsFighting)
        {
            bossVisual.transform.position = hoverPosition + new Vector3(Mathf.Sin(Time.time * 0.7f) * 0.6f, Mathf.Sin(Time.time * 1.8f) * 0.2f, 0f);
            bossVisual.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 1.2f) * 4f);
            bossVisual.color = Time.time < nextHit ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
        }
    }
    private void LateUpdate()
    {
        if (reflection == null || playerSprite == null) return;
        bool nearby = !Defeated && player != null && Mathf.Abs(player.position.x - mirror.transform.position.x) < 10f;
        reflection.enabled = nearby;
        if (!nearby || playerSprite.sprite == null) return;
        reflection.sprite = playerSprite.sprite;
        // Reuse the actual animation frame, tinted solid black, constrained inside the glass.
        Vector2 size = playerSprite.sprite.bounds.size;
        float scale = Mathf.Min(0.75f / Mathf.Max(size.x, 0.01f), 1.9f / Mathf.Max(size.y, 0.01f));
        reflection.transform.localScale = Vector3.one * scale;
        reflection.flipX = !(playerSprite.flipX ^ (playerSprite.transform.lossyScale.x < 0f));
        reflection.color = Color.black;
    }
    private IEnumerator BeginEncounter()
    {
        introducing = true;
        CurrentHealth = maxHealth;
        nextHit = 0f; nextPlayerHit = Time.time + 1.5f;
        bossVisual.gameObject.SetActive(true);
        for (float t = 0f; t < 1.4f; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / 1.4f);
            bossVisual.transform.position = hoverPosition + Vector3.up * (2.5f * (1f - k));
            bossVisual.transform.localScale = bossScale * Mathf.Lerp(0.25f, 1f, k);
            bossVisual.color = new Color(1f, 1f, 1f, k);
            mirror.color = Color.Lerp(mirrorColor, Color.gray, k);
            yield return null;
        }
        bossVisual.transform.localScale = bossScale;
        introducing = false; IsFighting = true;
        yield return new WaitForSeconds(1f);
        int cycle = 0;
        while (IsFighting)
        {
            yield return PaperBurst(cycle++);
            yield return new WaitForSeconds(attackRest);
            yield return LightColumns();
            yield return new WaitForSeconds(CurrentHealth <= maxHealth / 2 ? attackRest * 0.75f : attackRest);
        }
    }
    private IEnumerator PaperBurst(int cycle)
    {
        // Contract before the radial attack: a readable monochrome wind-up.
        for (float t = 0f; t < 0.65f; t += Time.deltaTime)
        { bossVisual.transform.localScale = bossScale * (1f - 0.12f * Mathf.Sin(t / 0.65f * Mathf.PI)); yield return null; }
        bossVisual.transform.localScale = bossScale;
        int waves = CurrentHealth <= maxHealth / 2 ? 2 : 1;
        for (int wave = 0; wave < waves; wave++)
        {
            for (int i = 0; i < paperCount; i++)
            {
                float angle = (i * 360f / paperCount + cycle * 17f + wave * 15f) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var sheet = new GameObject("Small exam paper");
                sheet.transform.SetParent(hazardsRoot, false);
                sheet.transform.position = bossVisual.transform.position;
                var visual = sheet.AddComponent<SpriteRenderer>();
                visual.sprite = paperSprite; visual.sortingOrder = 24;
                sheet.transform.localScale = Vector3.one * (0.55f / paperSprite.bounds.size.y);
                sheet.AddComponent<ExamHazard>().Configure(this, direction * paperSpeed, 6f, false);
            }
            yield return new WaitForSeconds(0.55f);
        }
    }
    private LineRenderer Line(string label, float x, float width, Color color)
    {
        var go = new GameObject(label);
        go.transform.SetParent(hazardsRoot, false);
        go.transform.position = new Vector3(x, 6f, 0f);
        var line = go.AddComponent<LineRenderer>();
        line.sharedMaterial = inkMaterial; line.useWorldSpace = false;
        line.positionCount = 2; line.SetPosition(0, Vector3.down * 6f); line.SetPosition(1, Vector3.up * 6f);
        line.startWidth = line.endWidth = width;
        line.startColor = line.endColor = color; line.sortingOrder = 22;
        return line;
    }
    private IEnumerator LightColumns()
    {
        // Snapshot aim at warning start. Columns never track the player afterward.
        float x = Mathf.Clamp(player.position.x, 164f, 192f);
        float[] positions = { x - 4f, x, x + 4f };
        LineRenderer[] warnings = new LineRenderer[positions.Length];
        for (int i = 0; i < positions.Length; i++) warnings[i] = Line("Column warning", positions[i], 0.1f, Color.black);
        for (float t = 0f; t < beamWarning; t += Time.deltaTime)
        {
            float alpha = 0.3f + Mathf.PingPong(t * 3f, 0.7f);
            foreach (var line in warnings) line.startColor = line.endColor = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        foreach (var line in warnings) Destroy(line.gameObject);
        foreach (float columnX in positions)
        {
            var edge = Line("Light column / ink border", columnX, 1f, new Color(0.08f, 0.08f, 0.08f));
            edge.gameObject.AddComponent<ExamHazard>().Configure(this, Vector2.zero, beamDuration, true, 0.8f);
            var core = Line("Light column / white core", columnX, 0.72f, Color.white);
            core.sortingOrder = 23;
            Destroy(core.gameObject, beamDuration);
        }
        yield return new WaitForSeconds(beamDuration);
    }
    public void TakeBossHit()
    {
        if (!IsFighting || Time.time < nextHit) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - 1);
        nextHit = Time.time + hitInvulnerability;
        if (CurrentHealth == 0)
        {
            Defeated = true; IsFighting = false; introducing = false;
            StopAllCoroutines(); ClearHazards();
            StartCoroutine(DefeatAnimation());
        }
    }
    private IEnumerator DefeatAnimation()
    {
        Vector3 center = bossVisual.transform.position;
        // A brief violent wobble reads as the paper tearing before it scatters.
        for (float t = 0f; t < 0.34f; t += Time.deltaTime)
        {
            bossVisual.transform.position = center + Vector3.right * Mathf.Sin(t * 95f) * (0.05f + t * 0.35f);
            bossVisual.transform.localScale = bossScale * (1f + t * 0.25f);
            bossVisual.color = (Mathf.FloorToInt(t * 22f) & 1) == 0 ? Color.white : new Color(0.55f, 0.55f, 0.55f);
            yield return null;
        }
        ScatterExamPapers(center);
        if (victoryPresentation != null) victoryPresentation.ShowVictory();
        for (float t = 0f; t < 1.05f; t += Time.deltaTime)
        {
            float k = Mathf.Clamp01(t / 1.05f);
            bossVisual.transform.position = center + Vector3.up * (k * 1.5f);
            bossVisual.transform.localScale = bossScale * Mathf.Lerp(1f, 0.05f, k);
            bossVisual.transform.rotation = Quaternion.Euler(0f, 0f, k * 100f);
            bossVisual.color = new Color(1f, 1f, 1f, 1f - k);
            yield return null;
        }
        bossVisual.gameObject.SetActive(false);
        mirror.color = Color.white;
    }

    private void ScatterExamPapers(Vector3 center)
    {
        if (paperSprite == null || hazardsRoot == null) return;
        for (int i = 0; i < 22; i++)
        {
            var piece = new GameObject("Defeated exam fragment " + i);
            piece.transform.SetParent(hazardsRoot, false);
            piece.transform.position = center + new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(-1.1f, 1.1f), 0f);
            piece.transform.localScale = Vector3.one * (Random.Range(0.22f, 0.48f) / paperSprite.bounds.size.y);
            var visual = piece.AddComponent<SpriteRenderer>();
            visual.sprite = paperSprite;
            visual.sortingOrder = 25;
            Vector2 direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.1f, 1f)).normalized;
            piece.AddComponent<DefeatedExamFragment>().Launch(direction * Random.Range(3f, 7f), Random.Range(-300f, 300f));
        }
    }

    public void TryHurtPlayer()
    {
        if (!IsFighting || attributes == null || Time.time < nextPlayerHit) return;
        nextPlayerHit = Time.time + 0.9f;
        attributes.TakeDamage(1);
    }
    private void ClearHazards()
    {
        if (hazardsRoot == null) return;
        foreach (Transform child in hazardsRoot)
        { child.gameObject.SetActive(false); Destroy(child.gameObject); }
    }
    public void ResetEncounter()
    {
        StopAllCoroutines(); IsFighting = false; introducing = false;
        ClearHazards(); CurrentHealth = maxHealth; nextHit = 0f;
        bossVisual.gameObject.SetActive(false);
        bossVisual.transform.localScale = bossScale;
        bossVisual.transform.rotation = Quaternion.identity;
        mirror.color = mirrorColor;
    }
    private void OnDisable()
    {
        if (Application.isPlaying && bossVisual != null) ResetEncounter();
    }
    private void OnDestroy()
    {
        if (attributes != null) attributes.HealthChanged -= OnPlayerHealth;
        if (inkMaterial != null) Destroy(inkMaterial);
    }
    private void OnGUI()
    {
        if (!IsFighting && !introducing) return;
        float width = Mathf.Min(420f, Screen.width * 0.45f);
        float x = (Screen.width - width) * 0.5f;
        GUI.Box(new Rect(x, 22f, width, 48f), "FINAL EXAM    " + CurrentHealth + " / " + maxHealth);
        Color previous = GUI.color;
        GUI.color = Color.gray; GUI.DrawTexture(new Rect(x + 12f, 53f, width - 24f, 6f), Texture2D.whiteTexture);
        GUI.color = Color.white; GUI.DrawTexture(new Rect(x + 12f, 53f, (width - 24f) * CurrentHealth / maxHealth, 6f), Texture2D.whiteTexture);
        GUI.color = previous;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireCube(triggerCenter, triggerSize);
        Gizmos.DrawWireSphere(hoverPosition, 1.5f);
    }
}
