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
            this.hearts = hearts;
            this.armor = armor;
            this.intelligence = intelligence;
            this.strength = strength;
            this.charisma = charisma;
            this.dexterity = dexterity;
            this.attacks = new List<Attack>();
            this.name = name;
            SetHitpoints();
        }

        public int currentGroup;
        public int hearts;
        public int hitpoints;
        public int armor;
        public int intelligence;
        public int strength;
        public int charisma;
        public int dexterity;
        public string name;
        public List<Attack> attacks;

        public Creature SetCurrentGroup(int numGroups)
        {
            currentGroup = Dice.Random(numGroups);
            return this;
        }

        public Creature AddNWAttack(int damage = 0, int numAttacks = 1, bool ranged = false)
        {
            attacks.Add(new Attack { damage = damage, numAttacks = numAttacks, weaponType = Dice.enWeaponTypes.NW, ranged = ranged });
            return this;
        }

        public Creature AddOHAttack(int damage = 0, int numAttacks = 1, bool ranged = false)
        {
            attacks.Add(new Attack { damage = damage, numAttacks = numAttacks, weaponType = Dice.enWeaponTypes.OH, ranged = ranged });
            return this;
        }

        public Creature AddTHAttack(int damage = 0, int numAttacks = 1, bool ranged = false)
        {
            attacks.Add(new Attack { damage = damage, numAttacks = numAttacks, weaponType = Dice.enWeaponTypes.TH, ranged = ranged });
            return this;
        }

        public Creature AddDefaultWeapon()
        {
            var attackResults = attacks.Where(x => x.weaponType == Dice.enWeaponTypes.NW).ToList();
            if (attackResults.Count == 0 && attacks != null) attacks.Add(new Attack { weaponType = Dice.enWeaponTypes.NW});
            return this;
        }

        private void SetHitpoints()
        {
            hitpoints = Dice.Roll2D6() * hearts;
        }

        public void LogCreatureStats(List<string> log)
        {
            log.Add("------------------------------");
            log.Add($"Armor:{armor} Hearts:{hearts} Hitpoints:{hitpoints}");
            log.Add($"Stats: STR:{strength} INT:{intelligence} CHA:{charisma} DEX:{dexterity}");
            log.Add($"Creature is at position {currentGroup}");
            attacks.ForEach(attack => log.Add($"Attack:{attack.numAttacks}x{attack.weaponType.ToString()}+{attack.damage}"));
            log.Add("------------------------------");
        }

        public List<Attack> GetAttacksByType(Dice.enWeaponTypes weaponType, bool ranged)
        {
            return attacks.Where(x => x.weaponType == weaponType && x.ranged == ranged).ToList();
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
            if (attacks.Count > 0 && !mobile) return Dice.Choose(THAttacks);

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

    internal class Attack
    {
        public int damage = 0;
        public int numAttacks = 1;
        public bool ranged = false;
        public Dice.enWeaponTypes weaponType;
        public string MakeAttack(Creature attacker, Creature target)
        {
            int checkModifier = 0;
            if (weaponType == Dice.enWeaponTypes.NW || weaponType == Dice.enWeaponTypes.TH) checkModifier = attacker.strength;
            if (weaponType == Dice.enWeaponTypes.OH) checkModifier = attacker.dexterity;
            (int result, bool success) = Dice.MakeAttack(target.armor, weaponType, checkModifier, damage, attacker.strength);
            if (success)
            {
                target.hitpoints -= result;
                return $"{attacker.name} ({attacker.hitpoints}) dealt {weaponType.ToString()} {result} damage to {target.name} ({target.hitpoints})";
            }

            return $"{attacker.name} ({attacker.hitpoints}) swung {weaponType.ToString()} and missed against {target.name} ({target.hitpoints})";
        }

    }

}
