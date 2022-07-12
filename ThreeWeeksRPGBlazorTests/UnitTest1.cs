using Xunit;
using ThreeWeeksRPGBlazor;
using System.Collections.Generic;

namespace ThreeWeeksRPGBlazorTests
{
    public class UnitTest1
    {
        [Fact]
        public void SimulateCreatureCombat()
        {
            var creature1 = new CreatureSimulator.Creature("", 1, 8, 0, 0, 0, 0);
            var creature2 = new CreatureSimulator.Creature("", 1, 8, 0, 0, 0, 0);
            List<string> output = CreatureSimulator.Simulator.RunStatistics(3000, 3, new List<CreatureSimulator.Creature> { creature1 }, new List<CreatureSimulator.Creature> { creature2 });
            //Assert.True(true);
        }

        [Fact]
        public void TestMethod() {
            Xunit.Assert.True(2 == 2);
        }
    }
}