using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Wawalang;

[BepInPlugin("alduris.wawalang", "WAWAlang", "1.0.0")]
sealed class Plugin : BaseUnityPlugin
{
    bool init;
    public static new ManualLogSource Logger;

    public void OnEnable()
    {
        // Add hooks here
        On.RainWorld.OnModsInit += OnModsInit;
        Logger = base.Logger;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (init) return;

        init = true;

        // Initialize assets, your mod config, and anything that uses RainWorld here
        MachineConnector.SetRegisteredOI("WAWAlang", new Remix());

        if (ModManager.ActiveMods.Exists(x => x.id == "slime-cubed.devconsole"))
        {
            ConsoleInterface.Register();
        }
    }
}
