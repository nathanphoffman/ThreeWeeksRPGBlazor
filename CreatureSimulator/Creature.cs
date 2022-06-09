using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatureSimulator
{
    internal class Creature
    {
        public Creature(string name, int hearts, int armor, int intelligence, int strength, int charisma, int dexterity)
        {
            this.Hearts = hearts;
            this.Armor = armor;
            this.Intelligence = intelligence;
            this.Strength = strength;
            this.Charisma = charisma;
            this.Dexterity = dexterity;
            this.Attacks = new List<Attack>();
            this.Name = name;
            SetHitpoints();
        }

        public Creature(CreatureStats creature, int numOfGroups)
        {
            Enum.TryParse<Dice.enWeaponTypes>(creature.type, out Dice.enWeaponTypes weaponType);

            this.Hearts = creature.hearts;
            this.Dexterity = creature.dexterity;
            this.Intelligence = creature.intelligence;
            this.Charisma = creature.charisma;
            this.Strength = creature.strength;
            this.Armor = creature.armor;
            this.Attacks.Add(new Attack { damage = creature.damage, weaponType = weaponType, ranged = creature.ranged });
            this.Name = String.IsNullOrEmpty(creature.name) ? $"Creature #{Dice.Random(1000) + 1}" : creature.name;
            SetHitpoints().AddDefaultWeapon().SetCurrentGroup(numOfGroups);
        }
        /*
        public Creature(string stats)
        {


            SetHitpoints();
        }
        */

        public int CurrentGroup;
        public int Hearts;
        public int Hitpoints;
        public int Armor;
        public int Intelligence;
        public int Strength;
        public int Charisma;
        public int Dexterity;
        public string Name;
        public List<Attack> Attacks = new List<Attack>();

        public Creature SetCurrentGroup(int numGroups)
        {
            CurrentGroup = Dice.Random(numGroups);
            return this;
        }

        public Creature AddNWAttack(int damage = 0, int numAttacks = 1, bool ranged = false)
        {
            Attacks.Add(new Attack { damage = damage, numAttacks = numAttacks, weaponType = Dice.enWeaponTypes.NW, ranged = ranged });
            return this;
        }

        public Creature AddOHAttack(int damage = 0, int numAttacks = 1, bool ranged = false)
        {
            Attacks.Add(new Attack { damage = damage, numAttacks = numAttacks, weaponType = Dice.enWeaponTypes.OH, ranged = ranged });
            return this;
        }

        public Creature AddTHAttack(int damage = 0, int numAttacks = 1, bool ranged = false)
        {
            Attacks.Add(new Attack { damage = damage, numAttacks = numAttacks, weaponType = Dice.enWeaponTypes.TH, ranged = ranged });
            return this;
        }

        public Creature AddDefaultWeapon()
        {
            var attackResults = Attacks.Where(x => x.weaponType == Dice.enWeaponTypes.NW).ToList();
            if (attackResults.Count == 0 && Attacks != null) Attacks.Add(new Attack { weaponType = Dice.enWeaponTypes.NW});
            return this;
        }

        private Creature SetHitpoints()
        {
            Hitpoints = Dice.Roll2D6() * Hearts;
            return this;
        }

        public void LogCreatureStats(List<string> log)
        {
            log.Add("------------------------------");
            log.Add($"Armor:{Armor} Hearts:{Hearts} Hitpoints:{Hitpoints}");
            log.Add($"Stats: STR:{Strength} INT:{Intelligence} CHA:{Charisma} DEX:{Dexterity}");
            log.Add($"Creature is at position {CurrentGroup}");
            Attacks.ForEach(attack => log.Add($"Attack:{attack.numAttacks}x{attack.weaponType.ToString()}+{attack.damage}"));
            log.Add("------------------------------");
        }

        public List<Attack> GetAttacksByType(Dice.enWeaponTypes weaponType, bool ranged)
        {
            return Attacks.Where(x => x.weaponType == weaponType && x.ranged == ranged).ToList();
        }

        public Attack GetAttackByType(Dice.enWeaponTypes weaponType, bool ranged)
        {
            var attacks = GetAttacksByType(weaponType, ranged);
            if (attacks.Count == 0) return null;

            var attack = Dice.Choose(attacks);
            return attack;
        }

        public Attack SelectAttack(bool mobile, bool ranged = false)
        {
            // two handed attacks always take priority since they deal more damage on average
            var THAttacks = GetAttacksByType(Dice.enWeaponTypes.TH, ranged);
            if (Attacks.Count > 0 && !mobile) return Dice.Choose(THAttacks);

            var OHAttacks = GetAttacksByType(Dice.enWeaponTypes.OH, ranged);
            if (OHAttacks.Count > 0) return Dice.Choose(OHAttacks);

            var NWAttacks = GetAttacksByType(Dice.enWeaponTypes.NW, ranged);
            if (NWAttacks.Count > 0) return Dice.Choose(NWAttacks);

            // all creatures have a basic melee natural weapon, so if none are found return it
            if (!ranged) return new Attack { damage = 0, numAttacks = 1, ranged = false, weaponType = Dice.enWeaponTypes.NW };

            // if no weapon is found (asking for ranged but no ranged matches)
            return null;

        }
    }
}
