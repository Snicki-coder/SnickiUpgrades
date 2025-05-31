using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using REPOLib.Modules;
using System.IO;
using UnityEngine;

namespace SnickiUpgrades;

[BepInPlugin("Snicki.SnickiUpgrades", "SnickiUpgrades", "1.0")]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class SnickiUpgrades : BaseUnityPlugin
{
    public static GameObject teleporter;
    internal static SnickiUpgrades Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    public static PlayerUpgrade LastChanceUpgradeRegister;

    private static float initialGravity;
    private static bool initialGravitySet = false;

    private void Awake()
    {
        Instance = this;
        
        // Prevent the plugin from being deleted
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        Patch();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");

        string pluginFolderPath = Path.GetDirectoryName(Info.Location);
        string assetBundleFilePath = Path.Combine(pluginFolderPath, "snickiupgrades");
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);
        InitUpgrades(assetBundle);
    }

    private static void InitUpgrades(AssetBundle assetBundle)
    {
        SnickiUpgrades.Logger.LogDebug("Init Upgrades");
        Item gravityItem = assetBundle.LoadAsset<Item>("Item Upgrade Player Last Chance");
        gravityItem.prefab = assetBundle.LoadAsset<GameObject>("Item Upgrade Player Last Chance");
        Items.RegisterItem(gravityItem);
        LastChanceUpgradeRegister = Upgrades.RegisterUpgrade("Last Chance", gravityItem, InitLastChanceUpgrade, UseLastChanceUpgrade);
    }

    private static void InitLastChanceUpgrade(PlayerAvatar player, int level)
    {
        Logger.LogDebug("Upgrade got Init");
        if (!player.isLocal)
        {
            return;
        }
    }

    private static void UseLastChanceUpgrade(PlayerAvatar player, int level)
    {
        Logger.LogDebug("Upgrade got Used");
        if (!player.isLocal)
        {
            return;
        }
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        this.Harmony.PatchAll(typeof(PlayerManagerScript));
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }

    private void Update()
    {
        // Code that runs every frame goes here
    }
}