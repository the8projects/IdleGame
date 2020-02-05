using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class IdleGameScript : MonoBehaviour
{
    double coins = 0.00;
    double growthPerTap = 1.00;
    double growthPerSec = 0.00;
    double growthProgress = 0.00;
    double currentGrowingPercent = 0.00;
    bool isTapCooldown = false;
    double tapCooldown = 0.2;
    double tapCooldownTime = 0;
    double autoSaveFixedTime = 5;
    double autoSaveTime = 5;

    public Text txtCoins;
    public Text txtBtnTap;
    public Text txtGrowthPerTap;
    public Text txtGrowthPerSec;
    public Text txtGrowingProgress;
    public Text txtCoinsPerCrop;
    
    public Text[] txtLevel;
    public Text[] txtCurrentResult;
    public Text[] txtUpgradeCost;
    public Text[] txtUpgradeMultiCost;

    public CanvasGroup mainMenu;
    public CanvasGroup materialMenu;
    public CanvasGroup arenaMenu;
    public CanvasGroup masteryMenu;
    public CanvasGroup optionsMenu;

    bool isArenaStart = false;
    public Image imgCurrentHp;
    public Text txtPestHp;
    public Text txtDamage;
    float damageAppearTime = 1;
    float damageAppearTimeFix = 1;
    float damage;
    float pestHp;
    System.Random rand = new System.Random();

    Plants plant = new Plants(0);
    Weapons weapon = new Weapons(0);

    Pest pest = new Pest(0);
    
    Upgrades[] upgrades = new Upgrades[5]
    {
        new Upgrades(0),
        new Upgrades(1),
        new Upgrades(2),
        new Upgrades(3),
        new Upgrades(4)
    };

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        FloatingTextController.Initialize();
        Load();
    }

    // Update is called once per frame
    void Update()
    {
        if (growthProgress >= plant.GrowthNeeded)
        {
            coins += plant.Price;
            txtCoins.text = "Coins: " + coins.ToString("F2");
            growthProgress -= plant.GrowthNeeded;
        }

        if (isTapCooldown)
        {
            tapCooldownTime -= (1/tapCooldown) * Time.deltaTime;
            if (tapCooldownTime <= 0.00)
            {
                isTapCooldown = false;
                tapCooldownTime = 0.00;
            }
        }

        growthProgress += growthPerSec * Time.deltaTime;
        currentGrowingPercent = (growthProgress / plant.GrowthNeeded) * 100;
        txtGrowingProgress.text = "Growing Progress: " + growthProgress.ToString("F2") + " / " + plant.GrowthNeeded.ToString("F2") + " (" + currentGrowingPercent.ToString("F2") + "%)";
        
        autoSaveTime -= Time.deltaTime;
        if (autoSaveTime <= 0.00)
        {
            Save();
            autoSaveTime = autoSaveFixedTime;
        }

    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.OpenOrCreate);

        SaveData data = new SaveData();
        data.coins = coins;
        data.growthPerTap = growthPerTap;
        data.growthPerSec = growthPerSec;
        data.plant = this.plant;

        for (int i = 0; i < upgrades.Length; i++)
        {
            data.upgrades[i] = upgrades[i];
        }

        bf.Serialize(file, data);
        file.Close();
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);

            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();

            coins = data.coins;
            growthPerTap = data.growthPerTap;
            growthPerSec = data.growthPerSec;
            this.plant = data.plant;
            for (int i = 0; i < upgrades.Length; i++)
            {
                upgrades[i] = data.upgrades[i];
            }
        }
        else
        {
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.OpenOrCreate);
            file.Close();
        }

        changeTab(3);
        growthPerSec += (growthPerSec * weapon.Option);

        txtCoins.text = "Coins: " + coins.ToString("F2");
        txtGrowthPerTap.text = growthPerTap.ToString("F2") + " Growth/tap";
        txtGrowthPerSec.text = growthPerSec.ToString("F2") + " Growth/sec";
        txtCoinsPerCrop.text = this.plant.Price.ToString("F2") + " Coins/crop";

        for (int i = 0; i < upgrades.Length; i++)
        {
            txtLevel[i].text = "LV\n" + upgrades[i].Level;
            txtUpgradeCost[i].text = "+1 Level\nCost: " + upgrades[i].Cost.ToString("F2") + " Coins";
            txtUpgradeMultiCost[i].text = "+10 Level\nCost: " + upgrades[i].MultiCost.ToString("F2") + " Coins";
        }
    }

    public void CanvasGroupChanger(bool x, CanvasGroup y)
    {
        if (x)
        {
            y.alpha = 1;
            y.interactable = true;
            y.blocksRaycasts = true;
            return;
        }
        y.alpha = 0;
        y.interactable = false;
        y.blocksRaycasts = false;
    }

    public void changeTab(int tabFlags)
    {
        switch (tabFlags)
        {
            case 0:
                CanvasGroupChanger(true, mainMenu);
                CanvasGroupChanger(false, materialMenu);
                CanvasGroupChanger(false, arenaMenu);
                CanvasGroupChanger(false, masteryMenu);
                break;
            case 1:
                CanvasGroupChanger(false, mainMenu);
                CanvasGroupChanger(true, materialMenu);
                CanvasGroupChanger(false, arenaMenu);
                CanvasGroupChanger(false, masteryMenu);
                break;
            case 2:
                CanvasGroupChanger(false, mainMenu);
                CanvasGroupChanger(false, materialMenu);
                CanvasGroupChanger(true, arenaMenu);
                CanvasGroupChanger(false, masteryMenu);
                break;
            case 3:
                CanvasGroupChanger(false, mainMenu);
                CanvasGroupChanger(false, materialMenu);
                CanvasGroupChanger(false, arenaMenu);
                CanvasGroupChanger(true, masteryMenu);
                break;
            default:
                break;
        }
    }

    public void Tap()
    {
        if (!isTapCooldown)
        {
            growthProgress += growthPerTap;
            isTapCooldown = true;
        }
    }

    public void Attack()
    {
        //new enemy start timing / set hp value
        if (!isArenaStart)
        {
            pestHp = pest.HeathPoint;
            imgCurrentHp.fillAmount = 1;
            isArenaStart = true;
        }

        if (pestHp > 0)
        {
            damage = rand.Next(weapon.MinDamage,weapon.MaxDamage);
            pestHp -= damage;
            imgCurrentHp.fillAmount -= (rand.Next(weapon.MinDamage,weapon.MaxDamage) / pest.HeathPoint);
            FloatingTextController.CreateFloatingText(damage.ToString(), transform);
            //txtDamage.text = damage.ToString();
            //txtDamage.CrossFadeAlpha(1.0f, 0.001f, false);
            //txtDamage.CrossFadeAlpha(0.0f, 1.0f, false);
        }else{
            isArenaStart = false;
        }
        txtPestHp.text = "HP: " + pestHp.ToString() + "/" + pest.HeathPoint.ToString();
    }

    public void BtnUpgrade(string str)
    {
        int upgradeID = int.Parse(str.Substring(0,1));
        string multi = str.Substring(1,1);
        if (multi == "1")
        {
            if (coins >= upgrades[upgradeID].MultiCost)
            {
                growthPerSec += (upgrades[upgradeID].Growth*10);
                coins -= upgrades[upgradeID].MultiCost;
                upgrades[upgradeID].Upgrade(true);
            }
        }
        else
        {
            if (coins >= upgrades[upgradeID].Cost)
            {
                growthPerSec += upgrades[upgradeID].Growth;
                coins -= upgrades[upgradeID].Cost;
                upgrades[upgradeID].Upgrade(false);
            }
        }
        txtCoins.text = "Coins: " + coins.ToString("F2");
        txtGrowthPerSec.text = growthPerSec.ToString("F2") + " Growth/sec";
        txtLevel[upgradeID].text = "LV\n" + upgrades[upgradeID].Level;
        txtUpgradeCost[upgradeID].text = "+1 Level\nCost: " + upgrades[upgradeID].Cost.ToString("F2") + " Coins";
        txtUpgradeMultiCost[upgradeID].text = "+10 Level\nCost: " + upgrades[upgradeID].MultiCost.ToString("F2") + " Coins";
        Save();
    }
        
    public void BuyPlant(int plant)
    {
        Plants tempPlant = new Plants(plant);
        if (tempPlant.Bought)
        {
            this.plant = tempPlant;
            growthProgress = 0.00;
            txtCoinsPerCrop.text = this.plant.Price.ToString("F2") + " Coins/crop";
        }
        else
        {
            if (coins >= tempPlant.Cost)
            {
                coins -= tempPlant.Cost;
                this.plant = tempPlant;
                this.plant.Bought = true;
                growthProgress = 0.00;
                txtCoinsPerCrop.text = this.plant.Price.ToString("F2") + " Coins/crop";
            }
        }
        Save();
    }
}

[Serializable]
class SaveData
{
    public double coins;
    public double growthPerTap;
    public double growthPerSec;
    public Upgrades[] upgrades = new Upgrades[5];
    public Plants plant;
}   

[Serializable]
public class Plants
{
    private string _name;
    private int _price;
    private double _growthNeeded;
    private double _cost;
    private bool _bought;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    public int Price
    {
        get { return _price; }
        set { _price = value; }
    }

    public double GrowthNeeded
    {
        get { return _growthNeeded; }
        set { _growthNeeded = value; }
    }
    
    public double Cost
    {
        get { return _cost; }
        set { _cost = value; }
    }

    public bool Bought
    {
        get { return _bought; }
        set { _bought = value; }
    }
       
    public Plants(int plant)
    {
        switch (plant)
        {
            case 0:
                Name = "Kale";
                Price = 5;
                GrowthNeeded = 20.00;
                Cost = 0;
                Bought = true;
                break;
            case 1:
                Name = "Eggplant";
                Price = 12;
                GrowthNeeded = 150.00;
                Cost = 500;
                Bought = false;
                break;
            case 2:
                Name = "Cauliflower";
                Price = 30;
                GrowthNeeded = 350.00;
                Cost = 1500;
                Bought = false;
                break;
            case 3:
                Name = "Corn";
                Price = 50;
                GrowthNeeded = 500.00;
                Cost = 4000;
                Bought = false;
                break;
            case 4:
                Name = "Broccoli";
                Price = 70;
                GrowthNeeded = 600.00;
                Cost = 7500;
                Bought = false;
                break;
            case 5:
                Name = "BellPepper";
                Price = 200;
                GrowthNeeded = 1500.00;
                Cost = 10000;
                Bought = false;
                break;
            case 6:
                Name = "ShiitakeMushroom";
                Price = 300;
                GrowthNeeded = 2000.00;
                Cost = 25000;
                Bought = false;
                break;
            default:
                break;
        }
    }
}

[Serializable]
public class Upgrades
{
    private string _name;
    private float _baseCost;
    private double _cost;
    private double _multiCost;
    private int _lv = 0;
    private double _growth;
    private const double costIncrease = 1.07;
    private const double multipleCost = 10;

    public int Level
    {
        get { return _lv; }
        set { _lv = value; }
    }
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    public float BaseCost
    {
        get { return _baseCost; }
        set { _baseCost = value; }
    }

    public double Cost
    {
        get { return _cost; }
        set { _cost = value; }
    }

    public double MultiCost
    {
        get { return _multiCost; }
        set { _multiCost = value; }
    }

    public double Growth
    {
        get { return _growth; }
        set { _growth = value; }
    }
               

    public Upgrades(int upgrade)
    {
        switch (upgrade)
        {
            case 0:
                Name = "Watering";
                BaseCost = 10;
                Growth = 1;
                break;
            case 1:
                Name = "Employing";
                BaseCost = 100;
                Growth = 1.5;
                break;
            case 2:
                Name = "Shoveling";
                BaseCost = 500;
                Growth = 2.5;
                break;
            case 3:
                Name = "Automatic Watering Systems";
                BaseCost = 3000;
                Growth = 10;
                break;
            case 4:
                Name = "Greenhouse";
                BaseCost = 20000;
                Growth = 50;
                break;
            default:
                break;
        }
        Cost = BaseCost * (Math.Pow(costIncrease, Level));;
        MultiCost = CalculateUpgradeCost(Cost);
    }

    public void Upgrade(bool multi)
    {
        if (multi)
        {
            Level += 10;
        }else
        {
            ++Level;
        }
        Cost = BaseCost * (Math.Pow(costIncrease, Level));;
        MultiCost = CalculateUpgradeCost(Cost);
    }



    private double CalculateUpgradeCost(double cost)
    {
        double upgradeCost = cost * costIncrease;
        double sumCost = cost;

        for (int i = 0; i < multipleCost - 1; i++)
        {
            sumCost += upgradeCost;
            upgradeCost *= costIncrease;
        }
        return sumCost;
    }
}

public class Weapons
{   
    private string _name;
    private string _weaponType;
    private double _option;
    private int _minDamage;
    private int _maxDamage;
    private double _aspd;
    private int _critRate;
    private int _critDmg;
    
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string WeaponType
    {
        get { return _weaponType; }
        set { _weaponType = value; }
    }

    
    public double Option
    {
        get { return _option; }
        set { _option = value; }
    }
    
    
    public int MinDamage
    {
        get { return _minDamage; }
        set { _minDamage = value; }
    }
       
    public int MaxDamage
    {
        get { return _maxDamage; }
        set { _maxDamage = value; }
    }

    public double Aspd
    {
        get { return _aspd; }
        set { _aspd = value; }
    }

    public int CritRate
    {
        get { return _critRate; }
        set { _critRate = value; }
    }

    public int CritDmg
    {
        get { return _critDmg; }
        set { _critDmg = value; }
    }


    public Weapons(int weapon)
    {
        switch (weapon)
        {
            case 0:
                Name = "Hand";
                WeaponType = "light";
                Option = 0.00;
                MinDamage = 1;
                MaxDamage = 1;
                Aspd = 1;
                CritRate = 1;
                break;
            case 1:
                Name = "Knife";
                WeaponType = "light";
                Option = 2.00;
                MinDamage = 1;
                MaxDamage = 3;
                Aspd = 0.75;
                CritRate = 10;
                break;
            case 2:
                Name = "Hand Fork";
                WeaponType = "light";
                Option = 3.00;
                MinDamage = 8;
                MaxDamage = 12;
                Aspd = 1;
                CritRate = 5;
                break;
            case 3:
                Name = "Lawn Rake";
                WeaponType = "heavy";
                Option = 4.00;
                MinDamage = 22;
                MaxDamage = 28;
                Aspd = 1.25;
                CritRate = 5;
                break;
            case 4:
                Name = "Hoe";
                WeaponType = "heavy";
                Option = 5.00;
                MinDamage = 45;
                MaxDamage = 55;
                Aspd = 1.25;
                CritRate = 5;
                break;
            case 5:
                Name = "Hedge Shears";
                WeaponType = "light";
                Option = 6.00;
                MinDamage = 75;
                MaxDamage = 85;
                Aspd = 1;
                CritRate = 5;
                break;
            case 6:
                Name = "Sickle";
                WeaponType = "light";
                Option = 7.00;
                MinDamage = 140;
                MaxDamage = 160;
                Aspd = 0.75;
                CritRate = 8;
                break;
            case 7:
                Name = "Big Knife";
                WeaponType = "light";
                Option = 8.00;
                MinDamage = 175;
                MaxDamage = 225;
                Aspd = 0.5;
                CritRate = 10;
                break;
            case 8:
                Name = "Hammer";
                WeaponType = "heavy";
                Option = 8.50;
                MinDamage = 790;
                MaxDamage = 810;
                Aspd = 1.2;
                CritRate = 5;
                break;
            case 9:
                Name = "Scythe";
                WeaponType = "light";
                Option = 9.00;
                MinDamage = 700;
                MaxDamage = 900;
                Aspd = 0.9;
                CritRate = 8;
                break;
            default:
                break;
        }
        CritDmg = 150;
    }
}

public class Pest
{
    private string _name;
    private float _heathpoint;
    private double _time;
    private int _blockChance;
    private int _blockCD;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
        
    public float HeathPoint
    {
        get { return _heathpoint; }
        set { _heathpoint = value; }
    }

    public double Time
    {
        get { return _time; }
        set { _time = value; }
    }
       
    public int BlockChance
    {
        get { return _blockChance; }
        set { _blockChance = value; }
    }
       
    public int BlockCD
    {
        get { return _blockCD; }
        set { _blockCD = value; }
    }

    public Pest(int pest)
    {
        switch (pest)
        {
            case 0:
                Name = "0";
                HeathPoint = 8.0f;
                Time = 10;
                BlockChance = 0;
                BlockCD = 0;
                break;
            case 1:
                Name = "0";
                HeathPoint = 50.0f;
                Time = 15;
                BlockChance = 0;
                BlockCD = 0;
                break;
            case 2:
                Name = "0";
                HeathPoint = 250.0f;
                Time = 20;
                BlockChance = 0;
                BlockCD = 0;
                break;
            case 3:
                Name = "0";
                HeathPoint = 600.0f;
                Time = 25;
                BlockChance = 0;
                BlockCD = 0;
                break;
            case 4:
                Name = "0";
                HeathPoint = 1500.0f;
                Time = 30;
                BlockChance = 5;
                BlockCD = 10;
                break;
            case 5:
                Name = "0";
                HeathPoint = 3000.0f;
                Time = 30;
                BlockChance = 5;
                BlockCD = 10;
                break;
            case 6:
                Name = "0";
                HeathPoint = 7500.0f;
                Time = 30;
                BlockChance = 5;
                BlockCD = 8;
                break;
            case 7:
                Name = "0";
                HeathPoint = 20000.0f;
                Time = 30;
                BlockChance = 5;
                BlockCD = 6;
                break;
            case 8:
                Name = "0";
                HeathPoint = 30000.0f;
                Time = 30;
                BlockChance = 5;
                BlockCD = 5;
                break;
            case 9:
                Name = "0";
                HeathPoint = 50000.0f;
                Time = 30;
                BlockChance = 8;
                BlockCD = 4;
                break;
            default:
                break;
        }
    }
}