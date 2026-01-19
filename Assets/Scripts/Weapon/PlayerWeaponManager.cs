using System.Collections;
using System.Collections.Generic;
// PlayerWeaponManager.cs
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("武器列表")]
    public List<Weapon> weapons = new List<Weapon>();
    private int currentWeaponIndex = -1;
    
    [Header("切换设置")]
    public float switchCooldown = 0.3f;
    private float lastSwitchTime = 0f;
    
    [Header("UI显示")]
    public Text weaponNameText;
    public Image weaponIconImage;
    
    [Header("引用")]
    public Transform firePoint;
    
    // 当前装备的武器
    public Weapon CurrentWeapon { get; private set; }
    
    void Start()
    {
        Debug.Log("=== PlayerWeaponManager Start() ===");
        Debug.Log($"当前时间: {Time.time}");
    
    // 检查武器列表
        Debug.Log($"武器列表长度: {weapons.Count}");
        for (int i = 0; i < weapons.Count; i++)
        {
            Debug.Log($"武器 {i}: {weapons[i]?.name ?? "NULL"}");
        }
    
    // 检查Fire Point
        Debug.Log($"Fire Point: {firePoint?.name ?? "NULL"}");
    
        // 初始化所有武器
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].Initialize(this, firePoint);
            weapons[i].gameObject.SetActive(false);
        }
        
        // 装备第一个武器
        if (weapons.Count > 0)
        {
            EquipWeapon(0);
        }
    }
    
    void Update()
    {
        // 武器切换
        if (Input.GetKeyDown(KeyCode.T) && CanSwitch())
        {
            SwitchWeapon();
        }
        
        // 数字键快速切换
        for (int i = 1; i <= 9 && i <= weapons.Count; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                EquipWeapon(i - 1);
            }
        }
        
        // 鼠标滚轮切换
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int direction = scroll > 0 ? 1 : -1;
            int newIndex = (currentWeaponIndex + direction + weapons.Count) % weapons.Count;
            EquipWeapon(newIndex);
        }
        
        // 攻击输入
        if (Input.GetButtonDown("Fire1") && CurrentWeapon != null)
        {
            CurrentWeapon.Attack();
        }
        
        // 更新当前武器的逻辑
        if (CurrentWeapon != null)
        {
            CurrentWeapon.Update();
        }
    }
    
    void SwitchWeapon()
    {
        int newIndex = (currentWeaponIndex + 1) % weapons.Count;
        EquipWeapon(newIndex);
        lastSwitchTime = Time.time;
    }
    
    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count || index == currentWeaponIndex) 
            return;
        
        // 卸下当前武器
        if (CurrentWeapon != null)
        {
            CurrentWeapon.OnUnequip();
            CurrentWeapon.gameObject.SetActive(false);
        }
        
        // 装备新武器
        currentWeaponIndex = index;
        CurrentWeapon = weapons[index];
        CurrentWeapon.gameObject.SetActive(true);
        CurrentWeapon.OnEquip();
        
        // 更新UI
        UpdateWeaponUI();
    }
    
    bool CanSwitch()
    {
        return Time.time - lastSwitchTime >= switchCooldown;
    }
    
    void UpdateWeaponUI()
    {
        if (weaponNameText != null)
            weaponNameText.text = CurrentWeapon.weaponName;
        
        if (weaponIconImage != null && CurrentWeapon.weaponIcon != null)
            weaponIconImage.sprite = CurrentWeapon.weaponIcon;
    }
    
    // 添加武器到库存
    public void AddWeapon(Weapon newWeapon)
    {
        newWeapon.Initialize(this, firePoint);
        newWeapon.gameObject.SetActive(false);
        weapons.Add(newWeapon);
        
        // 自动装备新武器
        EquipWeapon(weapons.Count - 1);
    }
}
