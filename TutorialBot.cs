using PenguinGame;

namespace SKILLZ
{
    public class TutorialBot : ISkillzBot
    {
        public void DoTurn(Game game)
        {
            Iceberg destination = null;
            Iceberg[] myIcebergs = game.GetMyIcebergs();
            for (int i = 0; i < myIcebergs.Length; i++)
            {
                Iceberg myIceberg = myIcebergs[i];

                int myPenguinAmount = myIceberg.PenguinAmount;
                if (game.GetNeutralIcebergs().Length > 0)
                {
                    Iceberg[] neturalIcebergs = game.GetNeutralIcebergs();
                    destination = game.GetNeutralIcebergs()[0];
                    for (int j = 1; j < neturalIcebergs.Length; j++)
                    {
                        Iceberg neturalIceberg = neturalIcebergs[j];
                        if (myIcebergs[i].GetTurnsTillArrival(neturalIceberg) < myIcebergs[i].GetTurnsTillArrival(destination))
                        {
                            destination = neturalIceberg;
                        }
                    }
                }
                else
                {
                    destination = game.GetEnemyIcebergs()[0];
                }

                if (!myIceberg.IsIcepital && game.GetMyIcepitalIcebergs()[0].PenguinAmount < 30)
                {
                    destination = game.GetMyIcepitalIcebergs()[0];
                }

                int destinationPenguinAmount = destination.PenguinAmount;
                int Upgrade1 = 0;
                if (myIceberg.CanUpgrade() && Upgrade1 == 0)
                {
                    myIceberg.Upgrade();
                    Upgrade1 += 1;
                }
                if (myPenguinAmount > destinationPenguinAmount * 1.5)
                {
                    if (!myIceberg.IsIcepital)
                    {
                        System.Console.WriteLine(myIceberg + " sends " + (destinationPenguinAmount + 1) + " penguins to " + destination);
                        myIceberg.SendPenguins(destination, destinationPenguinAmount + 1);
                    }
                    else if (myPenguinAmount > 20)
                    {
                        if (game.GetMyIcebergs().Length <= 2)
                        {
                            System.Console.WriteLine(myIceberg + " sends " + (destinationPenguinAmount + 1) + " penguins to " + destination);
                            myIceberg.SendPenguins(destination, destinationPenguinAmount + 1);
                        }
                    }

                }
            }
        }
    }
}