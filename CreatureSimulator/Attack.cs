namespace CreatureSimulator
{
    public class Attack
    {
        public int damage = 0;
        public int numAttacks = 1;
        public bool ranged = false;
        public Dice.enWeaponTypes weaponType;
        public string MakeAttack(Creature attacker, Creature target)
        {
            int checkModifier = 0;
            if (weaponType == Dice.enWeaponTypes.NW || weaponType == Dice.enWeaponTypes.TH) checkModifier = attacker.Strength;
            if (weaponType == Dice.enWeaponTypes.OH) checkModifier = attacker.Dexterity;
            (int result, bool success) = Dice.MakeAttack(target.Armor, weaponType, checkModifier, damage, attacker.Strength);
            if (success)
            {
                target.Hitpoints -= result;
                return $"{attacker.Name} ({attacker.Hitpoints}) dealt {weaponType.ToString()} {result} {(ranged ? "ranged" : "")} damage to {target.Name} ({target.Hitpoints+result} => {target.Hitpoints})";
            }

            return $"{attacker.Name} ({attacker.Hitpoints}) attacked {(ranged ? "ranged" : "")} {weaponType.ToString()} and missed against {target.Name} ({target.Hitpoints})";
        }

    }
}
