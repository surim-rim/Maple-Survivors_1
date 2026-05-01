using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public class UpgradeOption
    {
        public int    id;
        public string name;
        public string description;
    }

    public static readonly List<UpgradeOption> AllUpgrades = new List<UpgradeOption>
    {
        new UpgradeOption { id = 0, name = "공격 쿨타임 감소", description = "공격 쿨타임 20% 감소" },
        new UpgradeOption { id = 1, name = "공격력 UP",    description = "투사체 데미지 +10" },
        new UpgradeOption { id = 2, name = "이동속도 UP",  description = "이동 속도 +1" },
        new UpgradeOption { id = 3, name = "최대 HP UP",   description = "최대 HP +20, 즉시 회복" },
        new UpgradeOption { id = 4, name = "흡수 범위 UP",   description = "젬 흡수 범위 +2" },
        new UpgradeOption { id = 10, name = "투사체 속도 UP", description = "투사체 속도 +2" },
        new UpgradeOption { id = 11, name = "방어력 UP",      description = "방어력 +5, 피해 감소" },
        new UpgradeOption { id = 12, name = "공격 범위 UP",   description = "모든 공격 범위 및 이펙트 증가" },
        new UpgradeOption { id = 13, name = "부활",           description = "사망 시 1회 부활 (게임당 1회)" },
        new UpgradeOption { id = 14, name = "경험치 획득 UP", description = "경험치 획득량 +10%" },
    };

    private bool reviveUsed = false;

    void Awake() => Instance = this;

    void Start()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnLevelUp += OnLevelUp;
    }

    void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnLevelUp -= OnLevelUp;
    }

    void OnLevelUp()
    {
        Time.timeScale = 0f;
        GameUI.Instance?.ShowUpgradePanel(PickRandom(3));
    }

    List<UpgradeOption> PickRandom(int count)
    {
        var pool = new List<UpgradeOption>(AllUpgrades);

        // 부활은 이미 선택한 경우 풀에서 제거
        if (reviveUsed)
            pool.RemoveAll(o => o.id == 13);

        var weaponOpt = GetWeaponUpgradeOption();
        if (weaponOpt != null)
            pool.Add(weaponOpt);

        var result = new List<UpgradeOption>();
        while (result.Count < count && pool.Count > 0)
        {
            int i = Random.Range(0, pool.Count);
            result.Add(pool[i]);
            pool.RemoveAt(i);
        }
        return result;
    }

    UpgradeOption GetWeaponUpgradeOption()
    {
        int charId = CharacterSelectUI.ActiveCharacterId;
        switch (charId)
        {
            case 2:
            {
                var sa = FindObjectOfType<SwordAttack>();
                if (sa == null || sa.weaponLevel >= 5) return null;
                string[] effects = { "데미지 +15", "공격 범위 +1.5", "공격속도 20% 증가", "데미지 +25 [최대]" };
                return new UpgradeOption { id = 5, name = $"검 강화 Lv.{sa.weaponLevel + 1}", description = effects[sa.weaponLevel - 1] };
            }
            case 3:
            {
                var io = FindObjectOfType<IceOrbAttack>();
                if (io == null || io.weaponLevel >= 5) return null;
                string[] effects = { "발사 주기 -0.4초", "도트 데미지 x2", "동시 2발 발사", "동시 3발 발사 [최대]" };
                return new UpgradeOption { id = 6, name = $"오브 강화 Lv.{io.weaponLevel + 1}", description = effects[io.weaponLevel - 1] };
            }
            case 4:
            {
                var ts = FindObjectOfType<ThrowingStarAttack>();
                if (ts == null || ts.weaponLevel >= 5) return null;
                string[] effects = { "연속 3발로 증가", "연속 4발로 증가", "연속 5발로 증가", "연속 7발 [최대]" };
                return new UpgradeOption { id = 7, name = $"표창 강화 Lv.{ts.weaponLevel + 1}", description = effects[ts.weaponLevel - 1] };
            }
            case 5:
            {
                var ca = FindObjectOfType<CannonAttack>();
                if (ca == null || ca.weaponLevel >= 5) return null;
                string[] effects = { "폭발 범위 +1", "화염 지속 +2초", "발사 주기 -0.6초", "폭발 데미지 x1.5 [최대]" };
                return new UpgradeOption { id = 8, name = $"대포 강화 Lv.{ca.weaponLevel + 1}", description = effects[ca.weaponLevel - 1] };
            }
            case 6:
            {
                var ba = FindObjectOfType<BowAttack>();
                if (ba == null || ba.weaponLevel >= 5) return null;
                string[] effects = { "화살 2발 동시 발사", "화살 3발 동시 발사", "화살 4발 동시 발사", "화살 5발 동시 발사 [최대]" };
                return new UpgradeOption { id = 9, name = $"활 강화 Lv.{ba.weaponLevel + 1}", description = effects[ba.weaponLevel - 1] };
            }
            default:
                return null;
        }
    }

    public void ApplyUpgrade(int id)
    {
        var ps = PlayerStats.Instance;
        var pc = FindObjectOfType<PlayerController>();
        var aa = FindObjectOfType<AutoAttack>();
        var sa = FindObjectOfType<SwordAttack>();
        var ba = FindObjectOfType<BowAttack>();
        var ta = FindObjectOfType<ThrowingStarAttack>();
        var ca = FindObjectOfType<CannonAttack>();
        var io = FindObjectOfType<IceOrbAttack>();

        switch (id)
        {
            case 0: // 공격속도
                if (ps != null) ps.attackInterval = Mathf.Max(0.15f, ps.attackInterval * 0.8f);
                if (aa != null && ps != null) aa.attackInterval = ps.attackInterval;
                if (sa != null && ps != null) sa.attackInterval = ps.attackInterval;
                if (ba != null && ps != null) ba.attackInterval = ps.attackInterval;
                if (ta != null && ps != null) ta.attackInterval = ps.attackInterval;
                if (ca != null && ps != null) ca.attackInterval = Mathf.Max(0.5f, ps.attackInterval * 2f);
                break;
            case 1: // 공격력
                if (ps != null) ps.damage += 10;
                break;
            case 2: // 이동속도
                if (pc != null) pc.moveSpeed += 1f;
                break;
            case 3: // 최대 HP
                if (pc != null) { pc.maxHP += 20; pc.Heal(20); }
                break;
            case 4: // 흡수 범위
                if (ps != null) ps.gemPickupRadius += 2f;
                break;
            case 10: // 투사체 속도
                if (ba != null) ba.projectileSpeed += 2f;
                if (ta != null) ta.projectileSpeed += 2f;
                if (io != null) io.projectileSpeed += 2f;
                break;
            case 11: // 방어력
                if (ps != null) ps.defense += 5;
                break;
            case 13: // 부활
                reviveUsed = true;
                if (pc != null) pc.hasRevive = true;
                break;
            case 14: // 경험치 획득 UP
                if (ps != null) ps.xpMultiplier += 0.1f;
                break;

            case 12: // 공격 범위
                if (aa != null) aa.attackRange  += 1f;
                if (sa != null) sa.attackRange  += 1f;
                if (io != null) { io.damageRange += 0.3f; io.orbScale  += 0.05f; }
                if (ta != null) ta.starScale  += 0.05f;
                if (ba != null) ba.arrowScale += 0.05f;
                if (ca != null) ca.explosionRadius += 0.5f;
                break;

            case 5: // 히어로 검 강화
                if (sa == null) break;
                sa.weaponLevel++;
                switch (sa.weaponLevel)
                {
                    case 2: sa.weaponDamageBonus += 15; break;
                    case 3: sa.attackRange += 1.5f; break;
                    case 4: sa.attackInterval = Mathf.Max(0.15f, sa.attackInterval * 0.8f); break;
                    case 5: sa.weaponDamageBonus += 25; break;
                }
                break;

            case 6: // 썬콜 오브 강화
                if (io == null) break;
                io.weaponLevel++;
                switch (io.weaponLevel)
                {
                    case 2: io.attackInterval = Mathf.Max(0.5f, io.attackInterval - 0.4f); break;
                    case 3: io.damageMultiplier *= 2f; break;
                    case 4: io.orbCount = 2; break;
                    case 5: io.orbCount = 3; break;
                }
                break;

            case 7: // 나이트로드 표창 강화
                if (ta == null) break;
                ta.weaponLevel++;
                switch (ta.weaponLevel)
                {
                    case 2: ta.burstCount = 3; break;
                    case 3: ta.burstCount = 4; break;
                    case 4: ta.burstCount = 5; break;
                    case 5: ta.burstCount = 7; break;
                }
                break;

            case 8: // 캐논슈터 대포 강화
                if (ca == null) break;
                ca.weaponLevel++;
                switch (ca.weaponLevel)
                {
                    case 2: ca.explosionRadius += 1f; break;
                    case 3: ca.burnDuration += 2f; break;
                    case 4: ca.attackInterval = Mathf.Max(1f, ca.attackInterval - 0.6f); break;
                    case 5: ca.damageMultiplier = 1.5f; break;
                }
                break;

            case 9: // 보우마스터 활 강화
                if (ba == null) break;
                ba.weaponLevel++;
                break;
        }

        Time.timeScale = 1f;
        GameUI.Instance?.HideUpgradePanel();
    }
}
