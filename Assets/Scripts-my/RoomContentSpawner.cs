using UnityEngine;

/// <summary>
/// Adds room collectibles and extra enemies when the main level starts.
/// Scene templates are used for runtime copies; each copy gets its own patrol markers.
/// </summary>
public sealed class RoomContentSpawner : MonoBehaviour
{
    [Header("Hierarchy")]
    [SerializeField] private Transform contentRoot;

    [Header("Enemies")]
    [SerializeField] private GameObject monsterTemplate;
    [SerializeField] private GameObject replacementMonsterTemplate;
    [SerializeField] private GameObject bossTemplate;
    [SerializeField] private Vector3 replacementMonsterPosition = new Vector3(88f, 8f, 0f);
    [SerializeField] private Vector3 replacementPatrolLeft = new Vector3(85f, 8f, 0f);
    [SerializeField] private Vector3 replacementPatrolRight = new Vector3(91f, 8f, 0f);
    [SerializeField] private Vector3[] extraMonsterPositions =
    {
        new Vector3(82f, 8f, 0f),
        new Vector3(105f, 1f, 0f),
        new Vector3(116f, 1f, 0f),
        new Vector3(145f, 1f, 0f),
        new Vector3(179f, 1f, 0f)
    };
    [SerializeField] private Vector3[] extraPatrolLeft =
    {
        new Vector3(79f, 8f, 0f),
        new Vector3(102f, 1f, 0f),
        new Vector3(113f, 1f, 0f),
        new Vector3(141f, 1f, 0f),
        new Vector3(175f, 1f, 0f)
    };
    [SerializeField] private Vector3[] extraPatrolRight =
    {
        new Vector3(85f, 8f, 0f),
        new Vector3(108f, 1f, 0f),
        new Vector3(119f, 1f, 0f),
        new Vector3(149f, 1f, 0f),
        new Vector3(183f, 1f, 0f)
    };
    [SerializeField] private Color enemyTint = new Color(1f, 0.45f, 0.05f, 1f);

    [Header("Boss")]
    [SerializeField] private Vector3 bossPosition = new Vector3(188f, 1f, 0f);
    [SerializeField] private Vector3 bossPatrolLeft = new Vector3(185f, 1f, 0f);
    [SerializeField] private Vector3 bossPatrolRight = new Vector3(193f, 1f, 0f);
    [SerializeField] private float bossScale = 1.6f;
    [SerializeField] private Sprite bossProjectileSprite;

    [Header("Coins")]
    [SerializeField] private Sprite coinSprite;
    [SerializeField] private Vector3[] coinPositions =
    {
        new Vector3(8f, 2f, 0f),
        new Vector3(40f, 2f, 0f),
        new Vector3(74f, 2f, 0f),
        new Vector3(110f, 2f, 0f),
        new Vector3(144f, 2f, 0f),
        new Vector3(178f, 2f, 0f)
    };

    private void Start()
    {
        Transform parent = contentRoot != null ? contentRoot : transform;
        TintExistingEnemies();
        SpawnExtraMonsters(parent);
        SpawnReplacementMonster(parent);
        if (FindObjectOfType<FinalExamEncounter>() == null) SpawnBoss(parent);
        DisableFirstRoomMonster();
        SpawnCoins(parent);
    }

    private void TintExistingEnemies()
    {
        Health[] enemies = FindObjectsOfType<Health>();
        for (int i = 0; i < enemies.Length; i++)
            ApplyEnemyTint(enemies[i].gameObject);
    }

    private void SpawnExtraMonsters(Transform parent)
    {
        if (monsterTemplate == null || extraMonsterPositions == null)
            return;

        for (int i = 0; i < extraMonsterPositions.Length; i++)
        {
            // Reserve the rooftop for the final exam encounter.
            if (extraMonsterPositions[i].x >= 160f && FindObjectOfType<FinalExamEncounter>() != null) continue;
            string monsterName = "zhipian_Room" + (i + 3);
            if (GameObject.Find(monsterName) != null)
                continue;

            GameObject monster = Instantiate(
                monsterTemplate,
                extraMonsterPositions[i],
                monsterTemplate.transform.rotation,
                parent
            );
            monster.name = monsterName;
            monster.SetActive(true);
            ApplyEnemyTint(monster);
            Vector3 leftPosition = extraPatrolLeft != null && i < extraPatrolLeft.Length
                ? extraPatrolLeft[i]
                : extraMonsterPositions[i] + Vector3.left * 3f;
            Vector3 rightPosition = extraPatrolRight != null && i < extraPatrolRight.Length
                ? extraPatrolRight[i]
                : extraMonsterPositions[i] + Vector3.right * 3f;
            ConfigurePatrol(monster, monsterName, leftPosition, rightPosition, parent);
        }
    }

    private void SpawnReplacementMonster(Transform parent)
    {
        if (replacementMonsterTemplate == null)
            return;

        const string replacementName = "zhongbiao_Room3_Replacement";
        if (GameObject.Find(replacementName) != null)
            return;

        GameObject replacement = Instantiate(
            replacementMonsterTemplate,
            replacementMonsterPosition,
            replacementMonsterTemplate.transform.rotation,
            parent
        );
        replacement.name = replacementName;
        replacement.SetActive(true);
        ApplyEnemyTint(replacement);
        ConfigurePatrol(
            replacement,
            replacementName,
            replacementPatrolLeft,
            replacementPatrolRight,
            parent
        );
    }

    private void SpawnBoss(Transform parent)
    {
        if (bossTemplate == null)
            return;

        const string bossName = "zhongbiao_Boss_Rooftop";
        if (GameObject.Find(bossName) != null)
            return;

        GameObject boss = Instantiate(
            bossTemplate,
            bossPosition,
            bossTemplate.transform.rotation,
            parent
        );
        boss.name = bossName;
        boss.SetActive(false);
        boss.transform.localScale = bossTemplate.transform.localScale * bossScale;
        ApplyEnemyTint(boss);
        ConfigurePatrol(boss, bossName, bossPatrolLeft, bossPatrolRight, parent);

        EnemyRangedAttack rangedAttack = boss.GetComponent<EnemyRangedAttack>();
        if (rangedAttack == null)
            rangedAttack = boss.AddComponent<EnemyRangedAttack>();

        rangedAttack.Configure(
            bossProjectileSprite,
            10f,
            2f,
            "rangedAttack",
            2.5f,
            1,
            0.86f,
            0.4f,
            8f,
            3f,
            0.12f
        );

        boss.SetActive(true);
    }

    private void ConfigurePatrol(
        GameObject monster,
        string monsterName,
        Vector3 leftPosition,
        Vector3 rightPosition,
        Transform parent
    )
    {
        EnemyAI ai = monster.GetComponent<EnemyAI>();
        if (ai == null)
            return;

        GameObject leftMarker = new GameObject(monsterName + "_PatrolLeft");
        leftMarker.transform.position = leftPosition;
        leftMarker.transform.SetParent(parent, true);
        GameObject rightMarker = new GameObject(monsterName + "_PatrolRight");
        rightMarker.transform.position = rightPosition;
        rightMarker.transform.SetParent(parent, true);
        ai.leftPoint = leftMarker.transform;
        ai.rightPoint = rightMarker.transform;
    }

    private void DisableFirstRoomMonster()
    {
        if (monsterTemplate != null)
            monsterTemplate.SetActive(false);
    }

    private void ApplyEnemyTint(GameObject enemy)
    {
        SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = enemyTint;

        EnemyVisualColor visualColor = enemy.GetComponent<EnemyVisualColor>();
        if (visualColor == null)
            visualColor = enemy.AddComponent<EnemyVisualColor>();

        visualColor.normalColor = enemyTint;
        visualColor.ApplyNow();
    }

    private void SpawnCoins(Transform parent)
    {
        if (coinSprite == null || coinPositions == null)
            return;

        for (int i = 0; i < coinPositions.Length; i++)
        {
            string coinName = "coin_room_" + (i + 1);
            if (GameObject.Find(coinName) != null)
                continue;

            GameObject coinObject = new GameObject(coinName);
            coinObject.transform.position = coinPositions[i];
            coinObject.transform.localScale = Vector3.one * 0.5f;
            coinObject.transform.SetParent(parent, true);

            SpriteRenderer renderer = coinObject.AddComponent<SpriteRenderer>();
            renderer.sprite = coinSprite;
            renderer.sortingOrder = 15;

            BoxCollider2D trigger = coinObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.8f, 0.8f);

            Coin coin = coinObject.AddComponent<Coin>();
            coin.scoreValue = 1;
            coin.rotateSpeed = 0f;
            CoinRoomPickup pickup = coinObject.AddComponent<CoinRoomPickup>();
            pickup.SetCoinId(coinName);
            coinObject.AddComponent<CoinSideRotation>().rotateSpeed = 100f;
        }
    }
}
