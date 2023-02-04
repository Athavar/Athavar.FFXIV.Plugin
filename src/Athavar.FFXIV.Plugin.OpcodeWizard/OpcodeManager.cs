namespace Athavar.FFXIV.Plugin.OpcodeWizard;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Machina.FFXIV;

internal class OpcodeManager : IOpcodeManager
{
    private readonly OpcodeWizardConfiguration configuration;
    private readonly IDefinitionManager definitionManager;

    private readonly Dictionary<ushort, Opcode> opcodes = new();

    public OpcodeManager(Configuration configuration, IDefinitionManager definitionManager)
    {
        this.configuration = configuration.OpcodeWizard!;
        this.definitionManager = definitionManager;
        this.Populate();
    }

    public IReadOnlyDictionary<ushort, Opcode> Opcodes => this.opcodes;

    public ushort GetOpcode(Opcode opcode) => this.opcodes.Where(op => op.Value == opcode).Select(op => op.Key).FirstOrDefault();

    public void AddOrUpdate(Opcode opcode, ushort value)
    {
        if (this.configuration.Opcodes.TryGetValue(opcode, out var current))
        {
            this.opcodes.Remove(current);
            this.configuration.Opcodes[opcode] = value;
        }
        else
        {
            this.configuration.Opcodes.TryAdd(opcode, value);
        }

        this.opcodes.TryAdd(value, opcode);
        this.configuration.Save();
    }

    public void Remove(Opcode opcode)
    {
        if (this.configuration.Opcodes.TryGetValue(opcode, out var current))
        {
            this.configuration.Opcodes.Remove(opcode);
            this.opcodes.Remove(current);
            this.configuration.Save();
        }
    }

    private void Populate()
    {
        void Add(Opcode opcode, ushort code) => this.opcodes.TryAdd(code, opcode);

        if (this.configuration.GameVersion != this.definitionManager.StartInfo.GameVersion?.ToString())
        {
            // reset
            this.configuration.Opcodes.Clear();
            this.configuration.GameVersion = this.definitionManager.StartInfo.GameVersion?.ToString() ?? string.Empty;

            // TODO: download from repo.
            this.configuration.Save();
        }

        foreach (var (opcode, value) in this.configuration.Opcodes)
        {
            Add(opcode, value);
        }

        Machina.FFXIV.Headers.Opcodes.OpcodeManager machinaManager = new();
        machinaManager.SetRegion(GameRegion.Global);
        var codes = machinaManager.CurrentOpcodes;
        // Add(Opcode.StatusEffectListBozja, codes["StatusEffectList2"]);
        // Add(Opcode.StatusEffectListPlayer, codes["StatusEffectList3"]);
        // Add(Opcode.StatusEffectListDouble, codes["BossStatusEffectList"]);
        // Add(Opcode.SpawnBoss, codes["NpcSpawn2"]);
    }
}