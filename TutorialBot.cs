using PenguinGame;
using System.Linq;
using System.Collections.Generic;

namespace MyBot
{
    public class TutorialBot : ISkillzBot
    {
        int LastTurnProtectionIcepital = -1;
        int[,] IcebergsProtectedArr = new int[100, 2];
        Iceberg[] frendlyIcbergs = new Iceberg[0];
        Iceberg[] notMineIcebergs = new Iceberg[0];
        Iceberg[] lowestPANeturalIceInMySide;
        Iceberg[] icebergsAttackFirstArr;
        Dictionary<Iceberg, int> penguinAmountWithSiegeArr = new Dictionary<Iceberg, int>();
        Dictionary<Iceberg, int> bonusProtections = new Dictionary<Iceberg, int>();
        PenguinGroup[] enemySiegePG = new PenguinGroup[0];
        PenguinGroup[] mySiegePG = new PenguinGroup[0];
        IcebergPlus[] icebergsPlusArr = new IcebergPlus[0];
        //[0] => Attack || [1] => SendSiege
        Dictionary<Iceberg, bool[]> alreadyActedPlus = new Dictionary<Iceberg, bool[]>();
        Dictionary<PenguinGroup, int> speedtopg = new Dictionary<PenguinGroup, int>();
        Dictionary<Iceberg, int[]> icebergsWillBeUnderSiege = new Dictionary<Iceberg, int[]>();
        bool protectIcepital = false;
        bool protectionBonusOn = false;
        int lowestNumNetural;
        Iceberg cloestIceToClone;
        double multiplyAttack;
        int myIcepitalBonus;
        Iceberg myIcepital;
        bool SendToCloseToClone = false;
        int isProtected = 1000000;
        int sumOfMypenguin;
        int sumOfEnemypenguin;
        int sumUpgradeMyIcebergs;
        int sumUpgradeEnemyIcebergs;

        Dictionary<int, int[]> turnsTillArrivalRealDistacneArr = new Dictionary<int, int[]>();
        protected Game game;

        public void DoTurn(Game game)
        {
            this.game = game;
            Iceberg[] myicebergsreal = new Iceberg[0];
            Iceberg[] enemtrealicebergs = new Iceberg[0];
            penguinAmountWithSiegeArr = new Dictionary<Iceberg, int>();


            if (game.GetEnemyIcepitalIcebergs().Length > 0)
            {
                TurnsTillArrivalRealDistacne();

                icebergsPlusArr = CreateIcebergPlusArr(game.GetAllIcebergs());

                /*foreach(PenguinGroup pg in game.GetMyPenguinGroups())
                {
                    if(pg.IsSiegeGroup)
                    {
                        game.Debug(pg);
                        game.Debug(GetDistacnePGtoIceberg(pg));
                    }
                }*/


                /*foreach(Iceberg i in icebergsWillBeUnderSiege.Keys)
                {
                    game.Debug(i);
                    game.Debug(icebergsWillBeUnderSiege[i][0]);
                    game.Debug(icebergsWillBeUnderSiege[i][1]);
                    game.Debug(" ");
                }*/

                /*foreach(IcebergPlus ice in icebergsPlusArr) 
                {
                    if(ice !=null) 
                    {
                        string text="";

                        for (int n=0; n <=ice.GetLastTurn(); n++) 
                        {
                            text += ice.GetPAByTurn(n).ToString() + "," + ice.GetSiegeByTurn(n).ToString() + "," + ice.GetOwnerByTurn(n).ToString()+" | ";
                        }
                        
                        game.Debug(ice);
                        game.Debug(text);
                        game.Debug("isProtected:" + ice.IsProtected());
                        int[] minPA=ice.MinPAarr(ice.GetIceberg().Owner);
                        game.Debug("MinPA: " + minPA[0] + ", " + minPA[1]);
                        game.Debug("__________________________________________");
                    }
                }*/

                IcebergsProtectedArr = GetIfprotectedIcbergs(game.GetAllIcebergs());
                //Create Variables
                sumOfMypenguin = SumOfpenguin(game.GetMyself());
                sumOfEnemypenguin = SumOfpenguin(game.GetEnemy());

                CreateAlreadyActedPlusArr();
                CreateIcebergsWillBeUnderSiege();
                myIcepitalBonus = SumOfpenguin(game.GetEnemyIcebergs(), true) + 1;
                if (sumOfEnemypenguin >= 30) protectionBonusOn = true;
                if (!protectionBonusOn) myIcepitalBonus = 0;

                myIcepital = game.GetMyIcepitalIcebergs()[0];
                frendlyIcbergs = SortIcebergsByPAmount(game.GetMyIcebergs());
                notMineIcebergs = SortIcebergsByPAmount(game.GetNeutralIcebergs().Concat(game.GetEnemyIcebergs()).ToArray());

                if (game.GetEnemyIcebergs().Length == 1 && game.GetMyIcebergs().Length > 2 && game.GetEnemyIcebergs()[0].PenguinAmount > 20) alreadyActedPlus[myIcepital] = new bool[2] { true, true };

                foreach (Iceberg i in frendlyIcbergs)
                {
                    sumUpgradeMyIcebergs += i.Level;
                }

                foreach (Iceberg i in game.GetEnemyIcebergs())
                {
                    sumUpgradeEnemyIcebergs += i.Level;
                }


                if (game.Turn == 1)
                {
                    lowestPANeturalIceInMySide = LowestPenguinAmount(DistanceFromIcberg(game.GetNeutralIcebergs(), myIcepital, 20));
                    cloestIceToClone = findClosestToClone(game.GetAllIcebergs());
                    lowestNumNetural = lowestPANeturalIceInMySide[0].PenguinAmount;
                    icebergsAttackFirstArr = GetIcebergsAttackFirst();
                }

                //make arr with all enemySiegePG
                enemySiegePG = new PenguinGroup[0];

                foreach (PenguinGroup pg in game.GetEnemyPenguinGroups())
                {
                    /*game.Debug(pg);
                    game.Debug("pg.CurrentSpeed: " + pg.CurrentSpeed);
                    game.Debug("GetDistacnePGtoIceberg(pg): " + GetDistacnePGtoIceberg(pg));
                    game.Debug(pg.Source.GetTurnsTillArrival(pg.Destination));*/
                    if (pg.IsSiegeGroup) enemySiegePG = AddItemToArr(enemySiegePG, pg);
                }

                //make arr with all mySiegePG
                mySiegePG = new PenguinGroup[0];

                foreach (PenguinGroup pg in game.GetMyPenguinGroups())
                {
                    if (pg.IsSiegeGroup) mySiegePG = AddItemToArr(mySiegePG, pg);
                }

                foreach (Iceberg iceberg in game.GetAllIcebergs())
                {
                    if (icebergsPlusArr[iceberg.UniqueId].GetOwnerByTurn(99999) == game.GetMyself())
                    {
                        myicebergsreal = AddItemToArr(myicebergsreal, iceberg);
                    }
                    if (icebergsPlusArr[iceberg.UniqueId].GetOwnerByTurn(99999) == game.GetEnemy())
                    {
                        enemtrealicebergs = AddItemToArr(enemtrealicebergs, iceberg);
                    }

                }
                //-----------------------------------------------------------------------------------------
                if (IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && game.GetTimeRemaining() > -90) ProtectionIcepital2();

                //DOOMSDAYattack
                if (game.GetTimeRemaining() > -90) DOOMSDAYattack2();
                //-----------------------------------------------------------------------------------------

                //-----------------------------------------------------------------------------------------
                if (IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && game.GetTimeRemaining() > -90)
                {
                    //Protection
                    Protection();
                }

                //-----------------------------------------------------------------------------------------
                if (IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && game.GetTimeRemaining() > -90) HelpOthers();

                if (game.Turn < 16)
                {
                    if (IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && game.GetTimeRemaining() > -90) SendToSetSiege();
                    //-----------------------------------------------------------------------------------------
                    if (game.Turn > 5 && IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && (frendlyIcbergs.Length < 5 || frendlyIcbergs.Length <= game.GetEnemyIcebergs().Length || sumOfMypenguin > sumOfEnemypenguin * 2.2 || NeturalTargets(game.GetEnemy()).Length > 0) && game.GetTimeRemaining() > -90)
                    {
                        //Attack
                        Attack2();
                    }

                    if (IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && (sumOfMypenguin < sumOfEnemypenguin * 1.3 || sumOfEnemypenguin <= 100) && game.GetTimeRemaining() > -90)
                    {
                        //SendToClone
                        SendToClone();
                    }
                }

                else
                {

                    if (game.Turn > 5 && IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && (frendlyIcbergs.Length < 5 || frendlyIcbergs.Length <= game.GetEnemyIcebergs().Length || sumOfMypenguin > sumOfEnemypenguin * 2.2 || NeturalTargets(game.GetEnemy()).Length > 0) && game.GetTimeRemaining() > -90)
                    {
                        //Attack
                        Attack2();
                    }

                    if (IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && (sumOfMypenguin < sumOfEnemypenguin * 1.3 || sumOfMypenguin <= 100) && game.GetTimeRemaining() > -90)
                    {
                        //SendToClone
                        SendToClone();
                    }
                    if (IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && game.GetTimeRemaining() > -90) SendToSetSiege();
                    //-----------------------------------------------------------------------------------------
                }

                //-----------------------------------------------------------------------------------------
                if (game.Turn > 5 && frendlyIcbergs.Length > 0 && IcebergAlreadyActedArr(frendlyIcbergs, 0).Length != 0 &&
                    IcebergAlreadyActedArr(frendlyIcbergs, 1).Length != 0 && game.GetTimeRemaining() > -90)
                {
                    //Upgrade
                    Upgrade();
                }

                if (game.GetTimeRemaining() > -90) AcceleratePGAttack();
                //-----------------------------------------------------------------------------------------
            }
        }

        //------------------------------------------------------------------------------------------
        public int FindBestSpeedsIcepital(int icebergIndex, int[] currentSpeeds, Iceberg[] sourceIcebergs, int[] bestSpeeds, Iceberg targetIceberg, int[] sumsofattack = null, Dictionary<Iceberg, PGbestSpeed> pgAmountAndSpeed = null)
        {
            if (icebergIndex >= sourceIcebergs.Length)
            {
                IcebergPlus ice = DataOnIceberg(targetIceberg, 0, -1, 0, null, pgAmountAndSpeed);

                int penguinDifference = ice.GetPAByTurn(ice.MinPAarr()[0]);

                System.Array.Copy(currentSpeeds, bestSpeeds, currentSpeeds.Length);
                return penguinDifference;
            }

            int bonus = 0;
            if (sumsofattack == null) sumsofattack = new int[10000];
            int minPenguinDifference = int.MaxValue;

            for (int speed = 1; speed <= 8; speed *= 2)
            {
                int penguinAmount = sourceIcebergs[icebergIndex].PenguinAmount;
                int penguinAmountToSend = (int)PenguinAmountAfterAccelerate(penguinAmount, speed);

                PGbestSpeed pg = new PGbestSpeed(game.GetEnemy(), penguinAmountToSend, GetTurnsTillArrivalAccelerat(sourceIcebergs[icebergIndex].GetTurnsTillArrival(targetIceberg), speed) + 1, speed);

                if (pgAmountAndSpeed != null)
                {
                    int icebergId = sourceIcebergs[icebergIndex].UniqueId;

                    if (pgAmountAndSpeed.ContainsKey(sourceIcebergs[icebergIndex]))
                    {
                        pgAmountAndSpeed[sourceIcebergs[icebergIndex]] = pg;
                    }
                    else
                    {
                        pgAmountAndSpeed.Add(sourceIcebergs[icebergIndex], pg);
                    }
                }
                else
                {
                    pgAmountAndSpeed = new Dictionary<Iceberg, PGbestSpeed>();
                    pgAmountAndSpeed.Add(sourceIcebergs[icebergIndex], pg);
                }

                sumsofattack[sourceIcebergs[icebergIndex].UniqueId] = penguinAmountToSend;
                currentSpeeds[sourceIcebergs[icebergIndex].UniqueId] = speed;

                int penguinDifference = FindBestSpeedsIcepital(icebergIndex + 1, currentSpeeds, sourceIcebergs, bestSpeeds, targetIceberg, sumsofattack, pgAmountAndSpeed);
                minPenguinDifference = System.Math.Min(minPenguinDifference, penguinDifference);
            }

            return minPenguinDifference;
        }
        public bool Sendattackbestspeed()
        {

            Iceberg[] enemyIcebergs = game.GetEnemyIcebergs().Concat(NeturalTargets(game.GetEnemy())).ToArray();
            Iceberg[] myIcbergs = game.GetMyIcebergs();
            foreach (Iceberg i in enemyIcebergs)
            {
                if (icebergsPlusArr[i.UniqueId].GetOwnerByTurn(icebergsPlusArr[i.UniqueId].GetLastTurn()) == game.GetMyself()) enemyIcebergs = RemoveItemFromArr(enemyIcebergs, i);
            }

            enemyIcebergs = SortIcebergsByPAmount(enemyIcebergs);

            foreach (Iceberg i in myIcbergs)
            {
                if (icebergsPlusArr[i.UniqueId].GetOwnerByTurn(icebergsPlusArr[i.UniqueId].GetLastTurn()) != game.GetMyself() || i.IsUnderSiege || alreadyActedPlus[i][0]) myIcbergs = RemoveItemFromArr(myIcbergs, i);
            }

            Dictionary<int, int[]> targetSpeeds = FindBestSpeedsToConquerTargets(myIcbergs, enemyIcebergs);

            int[] bestspeed = null;


            foreach (Iceberg enemyIce in enemyIcebergs)
            {
                //game.Debug(enemyIce);
                bestspeed = targetSpeeds[enemyIce.UniqueId];

                foreach (Iceberg myIce in myIcbergs)
                {
                    int bounus = 0;
                    if (myIce.IsIcepital) bounus = myIcepitalBonus;
                    //game.Debug(bestspeed[myIce.UniqueId]);

                    if (bestspeed[myIce.UniqueId] > 0 && icebergsPlusArr[myIce.UniqueId].MinPAarr(myIce.Owner)[1] <= myIce.PenguinAmount)
                    {
                        myIce.SendPenguins(enemyIce, myIce.PenguinAmount - bounus);
                        game.Debug("Sendattackbestspeed");
                    }
                }
            }

            return true;
        }

        public bool Protection()
        {
            Iceberg destination;
            bool canSend;
            int attackPAmount = 0;
            int extraBonus = 0;
            int bonusProtection = (int)(SumOfpenguin(game.GetEnemyIcebergs()) / 10.0 + 0.99);
            if (game.GetMyIcebergs().Length + NeturalTargets(game.GetMyself()).Length < 4 && game.Turn < 50)
            {
                bonusProtection = 0;
            }


            Iceberg[] myIcbergs = new Iceberg[0];
            Iceberg[] myIcbergsWithSiege = new Iceberg[0];
            int[] myIcbergsAttacks = new int[0];
            int[] myIcbergsAttacksAfterSpeed = new int[0];

            //Icebergs need Protection
            Iceberg[] destinationIcebergs = GetNotProtectedIcbergs(game.GetMyIcebergs());
            destinationIcebergs = RemoveItemFromArr(destinationIcebergs, myIcepital);
            destinationIcebergs = SortIcebergsByPAmount(destinationIcebergs);

            //All Icebergs that can help
            Iceberg[] canSendIcebergs = IcebergAlreadyActedArr(GetProtectedIcbergs(game.GetMyIcebergs()), 1);

            Dictionary<Iceberg, int> bonusProtectionIcebergs = new Dictionary<Iceberg, int>();


            foreach (Iceberg myIce in canSendIcebergs)
            {
                bonusProtectionIcebergs.Add(myIce, CaculateBonusProtection(myIce));
                extraBonus = bonusProtectionIcebergs[myIce];
                if (myIce.PenguinAmount - PenguinAmountWithSiege(myIce) > sumOfMypenguin / 5.0 || PenguinAmountWithSiege(myIce) - extraBonus <= 0 ||
                    myIce.PenguinAmount - PenguinAmountWithSiege(myIce) + extraBonus >= icebergsPlusArr[myIce.UniqueId].MinPAarr(myIce.Owner)[1])
                {
                    canSendIcebergs = RemoveItemFromArr(canSendIcebergs, myIce);
                }
            }


            for (int i = 0; i < destinationIcebergs.Length; i++)
            {
                //game.Debug("___________________________________________________________________________________________________________");
                //game.Debug(destinationIcebergs[i]);
                if (game.GetTimeRemaining() < -90) return false;
                int len = 0;
                myIcbergs = new Iceberg[0];
                myIcbergsWithSiege = new Iceberg[0];
                destination = destinationIcebergs[i];
                int conquerTurn = IcebergsProtectedArr[destination.UniqueId, 0];
                attackPAmount = IcebergsProtectedArr[destination.UniqueId, 1];


                canSendIcebergs = IcebergAlreadyActedArr(canSendIcebergs, 1);
                foreach (Iceberg ice in canSendIcebergs)
                {
                    if (!ice.IsUnderSiege)
                    {
                        myIcbergs = AddItemToArr(myIcbergs, ice);
                    }
                    else
                    {
                        myIcbergsWithSiege = AddItemToArr(myIcbergsWithSiege, ice);
                    }
                }

                myIcbergs = DistanceFromIcberg(myIcbergs, destination, 100);
                myIcbergsWithSiege = SortBySiegeSize(myIcbergsWithSiege);

                myIcbergs = myIcbergs.Concat(myIcbergsWithSiege).ToArray();

                //if(myIcbergs!=null)
                //game.Debug(myIcbergs.Length);
                canSend = false;

                if (myIcbergs != null && myIcbergs.Length > 0)
                {
                    myIcbergsAttacks = new int[0];
                    myIcbergsAttacksAfterSpeed = new int[0];
                    int lastTurn = 0;
                    int currentTurn = 0;
                    int minTurn = 0;
                    int bestSpeed = 0;
                    int maxAttackIce = 0;
                    int attackIceWithSpeed = 0;

                    if (destination.IsUnderSiege && destination.PenguinAmount - PenguinAmountWithSiege(destination, true) > destination.Level)
                    {
                        minTurn = conquerTurn;
                    }

                    foreach (Iceberg iceberg in myIcbergs)
                    {
                        if (iceberg.GetTurnsTillArrival(destination) <= minTurn)
                        {
                            myIcbergs = RemoveItemFromArr(myIcbergs, iceberg);
                            continue;
                        }

                        len++;
                        extraBonus = bonusProtectionIcebergs[iceberg];
                        maxAttackIce = icebergsPlusArr[iceberg.UniqueId].MinPAarr(iceberg.Owner)[1] - extraBonus;
                        bestSpeed = TheBestSpeed(destination, destination.GetTurnsTillArrival(iceberg), maxAttackIce, iceberg.Owner);
                        currentTurn = GetTurnsTillArrivalAccelerat(destination.GetTurnsTillArrival(iceberg), bestSpeed, 1, true);
                        attackIceWithSpeed = PenguinAmountAfterAccelerate(maxAttackIce - iceberg.PenguinAmount + PenguinAmountWithSiege(iceberg), bestSpeed);

                        if (currentTurn > lastTurn)
                        {
                            lastTurn = currentTurn;
                            if (lastTurn >= conquerTurn)
                            {
                                attackPAmount = icebergsPlusArr[destination.UniqueId].GetPAByTurn(lastTurn) + 1;
                            }
                        }

                        myIcbergsAttacks = AddItemToArr(myIcbergsAttacks, maxAttackIce);
                        myIcbergsAttacksAfterSpeed = AddItemToArr(myIcbergsAttacksAfterSpeed, attackIceWithSpeed);

                        int sum = myIcbergsAttacksAfterSpeed.Sum();
                        if (sum >= attackPAmount)
                        {
                            int maxBonusAttack = (int)(SumOfpenguin(game.GetEnemyIcebergs()) / 5.0 + 0.99);
                            if (sum > attackPAmount + maxBonusAttack)
                            {
                                myIcbergsAttacksAfterSpeed[len - 1] -= sum - attackPAmount - maxBonusAttack;

                                int bestPenguinAmount = PenguinAmountNeededForAccelerate(myIcbergsAttacksAfterSpeed[len - 1], bestSpeed) + myIcbergs[len - 1].PenguinAmount - PenguinAmountWithSiege(myIcbergs[len - 1], true);

                                myIcbergsAttacks[len - 1] = bestPenguinAmount;
                            }
                            canSend = true;
                            game.Debug("attackPAmount: " + attackPAmount);
                            game.Debug("myIcbergsAttacksAfterSpeed: " + myIcbergsAttacksAfterSpeed.Sum());
                            game.Debug("attackPAmount: " + attackPAmount);
                            game.Debug("lastTurn: " + lastTurn);


                            break;
                        }
                    }

                    //SendPenguins

                }

                if (canSend)
                {
                    for (int index = 0; index < len; index++)
                    {
                        if (myIcbergsAttacks[index] > 0)
                        {
                            myIcbergs[index].SendPenguins(destination, myIcbergsAttacks[index]);
                            IcebergPlay(myIcbergs[index]);
                            game.Debug("Protection");
                        }
                    }
                }
            }

            return true;

        }

        public bool ProtectionIcepital2()
        {
            if (game.GetTimeRemaining() < -90) return false;
            int len = 0;
            int bonus = 0;
            bool limitPA = false;
            Iceberg[] myIcbergs = new Iceberg[0];
            Iceberg[] myIcbergsWithSiege = new Iceberg[0];
            int[] myIcbergsAttacks = new int[0];
            int[] myIcbergsAttacksAfterSpeed = new int[0];
            Iceberg destination = myIcepital;
            int conquerTurn = IcebergsProtectedArr[destination.UniqueId, 0];
            int attackPAmount = IcebergsProtectedArr[destination.UniqueId, 1] + CaculateBonusProtection(myIcepital);

            if (conquerTurn == isProtected)
            {
                if (icebergsPlusArr[myIcepital.UniqueId].GetPAByTurn(icebergsPlusArr[myIcepital.UniqueId].GetLastTurn()) > CaculateBonusProtection(myIcepital) ||
                    game.Turn < 30 || game.GetMyIcebergs().Length <= game.GetEnemyIcebergs().Length + 1 || myIcepital.IsUnderSiege ||
                        sumOfEnemypenguin * 1.2 > sumOfMypenguin || sumUpgradeMyIcebergs < sumUpgradeEnemyIcebergs)
                    return false;
                else
                {
                    attackPAmount = CaculateBonusProtection(myIcepital);
                    conquerTurn = 0;
                    limitPA = true;
                }
            }

            foreach (Iceberg ice in IcebergAlreadyActedArr(game.GetMyIcebergs(), 1))
            {
                if (ice != myIcepital && !ice.IsUnderSiege)
                {
                    if (!ice.IsUnderSiege)
                    {
                        myIcbergs = AddItemToArr(myIcbergs, ice);
                    }
                    else
                    {
                        myIcbergsWithSiege = AddItemToArr(myIcbergsWithSiege, ice);
                    }
                }
            }

            myIcbergs = DistanceFromIcberg(myIcbergs, destination, 100);
            myIcbergsWithSiege = SortBySiegeSize(myIcbergsWithSiege);

            myIcbergs = myIcbergs.Concat(myIcbergsWithSiege).ToArray();

            //if(myIcbergs!=null)
            //game.Debug(myIcbergs.Length);
            bool canSend = false;

            if (myIcbergs != null && myIcbergs.Length > 0)
            {
                int lastTurn = 0;
                int currentTurn = 0;
                int minTurn = 0;
                int bestSpeed = 0;
                int maxAttackIce = 0;
                int attackIceWithSpeed = 0;

                if (destination.IsUnderSiege && destination.PenguinAmount - PenguinAmountWithSiege(destination, true) > destination.Level)
                {
                    minTurn = conquerTurn;
                }

                foreach (Iceberg iceberg in myIcbergs)
                {
                    if (limitPA) bonus = CaculateBonusProtection(iceberg);
                    if (iceberg.GetTurnsTillArrival(destination) <= minTurn || PenguinAmountWithSiege(iceberg) - bonus <= 0)
                    {
                        myIcbergs = RemoveItemFromArr(myIcbergs, iceberg);
                        continue;
                    }

                    len++;
                    maxAttackIce = icebergsPlusArr[iceberg.UniqueId].MinPAarr(iceberg.Owner)[1] - bonus;
                    bestSpeed = TheBestSpeed(destination, destination.GetTurnsTillArrival(iceberg), maxAttackIce, iceberg.Owner);
                    currentTurn = GetTurnsTillArrivalAccelerat(destination.GetTurnsTillArrival(iceberg), bestSpeed, 1, true);
                    attackIceWithSpeed = PenguinAmountAfterAccelerate(maxAttackIce - iceberg.PenguinAmount + PenguinAmountWithSiege(iceberg), bestSpeed);

                    myIcbergsAttacks = AddItemToArr(myIcbergsAttacks, maxAttackIce);
                    myIcbergsAttacksAfterSpeed = AddItemToArr(myIcbergsAttacksAfterSpeed, attackIceWithSpeed);

                    int sum = myIcbergsAttacksAfterSpeed.Sum();
                    if (sum >= attackPAmount)
                    {
                        if (limitPA && sum > attackPAmount)
                        {
                            myIcbergsAttacksAfterSpeed[len - 1] -= sum - attackPAmount;

                            int bestPenguinAmount = PenguinAmountNeededForAccelerate(myIcbergsAttacksAfterSpeed[len - 1], bestSpeed) + myIcbergs[len - 1].PenguinAmount - PenguinAmountWithSiege(myIcbergs[len - 1], true);

                            myIcbergsAttacks[len - 1] = bestPenguinAmount;
                        }
                        break;
                    }
                }

                //SendPenguins

            }

            for (int index = 0; index < len; index++)
            {
                if (myIcbergsAttacks[index] > 0)
                {
                    myIcbergs[index].SendPenguins(destination, myIcbergsAttacks[index]);
                    IcebergPlay(myIcbergs[index]);
                    game.Debug("ProtectionIcepital2");
                    //game.Debug("attackPAmount " + attackPAmount);
                }
            }

            return true;
        }

        public bool Upgrade()
        {
            bool icebergCanUpgrade;
            Iceberg[] icebergs = IcebergAlreadyActedArr(game.GetMyIcebergs(), 0).Concat(IcebergAlreadyActedArr(game.GetMyIcebergs(), 1)).Distinct().ToArray();

            foreach (Iceberg i in icebergs)
            {
                int bonus = CaculateBonusProtection(i);

                //CanUpgrade?
                icebergCanUpgrade = IcebergCanUpgrade(i, bonus);

                if (icebergCanUpgrade)
                {
                    i.Upgrade();
                    IcebergPlay(i, true);
                    game.Debug("Upgrade");
                }
            }

            return true;
        }

        public bool Attack2()
        {
            int sumAttacks = 0;
            int attackPAmount = 0;
            int oneIcebergAttack = 0;
            int extraBonus = 0;
            int[] myIcbergsAttacks = new int[0];
            int[] myIcbergsAttacksAfterSpeed = new int[0];
            IcebergAttack[] icebergsData = new IcebergAttack[0];
            IcebergAttack icebergData;
            int bonusProtection;

            Dictionary<Iceberg, int> bonusProtectionIcebergs = new Dictionary<Iceberg, int>();

            int maxBonusAttack = (int)(SumOfpenguin(game.GetEnemyIcebergs()) / 5.0 + 0.99);
            /*if(game.GetMyIcebergs().Length == 1 && NeturalTargets(game.GetMyself()).Length == 0)
            {
                maxBonusAttack = 0;
            }*/

            Iceberg[] dangerousEnemyIcebergs = DistanceFromIcberg(game.GetEnemyIcebergs().Concat(NeturalTargets(game.GetEnemy())).ToArray(), myIcepital, 8);

            Iceberg[] myIcbergs = new Iceberg[0];
            Iceberg[] myIcbergsCanUse = new Iceberg[0];
            Iceberg[] myIcbergsWithSiege = new Iceberg[0];
            Iceberg destination = null;
            bool canSend = false;
            Iceberg[] destinationIcebergs = new Iceberg[0];
            myIcbergsCanUse = game.GetMyIcebergs();

            foreach (Iceberg myIce in myIcbergsCanUse)
            {
                bonusProtectionIcebergs.Add(myIce, CaculateBonusProtection(myIce));
                bonusProtection = bonusProtectionIcebergs[myIce];

                if (/*(DistanceFromIcberg(game.GetEnemyIcebergs(), myIce, 8).Length > 0 && DistanceFromIcberg(game.GetMyIcebergs(), myIce, 15).Length == 0) ||*/ myIce.PenguinAmount - PenguinAmountWithSiege(myIce) > sumOfMypenguin / 5.0 || IcebergsProtectedArr[myIce.UniqueId, 0] < isProtected || PenguinAmountWithSiege(myIce) - bonusProtection <= 0 ||
                    myIce.PenguinAmount - PenguinAmountWithSiege(myIce) + bonusProtection >= icebergsPlusArr[myIce.UniqueId].MinPAarr(myIce.Owner)[1])
                {
                    myIcbergsCanUse = RemoveItemFromArr(myIcbergsCanUse, myIce);
                }
            }

            //Choose the destinationArr
            if ((game.Turn > 10 || game.GetEnemyIcebergs().Length + NeturalTargets(game.GetEnemy(), true).Length > 1 || game.GetMyIcebergs().Length + NeturalTargets(game.GetMyself()).Length == 1) &&
                (frendlyIcbergs.Length < 5 || frendlyIcbergs.Length <= game.GetEnemyIcebergs().Length || sumOfMypenguin > sumOfEnemypenguin * 2.2) &&
                    (dangerousEnemyIcebergs.Length == 0))
            {
                Iceberg[] tmp = DistanceFromIcberg(game.GetNeutralIcebergs(), myIcepital, 100);
                Iceberg[] icebergsInMySide = new Iceberg[(int)(game.GetNeutralIcebergs().Length / 2)];

                for (int i = 0; i < icebergsInMySide.Length; i++)
                {
                    icebergsInMySide[i] = tmp[i];
                }

                if (NeturalTargets(game.GetMyself()).Length == 0 || game.Turn > 6) destinationIcebergs = icebergsAttackFirstArr;

                if (game.Turn > 5)
                {
                    destinationIcebergs = destinationIcebergs.Concat(NeturalTargets(game.GetEnemy())).ToArray();
                }

                else
                {
                    bool find = false;

                    foreach (Iceberg enemyI in NeturalTargets(game.GetEnemy()))
                    {
                        foreach (Iceberg myI in NeturalTargets(game.GetMyself()))
                        {
                            if (enemyI == myI)
                            {
                                destinationIcebergs = AddItemToArr(destinationIcebergs, myI);
                                find = true;
                                break;
                            }
                        }

                        if (find) break;
                    }
                }


                if ((NeturalTargets(game.GetMyself()).Length == 0 || game.Turn > 6) && (icebergsAttackFirstArr.Length == 0 || game.Turn >= 20 || cloestIceToClone.GetTurnsTillArrival(myIcepital) > cloestIceToClone.GetTurnsTillArrival(game.GetEnemyIcepitalIcebergs()[0])))
                {
                    destinationIcebergs = destinationIcebergs.Concat(game.GetEnemyIcebergs()).ToArray();
                    if (game.GetMyIcebergs().Length + NeturalTargets(game.GetMyself()).Length <= game.GetEnemyIcebergs().Length + NeturalTargets(game.GetEnemy()).Length + 1) destinationIcebergs = destinationIcebergs.Concat(icebergsInMySide).ToArray();

                    destinationIcebergs = destinationIcebergs.Distinct().ToArray();
                }

                foreach (Iceberg i in destinationIcebergs)
                {
                    if (icebergsPlusArr[i.UniqueId].IsProtected() < isProtected /*|| (!i.IsUnderSiege && !icebergsWillBeUnderSiege.ContainsKey(i)))*/ && i.Owner != game.GetNeutral()) destinationIcebergs = RemoveItemFromArr(destinationIcebergs, i);
                }
            }
            else
            {
                destinationIcebergs = dangerousEnemyIcebergs.Concat(NeturalTargets(game.GetEnemy())).ToArray();
                destinationIcebergs = destinationIcebergs.Distinct().ToArray();
            }

            foreach (Iceberg i in destinationIcebergs)
            {
                if ((i.Owner != game.GetNeutral() && icebergsPlusArr[i.UniqueId].GetOwnerByTurn(icebergsPlusArr[i.UniqueId].GetLastTurn()) != i.Owner) ||
                    (i.Owner == game.GetNeutral() && icebergsPlusArr[i.UniqueId].GetOwnerByTurn(icebergsPlusArr[i.UniqueId].GetLastTurn()) == game.GetMyself()) || i.IsIcepital)
                {
                    destinationIcebergs = RemoveItemFromArr(destinationIcebergs, i);
                }
            }

            destinationIcebergs = SortIcebergsByPAmount(destinationIcebergs);
            bool changeBonus = true;

            for (int i = 0; i < destinationIcebergs.Length; i++)
            {
                game.Debug(destinationIcebergs[i]);
                icebergData = CreateIcebergAttack(destinationIcebergs[i], myIcbergsCanUse, bonusProtectionIcebergs, maxBonusAttack);
                if (icebergData != null)
                {
                    icebergsData = AddItemToArr(icebergsData, icebergData);
                }
            }

            IcebergAttack[] firstIcebergsData = new IcebergAttack[0];
            foreach (IcebergAttack iceAttack in icebergsData)
            {
                if (IcebergsProtectedArr[iceAttack.GetIceberg().UniqueId, 0] != isProtected)
                {
                    firstIcebergsData = AddItemToArr(firstIcebergsData, iceAttack);
                    icebergsData = RemoveItemFromArr(icebergsData, iceAttack);
                }
            }

            firstIcebergsData = SortIcebergAttackBySum(firstIcebergsData);
            icebergsData = SortIcebergAttackBySum(icebergsData);

            icebergsData = firstIcebergsData.Concat(icebergsData).ToArray();
            if (icebergsData.Length > 0)
            {
                //game.Debug("maxBonusAttack: " + maxBonusAttack);
                bool onlyOne = false;
                if (icebergsData.Length == 1) onlyOne = true;

                foreach (IcebergAttack iceAttack in icebergsData)
                {
                    //game.Debug(iceAttack.GetIceberg());
                    if (onlyOne)
                    {
                        onlyOne = false;
                        icebergData = iceAttack;
                    }
                    else
                    {
                        if (game.Turn < 30 && firstIcebergsData.Length <= 0)
                            icebergData = CreateIcebergAttack(iceAttack.GetIceberg(), myIcbergsCanUse, bonusProtectionIcebergs, 0);
                        else
                            icebergData = CreateIcebergAttack(iceAttack.GetIceberg(), myIcbergsCanUse, bonusProtectionIcebergs, maxBonusAttack);
                    }

                    if (icebergData != null)
                    {
                        foreach (Iceberg i in icebergData.attackAmount.Keys)
                        {
                            icebergsAttackFirstArr = RemoveItemFromArr(icebergsAttackFirstArr, icebergData.GetIceberg());
                            i.SendPenguins(icebergData.GetIceberg(), icebergData.attackAmount[i]);
                            IcebergPlay(i);
                            game.Debug("RegularAttack");
                        }
                    }
                }
            }

            return true;
        }

        public IcebergAttack CreateIcebergAttack(Iceberg destination, Iceberg[] myIcbergsCanUse, Dictionary<Iceberg, int> bonusProtectionIcebergs, int maxBonusAttack)
        {
            if (game.GetTimeRemaining() < -90) return null;
            int len = 0;
            Iceberg[] myIcbergs = new Iceberg[0];
            Iceberg[] myIcbergsWithSiege = new Iceberg[0];
            int[] myIcbergsAttacks = new int[0];
            int[] myIcbergsAttacksAfterSpeed = new int[0];
            int bonusProtection = 0;

            foreach (Iceberg ice in myIcbergsCanUse)
            {
                if (!ice.IsUnderSiege)
                {
                    myIcbergs = AddItemToArr(myIcbergs, ice);
                }
                else
                {
                    myIcbergsWithSiege = AddItemToArr(myIcbergsWithSiege, ice);
                }
            }

            myIcbergs = DistanceFromIcberg(myIcbergs, destination, 100);
            myIcbergsWithSiege = SortBySiegeSize(myIcbergsWithSiege);

            myIcbergs = myIcbergs.Concat(myIcbergsWithSiege).ToArray();


            if (destination.Owner == game.GetNeutral() && IcebergsProtectedArr[destination.UniqueId, 0] < isProtected &&
                icebergsPlusArr[destination.UniqueId].GetOwnerByTurn(IcebergsProtectedArr[destination.UniqueId, 0] - 1) != game.GetMyself())
            {
                foreach (Iceberg iceberg in myIcbergs)
                {
                    bonusProtection = bonusProtectionIcebergs[iceberg];

                    if (iceberg.GetTurnsTillArrival(destination) <= IcebergsProtectedArr[destination.UniqueId, 0] ||
                        bonusProtection >= icebergsPlusArr[iceberg.UniqueId].MinPAarr(iceberg.Owner)[1])
                    {
                        myIcbergs = RemoveItemFromArr(myIcbergs, iceberg);
                    }
                }
            }

            bool canSend = false;


            if (myIcbergs != null && myIcbergs.Length > 0)
            {
                int lastTurn = 0;
                int currentTurn = 0;
                int bestSpeed = 0;
                int maxAttackIce = 0;
                int attackIceWithSpeed = 0;
                int attackPAmount = 0;

                foreach (Iceberg iceberg in myIcbergs)
                {
                    bonusProtection = bonusProtectionIcebergs[iceberg];

                    len++;

                    maxAttackIce = icebergsPlusArr[iceberg.UniqueId].MinPAarr(iceberg.Owner)[1] - bonusProtection;
                    bestSpeed = TheBestSpeed(destination, destination.GetTurnsTillArrival(iceberg), maxAttackIce, iceberg.Owner);
                    currentTurn = GetTurnsTillArrivalAccelerat(destination.GetTurnsTillArrival(iceberg), bestSpeed, 1, true);
                    attackIceWithSpeed = PenguinAmountAfterAccelerate(maxAttackIce - iceberg.PenguinAmount + PenguinAmountWithSiege(iceberg), bestSpeed);

                    /*game.Debug(iceberg);
                    game.Debug("bestSpeed: " + bestSpeed);
                    game.Debug(destination.GetTurnsTillArrival(iceberg));
                    game.Debug(currentTurn);*/

                    /*if(destination.Owner != game.GetNeutral() && currentTurn > 10)
                    {
                        len--;
                        myIcbergs = RemoveItemFromArr(myIcbergs, iceberg);
                        continue;
                    }*/

                    if (currentTurn > lastTurn)
                    {
                        lastTurn = currentTurn;
                        if (destination.Owner == game.GetNeutral() && icebergsPlusArr[destination.UniqueId].GetOwnerByTurn(icebergsPlusArr[destination.UniqueId].GetLastTurn()) == game.GetEnemy() &&
                            icebergsPlusArr[destination.UniqueId].GetOwnerByTurn(currentTurn) != game.GetEnemy())
                        {
                            lastTurn = icebergsPlusArr[destination.UniqueId].MinPAarr(game.GetEnemy(), lastTurn)[0];
                            if (lastTurn < 0) lastTurn = currentTurn;
                        }
                        attackPAmount = icebergsPlusArr[destination.UniqueId].GetPAByTurn(lastTurn) + 1;
                        if (game.Turn > 6 && destination.Owner == game.GetNeutral() && (DistanceFromIcberg(game.GetEnemyIcebergs(), destination, 15).Length >= DistanceFromIcberg(game.GetMyIcebergs(), destination, 8).Length || NeturalTargets(game.GetMyself()).Length > 0) && IcebergsProtectedArr[destination.UniqueId, 0] >= isProtected)
                            attackPAmount += 5;
                    }

                    myIcbergsAttacks = AddItemToArr(myIcbergsAttacks, maxAttackIce);
                    myIcbergsAttacksAfterSpeed = AddItemToArr(myIcbergsAttacksAfterSpeed, attackIceWithSpeed);

                    if (myIcbergsAttacksAfterSpeed.Sum() >= attackPAmount)
                    {
                        // game.Debug("maxBonusAttack: " + maxBonusAttack);
                        int sum = myIcbergsAttacksAfterSpeed.Sum();

                        if (sum > attackPAmount + maxBonusAttack)
                        {
                            myIcbergsAttacksAfterSpeed[len - 1] -= sum - (attackPAmount + maxBonusAttack);

                            int bestPenguinAmount = PenguinAmountNeededForAccelerate(myIcbergsAttacksAfterSpeed[len - 1], bestSpeed) + myIcbergs[len - 1].PenguinAmount - PenguinAmountWithSiege(myIcbergs[len - 1]);

                            myIcbergsAttacks[len - 1] = bestPenguinAmount;
                            //if(maxBonusAttack == 0 && iceberg == myIcepital) myIcbergsAttacks[len - 1] = myIcbergs[len -1].PenguinAmount - 4;
                        }
                        canSend = true;
                        /*game.Debug("attackPAmount: " + attackPAmount);
                        game.Debug("myIcbergsAttacksAfterSpeed: " + myIcbergsAttacksAfterSpeed.Sum());*/

                        break;
                    }
                }

                //SendPenguins
            }

            if (canSend)
            {
                Dictionary<Iceberg, int> attackAmountDic = new Dictionary<Iceberg, int>();
                Dictionary<Iceberg, int> attackAmountAfterAccelerateDic = new Dictionary<Iceberg, int>();

                for (int index = 0; index < len; index++)
                {
                    if (myIcbergsAttacks[index] > 0)
                    {
                        attackAmountDic.Add(myIcbergs[index], myIcbergsAttacks[index]);
                        attackAmountAfterAccelerateDic.Add(myIcbergs[index], myIcbergsAttacksAfterSpeed[index]);
                    }
                }

                return new IcebergAttack(destination, attackAmountDic, attackAmountAfterAccelerateDic);
            }
            return null;
        }

        public bool SendToClone()
        {
            foreach (PenguinGroup pg in game.GetMyPenguinGroups())
            {
                if (pg.Destination == game.GetCloneberg() && pg.CurrentSpeed == 1 &&
                    !pg.AlreadyActed && !icebergsWillBeUnderSiege.ContainsKey((Iceberg)pg.Source) &&
                        !((Iceberg)pg.Source).IsUnderSiege && pg.PenguinAmount <= 8 && pg.PenguinAmount > 1)
                {
                    pg.Accelerate();
                    game.Debug("SendToClone");
                    game.Debug(pg);
                }

            }

            bool toClone = true;
            if (game.GetEnemyIcebergs().Length == 1 && NeturalTargets(game.GetEnemy()).Length > 0)
            {
                toClone = false;
            }

            if (toClone)
            {
                Iceberg[] icey = IcebergAlreadyActedArr(DistanceFromIcberg(game.GetMyIcebergs(), game.GetCloneberg(), 10), 1);

                if (icey.Length > 0)
                {
                    int[] icebergProtectedArr;
                    int bonusProtection = 5;

                    foreach (Iceberg i in icey)
                    {
                        bool canSend = true;
                        int icePAwithClone = IcebergPAwithClone(i);
                        foreach (Iceberg ice in DistanceFromIcberg(game.GetMyIcebergs(), i, 10))
                        {
                            if (IcebergsProtectedArr[ice.UniqueId, 0] < isProtected && icePAwithClone <= sumOfMypenguin - icePAwithClone)
                            {
                                canSend = false;
                                break;
                            }
                        }

                        int attackPA = (int)(i.PenguinAmount / 1.3);

                        if (i.IsUnderSiege && canSend)
                        {
                            //if(i.PenguinAmount < (int)(PenguinAmountWithSiege(i) * 1.5)) attackPA+=i.PenguinAmount - PenguinAmountWithSiege(i);
                            //else canSend=false;
                            canSend = false;
                        }

                        if (icebergsWillBeUnderSiege.ContainsKey(i) && canSend)
                        {
                            if (icebergsWillBeUnderSiege[i][1] > sumOfEnemypenguin / 12)
                                canSend = false;
                        }

                        icebergProtectedArr = IcebergProtection(i, attackPA);
                        if (icebergProtectedArr[0] == isProtected && icebergProtectedArr[1] == i.PenguinAmount - attackPA) icebergProtectedArr[1] = bonusProtection + 1;

                        if (canSend && icebergProtectedArr[0] == isProtected && attackPA > 2 && icebergProtectedArr[1] > bonusProtection)
                        {
                            //game.Debug("PenguinAmountWithSiege(i): " + PenguinAmountWithSiege(i));
                            //game.Debug(i);
                            i.SendPenguins(game.GetCloneberg(), attackPA);
                            IcebergPlay(i);
                            game.Debug("SendToClone");
                        }
                    }
                }
            }

            return true;
        }

        public bool SendToSetSiege()
        {
            Iceberg[] myIcebergs = IcebergAlreadyActedArr(SortByPenguinCanSend(frendlyIcbergs), 1);
            /*foreach(Iceberg i in myIcebergs)
            {
                if(DistanceFromIcberg(game.GetEnemyIcebergs(), i, 10).Length > 0)
                    myIcebergs = RemoveItemFromArr(myIcebergs, i);
            }*/
            myIcebergs = myIcebergs.Reverse().ToArray();
            Iceberg[] enemyIcebergs = game.GetEnemyIcebergs();
            int bonus = 0;
            /*if(game.Turn == 1)
            {
                myIcepital.SendPenguinsToSetSiege(game.GetEnemyIcepitalIcebergs()[0], 4);
                alreadyActedPlus[myIcepital][1] = true;
                return true;
            }*/
            foreach (PenguinGroup pg in game.GetEnemyPenguinGroups())
            {
                if (pg.Destination == game.GetCloneberg())
                {
                    if (pg.Source.Owner == game.GetEnemy() && !((Iceberg)(pg.Source)).IsUnderSiege && IcebergsProtectedArr[pg.Source.UniqueId, 0] > 4)
                    {
                        foreach (Iceberg i in myIcebergs)
                        {
                            bonus = CaculateBonusProtection(i);
                            int attackPA = (int)((IcebergPAwithClone((Iceberg)pg.Source)) / 3);

                            if (attackPA > 3)
                            {
                                if (!i.IsUnderSiege && i.CanSendPenguinsToSetSiege((Iceberg)pg.Source, attackPA + bonus) && DataOnIceberg(i, attackPA + bonus).IsProtected() >= isProtected)
                                {
                                    i.SendPenguinsToSetSiege((Iceberg)pg.Source, attackPA);
                                    IcebergPlay(i);
                                    enemyIcebergs = RemoveItemFromArr(enemyIcebergs, (Iceberg)pg.Source);
                                    //myIcebergs = RemoveItemFromArr(myIcebergs, i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (game.GetEnemyIcebergs().Length <= 2 && game.GetMyIcebergs().Length >= 4 && NeturalTargets().Length == 0) return false;


            foreach (Iceberg d in enemyIcebergs)
            {
                if (IcebergsProtectedArr[d.UniqueId, 0] > 8)
                {
                    int attackPA = (int)(sumOfMypenguin / game.GetMyIcebergs().Length / 10.0 + 0.99);

                    if (attackPA * 3 >= (int)(icebergsPlusArr[d.UniqueId].GetPAByTurn(6) * 1.2) /*|| game.GetEnemyIcebergs().Length == 2*/)
                    {
                        attackPA = (icebergsPlusArr[d.UniqueId].GetPAByTurn(6)) / 3;
                    }

                    if (attackPA == 0) continue;
                    foreach (Iceberg i in myIcebergs)
                    {

                        bonus = CaculateBonusProtection(i);

                        if (!i.IsUnderSiege && i.CanSendPenguinsToSetSiege(d, attackPA) && DataOnIceberg(i, attackPA + bonus).IsProtected() >= isProtected)
                        {
                            i.SendPenguinsToSetSiege(d, attackPA);
                            IcebergPlay(i);
                            //myIcebergs = RemoveItemFromArr(myIcebergs, i);
                            break;
                        }

                        //bonus = 0;
                        if (game.GetTimeRemaining() < -90) return false;
                    }
                }
            }

            return true;
        }

        public bool HelpOthers()
        {
            bool send;
            int minAmount = 200;

            if (game.Turn % 2 == 0)
            {
                foreach (Iceberg i in frendlyIcbergs)
                {
                    send = false;
                    if (IcebergsProtectedArr[i.UniqueId, 1] >= minAmount && i.PenguinAmount >= 100 && IcebergsProtectedArr[i.UniqueId, 1] * 1.3 > sumOfMypenguin - IcebergsProtectedArr[i.UniqueId, 1] && sumOfMypenguin > sumOfEnemypenguin) send = true;

                    if (send)
                    {
                        foreach (Iceberg iceberg in frendlyIcbergs)
                        {
                            if (iceberg != i && !alreadyActedPlus[i][1])
                            {
                                i.SendPenguins(iceberg, (int)(i.PenguinAmount / 50));
                                IcebergPlay(i);
                                game.Debug("HelpOthers");
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool DOOMSDAYattack2()
        {
            if (game.GetTimeRemaining() < -90) return false;
            int len = 0;
            int attackPAmount = 0;
            bool sendSiege = false;
            int bonusProtection = 0;
            int bonusAttack = 0;
            int siegePA = CaculateBonusProtection(game.GetEnemyIcepitalIcebergs()[0], game.GetEnemy()) / 3;


            Iceberg[] myIcbergs = new Iceberg[0];
            Iceberg[] myIcbergsWithSiege = new Iceberg[0];
            Iceberg destination = game.GetEnemyIcepitalIcebergs()[0];

            if (IcebergsProtectedArr[game.GetEnemyIcepitalIcebergs()[0].UniqueId, 0] != isProtected) return false;
            foreach (Iceberg ice in IcebergAlreadyActedArr(game.GetMyIcebergs(), 1))
            {
                if (ice.IsIcepital) bonusProtection = CaculateBonusProtection(game.GetEnemyIcepitalIcebergs()[0]);
                else bonusProtection = 0;

                if (ice.IsIcepital && (game.GetEnemyIcebergs().Length > 2 || sumOfEnemypenguin > 100))
                {
                    if (!game.GetEnemyIcepitalIcebergs()[0].IsUnderSiege)
                    {
                        if (!alreadyActedPlus[myIcepital][1] && PenguinAmountWithSiege(myIcepital) - bonusProtection - siegePA > 0 && game.GetEnemyIcebergs().Length > 1 &&
                        icebergsPlusArr[myIcepital.UniqueId].MinPAarr()[1] - siegePA - bonusProtection > 0)
                        {
                            sendSiege = true;
                            bonusProtection = 0;
                            continue;
                        }
                        return false;
                    }
                    else if (siegePA * 3 > game.GetEnemyIcepitalIcebergs()[0].PenguinAmount - PenguinAmountWithSiege(game.GetEnemyIcepitalIcebergs()[0], true))
                    {
                        bonusAttack = siegePA * 3 - game.GetEnemyIcepitalIcebergs()[0].PenguinAmount + PenguinAmountWithSiege(game.GetEnemyIcepitalIcebergs()[0], true);
                    }
                }

                if (!ice.IsUnderSiege)
                {
                    myIcbergs = AddItemToArr(myIcbergs, ice);
                }
                else
                {
                    myIcbergsWithSiege = AddItemToArr(myIcbergsWithSiege, ice);
                }
            }

            myIcbergs = DistanceFromIcberg(myIcbergs, destination, 100);
            myIcbergsWithSiege = SortBySiegeSize(myIcbergsWithSiege);

            myIcbergs = myIcbergs.Concat(myIcbergsWithSiege).ToArray();

            bool canSend = false;
            int[] myIcbergsAttacks = new int[0];

            if (myIcbergs.Length > 0)
            {
                int[] myIcbergsAttacksAfterSpeed = new int[0];
                int lastTurn = 0;
                int currentTurn = 0;
                int bestSpeed = 0;
                int maxAttackIce = 0;
                int attackIceWithSpeed = 0;

                foreach (Iceberg iceberg in myIcbergs)
                {
                    len++;

                    maxAttackIce = icebergsPlusArr[iceberg.UniqueId].MinPAarr(iceberg.Owner)[1] - bonusProtection;
                    bestSpeed = TheBestSpeed(destination, destination.GetTurnsTillArrival(iceberg), maxAttackIce, iceberg.Owner);
                    currentTurn = GetTurnsTillArrivalAccelerat(destination.GetTurnsTillArrival(iceberg), bestSpeed, 1, true);
                    attackIceWithSpeed = PenguinAmountAfterAccelerate(maxAttackIce - iceberg.PenguinAmount + PenguinAmountWithSiege(iceberg), bestSpeed);

                    if (currentTurn > 12)
                    {
                        myIcbergs = RemoveItemFromArr(myIcbergs, iceberg);
                        len--;
                        continue;
                    }
                    if (currentTurn > lastTurn)
                    {
                        lastTurn = currentTurn;
                        attackPAmount = icebergsPlusArr[destination.UniqueId].GetPAByTurn(lastTurn) + bonusAttack + 1;
                    }

                    myIcbergsAttacks = AddItemToArr(myIcbergsAttacks, maxAttackIce);
                    myIcbergsAttacksAfterSpeed = AddItemToArr(myIcbergsAttacksAfterSpeed, attackIceWithSpeed);

                    if (myIcbergsAttacksAfterSpeed.Sum() >= attackPAmount)
                    {
                        canSend = true;
                        break;
                    }
                }

                //SendPenguins
            }

            if (canSend)
            {
                if (sendSiege)
                {
                    myIcepital.SendPenguinsToSetSiege(game.GetEnemyIcepitalIcebergs()[0], siegePA);
                }
                game.Debug("DOOMSDAYattack2");
                for (int index = 0; index < len; index++)
                {
                    myIcbergs[index].SendPenguins(destination, myIcbergsAttacks[index]);
                    IcebergPlay(myIcbergs[index]);
                }

                IcebergPlay(myIcepital);

                //game.Debug("siegePA: " + siegePA);
            }

            return true;
        }

        public bool AcceleratePGAttack()
        {
            foreach (PenguinGroup pg in game.GetMyPenguinGroups())
            {
                if (pg.Destination != game.GetCloneberg() /*&& pg.Destination.Owner == game.GetMyself()*/)
                {
                    if (!pg.AlreadyActed && !pg.IsSiegeGroup)
                    {
                        if (TheBestSpeed((Iceberg)pg.Destination, pg.TurnsTillArrival, pg.PenguinAmount, pg.Owner, pg, pg.CurrentSpeed) > pg.CurrentSpeed)
                        {
                            pg.Accelerate();
                            game.Debug("AcceleratePGAttack");
                            game.Debug(pg);
                        }
                        //game.Debug("Turn: " + game.Turn);
                        //game.Debug("pg: " + pg);
                    }

                    if (game.GetTimeRemaining() < -90) return false;
                }
            }

            return true;
        }


        //---------------------------------------------------------

        public int[] FindBestSpeedsToConquerTarget(Iceberg[] sourceIcebergs, Iceberg targetIceberg)
        {
            // Sort source icebergs by distance to the target iceberg, from farthest to nearest
            sourceIcebergs = sourceIcebergs.OrderByDescending(i => i.GetTurnsTillArrival(targetIceberg)).ToArray();
            // Initialize an array to store the best speeds for each source iceberg
            int[] bestSpeeds = new int[100000];
            // Recursive function to explore all possible combinations of source icebergs and speeds
            // Call the recursive function with the initial parameters
            int initialTotalPenguinAmount = icebergsPlusArr[targetIceberg.UniqueId].GetPAByTurn(icebergsPlusArr[targetIceberg.UniqueId].GetLastTurn());
            FindBestSpeeds(0, new int[bestSpeeds.Length], sourceIcebergs, bestSpeeds, targetIceberg);
            // Return the best speeds array
            return bestSpeeds;
        }

        public bool FindBestSpeeds(int icebergIndex, int[] currentSpeeds, Iceberg[] sourceIcebergs, int[] bestSpeeds, Iceberg targetIceberg, int[] sumsofattack = null, Dictionary<Iceberg, PGbestSpeed> pgAmountAndSpeed = null)
        {
            IcebergPlus ice = DataOnIceberg(targetIceberg, 0, -1, 0, null, pgAmountAndSpeed);

            if (ice.GetOwnerByTurn(100000) == game.GetMyself())
            {
                System.Array.Copy(currentSpeeds, bestSpeeds, currentSpeeds.Length);
                return true;
            }

            if (game.GetTimeRemaining() < -80)
            {
                return false;
            }

            if (icebergIndex >= sourceIcebergs.Length)
            {
                return false;
            }

            int bounus = 0;
            if (sourceIcebergs[icebergIndex].IsIcepital) bounus = myIcepitalBonus;
            if (sumsofattack == null) sumsofattack = new int[10000];
            bool foundConquerSpeed = false;

            for (int speed = 1; speed <= 8; speed *= 2)
            {
                if (game.GetTimeRemaining() < -50)
                {
                    return false;
                }

                if (sourceIcebergs[icebergIndex].IsIcepital) bounus = myIcepitalBonus;
                int penguinAmount = sourceIcebergs[icebergIndex].PenguinAmount - bounus;
                int penguinAmountToSend = (int)PenguinAmountAfterAccelerate(penguinAmount, speed);
                if (DataOnIceberg(sourceIcebergs[icebergIndex], penguinAmount).IsProtected() == isProtected)
                {
                    PGbestSpeed pg = new PGbestSpeed(game.GetMyself(), penguinAmountToSend, GetTurnsTillArrivalAccelerat(sourceIcebergs[icebergIndex].GetTurnsTillArrival(targetIceberg), speed) + 1, speed);

                    if (pgAmountAndSpeed != null)
                    {
                        int icebergId = sourceIcebergs[icebergIndex].UniqueId;

                        if (pgAmountAndSpeed.ContainsKey(sourceIcebergs[icebergIndex]))
                        {
                            pgAmountAndSpeed[sourceIcebergs[icebergIndex]] = pg;
                        }

                        else
                        {
                            pgAmountAndSpeed.Add(sourceIcebergs[icebergIndex], pg);
                        }
                    }

                    else
                    {
                        pgAmountAndSpeed = new Dictionary<Iceberg,
                        PGbestSpeed>();
                        pgAmountAndSpeed.Add(sourceIcebergs[icebergIndex], pg);
                    }

                    sumsofattack[sourceIcebergs[icebergIndex].UniqueId] = penguinAmountToSend;
                    currentSpeeds[sourceIcebergs[icebergIndex].UniqueId] = speed;
                }
                if (FindBestSpeeds(icebergIndex + 1, currentSpeeds, sourceIcebergs, bestSpeeds, targetIceberg, sumsofattack, pgAmountAndSpeed))
                {
                    foundConquerSpeed = true;
                    break;
                }
            }

            return foundConquerSpeed;
        }

        public Dictionary<int, int[]> FindBestSpeedsToConquerTargets(Iceberg[] sourceIcebergs, Iceberg[] targetIcebergs)
        {
            Dictionary<int,
            int[]> targetSpeeds = new Dictionary<int, int[]>();

            foreach (Iceberg targetIceberg in targetIcebergs)
            {
                int[] bestSpeeds = FindBestSpeedsToConquerTarget(sourceIcebergs, targetIceberg);
                targetSpeeds.Add(targetIceberg.UniqueId, bestSpeeds);
            }

            return targetSpeeds;
        }

        public int GetMaximumSpeedForDistance(int distance, int maxSpeed, int speed = 1, bool beforeSending = false)
        {
            int currentDistance = 0;
            int count = 0;

            if (beforeSending)
            {
                currentDistance = 2;
                count = 2;
            }

            while (currentDistance < distance)
            {
                count++;

                if (speed < maxSpeed)
                {
                    speed *= 2;
                }

                currentDistance += speed;
            }

            return speed;
        }

        public Iceberg[] NeturalTargets(Player owner = null, bool onlyOneOwner = false)
        {
            Iceberg[] targets = new Iceberg[0];

            foreach (PenguinGroup pg in game.GetAllPenguinGroups())
            {
                if (!pg.IsSiegeGroup && pg.Destination != game.GetCloneberg() && pg.Destination.Owner == game.GetNeutral() && (pg.Owner == owner || owner == null)) targets = AddItemToArr(targets, (Iceberg)pg.Destination);
            }

            if (onlyOneOwner && owner != null)
            {
                foreach (PenguinGroup pg in game.GetAllPenguinGroups())
                {
                    if (!pg.IsSiegeGroup && pg.Destination != game.GetCloneberg() && pg.Destination.Owner == game.GetNeutral() && pg.Owner != owner) targets = RemoveItemFromArr(targets, (Iceberg)pg.Destination);
                }
            }

            //game.Debug("len :"+targets.Length);
            targets = targets.Distinct().ToArray();
            return targets;
        }

        public void TurnsTillArrivalRealDistacne()
        {
            int distance = 0;

            foreach (PenguinGroup pg in game.GetAllPenguinGroups())
            {
                if (pg.IsSiegeGroup) distance = 6;
                else if (pg.Destination == game.GetCloneberg() || pg.Source == game.GetCloneberg())
                    distance = pg.Source.GetTurnsTillArrival(pg.Destination) * 2 + 2 * pg.CurrentSpeed;
                else distance = pg.Source.GetTurnsTillArrival(pg.Destination);


                /*game.Debug("+======================+");
                game.Debug(pg);
                game.Debug(turnsTillArrivalRealDistacneArr[pg.UniqueId,0]);
                game.Debug("+======================+");*/
                int[] arr = new int[] { 1, distance };

                if (!turnsTillArrivalRealDistacneArr.ContainsKey(pg.UniqueId))
                {
                    turnsTillArrivalRealDistacneArr[pg.UniqueId] = arr;
                    turnsTillArrivalRealDistacneArr[pg.UniqueId][1] = distance;
                }
                else
                {
                    turnsTillArrivalRealDistacneArr[pg.UniqueId][0] += pg.CurrentSpeed;
                    turnsTillArrivalRealDistacneArr[pg.UniqueId][1] = distance;
                }

                if (turnsTillArrivalRealDistacneArr[pg.UniqueId][0] >= distance) distance = -1;
            }
        }

        public object[] GetOpponentPenguinsSentToNeutralIcebergs()
        {
            int penguinsDifference = 0;
            object[] Array = new object[2];

            foreach (Iceberg iceberg in game.GetNeutralIcebergs())
            {
                // Check if our team sent penguin groups to this neutral iceberg
                PenguinGroup[] myPenguinGroups = game.GetMyPenguinGroups();
                bool hasMyPenguinGroupsSent = false;

                foreach (PenguinGroup myPenguinGroup in myPenguinGroups)
                {
                    if (myPenguinGroup.Destination == iceberg)
                    {
                        hasMyPenguinGroupsSent = true;
                        break;
                    }
                }

                if (hasMyPenguinGroupsSent)
                {
                    // Get all penguin groups sent to this neutral iceberg
                    PenguinGroup[] opponentPenguinGroups = game.GetEnemyPenguinGroups();
                    int opponentPenguinsAmount = 0;

                    foreach (PenguinGroup opponentPenguinGroup in opponentPenguinGroups)
                    {
                        if (opponentPenguinGroup.Destination == iceberg)
                        {
                            opponentPenguinsAmount += opponentPenguinGroup.PenguinAmount;
                        }
                    }

                    int myPenguinsAmount = 0;

                    foreach (PenguinGroup myPenguinGroup in myPenguinGroups)
                    {
                        if (myPenguinGroup.Destination == iceberg)
                        {
                            myPenguinsAmount += myPenguinGroup.PenguinAmount;
                        }
                    }

                    // Check if opponent has more penguins sent to this iceberg than us
                    if (opponentPenguinsAmount > myPenguinsAmount - iceberg.PenguinAmount)
                    {
                        penguinsDifference += opponentPenguinsAmount - (myPenguinsAmount - iceberg.PenguinAmount);
                        Array[1] = iceberg;
                        break;
                    }
                }
            }

            Array[0] = penguinsDifference;
            return Array;
        }

        public int PenguinAmountWithSiege(Iceberg iceberg, bool withNegetivePA = false)
        {
            if (penguinAmountWithSiegeArr.ContainsKey(iceberg))
            {
                if (!withNegetivePA && penguinAmountWithSiegeArr[iceberg] <= 0)
                    return 0;
                return penguinAmountWithSiegeArr[iceberg];
            }

            if (iceberg.IsUnderSiege)
            {
                if (iceberg.Owner == game.GetMyself())
                {
                    foreach (PenguinGroup pg in enemySiegePG)
                    {
                        if (pg.Destination == iceberg && pg.TurnsTillArrival <= 1)
                        {
                            penguinAmountWithSiegeArr.Add(iceberg, iceberg.PenguinAmount - pg.PenguinAmount * game.GoThroughSiegeCost);
                            if (!withNegetivePA && penguinAmountWithSiegeArr[iceberg] <= 0) return 0;
                            else return penguinAmountWithSiegeArr[iceberg];
                        }

                        /*if(iceberg.Id == 3 && iceberg.Owner == game.GetMyself() && pg.Destination == iceberg)
                        {
                            game.Debug("pg.TurnsTillArrival: " + pg.TurnsTillArrival);
                        }*/
                    }
                }

                else
                {
                    foreach (PenguinGroup pg in mySiegePG)
                    {
                        if (pg.Destination == iceberg && pg.TurnsTillArrival <= 1)
                        {
                            penguinAmountWithSiegeArr.Add(iceberg, iceberg.PenguinAmount - pg.PenguinAmount * game.GoThroughSiegeCost);
                            if (!withNegetivePA && penguinAmountWithSiegeArr[iceberg] <= 0) return 0;
                            else return penguinAmountWithSiegeArr[iceberg];
                        }
                    }
                }
            }

            return iceberg.PenguinAmount;
        }

        public int PenguinAmountAfterAccelerate(int penguinAmount, int finalSpeed, int currentSpeed = 1)
        {
            for (int speed = currentSpeed; speed < finalSpeed; speed *= 2)
            {
                penguinAmount = (int)(penguinAmount / 1.3);
            }

            return penguinAmount;
        }

        public int PenguinAmountNeededForAccelerate(int penguinAmount, int currentSpeed, int finalSpeed = 1)
        {
            for (int speed = currentSpeed; speed > finalSpeed; speed /= 2)
            {
                penguinAmount = (int)(penguinAmount * 1.3 + 0.99);
            }

            return penguinAmount;
        }

        public int TheBestSpeed(Iceberg destination, int turns, int pa, Player owner, PenguinGroup pg = null, int startSpeed = -1)
        {
            int currentPA = pa;
            int currentTurns = turns;
            int tmpPA;

            if (pg != null)
            {
                turns = GetDistacnePGtoIceberg(pg);
            }

            int realCurrentPA = pa;
            int level = destination.Level;
            int conquerTurn = 0;
            bool notRealPG = false;



            if (startSpeed == -1)
            {
                startSpeed = 1;
                notRealPG = true;
            }

            int[] icebergProtected = IcebergProtection(destination, true);

            /*if(owner == game.GetEnemy())
            {
                game.Debug("--=-=--=--=--");
                game.Debug(icebergProtected[0]);
                game.Debug("--=-=--=--=--");
            }*/
            if (owner == game.GetMyself())
            {
                if (destination.Owner != game.GetEnemy())
                {
                    if (icebergProtected[0] < isProtected)
                    {
                        conquerTurn = icebergProtected[0];
                    }
                    else level = 0;
                    //if(destination.Owner == game.GetNeutral()) conquerTurn++;
                }
            }

            else
            {
                if (destination.Owner == game.GetEnemy())
                {
                    if (icebergProtected[0] < isProtected)
                    {
                        conquerTurn = icebergProtected[0];
                    }
                    else
                    {
                        level = 0;
                    }
                }
                else if (destination.Owner == game.GetNeutral())
                {
                    if (icebergProtected[0] > isProtected)
                    {
                        conquerTurn = icebergProtected[0];
                    }
                    else
                    {
                        level = 0;
                    }
                }

            }

            int turnsUpgrade = currentTurns - conquerTurn;
            if (turnsUpgrade < 0) turnsUpgrade = 0;
            int bestSpeed = startSpeed;
            int bestPA = realCurrentPA - level * turnsUpgrade;
            if (destination.Owner == owner)
                bestPA -= icebergsPlusArr[destination.UniqueId].GetSiegeByTurn(currentTurns) * 3;

            if (destination is Iceberg && ((Iceberg)destination).IsIcepital &&
                ((destination.Owner != owner && currentTurns > 12) || (destination.Owner == owner && currentTurns > conquerTurn && conquerTurn != 0)))
            {
                bestPA = -999;
            }

            for (int speed = startSpeed * 2; speed <= 8; speed *= 2)
            {
                //game.Debug("bestPA: " + bestPA);
                if (notRealPG) currentTurns = GetTurnsTillArrivalAccelerat(turns, speed, startSpeed, true);
                else currentTurns = GetTurnsTillArrivalAccelerat(turns, speed, startSpeed);
                turnsUpgrade = currentTurns - conquerTurn;
                if (turnsUpgrade < 0) turnsUpgrade = 0;
                if (destination.Owner == game.GetNeutral() && turnsUpgrade <= 0) break;
                {
                    realCurrentPA = (int)((realCurrentPA) / 1.3);
                }

                if (realCurrentPA == 0) break;

                tmpPA = realCurrentPA - level * turnsUpgrade;

                /*if(destination is Iceberg && !((Iceberg)destination).IsIcepital && destination.Owner != game.GetNeutral() && destination.Owner != owner && currentTurns > 10)
                {  
                    tmpPA = -999;
                }*/

                if (destination.Owner == owner)
                    tmpPA -= icebergsPlusArr[destination.UniqueId].GetSiegeByTurn(currentTurns) * 3;

                if (bestPA < tmpPA)
                {
                    bestSpeed = speed;
                    bestPA = tmpPA;
                }

                if (destination is Iceberg && ((Iceberg)destination).IsIcepital &&
                    ((destination.Owner != owner && currentTurns > 12) || (destination.Owner == owner && currentTurns > conquerTurn && conquerTurn != 0)))
                {
                    bestPA = -999;
                }

                //currentPA = realCurrentPA - level * turnsUpgrade;
            }

            /*if(destination.Owner == game.GetNeutral() && pg != null && pg.Owner == game.GetEnemy())
            {
                game.Debug("______________________");
                game.Debug(destination);
                game.Debug("turns" + turns);
                game.Debug("bestSpeed" + bestSpeed);
            }*/
            return bestSpeed;
        }

        public Iceberg[] GetIcebergsAttackFirst()
        {
            Iceberg[] tmp = DistanceFromIcberg(game.GetNeutralIcebergs(), myIcepital, 100);
            Iceberg[] icebergsInMySide = new Iceberg[(int)(game.GetNeutralIcebergs().Length / 2)];

            for (int i = 0; i < icebergsInMySide.Length; i++)
            {
                icebergsInMySide[i] = tmp[i];
            }

            Iceberg[] icebergs = new Iceberg[2];

            if (icebergsInMySide[0].GetTurnsTillArrival(game.GetCloneberg()) > icebergsInMySide[1].GetTurnsTillArrival(game.GetCloneberg()))
            {
                icebergs[0] = icebergsInMySide[1];
                icebergs[1] = icebergsInMySide[0];
            }
            else
            {
                icebergs[0] = icebergsInMySide[0];
                icebergs[1] = icebergsInMySide[1];
            }

            /* if(icebergsInMySide[0].GetTurnsTillArrival(game.GetCloneberg()) < icebergsInMySide[1].GetTurnsTillArrival(game.GetCloneberg())) icebergs[0]=icebergsInMySide[0];
             else icebergs[0]=icebergsInMySide[1];
             if(icebergs[0].GetTurnsTillArrival(game.GetCloneberg()) > 10) icebergs[1]=DistanceFromIcberg(icebergsInMySide, game.GetCloneberg(), 10)[0];
             else if(icebergs[0] == icebergsInMySide[0]) icebergs[1]=icebergsInMySide[1];
             else icebergs[1] = icebergsInMySide[0];*/
            icebergs = icebergs.Distinct().ToArray();
            return icebergs;
        }

        public int GetTurnsTillArrivalAccelerat(int turns, int maxSpeed, int speed = 1, bool beforeSending = false)
        {
            int distance = 0;
            int count = 0;

            if (beforeSending)
            {
                distance = 1;
                count = 1;
            }

            while (turns > distance)
            {
                count++;

                if (speed < maxSpeed)
                {
                    speed *= 2;
                }

                distance += speed;
            }

            return count;
        }

        public Iceberg[] DistanceFromIcberg(Iceberg[] icebergs, IceBuilding destination, int maxDistance)
        {
            icebergs = icebergs.Where(i => i.GetTurnsTillArrival(destination) <= maxDistance && i != destination).ToArray();
            System.Array.Sort(icebergs, (x, y) => x.GetTurnsTillArrival(destination).CompareTo(y.GetTurnsTillArrival(destination)));
            return icebergs;
        }

        public Iceberg[] LowestPenguinAmount(Iceberg[] icebergs)
        {
            //find the lowest num
            int lowestNum = 1000;
            int len = 0;

            foreach (Iceberg i in icebergs)
            {
                if (i.PenguinAmount < lowestNum)
                {
                    lowestNum = i.PenguinAmount;
                    len = 1;
                }

                else if (i.PenguinAmount == lowestNum) len++;
            }

            Iceberg[] result = new Iceberg[len];
            int count = 0;

            //find the icebergs
            foreach (Iceberg i in icebergs)
            {
                if (count < len && i.PenguinAmount == lowestNum) result[count++] = i;
            }

            return result;
        }

        public int GetDistacnePGtoIceberg(PenguinGroup pg)
        {
            if (pg.IsSiegeGroup && turnsTillArrivalRealDistacneArr[pg.UniqueId][1] <= turnsTillArrivalRealDistacneArr[pg.UniqueId][0]) return -1;
            if (turnsTillArrivalRealDistacneArr[pg.UniqueId][1] - turnsTillArrivalRealDistacneArr[pg.UniqueId][0] <= 0) return 1;
            return turnsTillArrivalRealDistacneArr[pg.UniqueId][1] - turnsTillArrivalRealDistacneArr[pg.UniqueId][0];
        }

        public Iceberg findClosestToClone(Iceberg[] icebergs)
        {
            Iceberg ice = icebergs[0];

            foreach (Iceberg i in icebergs)
            {
                if (i.GetTurnsTillArrival(game.GetCloneberg()) == ice.GetTurnsTillArrival(game.GetCloneberg()) && i.GetTurnsTillArrival(myIcepital) < ice.GetTurnsTillArrival(myIcepital)) ice = i;
                else if (i.GetTurnsTillArrival(game.GetCloneberg()) < ice.GetTurnsTillArrival(game.GetCloneberg())) ice = i;
            }

            return ice;
        }

        public int[,] GetIfprotectedIcbergs(Iceberg[] icebergs)
        {

            int[,] protectedIcbergs = new int[100, 2];
            int[] current;

            foreach (Iceberg i in icebergs)
            {
                current = IcebergProtection(i);
                protectedIcbergs[i.UniqueId, 0] = current[0];
                protectedIcbergs[i.UniqueId, 1] = current[1];
            }

            return protectedIcbergs;
        }

        public Iceberg[] GetProtectedIcbergs(Iceberg[] icebergs)
        {
            Iceberg[] protectedIcbergs = new Iceberg[0];

            foreach (Iceberg i in icebergs)
            {
                if (IcebergsProtectedArr[i.UniqueId, 0] >= isProtected)
                {
                    protectedIcbergs = AddItemToArr(protectedIcbergs, i);
                }
            }

            return protectedIcbergs;
        }

        public Iceberg[] GetNotProtectedIcbergs(Iceberg[] icebergs)
        {
            Iceberg[] protectedIcbergs = new Iceberg[0];

            foreach (Iceberg i in icebergs)
            {
                if (IcebergsProtectedArr[i.UniqueId, 0] < isProtected)
                {
                    protectedIcbergs = AddItemToArr(protectedIcbergs, i);
                }
            }

            return protectedIcbergs;
        }

        public T[] AddItemToArr<T>(T[] array, T item)
        {
            List<T> list = new List<T>(array);
            list.Add(item);
            return list.ToArray();
        }

        public T[] RemoveItemFromArr<T>(T[] arr, T itemToRemove)
        {
            List<T> list = new List<T>(arr);
            list.Remove(itemToRemove);
            return list.ToArray();
        }

        public bool IcebergCanUpgrade(Iceberg iceberg, int bonus)
        {
            if (iceberg.IsIcepital && icebergsPlusArr[iceberg.UniqueId].MinPAarr(iceberg.Owner)[1] < iceberg.PenguinAmount)
            {
                return false;
            }

            if (DataOnIceberg(iceberg, iceberg.UpgradeCost + bonus, -1, iceberg.Level + 1, null, null, null).IsProtected() == isProtected && iceberg.PenguinAmount - iceberg.UpgradeCost - bonus > 0 && iceberg.CanUpgrade() && !alreadyActedPlus[iceberg][1] && !alreadyActedPlus[iceberg][0])
            {
                return true;
            }

            return false;
        }

        public Iceberg[] SortIcebergsByPAmount(Iceberg[] icebergs)
        {
            System.Array.Sort(icebergs, (x, y) => icebergsPlusArr[x.UniqueId].GetPAByTurn(icebergsPlusArr[x.UniqueId].GetLastTurn()).CompareTo(icebergsPlusArr[y.UniqueId].GetPAByTurn(icebergsPlusArr[y.UniqueId].GetLastTurn())));
            return icebergs;
        }

        public PenguinGroup[] SortPGByPAmount(PenguinGroup[] penguinGroups)
        {
            PenguinGroup[] sorted = penguinGroups.OrderBy(ob => ob.PenguinAmount).ToArray();
            return sorted;
        }

        public int SumOfpenguin(Player player)
        {
            int sum = 0;
            Iceberg[] icebergs = game.GetEnemyIcebergs();
            PenguinGroup[] pgs = game.GetEnemyPenguinGroups();

            if (player == game.GetMyself())
            {
                icebergs = game.GetMyIcebergs();
                pgs = game.GetMyPenguinGroups();
            }

            foreach (Iceberg i in icebergs)
            {
                sum += i.PenguinAmount;
            }

            foreach (PenguinGroup pg in pgs)
            {
                sum += pg.PenguinAmount;
            }

            return sum;
        }

        public int SumOfpenguin(Iceberg[] icebergs, Iceberg destination = null)
        {
            int sumP = 0;
            int speed = 0;

            //Caculate SumOfpenguin with TheBestSpeed for each Iceberg
            if (destination != null)
            {
                for (int i = 0; i < icebergs.Length; i++)
                {
                    speed = TheBestSpeed(destination, icebergs[i].GetTurnsTillArrival(destination), PenguinAmountWithSiege(icebergs[i]), icebergs[i].Owner);
                    sumP += PenguinAmountAfterAccelerate(PenguinAmountWithSiege(icebergs[i]), speed);
                }
            }

            else
            {
                for (int i = 0; i < icebergs.Length; i++)
                {
                    sumP += (PenguinAmountWithSiege(icebergs[i]));
                }
            }

            return sumP;
        }

        public int SumOfTotalpenguin(Iceberg[] icebergs)
        {
            int sumP = 0;

            for (int i = 0; i < icebergs.Length; i++)
            {
                sumP += icebergsPlusArr[icebergs[i].UniqueId].MinPAarr(game.GetMyself())[1];
            }

            return sumP;
        }

        public int SumOfpenguin(Iceberg[] icebergs, bool denominator)
        {
            int sumP = 0;

            for (int i = 0; i < icebergs.Length; i++)
            {
                if (denominator)
                {
                    sumP += (int)((int)((int)(icebergs[i].PenguinAmount / 1.3) / 1.3) / 1.3);
                }
            }

            return sumP;
        }

        public Iceberg[] IcebergAlreadyActedArr(Iceberg[] icebergs, int type)
        {
            foreach (Iceberg i in icebergs)
            {
                if (alreadyActedPlus[i][type])
                {
                    icebergs = RemoveItemFromArr(icebergs, i);
                }
            }

            return icebergs;
        }

        /*public Iceberg[] GetMyIcebergsAndTargets()
        {
            List < Iceberg>myIcebergs=new List < Iceberg>();
            List < PenguinGroup>myPenguinGroups=game.GetMyPenguinGroups().ToList();

            // Add my Icebergs to the list
            foreach(Iceberg iceberg in game.GetMyIcebergs()) {
                myIcebergs.Add(iceberg);
            }
            
            // Add target Icebergs to the list
            foreach(PenguinGroup penguinGroup in myPenguinGroups) {
                if (penguinGroup.Destination !=game.GetCloneberg()) {
                    Iceberg targetIceberg=(Iceberg) penguinGroup.Destination;

                    if (TotalPenguinAmountWorstCase(game.GetAllPenguinGroups(), targetIceberg, null, 0) !=isProtected) {
                        myIcebergs.Add(targetIceberg);
                    }
                }
            }
            
            return myIcebergs.ToArray();
        }
        
        public Iceberg[] GetEnemyIcebergsAndTargets() 
        {
            List < Iceberg>myIcebergs=new List < Iceberg>();
            List < PenguinGroup>myPenguinGroups=game.GetEnemyPenguinGroups().ToList();

            // Add my Icebergs to the list
            foreach(Iceberg iceberg in game.GetEnemyIcebergs()) {
                myIcebergs.Add(iceberg);
            }
            
            // Add target Icebergs to the list
            foreach(PenguinGroup penguinGroup in myPenguinGroups) {
                if (penguinGroup.Destination !=game.GetCloneberg()) {
                    Iceberg targetIceberg=(Iceberg) penguinGroup.Destination;

                    if (TotalPenguinAmountWorstCase(game.GetAllPenguinGroups(), targetIceberg, null, 0) !=isProtected) 
                    {
                        myIcebergs.Add(targetIceberg);
                    }
                }
            }
            
            return myIcebergs.ToArray();
        }*/

        public int GetPlayerLevelSum(Player player)
        {
            int sum = 0;

            foreach (Iceberg iceberg in game.GetAllIcebergs())
            {
                if (iceberg.Owner == player)
                {
                    sum += iceberg.Level;
                }
            }

            return sum;
        }

        public PGbestSpeed[] SortPGbestSpeedByTurns(PGbestSpeed[] icebergs)
        {
            System.Array.Sort(icebergs, (x, y) => x.TurnsTillArrival.CompareTo(y.TurnsTillArrival));
            return icebergs;
        }

        public IcebergPlus DataOnIceberg(Iceberg iceberg, int lessP = 0, int maxTurns = -1, int level = 0, Dictionary<PenguinGroup, int> pgandspeed = null, Dictionary<Iceberg, PGbestSpeed> pgamountandspeed = null, Dictionary<PenguinGroup, PGbestSpeed> pgamountandspeed2 = null)
        {
            int totalPenguinAmount = iceberg.PenguinAmount - lessP;
            PGbestSpeed[] newAllPG = new PGbestSpeed[0];
            PGbestSpeed curretNewPG = null;
            Player icebergOwner = iceberg.Owner;
            Player newIcebergOwner = icebergOwner;
            PenguinGroup[] allPG = game.GetAllPenguinGroups();
            int curretTurn = 1;
            int prvTurn = 0;
            int i = 0;
            if (level == 0) level = iceberg.Level;


            int[] siegeTime = new int[2] { -1, -1 };
            int siegeAmount = 0;

            if (iceberg.IsUnderSiege)
            {
                siegeTime = new int[2] { 0, game.SiegeMaxTurns - iceberg.SiegeTurns };
                siegeAmount = (iceberg.PenguinAmount - PenguinAmountWithSiege(iceberg, true)) / 3;
            }

            else if (iceberg.Owner == game.GetEnemy())
            {
                foreach (PenguinGroup pg in mySiegePG)
                {
                    if ((Iceberg)pg.Destination == iceberg)
                    {
                        siegeTime = new int[2] { GetDistacnePGtoIceberg(pg), game.SiegeMaxTurns + GetDistacnePGtoIceberg(pg) };
                        siegeAmount = pg.PenguinAmount;
                        break;
                    }
                }
            }

            else
            {
                foreach (PenguinGroup pg in enemySiegePG)
                {
                    if ((Iceberg)pg.Destination == iceberg)
                    {
                        siegeTime = new int[2] { GetDistacnePGtoIceberg(pg), game.SiegeMaxTurns + GetDistacnePGtoIceberg(pg) };
                        siegeAmount = pg.PenguinAmount;
                        break;
                    }
                }
            }

            IcebergPlus icebergPlus = new IcebergPlus(iceberg, game, lessP, -1, (iceberg.PenguinAmount - PenguinAmountWithSiege(iceberg, true)) / 3);

            foreach (PenguinGroup pg in allPG)
            {

                if (pgamountandspeed2 != null)
                {
                    if (pgamountandspeed2.ContainsKey(pg))
                    {
                        curretNewPG = pgamountandspeed2[pg];
                        newAllPG = AddItemToArr(newAllPG, curretNewPG);
                    }
                }

                if ((pg.Destination == iceberg || (pg.Destination == game.GetCloneberg() && pg.Source == iceberg)) && (pg.TurnsTillArrival <= maxTurns || maxTurns == -1) && !pg.IsSiegeGroup)
                {
                    if (pg.Owner != iceberg.Owner)
                    {
                        int speed = 0;
                        if (pg.Source == iceberg)
                            speed = TheBestSpeed(iceberg, (int)((GetDistacnePGtoIceberg(pg) - 2) / pg.CurrentSpeed + 0.99) + 2, pg.PenguinAmount, pg.Owner, pg, pg.CurrentSpeed);
                        else
                            speed = TheBestSpeed(iceberg, pg.TurnsTillArrival, pg.PenguinAmount, pg.Owner, pg, pg.CurrentSpeed);

                        if (pgandspeed != null)
                        {
                            if (pgandspeed.ContainsKey(pg))
                            {
                                speed = pgandspeed[pg];
                            }
                        }

                        if (pgamountandspeed2 != null)
                        {
                            if (pgamountandspeed2.ContainsKey(pg))
                            {
                                speed = pgamountandspeed2[pg].CurrentSpeed;
                            }
                        }

                        if (speed > pg.CurrentSpeed)
                        {
                            int penguinAmount = PenguinAmountAfterAccelerate(pg.PenguinAmount, speed, pg.CurrentSpeed);
                            int turns = GetTurnsTillArrivalAccelerat(GetDistacnePGtoIceberg(pg), speed, pg.CurrentSpeed);
                            curretNewPG = new PGbestSpeed(pg.Owner, penguinAmount, turns, speed);
                            newAllPG = AddItemToArr(newAllPG, curretNewPG);
                        }

                        else
                        {
                            curretNewPG = new PGbestSpeed(pg);
                            newAllPG = AddItemToArr(newAllPG, curretNewPG);
                        }
                    }

                    else
                    {
                        curretNewPG = new PGbestSpeed(pg);
                        newAllPG = AddItemToArr(newAllPG, curretNewPG);
                    }
                }
            }

            if (pgamountandspeed != null)
            {
                foreach (PGbestSpeed pg in pgamountandspeed.Values) newAllPG = AddItemToArr(newAllPG, pg);
            }

            newAllPG = newAllPG.Distinct().ToArray();
            newAllPG = SortPGbestSpeedByTurns(newAllPG);
            /*game.Debug("newAllPG.Length: " + newAllPG.Length);
            game.Debug("maxTurns: " + maxTurns);
            game.Debug("curretTurn: " + curretTurn);
            game.Debug("i: " + i);
            game.Debug("curretTurn: " + curretTurn);*/


            while (i < newAllPG.Length || curretTurn <= maxTurns)
            {
                if (icebergOwner != game.GetNeutral())
                {
                    totalPenguinAmount += level;
                }

                if (i < newAllPG.Length && curretTurn == newAllPG[i].TurnsTillArrival)
                {
                    while (i < newAllPG.Length && curretTurn == newAllPG[i].TurnsTillArrival)
                    {
                        if (newAllPG[i].Owner == icebergOwner)
                        {
                            if (siegeAmount > 0 && siegeTime[0] != -1 && siegeTime[0] <= curretTurn && siegeTime[1] >= curretTurn)
                            {
                                siegeAmount -= (int)(newAllPG[i].PenguinAmount / game.GoThroughSiegeCost);

                                if (siegeAmount <= 0)
                                {
                                    totalPenguinAmount += -siegeAmount * game.GoThroughSiegeCost + newAllPG[i].PenguinAmount % 3;
                                    siegeAmount = 0;
                                }
                            }
                            else
                            {
                                totalPenguinAmount += newAllPG[i].PenguinAmount;
                            }
                        }

                        else
                        {
                            totalPenguinAmount -= newAllPG[i].PenguinAmount;
                        }

                        if (totalPenguinAmount < 0)
                        {
                            if (newIcebergOwner == icebergOwner)
                            {
                                newIcebergOwner = newAllPG[i].Owner;
                            }
                        }

                        else newIcebergOwner = icebergOwner;
                        i++;
                    }

                    if (totalPenguinAmount < 0)
                    {
                        icebergOwner = newIcebergOwner;
                        totalPenguinAmount = -totalPenguinAmount;
                        siegeAmount = 0;
                    }
                }

                if (pgamountandspeed2 != null)
                {
                    //game.Debug("data "+icebergPlus);
                    //game.Debug(icebergPlus.GetPAByTurn(icebergPlus.GetLastTurn()));
                }
                if (siegeTime[0] != -1 && siegeTime[0] <= curretTurn && siegeTime[1] >= curretTurn)
                    icebergPlus.AddTurn(icebergOwner, totalPenguinAmount, siegeAmount);
                else
                    icebergPlus.AddTurn(icebergOwner, totalPenguinAmount, 0);

                curretTurn++;
            }

            return icebergPlus;
        }

        public IcebergPlus DataOnIceberg(bool notBST, Iceberg iceberg, int lessP = 0, int maxTurns = -1, int level = 0, Dictionary<PenguinGroup, int> pgandspeed = null, Dictionary<Iceberg, PGbestSpeed> pgamountandspeed = null, Dictionary<PenguinGroup, PGbestSpeed> pgamountandspeed2 = null)
        {
            int totalPenguinAmount = iceberg.PenguinAmount - lessP;
            PGbestSpeed[] newAllPG = new PGbestSpeed[0];
            PGbestSpeed curretNewPG = null;
            Player icebergOwner = iceberg.Owner;
            Player newIcebergOwner = icebergOwner;
            PenguinGroup[] allPG = game.GetAllPenguinGroups();
            int curretTurn = 1;
            int prvTurn = 0;
            int i = 0;
            if (level == 0) level = iceberg.Level;


            int[] siegeTime = new int[2] { -1, -1 };
            int siegeAmount = 0;

            if (iceberg.IsUnderSiege)
            {
                siegeTime = new int[2] { 0, game.SiegeMaxTurns - iceberg.SiegeTurns };
                siegeAmount = (iceberg.PenguinAmount - PenguinAmountWithSiege(iceberg, true)) / 3;
            }

            else if (iceberg.Owner == game.GetEnemy())
            {
                foreach (PenguinGroup pg in mySiegePG)
                {
                    if ((Iceberg)pg.Destination == iceberg)
                    {
                        siegeTime = new int[2] { GetDistacnePGtoIceberg(pg), game.SiegeMaxTurns + GetDistacnePGtoIceberg(pg) };
                        siegeAmount = pg.PenguinAmount;
                        break;
                    }
                }
            }

            else
            {
                foreach (PenguinGroup pg in enemySiegePG)
                {
                    if ((Iceberg)pg.Destination == iceberg)
                    {
                        siegeTime = new int[2] { GetDistacnePGtoIceberg(pg), game.SiegeMaxTurns + GetDistacnePGtoIceberg(pg) };
                        siegeAmount = pg.PenguinAmount;
                        break;
                    }
                }
            }

            IcebergPlus icebergPlus = new IcebergPlus(iceberg, game, lessP, -1, (iceberg.PenguinAmount - PenguinAmountWithSiege(iceberg, true)) / 3);

            foreach (PenguinGroup pg in allPG)
            {
                if (pg.Destination == iceberg && (pg.TurnsTillArrival <= maxTurns || maxTurns == -1) && !pg.IsSiegeGroup)
                {
                    curretNewPG = new PGbestSpeed(pg);
                    newAllPG = AddItemToArr(newAllPG, curretNewPG);
                }
            }

            if (pgamountandspeed != null)
            {
                foreach (PGbestSpeed pg in pgamountandspeed.Values) newAllPG = AddItemToArr(newAllPG, pg);
            }

            newAllPG = newAllPG.Distinct().ToArray();
            newAllPG = SortPGbestSpeedByTurns(newAllPG);

            while (i < newAllPG.Length || curretTurn <= maxTurns)
            {
                if (icebergOwner != game.GetNeutral())
                {
                    totalPenguinAmount += level;
                }

                if (i < newAllPG.Length && curretTurn == newAllPG[i].TurnsTillArrival)
                {
                    while (i < newAllPG.Length && curretTurn == newAllPG[i].TurnsTillArrival)
                    {
                        if (newAllPG[i].Owner == icebergOwner)
                        {
                            if (siegeAmount > 0 && siegeTime[0] != -1 && siegeTime[0] <= curretTurn && siegeTime[1] >= curretTurn)
                            {
                                siegeAmount -= (int)(newAllPG[i].PenguinAmount / game.GoThroughSiegeCost);

                                if (siegeAmount <= 0)
                                {
                                    totalPenguinAmount += -siegeAmount * game.GoThroughSiegeCost + newAllPG[i].PenguinAmount % 3;
                                    siegeAmount = 0;
                                }
                            }
                            else
                            {
                                totalPenguinAmount += newAllPG[i].PenguinAmount;
                            }
                        }

                        else
                        {
                            totalPenguinAmount -= newAllPG[i].PenguinAmount;
                        }

                        if (totalPenguinAmount < 0)
                        {
                            if (newIcebergOwner == icebergOwner)
                            {
                                newIcebergOwner = newAllPG[i].Owner;
                            }
                        }

                        else newIcebergOwner = icebergOwner;
                        i++;
                    }

                    if (totalPenguinAmount < 0)
                    {
                        icebergOwner = newIcebergOwner;
                        totalPenguinAmount = -totalPenguinAmount;
                        siegeAmount = 0;
                    }
                }

                if (pgamountandspeed2 != null)
                {
                    //game.Debug("data "+icebergPlus);
                    //game.Debug(icebergPlus.GetPAByTurn(icebergPlus.GetLastTurn()));
                }
                if (siegeTime[0] != -1 && siegeTime[0] <= curretTurn && siegeTime[1] >= curretTurn)
                    icebergPlus.AddTurn(icebergOwner, totalPenguinAmount, siegeAmount);
                else
                    icebergPlus.AddTurn(icebergOwner, totalPenguinAmount, 0);

                curretTurn++;
            }

            return icebergPlus;
        }

        public IcebergPlus[] CreateIcebergPlusArr(Iceberg[] iceArr)
        {
            IcebergPlus[] result = new IcebergPlus[100];

            foreach (Iceberg i in iceArr)
            {
                result[i.UniqueId] = DataOnIceberg(i);
            }

            return result;
        }

        public int[] IcebergProtection(Iceberg iceberg, int attackAmount = 0)
        {
            IcebergPlus icePlus = icebergsPlusArr[iceberg.UniqueId];

            if (attackAmount != 0)
            {
                icePlus = DataOnIceberg(iceberg, attackAmount);
            }
            int turn = icePlus.IsProtected();
            int pa = icePlus.GetPAByTurn(icePlus.GetLastTurn());
            if (turn < isProtected) pa = icePlus.GetPAByTurn(turn);

            return new int[2] { turn, pa };
        }

        public int[] IcebergProtection(Iceberg iceberg, bool withoutTBS)
        {
            IcebergPlus icePlus = DataOnIceberg(true, iceberg);

            int turn = icePlus.IsProtected();
            int pa = icePlus.GetPAByTurn(icePlus.GetLastTurn());

            if (turn < 0)
                pa = icePlus.GetPAByTurn(turn);

            return new int[2] { turn, pa };
        }

        public Iceberg[] SortByPenguinCanSend(Iceberg[] icebergs)
        {
            System.Array.Sort(icebergs, (x, y) => (icebergsPlusArr[x.UniqueId].MinPAarr(x.Owner)[1] - x.PenguinAmount + PenguinAmountWithSiege(x)).CompareTo(icebergsPlusArr[y.UniqueId].MinPAarr(y.Owner)[1] - y.PenguinAmount + PenguinAmountWithSiege(y)));
            return icebergs;
        }

        public void CreateAlreadyActedPlusArr()
        {
            foreach (Iceberg ice in game.GetMyIcebergs())
            {
                alreadyActedPlus[ice] = new bool[2] { false, false };
            }
        }

        public void CreateIcebergsWillBeUnderSiege()
        {
            icebergsWillBeUnderSiege = new Dictionary<Iceberg, int[]>();

            foreach (PenguinGroup pg in game.GetEnemyPenguinGroups())
            {
                if (pg.IsSiegeGroup)
                {
                    if (!icebergsWillBeUnderSiege.ContainsKey((Iceberg)pg.Destination) && GetDistacnePGtoIceberg(pg) != -1)
                        icebergsWillBeUnderSiege.Add((Iceberg)pg.Destination, new int[2] { GetDistacnePGtoIceberg(pg), pg.PenguinAmount });
                }
            }
        }

        public int IcebergPAwithClone(Iceberg i)
        {
            int penguinAmount = i.PenguinAmount;
            //game.Debug(i.Owner.PenguinAmountOnCloneberg);

            foreach (PenguinGroup pg in i.Owner.PenguinGroups)
            {
                if (pg.Destination == game.GetCloneberg() && pg.Source == i) penguinAmount += pg.PenguinAmount * 2;
                else if (pg.Destination == i && pg.Source == game.GetCloneberg()) penguinAmount += pg.PenguinAmount;
            }

            return penguinAmount;
        }

        public Iceberg[] SortBySiegeSize(Iceberg[] icebergs)
        {
            System.Array.Sort(icebergs, (x, y) => (x.PenguinAmount - PenguinAmountWithSiege(x, true)).CompareTo(y.PenguinAmount - PenguinAmountWithSiege(y, true)));
            return icebergs;
        }

        public IcebergAttack[] SortIcebergAttackBySum(IcebergAttack[] icebergs)
        {
            System.Array.Sort(icebergs, (x, y) => x.sum.CompareTo(y.sum));
            return icebergs;
        }

        public int CaculateBonusProtection(Iceberg ice, Player owner = null)
        {
            if (bonusProtections.ContainsKey(ice)) return bonusProtections[ice];
            int bonusProtection = 0;
            if (game.Turn <= 6)
            {
                bonusProtections.Add(ice, bonusProtection);
                return bonusProtection;
            }

            if (game.GetMyIcebergs().Length + NeturalTargets(game.GetMyself()).Length < 4 && (game.GetEnemyIcebergs().Length > 1 && game.Turn < 30))
            {
                if (ice.IsIcepital)
                {
                    bonusProtections.Add(ice, myIcepitalBonus);
                    return (myIcepitalBonus);
                }
                bonusProtections.Add(ice, 0);
                return 0;
            }
            foreach (Iceberg i in game.GetAllIcebergs())
            {
                if (i != ice)
                {
                    if (i.Owner == game.GetEnemy() && (owner == null || owner == game.GetEnemy()))
                    {
                        bonusProtection += (int)(PenguinAmountWithSiege(i) / (i.GetTurnsTillArrival(ice) / 3.5));
                    }
                    else if (i.Owner == game.GetMyself() && i.GetTurnsTillArrival(ice) < 15 && (owner == null || owner == game.GetMyself()))
                    {
                        bonusProtection -= (int)(PenguinAmountWithSiege(i) / (i.GetTurnsTillArrival(ice) / 3.5));
                    }
                }
                /*if(bonusProtection > 1000)
                game.Debug("_+_+_");
                    game.Debug(i);
                    game.Debug(PenguinAmountWithSiege(ice) /(i.GetTurnsTillArrival(ice) / 8.0));
                    game.Debug((i.GetTurnsTillArrival(ice) / 8.0));
                    game.Debug(PenguinAmountWithSiege(ice));
                game.Debug("_+_+_");*/
            }

            if (bonusProtection < 0) bonusProtection = 0;
            if (ice.IsIcepital) bonusProtection += myIcepitalBonus;

            bonusProtections.Add(ice, bonusProtection);
            return bonusProtection;

        }

        public void IcebergPlay(Iceberg iceberg, bool upgrade = false)
        {
            //Update ActivateArr
            if (upgrade) alreadyActedPlus[iceberg][1] = true;
            else alreadyActedPlus[iceberg][0] = true;

            //reset some arrays
            if (penguinAmountWithSiegeArr.ContainsKey(iceberg))
            {
                penguinAmountWithSiegeArr.Remove(iceberg);
            }

            bonusProtections = new Dictionary<Iceberg, int>();

            icebergsPlusArr[iceberg.UniqueId] = DataOnIceberg(iceberg);

            int[] protction = IcebergProtection(iceberg);
            IcebergsProtectedArr[iceberg.UniqueId, 0] = protction[0];
            IcebergsProtectedArr[iceberg.UniqueId, 1] = protction[1];
        }
    }

    public class PGbestSpeed
    {
        public Player Owner;
        public int PenguinAmount;
        public int TurnsTillArrival;
        public int CurrentSpeed;

        public PGbestSpeed(Player Owner, int PenguinAmount, int TurnsTillArrival, int CurrentSpeed)
        {
            this.Owner = Owner;
            this.PenguinAmount = PenguinAmount;
            this.TurnsTillArrival = TurnsTillArrival;
            this.CurrentSpeed = CurrentSpeed;
        }

        public PGbestSpeed(PenguinGroup pg)
        {
            this.Owner = pg.Owner;
            this.PenguinAmount = pg.PenguinAmount;
            this.TurnsTillArrival = pg.TurnsTillArrival;
            this.CurrentSpeed = pg.CurrentSpeed;
        }

        public override string ToString()
        {
            return $"Owner:{Owner} | PenguinAmount:{PenguinAmount} | TurnsTillArrival:{TurnsTillArrival} | CurrentSpeed:{CurrentSpeed}";
        }
    }

    public class IcebergPlus
    {
        Iceberg iceberg;
        List<int> paPerTurn;
        List<Player> ownerPerTurn;
        List<int> siegePerTurn;
        int level;
        Game game;

        public IcebergPlus(Iceberg iceberg, Game game, int lessP = 0, int level = -1, int siege = 0)
        {
            this.iceberg = iceberg;
            this.game = game;
            paPerTurn = new List<int> { iceberg.PenguinAmount - lessP };
            ownerPerTurn = new List<Player> { iceberg.Owner };
            siegePerTurn = new List<int> { siege };
            if (level == -1)
                this.level = iceberg.Level;
            else
                this.level = level;
        }

        public Player GetOwnerByTurn(int turn = 0)
        {
            if (turn < ownerPerTurn.Count)
                return ownerPerTurn[turn];
            return ownerPerTurn.Last();
        }

        public int GetPAByTurn(int turn = 0)
        {
            if (turn < paPerTurn.Count)
                return paPerTurn[turn];

            if (ownerPerTurn.Last() == game.GetNeutral())
                return paPerTurn.Last();

            return paPerTurn.Last() + level * (turn - paPerTurn.Count + 1);
        }

        public int GetSiegeByTurn(int turn = 0)
        {
            if (turn < siegePerTurn.Count)
                return siegePerTurn[turn];
            return siegePerTurn.Last();
        }

        public int GetLastTurn()
        {
            return paPerTurn.Count - 1;
        }

        public Iceberg GetIceberg()
        {
            return iceberg;
        }

        public List<Player> GetOwnerList()
        {
            return ownerPerTurn;
        }

        public List<int> GetPAList()
        {
            return paPerTurn;
        }

        public void AddTurn(Player owner, int pa, int siege)
        {
            paPerTurn.Add(pa);
            ownerPerTurn.Add(owner);
            siegePerTurn.Add(siege);
        }

        public int[] MinPAarr(Player owner = null, int firstTurn = 0)
        {
            int minPA = 9999999;
            int turn = -1;
            for (int i = firstTurn; i < paPerTurn.Count; i++)
            {
                if (owner == null)
                {
                    if (minPA > paPerTurn[i])
                    {
                        turn = i;
                        minPA = paPerTurn[i];
                    }
                }
                else if (owner == ownerPerTurn[i])
                {
                    if (minPA > paPerTurn[i])
                    {
                        turn = i;
                        minPA = paPerTurn[i];
                    }
                }
                else if (turn != -1) return new int[2] { -1, -1 };
            }

            return new int[2] { turn, minPA };
        }

        public int IsProtected()
        {
            if (iceberg.Owner == game.GetMyself())
            {
                if (ownerPerTurn.Find(p => p.Equals(game.GetEnemy())) != null)
                {
                    Player enemy = game.GetEnemy();
                    int index = ownerPerTurn.FindIndex(p => p.Equals(enemy));
                    return index;
                }
            }
            else if (iceberg.Owner == game.GetEnemy())
            {
                if (ownerPerTurn.Find(p => p.Equals(game.GetMyself())) != null)
                {
                    Player myself = game.GetMyself();
                    int index = ownerPerTurn.FindIndex(p => p.Equals(myself));
                    return index;
                }
            }
            else
            {
                if (ownerPerTurn.Find(p => p.Equals(game.GetEnemy())) != null)
                {
                    Player enemy = game.GetEnemy();
                    int index = ownerPerTurn.FindIndex(p => p.Equals(enemy));
                    return index;
                }
                if (ownerPerTurn.Find(p => p.Equals(game.GetMyself())) != null) return 1000001;
            }

            return 1000000;
        }


        public override string ToString()
        {
            string text = "Iceberg: " + iceberg;
            if (level != iceberg.Level) text += " | Level:" + level;
            if (paPerTurn[0] != iceberg.PenguinAmount) text += "PenguinAmount:" + paPerTurn[0];
            return text;
        }
    }

    public class IcebergAttack
    {
        public Dictionary<Iceberg, int> attackAmount;
        public Dictionary<Iceberg, int> attackAmountAfterAccelerate;
        public int sum;
        Iceberg iceberg;

        public IcebergAttack(Iceberg iceberg, Dictionary<Iceberg, int> attackAmount, Dictionary<Iceberg, int> attackAmountAfterAccelerate)
        {
            this.attackAmount = attackAmount;
            this.attackAmountAfterAccelerate = attackAmountAfterAccelerate;
            this.iceberg = iceberg;
            sum = attackAmount.Values.Sum();
        }

        public Iceberg GetIceberg() { return iceberg; }
    }
}