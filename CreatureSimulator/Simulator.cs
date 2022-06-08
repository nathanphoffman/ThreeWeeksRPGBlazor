using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatureSimulator
{
    internal static class Simulator
    {

        public static void LogGroupPosition(int group, List<string> logging, params List<Creature>[] teams)
        {
            var creaturesInGroup = teams.ToList().SelectMany(y => y.Where(a => a.CurrentGroup == group).Select(z => z.Name)).ToList();
            if (creaturesInGroup.Count > 0) logging.Add($"Group {group}: " + String.Join(',', creaturesInGroup));
            else logging.Add($"Group {group}: none");
        }

        public static void LogGroupPositions(int numberofGroups, List<string> logging, params List<Creature>[] teams)
        {
            logging.Add("--------------------------");
            for (int i = 1; i <= numberofGroups; i++) LogGroupPosition(i, logging, teams);
            logging.Add("--------------------------");
        }

        public static void RunStatistics(int iterations)
        {
            int[] results = new int[3];

            int consensuses = 0;
            decimal lastNumber = 0;
            int i = 0;
            while (i++ < iterations)
            {
                (int result, List<string> logs) = Run(3);
                results[result] += 1;
                if (i % 10000 == 0)
                {
                    decimal thisNumber = (decimal)results[1] / ((decimal)results[2] + (decimal)results[1] + (decimal)0.00001);

                    if (Math.Abs(thisNumber - lastNumber) < (decimal)0.0004 || lastNumber == 0) consensuses++;
                    else consensuses = 0;

                    lastNumber = (decimal)results[1] / ((decimal)results[2] + (decimal)results[1] + (decimal)0.00001);
                    
                    Console.WriteLine($"Team1 wins: {results[1]} vs Team2 wins: {results[2]}  {thisNumber}");

                    if (consensuses > 10)
                    {
                        Console.WriteLine($"CONSENSUS FOUND! Team1 wins {Math.Round(thisNumber*100,1)}% of the time.");
                        break;
                    }
                }
            }

            
        }

        public static (int result,List<string> logs) Run(int numberOfGroups = 3)
        {
            List<string> logging = new List<string>();
            var creature1Ranged = new Creature("GoblinRanged", 6, 6, 1, 1, 1, 2).AddOHAttack(0, 1, false).SetCurrentGroup(numberOfGroups).AddDefaultWeapon();
            var creature1 = new Creature("Goblin", 6, 6, 1, 1, 1, 2).AddOHAttack().SetCurrentGroup(numberOfGroups).AddDefaultWeapon();
            var creature2Ranged = new Creature("OrcRanged", 3, 10, 1, 1, 1, 2).AddOHAttack(0, 1, true).SetCurrentGroup(numberOfGroups).AddDefaultWeapon();
            var creature2 = new Creature("Orc", 3, 10, 1, 1, 1, 2).AddOHAttack().SetCurrentGroup(numberOfGroups).AddDefaultWeapon();

            creature1.LogCreatureStats(logging);
            creature1Ranged.LogCreatureStats(logging);
            creature2.LogCreatureStats(logging);
            creature2Ranged.LogCreatureStats(logging);

            var team1 = new List<Creature> { creature1, creature1Ranged };
            var team2 = new List<Creature> { creature2, creature2Ranged };

            int stopper = 0;
            while (team1.Count > 0 && team2.Count > 0 && ++stopper < 100)
            {
                team1.ForEach(creature =>
                {
                    LogGroupPositions(numberOfGroups, logging, team1, team2);
                    if(creature != null) logging.Add(ProcessTurn(creature, team2, numberOfGroups));
                    RemoveDeadCreatures(logging, team2);
                });
                team2.ForEach(creature =>
                {
                    LogGroupPositions(numberOfGroups, logging, team1, team2);
                    if (creature != null) logging.Add(ProcessTurn(creature, team1, numberOfGroups));
                    RemoveDeadCreatures(logging, team1);
                });
            }

            logging.Add("Battle has ended");
            if (team1.Count > 0) return (1,logging);
            if (team2.Count > 0) return (2,logging);
            else return (0,logging);

        }

        public static void RemoveDeadCreatures(List<string> logging, params List<Creature>[] teams)
        {
            foreach (var team in teams)
            {
                var removeCreatures = team.Where(creature => creature.Hitpoints <= 0).ToList();
                removeCreatures.ForEach(creature => {
                    team.Remove(creature);
                    logging.Add($"{creature.Name} died");
                });
            }
        }

        public static string ProcessTurn(Creature creature, List<Creature> opposingTeam, int numberOfGroups)
        {
            if (opposingTeam.Count == 0) return "Battle has ended";
            var atSpace = opposingTeam.Where(x => x.CurrentGroup == creature.CurrentGroup).ToList();
            var otherSpaces = opposingTeam.Where(x => x.CurrentGroup != creature.CurrentGroup).ToList();
            var noSpaces = Enumerable.Range(1, numberOfGroups).Where(x => otherSpaces.Where(y => y.CurrentGroup == x).Count() == 0).ToList();

            var atChoice = Dice.Choose(atSpace);
            var otherChoice = Dice.Choose(otherSpaces);

            { // 1. If it has two-handed melee and an enemy is on their space, swing
                var attack = creature.GetAttackByType(Dice.enWeaponTypes.TH, false);
                if (attack != null && atSpace.Count > 0) return attack.MakeAttack(creature, atChoice);
            }
            { // 2. If it has two-handed ranged and an enemy is in an opposing space, shoot
                var attack = creature.GetAttackByType(Dice.enWeaponTypes.TH, true);
                if (attack != null && otherSpaces.Count > 0) return attack.MakeAttack(creature, otherChoice);
            }
            { // 3. If it has one-handed melee and there is an enemy in a space it can reach, go to it and swing (go to the group with the fewest enemies)
                var attack = creature.GetAttackByType(Dice.enWeaponTypes.OH, false);
                if (attack != null && otherSpaces.Count > 0)
                {
                    creature.CurrentGroup = otherChoice.CurrentGroup;
                    return attack.MakeAttack(creature, otherChoice);
                }
            }
            { // 4. If it has one-handed ranged shoot at a space if no enemy is on your space, otherwise move to an empty space and shoot if possible, if still not possible, shoot at a space even if your space is full
                var attack = creature.GetAttackByType(Dice.enWeaponTypes.OH, true);
                if (attack != null && otherSpaces.Count > 0 && atSpace.Count == 0)
                {
                    return attack.MakeAttack(creature, otherChoice);
                }
                else if (attack != null && noSpaces.Count > 0)
                {
                    var combinedList = new List<Creature> { atChoice, otherChoice };
                    combinedList = combinedList.RemoveNulls().ToList();
                    creature.CurrentGroup = Dice.Choose(noSpaces);
                    return attack.MakeAttack(creature, Dice.Choose(combinedList));
                }
                else if (attack != null && otherSpaces.Count > 0) // cant move
                {
                    return attack.MakeAttack(creature, otherChoice);
                }
            }
            { // 5. If it has one-handed melee and there is an enemy on its space, swing
                var attack = creature.GetAttackByType(Dice.enWeaponTypes.OH, false);
                if (attack != null && atSpace.Count > 0) return attack.MakeAttack(creature, atChoice);
            }
            { // 6. If it is natural-weapon attack a creature on its space
                var attack = creature.GetAttackByType(Dice.enWeaponTypes.NW, false);
                if (attack != null && atSpace.Count > 0) return attack.MakeAttack(creature, atChoice);
            }
            { // 7. If it is natural-weapon move and attack the nearest creature with the fewest creatures on the space
                var attack = creature.GetAttackByType(Dice.enWeaponTypes.NW, false);
                if (attack != null && otherSpaces.Count > 0 && atSpace.Count == 0)
                {
                    creature.CurrentGroup = otherChoice.CurrentGroup;
                    return attack.MakeAttack(creature, otherChoice);
                }
            }

            return null;
        }
    }
}
