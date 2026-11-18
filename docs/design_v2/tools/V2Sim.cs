using System;
using System.Collections.Generic;
using System.Text;

// v2 공격형 라인 디펜스 전투 시뮬레이터
// - 불꽃(Ember): 전진하여 적 유닛/적 본진 공격
// - 새싹(Verdant): 방어선 고정, 인구/생산 기여, 해킹 드론(본진 직행) 요격
// - 적: 적 본진에서 지상 유닛 생산, 주기적으로 자폭 드론 스폰
// 사용: Add-Type -Path tools\V2Sim.cs 후 [V2Sim]::Run(...) 호출
public static class V2Sim
{
    struct U { public double x, hp, atk, itv, range, cd, spd; public bool ember; }
    struct En { public double x, hp, atk, itv, range, cd, spd; }
    struct Dr { public double x, hp, spd; }

    static readonly double[][] EmberTraits = {
        new double[]{0.30,0,0,0}, new double[]{0,0.30,0,0}, new double[]{-0.15,0,0.30,0},
        new double[]{0,0,-0.20,1.5}, new double[]{0,0.15,0,0}, new double[]{0,-0.10,0,0},
        new double[]{0,-0.15,0,0}, new double[]{0,0,0.20,-1.0}, new double[]{0.50,0,0,0},
        new double[]{-0.10,0,0,0}, new double[]{-0.30,0,0,0}, new double[]{0.30,0,0,0}
    };
    static readonly double[][] VerdantTraits = {
        new double[]{0,-0.10,0,0}, new double[]{0.10,0,0,0}, new double[]{0,0.25,0,0},
        new double[]{0.25,0,0,0}, new double[]{-0.10,0,0.25,0}, new double[]{0,0.20,0,0},
        new double[]{0,-0.05,0,0}, new double[]{0,0.12,0,0}, new double[]{0,0.10,0,1.0},
        new double[]{0,0,0.15,0}, new double[]{-0.30,0,0,0}, new double[]{0,-0.15,0,0}
    };
    static readonly double[] EmberWeights = { 1, 1, 0, 0.5, 0, 0.5, 1, -0.5, 0.5, 0, -1, 0.5 };
    static readonly double[] VerdantWeights = { 1, 1, 0.5, 0.5, 0, 0, 0.5, 0, 0.5, 0.5, -1, 1 };

    const double Lane = 14.0;
    const double AllyBaseHp = 2000.0;
    const double StartCost = 120.0;
    const double SummonCooldown = 2.0;

    // 특성 12개 + 연령 보정으로 유닛 1기 생성
    static void Generate(Random r, bool ember, double meta, out U u, out double cost)
    {
        double[][] traits = ember ? EmberTraits : VerdantTraits;
        double[] weights = ember ? EmberWeights : VerdantWeights;
        double baseHp = ember ? 120 : 187;
        double baseAtk = ember ? 25 : 16;

        int mask = 0;
        for (int i = 0; i < 12; i++) if (r.NextDouble() < 0.5) mask |= (1 << i);

        double hp = baseHp, atk = baseAtk, aspd = 0, range = 3.5, power = 0;
        for (int i = 0; i < 12; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            hp *= (1 + traits[i][0]);
            atk *= (1 + traits[i][1]);
            aspd += traits[i][2];
            range += traits[i][3];
            power += weights[i];
        }

        double age = r.NextDouble();
        if (age < 0.20) hp *= 0.80;
        else if (age < 0.60) { }
        else if (age < 0.90) hp *= 1.10;
        else atk *= 1.15;

        hp *= meta; atk *= meta;
        double itv = 1.0 / (1.0 + aspd);
        if (itv < 0.3) itv = 0.3;
        if (range < 1.5) range = 1.5;

        double unitCost = 30 + 8 * power;
        if (unitCost < 20) unitCost = 20;
        if (unitCost > 150) unitCost = 150;

        u.x = ember ? 1.5 : 2.5;
        u.spd = ember ? 1.5 : 0.0;
        u.ember = ember;
        u.hp = hp; u.atk = atk; u.itv = itv; u.range = range; u.cd = 0;
        cost = unitCost;
    }

    // 단일 스테이지 시뮬레이션
    // K: 유지할 새싹 수, cap: 동시 소환 상한, period: N번째 스폰마다 드론
    // growth: 아군 기대 성장률, h: 적 HP 스케일, spawnc: 적 스폰 가속, basec: 적 본진 HP 스케일
    public static string Run(int stage, int K, int cap, int period, double dmgMul,
                             double h, double spawnc, double basec, double growth, int sims)
    {
        const double dt = 0.1;
        const double atkScale = 0.08;
        int wins = 0;
        double sumAlly = 0, sumTime = 0;
        int totalLeaks = 0;

        double enemyBaseMax = 4000.0 * (1.0 + basec * (stage - 1));
        double enemySpawn = 2.4 - spawnc * (stage - 1);
        if (enemySpawn < 1.0) enemySpawn = 1.0;
        double droneDamage = 60.0 * dmgMul * (1.0 + atkScale * (stage - 1));

        for (int sim = 0; sim < sims; sim++)
        {
            var r = new Random(stage * 99000 + K * 777 + sim * 71 + cap * 13 + period * 31
                               + (int)(h * 1000) + (int)(basec * 100) + (int)(spawnc * 1000) + (int)(growth * 10000));
            double meta = 1.0 + growth * (stage - 1);
            double pop = 0, cost = StartCost, allyBase = AllyBaseHp, enemyBase = enemyBaseMax, t = 0;
            var units = new List<U>();
            var enemies = new List<En>();
            var drones = new List<Dr>();

            U hero;
            hero.x = 1.5; hero.spd = 1.6; hero.ember = true;
            hero.hp = 500 * meta; hero.atk = 50 * meta; hero.itv = 0.9; hero.range = 2.0; hero.cd = 0;
            bool heroAlive = true;

            double summonCd = 0, spawnCd = 0;
            bool won = false;
            const double maxTime = 300;
            int spawnIdx = 0, leaks = 0;

            while (t < maxTime)
            {
                // 적 스폰 (지상 유닛 / 드론 교대)
                spawnCd -= dt;
                if (spawnCd <= 0)
                {
                    spawnIdx++;
                    bool isDrone = (spawnIdx % period == 0);
                    if (isDrone)
                    {
                        var d = new Dr();
                        d.x = 12.5; d.hp = 90.0 * (1 + h * (stage - 1)); d.spd = 2.2;
                        drones.Add(d);
                    }
                    else
                    {
                        var e = new En();
                        e.x = 12.5; e.spd = 1.5;
                        e.hp = 120 * (1 + h * (stage - 1));
                        e.atk = 10 * (1 + atkScale * (stage - 1));
                        e.itv = 1.0; e.range = 1.5; e.cd = 0;
                        enemies.Add(e);
                    }
                    spawnCd = enemySpawn;
                }

                // 인구/코스트
                int verdantCount = 0;
                foreach (var uu in units) if (!uu.ember) verdantCount++;
                pop += (0.3 + 0.1 * verdantCount) * dt;
                if (pop > 100) pop = 100;
                cost += (3.0 + pop / 50.0) * dt;
                summonCd -= dt;

                // 소환
                if (summonCd <= 0 && units.Count < cap)
                {
                    bool wantVerdant = verdantCount < K;
                    U cand;
                    double uc;
                    Generate(r, wantVerdant ? false : true, meta, out cand, out uc);
                    if (cost >= uc)
                    {
                        cost -= uc;
                        units.Add(cand);
                        summonCd = SummonCooldown;
                    }
                }

                // 영웅 (불꽃, 전진)
                if (heroAlive)
                {
                    hero.cd -= dt;
                    int tg = -1;
                    double bd = 1e9;
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        double dx = Math.Abs(enemies[i].x - hero.x);
                        if (dx <= hero.range && dx < bd) { bd = dx; tg = i; }
                    }
                    if (tg >= 0)
                    {
                        if (hero.cd <= 0) { var e = enemies[tg]; e.hp -= hero.atk; enemies[tg] = e; hero.cd = hero.itv; }
                    }
                    else if (hero.x + hero.range >= Lane)
                    {
                        if (hero.cd <= 0) { enemyBase -= hero.atk; hero.cd = hero.itv; }
                    }
                    else
                    {
                        hero.x += hero.spd * dt;
                        if (hero.x > 13.5) hero.x = 13.5;
                    }
                }

                // 아군 유닛
                for (int j = 0; j < units.Count; j++)
                {
                    var u = units[j];
                    u.cd -= dt;

                    if (!u.ember)
                    {
                        // 새싹: 드론 우선 요격, 없으면 사거리 내 지상 적
                        int dtg = -1;
                        double dbd = 1e9;
                        for (int i = 0; i < drones.Count; i++)
                        {
                            double dx = Math.Abs(drones[i].x - u.x);
                            if (dx <= u.range && dx < dbd) { dbd = dx; dtg = i; }
                        }
                        if (dtg >= 0)
                        {
                            if (u.cd <= 0) { var d = drones[dtg]; d.hp -= u.atk; drones[dtg] = d; u.cd = u.itv; }
                        }
                        else
                        {
                            int tg = -1;
                            double bd = 1e9;
                            for (int i = 0; i < enemies.Count; i++)
                            {
                                double dx = Math.Abs(enemies[i].x - u.x);
                                if (dx <= u.range && dx < bd) { bd = dx; tg = i; }
                            }
                            if (tg >= 0 && u.cd <= 0) { var e = enemies[tg]; e.hp -= u.atk; enemies[tg] = e; u.cd = u.itv; }
                        }
                    }
                    else
                    {
                        // 불꽃: 사거리 내 적 → 없으면 전진 → 본진 공격
                        int tg = -1;
                        double bd = 1e9;
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            double dx = Math.Abs(enemies[i].x - u.x);
                            if (dx <= u.range && dx < bd) { bd = dx; tg = i; }
                        }
                        if (tg >= 0)
                        {
                            if (u.cd <= 0) { var e = enemies[tg]; e.hp -= u.atk; enemies[tg] = e; u.cd = u.itv; }
                        }
                        else if (u.x + u.range >= Lane)
                        {
                            if (u.cd <= 0) { enemyBase -= u.atk; u.cd = u.itv; }
                        }
                        else
                        {
                            u.x += u.spd * dt;
                            if (u.x > 13.5) u.x = 13.5;
                        }
                    }
                    units[j] = u;
                }

                for (int i = enemies.Count - 1; i >= 0; i--) if (enemies[i].hp <= 0) enemies.RemoveAt(i);
                for (int i = drones.Count - 1; i >= 0; i--) if (drones[i].hp <= 0) drones.RemoveAt(i);

                // 적 지상 유닛: 아군 유닛 → 없으면 본진
                for (int i = 0; i < enemies.Count; i++)
                {
                    var e = enemies[i];
                    e.cd -= dt;
                    int tg = -1;
                    double bd = 1e9;
                    for (int j = 0; j < units.Count; j++)
                    {
                        double dx = Math.Abs(e.x - units[j].x);
                        if (dx <= e.range && dx < bd) { bd = dx; tg = j; }
                    }
                    if (tg >= 0)
                    {
                        if (e.cd <= 0) { var u = units[tg]; u.hp -= e.atk; units[tg] = u; e.cd = e.itv; }
                    }
                    else if (e.x - e.range <= 0)
                    {
                        if (e.cd <= 0) { allyBase -= e.atk; e.cd = e.itv; }
                    }
                    else
                    {
                        e.x -= e.spd * dt;
                        if (e.x < 0.5) e.x = 0.5;
                    }
                    enemies[i] = e;
                }

                // 드론: 본진 직행 (새싹만 요격 가능)
                for (int i = drones.Count - 1; i >= 0; i--)
                {
                    var d = drones[i];
                    d.x -= d.spd * dt;
                    if (d.x <= 0.8) { allyBase -= droneDamage; leaks++; drones.RemoveAt(i); }
                    else drones[i] = d;
                }

                for (int j = units.Count - 1; j >= 0; j--) if (units[j].hp <= 0) units.RemoveAt(j);
                if (heroAlive && hero.hp <= 0) heroAlive = false;

                if (enemyBase <= 0) { won = true; break; }
                if (allyBase <= 0) break;
                t += dt;
            }

            if (enemyBase <= 0) won = true;
            if (won) wins++;
            sumAlly += Math.Max(0, allyBase);
            sumTime += t;
            totalLeaks += leaks;
        }

        return "  s" + stage + " win " + (100.0 * wins / sims).ToString("0.0") + "%"
             + " | ally " + (sumAlly / sims).ToString("0")
             + " | leak " + (1.0 * totalLeaks / sims).ToString("0.0")
             + " | t " + (sumTime / sims).ToString("0") + "s";
    }

    // 기본 권장 설정 (v2 정본)
    // 성장 7.5%/s, 적 HP +13%/s, ATK +8%/s, 스폰 -0.05s/s, 적 본진 +20%/s
    // 드론 3스폰마다 1기, 동시 유닛 상한 12기, 새싹 3기 유지
    public static string RunRecommended(int stage, int sims)
    {
        return Run(stage, 3, 12, 3, 1.0, 0.13, 0.050, 0.20, 0.075, sims);
    }
}
