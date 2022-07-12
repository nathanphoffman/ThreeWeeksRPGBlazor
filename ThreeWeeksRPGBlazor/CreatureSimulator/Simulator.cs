using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatureSimulator
{
    public static class Simulator
    {

        public static void LogGroupPosition(int group, List<string> logging, params List<Creature>[] teams)
        {
            var creaturesInGroup = teams.ToList().SelectMany(y => y.Where(a => a.CurrentGroup == group).Select(z => $"{z.Name} (HP: {z.Hitpoints})")).ToList();
            if (creaturesInGroup.Count > 0) logging.Add($"Group {group}: " + String.Join(',', creaturesInGroup));
            else logging.Add($"Group {group}: none");
        }

        public static void LogGroupPositions(int numberofGroups, List<string> logging, params List<Creature>[] teams)
        {
            logging.Add("--------------------------");
            for (int i = 1; i <= numberofGroups; i++) LogGroupPosition(i, logging, teams);
            logging.Add("--------------------------");
        }

        public static List<string> RunStatistics(int iterations, int roomSize, List<Creature> team1, List<Creature> team2)
        {
            List<string> logging = new List<string>();
            int[] results = new int[3];

            int consensuses = 0;
            decimal lastNumber = 0;
            int i = 0;
            while (i++ < iterations)
            {
                (int result, List<string> logs) = Run(roomSize == 0 ? Dice.Random(4) /* default: 4 size */ : roomSize, team1, team2);
                results[result] += 1;
                if (i % 1000 == 0)
                {
                    decimal thisNumber = (decimal)results[1] / ((decimal)results[2] + (decimal)results[1] + (decimal)0.00001);

                    if (Math.Abs(thisNumber - lastNumber) < (decimal)0.004 || lastNumber == 0) consensuses++;
                    else consensuses = 0;

                    lastNumber = (decimal)results[1] / ((decimal)results[2] + (decimal)results[1] + (decimal)0.00001);

                    logging.Add($"Team1 wins: {results[1]} vs Team2 wins: {results[2]}  {thisNumber}");

                    if (consensuses > 2)
                    {
                        logging.Add($"CONSENSUS FOUND! Team1 wins {Math.Round(thisNumber*100,1)}% of the time. {results[1]} vs {results[2]}");
                        break;
                    }
                }
            }

            return logging;

            
        }

        public static (int result,List<string> logs) Run(int numberOfGroups, List<Creature> team1, List<Creature> team2)
        {
            List<string> logging = new List<string>() { "Initial Positions" };

            // deep cloning lists and resetting creatures (hp + random group position) is necessary so references do not carry to future iterations
            List<Creature> team1Clone = team1.Select(x => x.ResetCreature(numberOfGroups)).ToList();
            List<Creature> team2Clone = team2.Select(x => x.ResetCreature(numberOfGroups)).ToList();

            LogGroupPositions(numberOfGroups, logging, team1Clone, team2Clone);

            int rounds = 0;
            while (team1Clone.Count > 0 && team2Clone.Count > 0 && ++rounds < 100)
            {
                logging.Add($"Round: {rounds} of combat.");

                team1Clone.ForEach(creature =>
                {

                    if(creature != null) logging.Add(ProcessTurn(creature, team2Clone, numberOfGroups));
                    LogGroupPositions(numberOfGroups, logging, team1Clone, team2Clone);
                    RemoveDeadCreatures(logging, team2Clone);
                });
                team2Clone.ForEach(creature =>
                {
                    if (creature != null) logging.Add(ProcessTurn(creature, team1Clone, numberOfGroups));
                    LogGroupPositions(numberOfGroups, logging, team1Clone, team2Clone);
                    RemoveDeadCreatures(logging, team1Clone);
                });
            }

            logging.Add("Battle has ended");
            if (team1Clone.Count > 0) return (1,logging);
            if (team2Clone.Count > 0) return (2,logging);
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
                    creature.CurrentGroup = otherChoice.CurrentGroup;
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
                // !! there should be a default natural weapon here
            }

            return null;
        }
    }
}
