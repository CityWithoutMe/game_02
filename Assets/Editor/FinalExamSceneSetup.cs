using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class FinalExamSceneSetup
{
    private const string RootName = "Final Exam Encounter";
    [MenuItem("Tools/Final Exam/Rebuild Rooftop Boss")]
    public static void Build()
    {
        // Preserve any unsaved work if this menu is invoked interactively.
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/main.unity");
        Sprite paper = ImportSprite("Assets/Art/FinalExamBoss/BlackExam.png");
        Sprite mirrorSprite = ImportSprite("Assets/Art/FinalExamBoss/Mirror.png");
        var room = GameObject.Find("06_Bright_Rooftop");
        if (room == null) throw new Exception("Missing room six.");
        var old = GameObject.Find(RootName); if (old != null) UnityEngine.Object.DestroyImmediate(old);
        var oldPlatforms = room.transform.Find("Final Boss Platforms (Tilemap)");
        if (oldPlatforms != null) UnityEngine.Object.DestroyImmediate(oldPlatforms.gameObject);
        var root = new GameObject(RootName);
        root.transform.SetParent(room.transform, false);
        var encounter = root.AddComponent<FinalExamEncounter>();
        encounter.paperSprite = paper;
        encounter.bossVisual = Visual("Black Exam Boss (appears at center)", paper, new Vector3(178f, 8.8f), 4f, 20, root.transform);
        encounter.bossVisual.gameObject.layer = 1; // Exclude the trigger from the player ground mask.
        var target = encounter.bossVisual.gameObject.AddComponent<ExamPaperBoss>(); target.encounter = encounter;
        var hitbox = target.gameObject.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
        hitbox.size = paper.bounds.size * 0.8f;
        encounter.mirror = Visual("Mirror", mirrorSprite, new Vector3(178f, 3.1f), 4.2f, 8, root.transform);
        var player = GameObject.FindGameObjectWithTag("Player");
        Sprite playerSprite = player.GetComponentInChildren<SpriteRenderer>().sprite;
        encounter.reflection = Visual("Black player reflection", playerSprite, new Vector3(178f, 2.8f), 1.9f, 9, root.transform);
        encounter.reflection.color = Color.black;
        var hazards = new GameObject("Runtime papers and columns"); hazards.transform.SetParent(root.transform, false);
        encounter.hazardsRoot = hazards.transform;
        var platforms = new GameObject("Final Boss Platforms (Tilemap)", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D), typeof(PlatformEffector2D));
        platforms.transform.SetParent(room.transform, false);
        var map = platforms.GetComponent<Tilemap>();
        var floor = room.transform.Find("Floor and jump platforms").GetComponent<TilemapRenderer>();
        var renderer = platforms.GetComponent<TilemapRenderer>(); renderer.sharedMaterial = floor.sharedMaterial; renderer.sortingOrder = 12;
        var collider = platforms.GetComponent<TilemapCollider2D>(); collider.usedByEffector = true;
        var effector = platforms.GetComponent<PlatformEffector2D>(); effector.useOneWay = true; effector.useOneWayGrouping = true; effector.surfaceArc = 150f;
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Art/SixRoomSchool/Tiles/Floor_Charcoal.asset");
        // Two routes, 2-unit rises (< 2.82-unit full K jump), open center below the boss.
        foreach (Vector2Int start in new[] {new Vector2Int(170,2), new Vector2Int(183,2), new Vector2Int(173,4), new Vector2Int(180,4), new Vector2Int(175,6), new Vector2Int(179,6)})
            for (int dx = 0; dx < 3; dx++)
            {
                var cell = new Vector3Int(start.x + dx, start.y, 0);
                map.SetTile(cell, tile);
                map.SetTileFlags(cell, TileFlags.None);
                map.SetTransformMatrix(cell, Matrix4x4.Scale(new Vector3(1f, 0.32f, 1f)));
            }
        FinalExamVictorySceneSetup.Configure(encounter);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Validate();
        FinalExamVictorySceneSetup.Validate();
        Debug.Log("FINAL_EXAM_BUILD_OK");
    }
    private static Sprite ImportSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 256; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static SpriteRenderer Visual(string name, Sprite sprite, Vector3 position, float height, int order, Transform parent)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position;
        go.transform.localScale = Vector3.one * (height / sprite.bounds.size.y);
        var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = order;
        return renderer;
    }
    public static void Validate()
    {
        var encounter = UnityEngine.Object.FindObjectOfType<FinalExamEncounter>();
        if (encounter == null || encounter.bossVisual == null || encounter.paperSprite == null || encounter.mirror == null || encounter.reflection == null || encounter.hazardsRoot == null)
            throw new Exception("Missing final encounter references");
        if (encounter.bossVisual.GetComponent<ExamPaperBoss>().encounter != encounter) throw new Exception("Boss receiver disconnected");
        var map = GameObject.Find("Final Boss Platforms (Tilemap)").GetComponent<Tilemap>();
        int count = 0; foreach (var cell in map.cellBounds.allPositionsWithin) if (map.HasTile(cell)) count++;
        if (count != 18 || !map.GetComponent<TilemapCollider2D>().usedByEffector) throw new Exception("Platforms invalid");
        var player = GameObject.FindGameObjectWithTag("Player");
        var stats = player.GetComponent<PlayerAttributes>(); var body = player.GetComponent<Rigidbody2D>();
        float height = stats.JumpVelocity * stats.JumpVelocity / (2f * Mathf.Abs(Physics2D.gravity.y) * body.gravityScale);
        if (height < 2.5f) throw new Exception("Current jump is too low for the steps");
        foreach (var obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj) > 0) throw new Exception("Missing script on " + obj.name);
        Debug.Log("FINAL_EXAM_VALIDATION_OK: 18 collidable tiles; full jump height=" + height);
    }
}
