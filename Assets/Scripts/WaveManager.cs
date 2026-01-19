using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [Header("敌人设置")]
        [Tooltip("要生成的敌人预制体")]
        public GameObject enemyPrefab;
        
        [Tooltip("该波次此类敌人的总数")]
        [Min(1)]
        public int enemyCount = 10;
        
        [Tooltip("生成间隔（秒）")]
        [Min(0.1f)]
        public float spawnInterval = 2f;
        
        [Header("高级设置")]
        [Tooltip("是否为特殊波次（会覆盖阶段间隔设置）")]
        public bool isSpecialWave = false;
        
        [Tooltip("特殊波次延迟（仅在 isSpecialWave 为 true 时有效）")]
        [Min(0f)]
        public float specialWaveDelay = 0f;
    }

    [System.Serializable]
    public class PhaseSetting
    {
        [Header("阶段设置")]
        [Tooltip("选择游戏阶段")]
        public GameTimeManager.GamePhase phase = GameTimeManager.GamePhase.Day;
        
        [Tooltip("此阶段是否启用敌人生成")]
        public bool isActive = true;
        
        [Space(10)]
        [Header("波次设置")]
        [Tooltip("此阶段的所有波次配置")]
        public Wave[] wavesForThisPhase = new Wave[1];
        
        [Tooltip("波次间的间隔时间（秒）")]
        [Min(0f)]
        public float timeBetweenWaves = 10f;
        
        [Space(10)]
        [Header("阶段属性")]
        [Tooltip("此阶段的总时长（秒），0表示无限")]
        [Min(0f)]
        public float phaseDuration = 300f;
        
        [Tooltip("是否循环此阶段的波次")]
        public bool loopWaves = false;
        
        [Tooltip("循环时的整体间隔（秒）")]
        [Min(0f)]
        public float loopInterval = 30f;
    }
    
    [Header("=== 阶段配置 ===")]
    [Tooltip("为白天、夜晚、血月分别配置波次设置")]
    [SerializeField] private PhaseSetting[] phaseSettings = new PhaseSetting[3];
    
    [Header("=== 调试信息 ===")]
    [Tooltip("当前激活的阶段")]
    [SerializeField] private PhaseSetting currentPhaseSetting;
    
    [Tooltip("当前波次索引")]
    [SerializeField] private int currentWaveIndex = 0;
    
    [Tooltip("是否正在生成")]
    [SerializeField] private bool isSpawning = false;
    
    [Tooltip("阶段开始时间")]
    [SerializeField] private float phaseStartTime = 0f;
    
    [Header("=== 生成器引用 ===")]
    [Tooltip("敌人生成器实例，如果为空则自动查找")]
    [SerializeField] private OffScreenEnemySpawner enemySpawner;
    
    // 快速访问不同阶段的设置
    public PhaseSetting morningSetting { get; private set; }
    public PhaseSetting nightSetting { get; private set; }
    public PhaseSetting bloodMoonSetting { get; private set; }
    
    private Dictionary<GameTimeManager.GamePhase, PhaseSetting> phaseSettingsDict = 
        new Dictionary<GameTimeManager.GamePhase, PhaseSetting>();

    void Awake()
    {
        // 初始化字典以便快速查找
        InitializePhaseSettings();
        
        // 自动获取生成器引用
        if (enemySpawner == null)
        {
            enemySpawner = OffScreenEnemySpawner.Instance;
        }
    }

    void OnEnable()
    {
        GameTimeManager.OnPhaseChanged += OnGamePhaseChanged;
        GameTimeManager.OnBloodMoonStart += OnBloodMoonStart;
    }

    void OnDisable()
    {
        GameTimeManager.OnPhaseChanged -= OnGamePhaseChanged;
        GameTimeManager.OnBloodMoonStart -= OnBloodMoonStart;
    }
    
    private void InitializePhaseSettings()
    {
        phaseSettingsDict.Clear();
        
        foreach (var setting in phaseSettings)
        {
            if (!phaseSettingsDict.ContainsKey(setting.phase))
            {
                phaseSettingsDict[setting.phase] = setting;
                
                // 设置快速访问属性
                switch (setting.phase)
                {
                    case GameTimeManager.GamePhase.Day:
                        morningSetting = setting;
                        break;
                    case GameTimeManager.GamePhase.Night:
                        nightSetting = setting;
                        break;
                    case GameTimeManager.GamePhase.BloodMoon:
                        bloodMoonSetting = setting;
                        break;
                }
            }
            else
            {
                Debug.LogWarning($"重复的阶段配置: {setting.phase}");
            }
        }
    }

    private void OnGamePhaseChanged(GameTimeManager.GamePhase newPhase)
    {
        if (newPhase == GameTimeManager.GamePhase.BloodMoon)
        {
            // 血月有单独的事件处理
            return;
        }
        
        StartPhase(newPhase);
    }

    private void OnBloodMoonStart()
    {
        StartPhase(GameTimeManager.GamePhase.BloodMoon);
    }
    
    private void StartPhase(GameTimeManager.GamePhase phase)
    {
        StopAllCoroutines();
        isSpawning = false;
        
        if (phaseSettingsDict.TryGetValue(phase, out PhaseSetting setting))
        {
            if (!setting.isActive)
            {
                Debug.Log($"阶段 {phase} 被禁用，跳过敌人生成");
                return;
            }
            
            currentPhaseSetting = setting;
            currentWaveIndex = 0;
            phaseStartTime = Time.time;
            
            Debug.Log($"开始阶段: {phase}, 持续时间: {setting.phaseDuration}秒, 波次数: {setting.wavesForThisPhase.Length}");
            StartCoroutine(PhaseWaveRoutine());
        }
        else
        {
            Debug.LogWarning($"未找到阶段 {phase} 的配置");
        }
    }

    IEnumerator PhaseWaveRoutine()
    {
        isSpawning = true;
        float phaseEndTime = Time.time + currentPhaseSetting.phaseDuration;
        
        do
        {
            // 遍历此阶段的所有波次
            while (currentWaveIndex < currentPhaseSetting.wavesForThisPhase.Length)
            {
                // 检查阶段是否已结束（如果设置了持续时间）
                if (currentPhaseSetting.phaseDuration > 0 && Time.time >= phaseEndTime)
                {
                    Debug.Log($"阶段 {currentPhaseSetting.phase} 时间结束，停止波次生成");
                    isSpawning = false;
                    yield break;
                }
                
                Wave currentWave = currentPhaseSetting.wavesForThisPhase[currentWaveIndex];
                
                Debug.Log($"开始生成 {currentPhaseSetting.phase} 阶段的第 {currentWaveIndex + 1} 波敌人，数量: {currentWave.enemyCount}");
                
                // 处理特殊波次延迟
                if (currentWave.isSpecialWave && currentWave.specialWaveDelay > 0)
                {
                    Debug.Log($"特殊波次延迟: {currentWave.specialWaveDelay}秒");
                    yield return new WaitForSeconds(currentWave.specialWaveDelay);
                }
                
                // 生成一波敌人
                yield return StartCoroutine(SpawnWave(currentWave));

                currentWaveIndex++;
                
                // 如果不是最后一波，等待一段时间再开始下一波
                if (currentWaveIndex < currentPhaseSetting.wavesForThisPhase.Length)
                {
                    float waitTime = currentWave.isSpecialWave && currentWave.specialWaveDelay > 0 
                        ? currentWave.specialWaveDelay 
                        : currentPhaseSetting.timeBetweenWaves;
                    
                    yield return new WaitForSeconds(waitTime);
                }
            }
            
            // 如果开启循环，重置波次索引
            if (currentPhaseSetting.loopWaves)
            {
                currentWaveIndex = 0;
                yield return new WaitForSeconds(currentPhaseSetting.loopInterval);
            }
            
        } while (currentPhaseSetting.loopWaves);
        
        Debug.Log($"阶段 {currentPhaseSetting.phase} 所有波次生成完毕。");
        isSpawning = false;
    }

    IEnumerator SpawnWave(Wave wave)
    {
        if (wave.enemyPrefab == null)
        {
            Debug.LogError("敌人生成失败：未指定敌人预制体");
            yield break;
        }
        
        if (enemySpawner == null)
        {
            Debug.LogError("敌人生成失败：未找到 EnemySpawner 实例");
            yield break;
        }
        
        for (int i = 0; i < wave.enemyCount; i++)
        {
            enemySpawner.SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }
    
    // 编辑器工具方法
    #if UNITY_EDITOR
    [ContextMenu("添加所有阶段")]
    private void AddAllPhasesInEditor()
    {
        var phases = System.Enum.GetValues(typeof(GameTimeManager.GamePhase));
        phaseSettings = new PhaseSetting[phases.Length];
        
        for (int i = 0; i < phases.Length; i++)
        {
            phaseSettings[i] = new PhaseSetting
            {
                phase = (GameTimeManager.GamePhase)phases.GetValue(i),
                wavesForThisPhase = new Wave[1] { new Wave() }
            };
        }
    }
    #endif
    
    // 公共方法
    public void SetPhaseActive(GameTimeManager.GamePhase phase, bool isActive)
    {
        if (phaseSettingsDict.TryGetValue(phase, out PhaseSetting setting))
        {
            setting.isActive = isActive;
            
            if (!isActive && currentPhaseSetting != null && currentPhaseSetting.phase == phase)
            {
                StopAllCoroutines();
                isSpawning = false;
            }
        }
    }
    
    public PhaseSetting GetPhaseSetting(GameTimeManager.GamePhase phase)
    {
        phaseSettingsDict.TryGetValue(phase, out PhaseSetting setting);
        return setting;
    }
    
    public int GetCurrentWaveNumber()
    {
        return currentWaveIndex + 1;
    }
    
    public int GetTotalWavesInCurrentPhase()
    {
        return currentPhaseSetting?.wavesForThisPhase?.Length ?? 0;
    }
}
