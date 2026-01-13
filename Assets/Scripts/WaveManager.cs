using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab; // 敌人预制体
        public int enemyCount; // 该波次此类敌人的总数
        public float spawnInterval; // 生成间隔
    }

    [System.Serializable]
    public class PhaseSetting
    {
        public GameTimeManager.GamePhase phase;
        public Wave[] wavesForThisPhase; // 此阶段的所有波次设置
        public float timeBetweenWaves = 10f; // 波次间的间隔
    }

    public PhaseSetting[] phaseSettings; // 为白天、夜晚、血月分别配置

    private PhaseSetting currentPhaseSetting;
    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    // 声明一个变量来引用 EnemySpawner 实例

    void OnEnable()
    {
        // 监听时间管理器的阶段变化事件
        GameTimeManager.OnPhaseChanged += OnGamePhaseChanged;
        GameTimeManager.OnBloodMoonStart += OnBloodMoonStart;
    }

    void OnDisable()
    {
        GameTimeManager.OnPhaseChanged -= OnGamePhaseChanged;
        GameTimeManager.OnBloodMoonStart -= OnBloodMoonStart;
    }

    private void OnGamePhaseChanged(GameTimeManager.GamePhase newPhase)
    {
        // 当阶段变化时，停止当前所有生成，并开始新阶段的波次
        StopAllCoroutines();
        isSpawning = false;

        // 查找新阶段对应的设置
        foreach (var setting in phaseSettings)
        {
            if (setting.phase == newPhase)
            {
                currentPhaseSetting = setting;
                currentWaveIndex = 0;
                StartCoroutine(PhaseWaveRoutine());
                break;
            }
        }
    }

    private void OnBloodMoonStart()
    {
        // 血月有独立的设置，这里触发血月波次
        StopAllCoroutines();
        isSpawning = false;

        foreach (var setting in phaseSettings)
        {
            if (setting.phase == GameTimeManager.GamePhase.BloodMoon)
            {
                currentPhaseSetting = setting;
                currentWaveIndex = 0;
                StartCoroutine(PhaseWaveRoutine());
                break;
            }
        }
    }

    IEnumerator PhaseWaveRoutine()
    {
        isSpawning = true;
        // 遍历此阶段的所有波次
        while (currentWaveIndex < currentPhaseSetting.wavesForThisPhase.Length)
        {
            Wave currentWave = currentPhaseSetting.wavesForThisPhase[currentWaveIndex];
            Debug.Log($"开始生成 {currentWaveIndex + 1} 波敌人，数量: {currentWave.enemyCount}");

            // 生成一波敌人
            yield return StartCoroutine(SpawnWave(currentWave));

            currentWaveIndex++;
            // 如果不是最后一波，等待一段时间再开始下一波
            if (currentWaveIndex < currentPhaseSetting.wavesForThisPhase.Length)
            {
                yield return new WaitForSeconds(currentPhaseSetting.timeBetweenWaves);
            }
        }
        Debug.Log("当前阶段所有波次生成完毕。");
        isSpawning = false;
    }

    IEnumerator SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            // 调用你现有的敌人生成器来生成一个敌人
            OffScreenEnemySpawner.Instance.SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }
}
