// <copyright file="AutoSpear.Data.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.AutoSpear;

using Athavar.FFXIV.Plugin.AutoSpear.Enum;

internal sealed partial class AutoSpear
{
    private void ApplyData(IDictionary<uint, SpearFish> fishes)
    {
        this.Data4_0(fishes);
        this.Data4_1(fishes);
        this.Data5_0(fishes);
        this.Data6_0(fishes);
        this.Data6_1(fishes);
        this.Data6_3(fishes);
        this.Data6_5(fishes);
    }

    private void Data4_0(IDictionary<uint, SpearFish> data)
    {
        data.Apply(20144, Patch.Stormblood) // Wentletrap
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20145, Patch.Stormblood) // Black Boxfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20146, Patch.Stormblood) // Glass Manta
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20147, Patch.Stormblood) // Regal Silverside
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(20148, Patch.Stormblood) // Snowflake Moray
           .Spear(SpearfishSize.Average, SpearfishSpeed.ExtremelySlow);
        data.Apply(20149, Patch.Stormblood) // Hoppfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(20150, Patch.Stormblood) // Lightscale
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(20151, Patch.Stormblood) // Grass Fugu
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20152, Patch.Stormblood) // Giant Eel
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(20153, Patch.Stormblood) // Kamina Crab
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(20154, Patch.Stormblood) // Spider Crab
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(20155, Patch.Stormblood) // Little Dragonfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20156, Patch.Stormblood) // Black Fanfish
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20157, Patch.Stormblood) // Zebra Shark
           .Spear(SpearfishSize.Large, SpearfishSpeed.HyperFast);
        data.Apply(20158, Patch.Stormblood) // Nophica's Comb
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(20159, Patch.Stormblood) // Warty Wartfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.ExtremelySlow);
        data.Apply(20160, Patch.Stormblood) // Common Whelk
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20161, Patch.Stormblood) // Hairless Barb
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
        data.Apply(20162, Patch.Stormblood) // Hatchetfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(20163, Patch.Stormblood) // Threadfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(20164, Patch.Stormblood) // Garden Eel
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(20165, Patch.Stormblood) // Eastern Sea Pickle
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20166, Patch.Stormblood) // Brindlebass
           .Spear(SpearfishSize.Large, SpearfishSpeed.ExtremelyFast);
        data.Apply(20167, Patch.Stormblood) // Demon Stonefish
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20168, Patch.Stormblood) // Armored Crayfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(20169, Patch.Stormblood) // Bighead Carp
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(20170, Patch.Stormblood) // Zeni Clam
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20171, Patch.Stormblood) // Corpse-eater
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
        data.Apply(20172, Patch.Stormblood) // Ronin Trevally
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
        data.Apply(20173, Patch.Stormblood) // Toothsome Grouper
           .Spear(SpearfishSize.Large, SpearfishSpeed.SuperFast);
        data.Apply(20174, Patch.Stormblood) // Horned Turban
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20175, Patch.Stormblood) // Ruby Sea Star
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20176, Patch.Stormblood) // Gauntlet Crab
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(20177, Patch.Stormblood) // Hermit Goby
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(20178, Patch.Stormblood) // Skythorn
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20179, Patch.Stormblood) // Swordtip
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(20180, Patch.Stormblood) // False Scad
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(20181, Patch.Stormblood) // Snow Crab
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(20182, Patch.Stormblood) // Red-eyed Lates
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(20183, Patch.Stormblood) // Common Bitterling
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(20184, Patch.Stormblood) // Fifty-summer Cod
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20185, Patch.Stormblood) // Nagxian Mullet
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(20186, Patch.Stormblood) // Redcoat
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(20187, Patch.Stormblood) // Yanxian Tiger Prawn
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(20188, Patch.Stormblood) // Tengu Fan
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20189, Patch.Stormblood) // Star Turban
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20190, Patch.Stormblood) // Blue-fish
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(20191, Patch.Stormblood) // Steppe Bullfrog
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20192, Patch.Stormblood) // Cavalry Catfish
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(20193, Patch.Stormblood) // Redfin
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(20194, Patch.Stormblood) // Moondisc
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(20195, Patch.Stormblood) // Bleached Bonytongue
           .Spear(SpearfishSize.Large, SpearfishSpeed.ExtremelySlow);
        data.Apply(20196, Patch.Stormblood) // Salt Shark
           .Spear(SpearfishSize.Average, SpearfishSpeed.VeryFast);
        data.Apply(20197, Patch.Stormblood) // King's Mantle
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(20198, Patch.Stormblood) // Sea Lamp
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20199, Patch.Stormblood) // Amberjack
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(20200, Patch.Stormblood) // Cherry Salmon
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20201, Patch.Stormblood) // Yu-no-hana Crab
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20202, Patch.Stormblood) // Dotharli Gudgeon
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(20203, Patch.Stormblood) // River Clam
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20204, Patch.Stormblood) // Grass Shark
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(20205, Patch.Stormblood) // Typhoon Shrimp
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20206, Patch.Stormblood) // Rock Oyster
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20207, Patch.Stormblood) // Salt Urchin
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20208, Patch.Stormblood) // Carpenter Crab
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20209, Patch.Stormblood) // Spiny Lobster
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20210, Patch.Stormblood) // Mitsukuri Shark
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast)
           .Predators(data, 0, (20217, 7));
        data.Apply(20211, Patch.Stormblood) // Doman Bubble Eye
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(20212, Patch.Stormblood) // Dragon Squeaker
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(20213, Patch.Stormblood) // Dawn Herald
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(20214, Patch.Stormblood) // Salt Cellar
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(20215, Patch.Stormblood) // White Sturgeon
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20216, Patch.Stormblood) // Tithe Collector
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20217, Patch.Stormblood) // Bashful Batfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(20218, Patch.Stormblood) // River Bream
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast);
        data.Apply(20219, Patch.Stormblood) // Snipe Eel
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20220, Patch.Stormblood) // Cherubfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average)
           .Predators(data, 0, (20228, 7));
        data.Apply(20221, Patch.Stormblood) // Dusk Herald
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(20222, Patch.Stormblood) // Glaring Perch
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(20223, Patch.Stormblood) // Abalathian Pipira
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(20224, Patch.Stormblood) // Steel Loach
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(20225, Patch.Stormblood) // Ivory Sole
           .Spear(SpearfishSize.Average, SpearfishSpeed.ExtremelySlow);
        data.Apply(20226, Patch.Stormblood) // Motley Beakfish
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(20227, Patch.Stormblood) // Thousandfang
           .Spear(SpearfishSize.Large, SpearfishSpeed.SuperFast)
           .Predators(data, 0, (20217, 7));
        data.Apply(20228, Patch.Stormblood) // Ichthyosaur
           .Spear(SpearfishSize.Large, SpearfishSpeed.ExtremelyFast);
        data.Apply(20229, Patch.Stormblood) // Sailfin
           .Spear(SpearfishSize.Average, SpearfishSpeed.VeryFast);
        data.Apply(20230, Patch.Stormblood) // Fangshi
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow)
           .Predators(data, 0, (20228, 7));
        data.Apply(20231, Patch.Stormblood) // Flamefish
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
        data.Apply(20232, Patch.Stormblood) // Fickle Krait
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20233, Patch.Stormblood) // Eternal Eye
           .Spear(SpearfishSize.Small, SpearfishSpeed.VeryFast)
           .Predators(data, 0, (20222, 7));
        data.Apply(20234, Patch.Stormblood) // Soul of the Stallion
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average)
           .Predators(data, 0, (20222, 7));
        data.Apply(20235, Patch.Stormblood) // Flood Tuna
           .Spear(SpearfishSize.Large, SpearfishSpeed.ExtremelyFast);
        data.Apply(20236, Patch.Stormblood) // Mercenary Crab
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(20237, Patch.Stormblood) // Ashfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(20238, Patch.Stormblood) // Silken Sunfish
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow)
           .Predators(data, 0, (20236, 7));
        data.Apply(20239, Patch.Stormblood) // Mosasaur
           .Spear(SpearfishSize.Large, SpearfishSpeed.SuperFast)
           .Predators(data, 0, (20236, 7));
        data.Apply(20528, Patch.Stormblood) // Tiny Tatsunoko
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average)
           .Predators(data, 0, (20217, 7));
    }

    private void Data4_1(IDictionary<uint, SpearFish> data)
    {
        data.Apply(21179, Patch.TheLegendReturns) // Ichimonji
           .Time(120, 720)
           .Spear(SpearfishSize.Large, SpearfishSpeed.LynFast);
        data.Apply(21180, Patch.TheLegendReturns) // Snailfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast)
           .Predators(data, 0, (21179, 10));
    }

    private void Data5_0(IDictionary<uint, SpearFish> data)
    {
        data.Apply(27516, Patch.Shadowbringers) // Grey Carp
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27517, Patch.Shadowbringers) // Lilac Goby
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27518, Patch.Shadowbringers) // Purple Ghost
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(27519, Patch.Shadowbringers) // Lakelouse
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(27520, Patch.Shadowbringers) // Gazing Glass
           .Spear(SpearfishSize.Average, SpearfishSpeed.ExtremelySlow);
        data.Apply(27521, Patch.Shadowbringers) // Source Octopus
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(27522, Patch.Shadowbringers) // Elven Spear
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(27523, Patch.Shadowbringers) // Shade Gudgeon
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(27524, Patch.Shadowbringers) // Lakethistle
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(27525, Patch.Shadowbringers) // Platinum Bream
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(27526, Patch.Shadowbringers) // Wardenfish
           .Spear(SpearfishSize.Large, SpearfishSpeed.ExtremelySlow);
        data.Apply(27527, Patch.Shadowbringers) // Finned Eggplant
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(27528, Patch.Shadowbringers) // Skykisser
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27529, Patch.Shadowbringers) // Viola Clam
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(27530, Patch.Shadowbringers) // Geayi
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(27531, Patch.Shadowbringers) // Noblefish
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(27532, Patch.Shadowbringers) // Misteye
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
        data.Apply(27533, Patch.Shadowbringers) // Lakeland Cod
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(27534, Patch.Shadowbringers) // Little Bismarck
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast)
           .Predators(data, 0, (27531, 7));
        data.Apply(27535, Patch.Shadowbringers) // Bothriolepis
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow)
           .Predators(data, 0, (27531, 7));
        data.Apply(27536, Patch.Shadowbringers) // Big-eye
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast);
        data.Apply(27537, Patch.Shadowbringers) // Jenanna's Tear
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(27538, Patch.Shadowbringers) // Daisy Turban
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27539, Patch.Shadowbringers) // Shade Axolotl
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(27540, Patch.Shadowbringers) // Little Flirt
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(27541, Patch.Shadowbringers) // Peallaidh
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(27542, Patch.Shadowbringers) // Voeburt Bichir
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(27543, Patch.Shadowbringers) // Poecilia
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27544, Patch.Shadowbringers) // Gilded Batfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(27545, Patch.Shadowbringers) // Petalfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast);
        data.Apply(27546, Patch.Shadowbringers) // Mirrorfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(27547, Patch.Shadowbringers) // Glass Eel
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(27548, Patch.Shadowbringers) // Voeburt Salamander
           .Spear(SpearfishSize.Large, SpearfishSpeed.ExtremelySlow);
        data.Apply(27549, Patch.Shadowbringers) // Bedskipper
           .Spear(SpearfishSize.Small, SpearfishSpeed.Fast);
        data.Apply(27550, Patch.Shadowbringers) // Dandyfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27551, Patch.Shadowbringers) // Sauldia Ruby
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(27552, Patch.Shadowbringers) // Mock Pixie
           .Spear(SpearfishSize.Average, SpearfishSpeed.ExtremelySlow);
        data.Apply(27553, Patch.Shadowbringers) // Hunter's Arrow
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(27554, Patch.Shadowbringers) // Fuathgobbler
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(27555, Patch.Shadowbringers) // Paradise Crab
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27556, Patch.Shadowbringers) // Saint Fathric's Ire
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(27557, Patch.Shadowbringers) // Queensgown
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(27558, Patch.Shadowbringers) // Flowering Kelpie
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(27559, Patch.Shadowbringers) // Ghoulfish
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(27560, Patch.Shadowbringers) // Measan Deala
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(27561, Patch.Shadowbringers) // Dohn Horn
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(27562, Patch.Shadowbringers) // Aquabloom
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27563, Patch.Shadowbringers) // Jester Fish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(27564, Patch.Shadowbringers) // Blue Lightning
           .Spear(SpearfishSize.Large, SpearfishSpeed.ExtremelyFast);
        data.Apply(27565, Patch.Shadowbringers) // Maidenhair
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(27566, Patch.Shadowbringers) // Blue Mountain Bubble
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast)
           .Predators(data, 0, (27551, 7));
        data.Apply(27567, Patch.Shadowbringers) // Elder Pixie
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow)
           .Predators(data, 0, (27551, 7));
        data.Apply(27568, Patch.Shadowbringers) // Ankle Snipper
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27569, Patch.Shadowbringers) // Treescale
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(27570, Patch.Shadowbringers) // Ronkan Pleco
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(27571, Patch.Shadowbringers) // Gourmand Crab
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(27572, Patch.Shadowbringers) // Gatorl's Bead
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(27573, Patch.Shadowbringers) // Diamondtongue
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(27574, Patch.Shadowbringers) // Hermit's Hood
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(27575, Patch.Shadowbringers) // Hermit Crab
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(27576, Patch.Shadowbringers) // Megapiranha
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast);
        data.Apply(27577, Patch.Shadowbringers) // Everdark Bass
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(27578, Patch.Shadowbringers) // Lozatl Pirarucu
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(27579, Patch.Shadowbringers) // Anpa's Handmaid
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(27580, Patch.Shadowbringers) // Viis Ear
           .Spear(SpearfishSize.Small, SpearfishSpeed.VeryFast)
           .Predators(data, 0, (27569, 10));
        data.Apply(27581, Patch.Shadowbringers) // Rak'tika Goby
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average)
           .Predators(data, 0, (27569, 10));
    }

    private void Data6_0(IDictionary<uint, SpearFish> data)
    {
        data.Apply(36522, Patch.Endwalker) // Thavnairian Cucumber
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(36523, Patch.Endwalker) // Spiny King Crab
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(36524, Patch.Endwalker) // Thavnairian Eel
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(36525, Patch.Endwalker) // Gilled Topknot
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast);
        data.Apply(36526, Patch.Endwalker) // Purusa Fish
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(36527, Patch.Endwalker) // Giantsgall Jaw
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
        data.Apply(36528, Patch.Endwalker) // Akyaali Sardine
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(36529, Patch.Endwalker) // Spicy Pickle
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(36530, Patch.Endwalker) // Mayavahana
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(36531, Patch.Endwalker) // Hedonfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(36532, Patch.Endwalker) // Satrap Trapfish
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(36533, Patch.Endwalker) // Blue Marlin
           .Spear(SpearfishSize.Large, SpearfishSpeed.HyperFast);
        data.Apply(36534, Patch.Endwalker) // Satrap's Whisper
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(36535, Patch.Endwalker) // Tebqeyiq Smelt
           .Spear(SpearfishSize.Small, SpearfishSpeed.SuperFast)
           .Predators(data, 0, (36531, 10), (36546, 2), (36547, 3));
        data.Apply(36536, Patch.Endwalker) // Shallows Cod
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(36537, Patch.Endwalker) // Meyhane Reveler
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(36538, Patch.Endwalker) // Daemir's Alloy
           .Spear(SpearfishSize.Large, SpearfishSpeed.ExtremelyFast);
        data.Apply(36539, Patch.Endwalker) // Rasa Fish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(36540, Patch.Endwalker) // Agama's Palm
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(36541, Patch.Endwalker) // Rummy-nosed Tetra
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(36542, Patch.Endwalker) // Monksblade
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(36543, Patch.Endwalker) // Atamra Cichlid
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast);
        data.Apply(36544, Patch.Endwalker) // Root of Maya
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(36545, Patch.Endwalker) // Floral Snakehead
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(36546, Patch.Endwalker) // Xiphactinus
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average)
           .Predators(data, 0, (36531, 10));
        data.Apply(36547, Patch.Endwalker) // Dusky Shark
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(36548, Patch.Endwalker) // Coffer Shell
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(36549, Patch.Endwalker) // Onihige
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(36550, Patch.Endwalker) // Onokoro Carp
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(36551, Patch.Endwalker) // Ruby-spotted Crab
           .Spear(SpearfishSize.Average, SpearfishSpeed.ExtremelySlow);
        data.Apply(36552, Patch.Endwalker) // Marrow-eater
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(36553, Patch.Endwalker) // Cloudy Catshark
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
        data.Apply(36554, Patch.Endwalker) // Red Gurnard
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(36555, Patch.Endwalker) // Dream Pickle
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(36556, Patch.Endwalker) // Ruby Haddock
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(36557, Patch.Endwalker) // Crown Fish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Fast);
        data.Apply(36558, Patch.Endwalker) // Sword of Isari
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(36559, Patch.Endwalker) // Blue Shark
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(36560, Patch.Endwalker) // Barb of Exile
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(36561, Patch.Endwalker) // Smooth Lumpfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
        data.Apply(36562, Patch.Endwalker) // Hells' Cap
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(36563, Patch.Endwalker) // Keeled Fugu
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(36564, Patch.Endwalker) // Eastern Seerfish
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(36565, Patch.Endwalker) // False Fusilier
           .Spear(SpearfishSize.Large, SpearfishSpeed.VerySlow);
        data.Apply(36566, Patch.Endwalker) // Skipping Stone
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(36567, Patch.Endwalker) // Red-spotted Blenny
           .Spear(SpearfishSize.Small, SpearfishSpeed.Fast);
        data.Apply(36568, Patch.Endwalker) // Othardian Wrasse
           .Spear(SpearfishSize.Average, SpearfishSpeed.ExtremelyFast);
        data.Apply(36569, Patch.Endwalker) // Grey Mullet
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(36570, Patch.Endwalker) // Prayer Cushion
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
        data.Apply(36571, Patch.Endwalker) // Deepbody Boarfish
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(36572, Patch.Endwalker) // Jointed Razorfish
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(36573, Patch.Endwalker) // Pipefish
           .Spear(SpearfishSize.Small, SpearfishSpeed.Fast)
           .Predators(data, 0, (36553, 10));
        data.Apply(36574, Patch.Endwalker) // Righteye Flounder
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(36575, Patch.Endwalker) // Mini Yasha
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
        data.Apply(36576, Patch.Endwalker) // Sawshark
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast);
        data.Apply(36577, Patch.Endwalker) // Othardian Lumpsucker
           .Spear(SpearfishSize.Average, SpearfishSpeed.Slow);
        data.Apply(36578, Patch.Endwalker) // Shogun's Kabuto
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow)
           .Predators(data, 0, (36553, 10));
        data.Apply(36579, Patch.Endwalker) // Bluefin Trevally
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(36580, Patch.Endwalker) // Kitefin Shark
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(36581, Patch.Endwalker) // Uzumaki
           .Spear(SpearfishSize.Large, SpearfishSpeed.Average);
        data.Apply(36582, Patch.Endwalker) // Natron Puffer
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(36583, Patch.Endwalker) // Diamond Dagger
           .Spear(SpearfishSize.Small, SpearfishSpeed.VeryFast);
        data.Apply(36584, Patch.Endwalker) // Queenly Fan
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(36585, Patch.Endwalker) // Pale Panther
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(36586, Patch.Endwalker) // Saltsquid
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(36587, Patch.Endwalker) // Platinum Hammerhead
           .Spear(SpearfishSize.Large, SpearfishSpeed.VeryFast);
    }

    private void Data6_1(IDictionary<uint, SpearFish> data)
    {
        data.Apply(36659, Patch.NewfoundAdventure) // Inksquid
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(36661, Patch.NewfoundAdventure) // Auroral Clam
           .Spear(SpearfishSize.Small, SpearfishSpeed.ExtremelySlow);
    }

    private void Data6_3(IDictionary<uint, SpearFish> data)
    {
        data.Apply(38811, Patch.GodsRevelLandsTremble) // Ken Kiln
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(38813, Patch.GodsRevelLandsTremble) // Glorianda's Tear
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(38837, Patch.GodsRevelLandsTremble) // Lakeskipper
           .Spear(SpearfishSize.Small, SpearfishSpeed.VerySlow);
        data.Apply(38838, Patch.GodsRevelLandsTremble) // Bronze Eel
           .Spear(SpearfishSize.Average, SpearfishSpeed.VerySlow);
        data.Apply(38839, Patch.GodsRevelLandsTremble) // Striped Peacock Bass
           .Spear(SpearfishSize.Average, SpearfishSpeed.Average);
        data.Apply(38840, Patch.GodsRevelLandsTremble) // Bronze Trout
           .Spear(SpearfishSize.Large, SpearfishSpeed.Slow);
        data.Apply(38841, Patch.GodsRevelLandsTremble) // Nosceasaur
           .Spear(SpearfishSize.Large, SpearfishSpeed.Fast)
           .Predators(data, 60, (38939, 4));
        data.Apply(38939, Patch.GodsRevelLandsTremble) // Verdigris Guppy
           .Spear(SpearfishSize.Small, SpearfishSpeed.Fast);
    }

    private void Data6_5(IDictionary<uint, SpearFish> data)
    {
        data.Apply(41060, Patch.GrowingLight) // Empyreal Spiral
           .Spear(SpearfishSize.Small, SpearfishSpeed.Slow);
        data.Apply(41062, Patch.GrowingLight) // Opal Tetra
           .Spear(SpearfishSize.Small, SpearfishSpeed.Average);
    }
}