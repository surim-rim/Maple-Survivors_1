using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class SceneSetup
{
    [MenuItem("Maple Survivors/Phase 1 씬 자동 세팅")]
    static void SetupScene()
    {
        // 태그 등록
        AddTag("Player");
        AddTag("Enemy");
        AddTag("Box");

        // 폴더 생성
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Textures");

        // 플레이어 스프라이트 로드 (left1/2, right1/2)
        Sprite[] rightSprites = LoadPlayerSprites("character/right1", "character/right2");
        Sprite[] leftSprites  = LoadPlayerSprites("character/left1",  "character/left2");
        Sprite   firstSprite  = rightSprites.Length > 0 ? rightSprites[0]
                                : CreateCircleSprite("Player", new Color(0.3f, 0.8f, 1f));

        // 달팽이 투사체 스프라이트 로드
        Sprite[] snailSprites = LoadPlayerSprites("Weapon/snail1", "Weapon/snail2");
        Sprite   snailFirst   = snailSprites.Length > 0 ? snailSprites[0]
                                : CreateCircleSprite("Projectile", new Color(1f, 1f, 0.2f), 32);

        // XP 젬 스프라이트 & 프리팹
        Sprite     gemSprite = LoadPlayerSprites("XPG")[0];
        GameObject gemPrefab = CreateXPGemPrefab(gemSprite);

        // 보스 프리팹 5종 (순서대로 스폰)
        GameObject[] bossPrefabs = new GameObject[]
        {
            CreateBossPrefabWithSprites("MixGolem", LoadPlayerSprites("boss/mg1","boss/mg2"), gemPrefab, 0.5f, 500,  1.5f, 25, 10),
            CreateBossPrefabWithSprites("Octopus",  LoadPlayerSprites("boss/op1","boss/op2"), gemPrefab, 0.5f, 700,  2.0f, 30, 15),
            CreateBossPrefabWithSprites("FireBoar", LoadPlayerSprites("boss/fb1","boss/fb2"), gemPrefab, 0.5f, 900,  2.5f, 35, 20),
            CreateBossPrefabWithSprites("Stumpy",   LoadPlayerSprites("boss/xt1","boss/xt2"), gemPrefab, 0.5f, 1200, 2.0f, 40, 25),
            CreateBossPrefabWithSprites("JrBalrog", LoadPlayerSprites("boss/jb1","boss/jb2"), gemPrefab, 0.6f, 1500, 1.8f, 50, 30),
        };

        // 프리팹 생성
        GameObject projectilePrefab = CreateProjectilePrefab(snailFirst, snailSprites);

        // 9종 몹 프리팹
        GameObject[] enemyPrefabs = new GameObject[]
        {
            CreateEnemyPrefabWithSprites("Red", LoadPlayerSprites("mob/red1","mob/red2"), gemPrefab, 0.15f),
            CreateEnemyPrefabWithSprites("Pig", LoadPlayerSprites("mob/pig1","mob/pig2"), gemPrefab, 0.25f),
            CreateEnemyPrefabWithSprites("Eye", LoadPlayerSprites("mob/eye1","mob/eye2"), gemPrefab, 0.3f),
            CreateEnemyPrefabWithSprites("Sl",  LoadPlayerSprites("mob/sl1", "mob/sl2"),  gemPrefab, 0.3f),
            CreateEnemyPrefabWithSprites("Mu",  LoadPlayerSprites("mob/mu1", "mob/mu2"),  gemPrefab, 0.2f),
            CreateEnemyPrefabWithSprites("G",   LoadPlayerSprites("mob/g1",  "mob/g2"),   gemPrefab, 0.15f),
            CreateEnemyPrefabWithSprites("Ss",  LoadPlayerSprites("mob/ss1", "mob/ss2"),  gemPrefab, 0.15f),
            CreateEnemyPrefabWithSprites("B",   LoadPlayerSprites("mob/b1",  "mob/b2"),   gemPrefab, 0.25f),
            CreateEnemyPrefabWithSprites("D",   LoadPlayerSprites("mob/d1",  "mob/d2"),   gemPrefab, 0.25f),
        };

        // 랜덤 박스 & 아이템 프리팹
        GameObject[] itemPrefabs = new GameObject[]
        {
            CreateRandomItemPrefab("RandomItem_Heal",   LoadPlayerSprites("RandomItem/heal")[0],   RandomItem.ItemType.Heal),
            CreateRandomItemPrefab("RandomItem_Magnet", LoadPlayerSprites("RandomItem/magnet")[0], RandomItem.ItemType.Magnet),
            CreateRandomItemPrefab("RandomItem_Gold1",  LoadPlayerSprites("RandomItem/mo1")[0],    RandomItem.ItemType.Gold1),
            CreateRandomItemPrefab("RandomItem_Gold2",  LoadPlayerSprites("RandomItem/mo2")[0],    RandomItem.ItemType.Gold2),
            CreateRandomItemPrefab("RandomItem_Gold3",  LoadPlayerSprites("RandomItem/mo3")[0],    RandomItem.ItemType.Gold3),
            CreateRandomItemPrefab("RandomItem_Gold4",  LoadPlayerSprites("RandomItem/mo4")[0],    RandomItem.ItemType.Gold4),
        };
        GameObject boxPrefab = CreateRandomBoxPrefab(LoadPlayerSprites("RandomItem/rbox")[0], itemPrefabs);

        // 씬 오브젝트 배치
        CreateBackground();
        GameObject player = CreatePlayer(firstSprite, rightSprites, leftSprites, projectilePrefab);
        CreateEnemySpawner(enemyPrefabs);
        SetupCamera(player.transform);
        CreateManagers(player, bossPrefabs);
        CreateRandomBoxSpawner(boxPrefab);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Maple Survivors] Phase 2 씬 세팅 완료! Ctrl+S 로 씬을 저장하세요.");
    }

    // ── 플레이어 스프라이트 로드 ───────────────────────────
    static Sprite[] LoadPlayerSprites(params string[] names)
    {
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var name in names)
        {
            string path = $"Assets/Textures/{name}.png";
            // Sprite 타입으로 임포트 보장
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType         = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) list.Add(sprite);
            else Debug.LogWarning($"[SceneSetup] 스프라이트를 찾을 수 없음: {path}");
        }
        return list.ToArray();
    }

    // ── 태그 ──────────────────────────────────────────────
    static void AddTag(string tag)
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    // ── 폴더 ──────────────────────────────────────────────
    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    // ── 스프라이트 ────────────────────────────────────────
    static Sprite CreateCircleSprite(string name, Color color, int size = 64)
    {
        string assetPath = $"Assets/Textures/{name}.png";
        string fullPath  = Application.dataPath + $"/Textures/{name}.png";

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float cx = size / 2f, r = size / 2f - 2;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
                tex.SetPixel(x, y, d <= r ? color : Color.clear);
            }
        tex.Apply();

        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(assetPath);

        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType          = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit  = size;
        importer.alphaIsTransparency  = true;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    // ── Projectile 프리팹 ──────────────────────────────────
    static GameObject CreateProjectilePrefab(Sprite firstSprite, Sprite[] sprites)
    {
        var obj = new GameObject("Projectile");
        obj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = firstSprite;

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

        var col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.2f;

        var proj = obj.AddComponent<Projectile>();
        proj.sprites = sprites;

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Prefabs/Projectile.prefab");
        Object.DestroyImmediate(obj);
        return prefab;
    }

    // ── Boss 프리팹 ───────────────────────────────────────
    static GameObject CreateBossPrefabWithSprites(string name, Sprite[] sprites, GameObject gemPrefab,
                                                   float visualScale = 0.5f, int maxHP = 500,
                                                   float moveSpeed = 1.5f, int contactDamage = 25,
                                                   int xpDropCount = 10)
    {
        var obj = new GameObject(name);
        obj.tag = "Enemy";

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.drag         = 1f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType     = RigidbodyType2D.Dynamic;

        var col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius    = 2.0f;

        var boss = obj.AddComponent<BossEnemy>();
        boss.xpGemPrefab   = gemPrefab;
        boss.sprites       = sprites;
        boss.maxHP         = maxHP;
        boss.moveSpeed     = moveSpeed;
        boss.contactDamage = contactDamage;
        boss.xpDropCount   = xpDropCount;

        var visual = new GameObject("Visual");
        visual.transform.SetParent(obj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale    = new Vector3(visualScale, visualScale, 1f);

        var sr = visual.AddComponent<SpriteRenderer>();
        if (sprites != null && sprites.Length > 0) sr.sprite = sprites[0];
        sr.sortingOrder = 1;

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, $"Assets/Prefabs/{name}Boss.prefab");
        Object.DestroyImmediate(obj);
        return prefab;
    }

    // ── XP 젬 프리팹 ──────────────────────────────────────
    static GameObject CreateXPGemPrefab(Sprite sprite)
    {
        var obj = new GameObject("XPGem");
        obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.2f);

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = 0;

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

        var col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.3f;

        obj.AddComponent<XPGem>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Prefabs/XPGem.prefab");
        Object.DestroyImmediate(obj);
        return prefab;
    }

    // ── 랜덤 박스 프리팹 ──────────────────────────────────────
    static GameObject CreateRandomBoxPrefab(Sprite boxSprite, GameObject[] itemPrefabs)
    {
        var obj = new GameObject("RandomBox");
        obj.tag = "Box";
        obj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite       = boxSprite;
        sr.sortingOrder = 2;

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType     = RigidbodyType2D.Kinematic;
        rb.constraints  = RigidbodyConstraints2D.FreezeAll;

        var col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.5f;

        var box = obj.AddComponent<RandomBox>();
        box.itemPrefabs = itemPrefabs;

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Prefabs/RandomBox.prefab");
        Object.DestroyImmediate(obj);
        return prefab;
    }

    // ── 랜덤 아이템 프리팹 ────────────────────────────────────
    static GameObject CreateRandomItemPrefab(string prefabName, Sprite sprite, RandomItem.ItemType type)
    {
        var obj = new GameObject(prefabName);
        obj.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = 2;

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

        var col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.3f;

        var item = obj.AddComponent<RandomItem>();
        item.itemType = type;

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, $"Assets/Prefabs/{prefabName}.prefab");
        Object.DestroyImmediate(obj);
        return prefab;
    }

    // ── 랜덤 박스 스포너 ──────────────────────────────────────
    static void CreateRandomBoxSpawner(GameObject boxPrefab)
    {
        var existing = GameObject.Find("RandomBoxSpawner");
        if (existing != null) Object.DestroyImmediate(existing);

        var obj     = new GameObject("RandomBoxSpawner");
        var spawner = obj.AddComponent<RandomBoxSpawner>();
        spawner.boxPrefab      = boxPrefab;
        spawner.spawnInterval  = 20f;
        spawner.minDist        = 8f;
        spawner.maxDist        = 14f;
        spawner.maxBoxes       = 5;
    }

    // ── Enemy 프리팹 ───────────────────────────────────────
    static GameObject CreateEnemyPrefab(Sprite sprite, GameObject gemPrefab)
    {
        var obj = new GameObject("Enemy");
        obj.tag = "Enemy";

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.drag          = 2f;   // 충돌 후 미끄러짐 방지
        rb.constraints   = RigidbodyConstraints2D.FreezeRotation;
        // Dynamic 으로 설정해야 Enemy끼리, Player와 물리 충돌 가능
        rb.bodyType      = RigidbodyType2D.Dynamic;

        var col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = false;   // 실제 물리 충돌 활성화
        col.radius    = 0.4f;

        var enemy = obj.AddComponent<Enemy>();
        enemy.xpGemPrefab = gemPrefab;

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(obj);
        return prefab;
    }

    // ── Managers ──────────────────────────────────────────
    static void CreateManagers(GameObject player, GameObject[] bossPrefabs)
    {
        // PlayerStats — Player 오브젝트에 붙임
        if (player.GetComponent<PlayerStats>() == null)
            player.AddComponent<PlayerStats>();

        // UpgradeManager
        var existingUM = GameObject.Find("UpgradeManager");
        if (existingUM != null) Object.DestroyImmediate(existingUM);
        new GameObject("UpgradeManager").AddComponent<UpgradeManager>();

        // GameUI
        var existingUI = GameObject.Find("GameUI");
        if (existingUI != null) Object.DestroyImmediate(existingUI);
        var gameUI = new GameObject("GameUI").AddComponent<GameUI>();

        // 업그레이드 아이콘 (id 순: 0=공격속도, 1=공격력, 2=이동속도, 3=최대HP, 4=흡수범위)
        gameUI.upgradeIcons = new Sprite[15];
        var ctSprites    = LoadPlayerSprites("status/Ct");     if (ctSprites.Length    > 0) gameUI.upgradeIcons[0]  = ctSprites[0];
        var dmgSprites   = LoadPlayerSprites("status/damage"); if (dmgSprites.Length   > 0) gameUI.upgradeIcons[1]  = dmgSprites[0];
        var spdSprites   = LoadPlayerSprites("status/speed");  if (spdSprites.Length   > 0) gameUI.upgradeIcons[2]  = spdSprites[0];
        var hpSprites    = LoadPlayerSprites("status/hp");     if (hpSprites.Length    > 0) gameUI.upgradeIcons[3]  = hpSprites[0];
        var jaSprites    = LoadPlayerSprites("status/Ja");     if (jaSprites.Length    > 0) gameUI.upgradeIcons[4]  = jaSprites[0];
        var tsSprites    = LoadPlayerSprites("status/Ts");     if (tsSprites.Length    > 0) gameUI.upgradeIcons[10] = tsSprites[0];
        var amSprites    = LoadPlayerSprites("status/am");     if (amSprites.Length    > 0) gameUI.upgradeIcons[11] = amSprites[0];
        var sizeSprites  = LoadPlayerSprites("status/size");   if (sizeSprites.Length  > 0) gameUI.upgradeIcons[12] = sizeSprites[0];
        var rsSprites    = LoadPlayerSprites("status/rs");     if (rsSprites.Length    > 0) gameUI.upgradeIcons[13] = rsSprites[0];
        var axpSprites   = LoadPlayerSprites("status/axp");    if (axpSprites.Length   > 0) gameUI.upgradeIcons[14] = axpSprites[0];
        // 무기 강화 아이콘 (id 5~9: 히어로, 썬콜, 나이트로드, 캐논슈터, 보우마스터)
        var swW2Sprites  = LoadPlayerSprites("Weapon/attack"); if (swW2Sprites.Length  > 0) gameUI.upgradeIcons[5] = swW2Sprites[0];
        var iceW2Sprites = LoadPlayerSprites("Weapon/ice_w");  if (iceW2Sprites.Length > 0) gameUI.upgradeIcons[6] = iceW2Sprites[0];
        var niW2Sprites  = LoadPlayerSprites("Weapon/ni_w");   if (niW2Sprites.Length  > 0) gameUI.upgradeIcons[7] = niW2Sprites[0];
        var caW2Sprites  = LoadPlayerSprites("Weapon/ca_w");   if (caW2Sprites.Length  > 0) gameUI.upgradeIcons[8] = caW2Sprites[0];
        var boW2Sprites  = LoadPlayerSprites("Weapon/boma_w"); if (boW2Sprites.Length  > 0) gameUI.upgradeIcons[9] = boW2Sprites[0];

        // DamageTextManager
        var existingDT = GameObject.Find("DamageTextManager");
        if (existingDT != null) Object.DestroyImmediate(existingDT);
        new GameObject("DamageTextManager").AddComponent<DamageTextManager>();

        // GameManager + Boss 프리팹 연결
        var existingGM = GameObject.Find("GameManager");
        if (existingGM != null) Object.DestroyImmediate(existingGM);
        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        if (bossPrefabs != null) gm.bossPrefabs = bossPrefabs;

        // PauseMenu
        var existingPM = GameObject.Find("PauseMenu");
        if (existingPM != null) Object.DestroyImmediate(existingPM);
        new GameObject("PauseMenu").AddComponent<PauseMenu>();

        // CharacterSelectUI
        var existingCS = GameObject.Find("CharacterSelectUI");
        if (existingCS != null) Object.DestroyImmediate(existingCS);
        var csObj = new GameObject("CharacterSelectUI");
        var csUI  = csObj.AddComponent<CharacterSelectUI>();
        csUI.autoAttack         = player.GetComponent<AutoAttack>();
        csUI.swordAttack        = player.GetComponent<SwordAttack>();
        csUI.iceOrbAttack       = player.GetComponent<IceOrbAttack>();
        csUI.throwingStarAttack = player.GetComponent<ThrowingStarAttack>();
        csUI.cannonAttack       = player.GetComponent<CannonAttack>();
        csUI.bowAttack          = player.GetComponent<BowAttack>();

        // 캐릭터별 대표 이미지 (인덱스 0~5 = 캐릭터 1~6)
        csUI.characterSprites = new Sprite[6];
        Sprite[] ch1sp = LoadPlayerSprites("character/right1"); if (ch1sp.Length > 0) csUI.characterSprites[0] = ch1sp[0];
        Sprite[] ch2sp = LoadPlayerSprites("character/hh2");    if (ch2sp.Length > 0) csUI.characterSprites[1] = ch2sp[0];
        Sprite[] ch3sp = LoadPlayerSprites("character/ice2");   if (ch3sp.Length > 0) csUI.characterSprites[2] = ch3sp[0];
        Sprite[] ch4sp = LoadPlayerSprites("character/ni2");    if (ch4sp.Length > 0) csUI.characterSprites[3] = ch4sp[0];
        Sprite[] ch5sp = LoadPlayerSprites("character/ca2");    if (ch5sp.Length > 0) csUI.characterSprites[4] = ch5sp[0];
        Sprite[] ch6sp = LoadPlayerSprites("character/boma2");  if (ch6sp.Length > 0) csUI.characterSprites[5] = ch6sp[0];

        // 캐릭터별 인게임 스프라이트 세트
        csUI.charSpriteSets = new CharacterSelectUI.CharSpriteSet[6];
        csUI.charSpriteSets[0] = new CharacterSelectUI.CharSpriteSet // 초보자
        {
            rightSprites = LoadPlayerSprites("character/right1", "character/right2"),
            leftSprites  = LoadPlayerSprites("character/left2",  "character/left1")
        };
        csUI.charSpriteSets[1] = new CharacterSelectUI.CharSpriteSet // 히어로
        {
            rightSprites = LoadPlayerSprites("character/hh2", "character/hh1"),
            leftSprites  = new Sprite[0]
        };
        csUI.charSpriteSets[2] = new CharacterSelectUI.CharSpriteSet // 썬콜
        {
            rightSprites = LoadPlayerSprites("character/ice2", "character/ice1"),
            leftSprites  = new Sprite[0]
        };
        csUI.charSpriteSets[3] = new CharacterSelectUI.CharSpriteSet // 나이트로드
        {
            rightSprites = LoadPlayerSprites("character/ni2", "character/ni1"),
            leftSprites  = new Sprite[0]
        };
        csUI.charSpriteSets[4] = new CharacterSelectUI.CharSpriteSet // 캐논슈터
        {
            rightSprites = LoadPlayerSprites("character/ca2", "character/ca1"),
            leftSprites  = new Sprite[0]
        };
        csUI.charSpriteSets[5] = new CharacterSelectUI.CharSpriteSet // 보우마스터
        {
            rightSprites = LoadPlayerSprites("character/boma2", "character/boma1"),
            leftSprites  = new Sprite[0]
        };
    }

    // ── Player ────────────────────────────────────────────
    static GameObject CreatePlayer(Sprite firstSprite, Sprite[] rightSprites, Sprite[] leftSprites, GameObject projectilePrefab)
    {
        var existing = GameObject.FindGameObjectWithTag("Player");
        if (existing != null) Object.DestroyImmediate(existing);

        var obj = new GameObject("Player");
        obj.tag = "Player";
        obj.transform.position   = Vector3.zero;
        obj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = firstSprite;
        sr.sortingOrder = 1;

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

        var col = obj.AddComponent<CapsuleCollider2D>();
        col.isTrigger = false;
        col.direction = CapsuleDirection2D.Vertical;
        // 스프라이트 723x1024px, PPU=100 → 로컬 크기 7.23 x 10.24
        col.size      = new Vector2(7.0f, 10.0f);

        var pc = obj.AddComponent<PlayerController>();
        pc.rightSprites = rightSprites;
        pc.leftSprites  = leftSprites;
        obj.AddComponent<PlayerHealthBar>();

        var autoAttack = obj.AddComponent<AutoAttack>();
        autoAttack.projectilePrefab = projectilePrefab;
        autoAttack.enabled = false;

        var swordAttack = obj.AddComponent<SwordAttack>();
        swordAttack.enabled = false;
        var attackSprites = LoadPlayerSprites("Weapon/attack");
        if (attackSprites.Length > 0) swordAttack.slashSprite = attackSprites[0];

        var iceOrb = obj.AddComponent<IceOrbAttack>();
        iceOrb.enabled = false;
        var iceWSprites = LoadPlayerSprites("Weapon/ice_w");
        if (iceWSprites.Length > 0) iceOrb.orbSprite = iceWSprites[0];

        var throwingStar = obj.AddComponent<ThrowingStarAttack>();
        throwingStar.enabled = false;
        var niWSprites = LoadPlayerSprites("Weapon/ni_w");
        if (niWSprites.Length > 0) throwingStar.starSprite = niWSprites[0];

        var cannon = obj.AddComponent<CannonAttack>();
        cannon.enabled = false;
        var caWSprites = LoadPlayerSprites("Weapon/ca_w");
        if (caWSprites.Length > 0) cannon.cannonballSprite = caWSprites[0];

        var bow = obj.AddComponent<BowAttack>();
        bow.enabled = false;
        var bomaWSprites = LoadPlayerSprites("Weapon/boma_w");
        if (bomaWSprites.Length > 0) bow.arrowSprite = bomaWSprites[0];

        return obj;
    }

    // ── EnemySpawner ──────────────────────────────────────
    static void CreateEnemySpawner(GameObject[] enemyPrefabs)
    {
        var existing = GameObject.Find("EnemySpawner");
        if (existing != null) Object.DestroyImmediate(existing);

        var obj = new GameObject("EnemySpawner");
        var spawner = obj.AddComponent<EnemySpawner>();
        spawner.enemyPrefabs = enemyPrefabs;
    }

    // ── 스프라이트 기반 Enemy 프리팹 ─────────────────────────
    static GameObject CreateEnemyPrefabWithSprites(string name, Sprite[] sprites, GameObject gemPrefab,
                                                    float visualScale = 0.2f)
    {
        var obj = new GameObject(name);
        obj.tag = "Enemy";

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.drag         = 2f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType     = RigidbodyType2D.Dynamic;

        var col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius    = 0.8f;

        var enemy = obj.AddComponent<Enemy>();
        enemy.xpGemPrefab = gemPrefab;
        enemy.sprites     = sprites;

        // 비주얼 자식 오브젝트 (히트박스와 독립적으로 크기 조절)
        var visual = new GameObject("Visual");
        visual.transform.SetParent(obj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale    = new Vector3(visualScale, visualScale, 1f);

        var sr = visual.AddComponent<SpriteRenderer>();
        if (sprites != null && sprites.Length > 0) sr.sprite = sprites[0];

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, $"Assets/Prefabs/{name}Enemy.prefab");
        Object.DestroyImmediate(obj);
        return prefab;
    }

    // ── Background ────────────────────────────────────────
    static void CreateBackground()
    {
        string assetPath = "Assets/Textures/mmapp.png";

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("[SceneSetup] mmapp.png 를 찾을 수 없습니다.");
            return;
        }

        importer.textureType          = TextureImporterType.Sprite;
        importer.alphaIsTransparency  = true;
        importer.wrapMode             = TextureWrapMode.Repeat;
        importer.spritePixelsPerUnit  = 20;
        importer.SaveAndReimport();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

        var existing = GameObject.Find("Background");
        if (existing != null) Object.DestroyImmediate(existing);

        var obj = new GameObject("Background");
        obj.transform.position   = Vector3.zero;
        obj.transform.localScale = Vector3.one;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -10;
        sr.drawMode     = SpriteDrawMode.Tiled;
        sr.tileMode     = SpriteTileMode.Continuous;
        sr.size         = new Vector2(150f, 150f); // 맵 전체(-75~+75) 커버
    }

    // ── Camera ────────────────────────────────────────────
    static void SetupCamera(Transform playerTransform)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main Camera를 찾을 수 없습니다.");
            return;
        }

        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographicSize = 6f;

        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = playerTransform;
    }
}
