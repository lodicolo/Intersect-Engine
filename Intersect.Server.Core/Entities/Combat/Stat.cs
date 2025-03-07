using System.Collections.Concurrent;
using Intersect.GameObjects;

namespace Intersect.Server.Entities.Combat;


public partial class Stat
{

    private ConcurrentDictionary<SpellDescriptor, Buff> mBuff = new ConcurrentDictionary<SpellDescriptor, Buff>();

    private Buff[] mCachedBuffs = new Buff[0];

    private bool mChanged;

    private Entity mOwner;

    private Enums.Stat mStatType;

    public override string ToString()
    {
        var cachedBuffs = string.Join(",\n", mCachedBuffs.Select(buff => $"\t\t{buff}"));
        return $"[{typeof(Stat).FullName}{{Owner={(mOwner?.GetType().Name ?? "UNKNOWN")}, StatType={mStatType}, Changed={mChanged}, CachedBuffs=[\n{cachedBuffs}\n\t], Buff.Count={mBuff.Count} }}]";
    }

    public Stat(Enums.Stat statType, Entity owner)
    {
        mOwner = owner;
        mStatType = statType;
    }

    public int BaseStat
    {
        get => mOwner.BaseStats[(int)mStatType];
        set => mOwner.BaseStats[(int)mStatType] = value;
    }

    public int Value()
    {
        // Get our base flat and percentage stats to calculate with.
        var flatStats = BaseStat + mOwner.StatPointAllocations[(int)mStatType];
        var percentageStats = 0;

        // Add item buffs
        if (mOwner is Player player)
        {
            var statBuffs = player.GetItemStatBuffs(mStatType);
            flatStats += statBuffs.Item1;
            percentageStats += statBuffs.Item2;
        }

        //Add spell buffs
        foreach (var buff in mCachedBuffs)
        {
            flatStats += buff.FlatStatcount;
            percentageStats += buff.PercentageStatcount;
        }

        // Calculate our final stat
        var finalStat = (int)Math.Ceiling(flatStats + (flatStats * (percentageStats / 100f)));
        if (finalStat <= 0)
        {
            finalStat = 1; //No 0 or negative stats, will give errors elsewhere in the code (especially divide by 0 errors).
        }

        return finalStat;
    }

    public bool Update(long time)
    {
        var origVal = Value();
        var changed = false;
        foreach (var buff in mBuff)
        {
            if (buff.Value.ExpireTime <= time)
            {
                changed |= mBuff.TryRemove(buff.Key, out Buff result);
            }
        }

        if (changed)
        {
            mCachedBuffs = mBuff.Values.ToArray();
        }

        changed |= Value() != origVal;
        changed |= mChanged;
        mChanged = false;

        return changed;
    }

    public void AddBuff(Buff buff)
    {
        var origVal = Value();
        mBuff.AddOrUpdate(buff.Spell, buff, (key, val) => buff);
        mCachedBuffs = mBuff.Values.ToArray();
        mChanged = Value() != origVal;
    }

    public void Reset()
    {
        mBuff.Clear();
        mCachedBuffs = mBuff.Values.ToArray();
    }

}
