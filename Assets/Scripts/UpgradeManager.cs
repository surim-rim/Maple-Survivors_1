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
        new UpgradeOption { id = 0, name = "공격속도 UP",  description = "공격 속도 20% 증가" },
        new UpgradeOption { id = 1, name = "공격력 UP",    description = "투사체 데미지 +10" },
        new UpgradeOption { id = 2, name = "이동속도 UP",  description = "이동 속도 +1" },
        new UpgradeOption { id = 3, name = "최대 HP UP",   description = "최대 HP +20, 즉시 회복" },
        new UpgradeOption { id = 4, name = "흡수 범위 UP", description = "젬 흡수 범위 +2" },
    };

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
        var pool   = new List<UpgradeOption>(AllUpgrades);
        var result = new List<UpgradeOption>();
        while (result.Count < count && pool.Count > 0)
        {
            int i = Random.Range(0, pool.Count);
            result.Add(pool[i]);
            pool.RemoveAt(i);
        }
        return result;
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
        }

        Time.timeScale = 1f;
        GameUI.Instance?.HideUpgradePanel();
    }
}
