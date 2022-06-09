using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatureSimulator
{
    internal static class Dice
    {
        public enum enWeaponTypes { NW, OH, TH  }
        private static Random rnd = new Random();

        public static int Random(int number)
        {
            return rnd.Next(number) + 1;
        }

        public static int RollD6()
        {
            return Random(6);
        }

        public static int Roll2D6()
        {
            return RollD6() + RollD6();
        }

        public static (int result, bool success) MakeAttack(int target, enWeaponTypes weaponType, int checkMod = 0, int damageMod = 0, int attackerStrength = 0)
        {
            var die1 = RollD6();
            var die2 = RollD6();

            // Natural weapons are the only weapons that add a modifier on the attack by default
            if (weaponType == enWeaponTypes.NW) damageMod += attackerStrength;

            int dieResult = 0;
            if(weaponType == enWeaponTypes.NW) dieResult = die1 < die2 ? die1: die2;
            if (weaponType == enWeaponTypes.OH) dieResult = die1 > die2 ? die1 : die2;
            if (weaponType == enWeaponTypes.TH) dieResult = die1 + die2;

            if (die1 == die2) return ((dieResult + damageMod) * 2, true);
            else return (dieResult + damageMod, die1 + die2 + checkMod >= target);
        }

        public static T Choose<T>(List<T> items) {
            if (items.Count == 0) return default(T);
            if (items.Count == 1) return items.First();
            else return items[rnd.Next(items.Count)];
        }



    }
}
