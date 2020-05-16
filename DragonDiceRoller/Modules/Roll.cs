using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DragonDiceRoller.Modules
{
    //TODO: Add emojis based on die type
    //TODO: Add math operations to die roll
    public class Roll : ModuleBase<SocketCommandContext>
    {
        private readonly char _cDieTypeCharacter = 'd';
        private readonly char[] _cMathOpCharacters = { '+', '-', '*', '/'};

        [Command("roll")]
        [Summary("Will roll dice if you specify the number (N) and type (T) of dice you want to roll in the NdT format. Defaults to 3d6 if no dice are specified.")]
        [Remarks("You may specify the number (N) and type (T) of dice you want to roll in the NdT format.\n" +
                 "-Ex. '/roll 2d10' will roll two ten-sided dice.")]
        public async Task RollAsync(string sInput = "")
        {            
            int iDieNum = 0;
            int iDieType = 0;
            int iPositionD = 0;
            int iPositionMathOp = 0;
            int iOperand = 0;
            int iTotalSum = 0;
            List<int> lstDieResults = new List<int>();
            
            string sResults = "";
            string sDragonDie = "";
            string sStuntPoints = "";
            string sUser = "";

            char cOperator = '¿';

            EmbedBuilder embedBuilder = new EmbedBuilder();

            //if a number of dice were specified as argument
            if (sInput != "")
            {    
                //Explain how to use roll command
                if (sInput == "?" || sInput.ToLower().Trim() == "help")
                {
                    sResults = "* /roll will default to 3d6 and specify the dragon die and stunt points.\n" +
                               "* Specify the number (N) and type (T) of dice you want to roll in NdT format.\n" +
                               "* Math operators (+,-,*,/) may also be used at the tail end of a roll command.\n" +
                               "-Ex. '/roll 2d10+4' will roll two ten-sided dice and add 4 to the total roll.\n" + 
                               "-Ex '/roll +4' will roll three six-sided dice and add 4 to the total roll."; 
                }

                //Example: sDiceRoll = '1d20'
                //if exists, determine index of 'd' character. Ex: iPositionD = '1'
                else if ((iPositionD = sInput.ToLower().IndexOf(_cDieTypeCharacter)) > 0)
                {
                    try
                    {
                        //determine value of number before 'd'. Ex: iDieNum = 1
                        iDieNum = int.Parse(sInput.Substring(0, iPositionD));
                    }

                    catch (Exception ex)
                    {
                        sResults += "Exception occured dice parsing: " + ex.Message + " ";
                    }
                }

                try
                {
                    //if exists, determine index of operator character 
                    if ((iPositionMathOp = sInput.IndexOfAny(_cMathOpCharacters)) > -1)
                    {
                        if (iPositionD > 0)
                        {
                            //determine value of number after 'd' but before math operator
                            iDieType = int.Parse(sInput.Substring(iPositionD + 1, iPositionMathOp - (iPositionD + 1)));
                        }

                        else
                        {
                            iDieNum = 3;
                            iDieType = 6;
                        }

                        //store value of operator
                        cOperator = sInput[iPositionMathOp];

                        //determine value of operand after operator
                        iOperand = int.Parse(sInput.Substring(iPositionMathOp + 1));
                    }
                    else if (iPositionD > 0)
                    {
                        //determine value of number after 'd'. Ex: iDieNum = 20
                        iDieType = int.Parse(sInput.Substring(iPositionD + 1));
                    }
                }

                catch (Exception ex)
                {
                    sResults += "Exception occured during math operation: " + ex.Message + " ";
                }
            }
            
            //else default to '3d6'
            else
            {
                iDieNum = 3;
                iDieType = 6;
            }   

            //generate random dice rolls
            if (iDieNum > 0 && iDieType > 0 && sResults == "")
            {
                for (int i = 0; i < iDieNum; i++)
                {
                    Random r = new Random();

                    int iNum = r.Next(1, iDieType+1);

                    iTotalSum += iNum;
                    lstDieResults.Add(iNum);

                    sResults += iNum.ToString();

                    if (i < iDieNum - 1)
                    {
                        sResults += " + ";
                    }  
                    
                    //this is last roll, but not the first
                    if (i == iDieNum - 1 && iDieNum > 1)
                    {
                        //show total
                        sResults += " = **" + iTotalSum.ToString() + "**";
                    }                                   
                }

                if (cOperator != '¿' && iOperand != 0 && iPositionMathOp > -1)
                {
                    //show total
                    sResults += " " + cOperator + " " + iOperand.ToString() + " = **";

                    switch (cOperator)
                    {
                        case '+':
                            sResults += (iTotalSum + iOperand).ToString();
                            break;

                        case '-':
                            sResults += (iTotalSum - iOperand).ToString();
                            break;

                        case '*':
                            sResults += (iTotalSum * iOperand).ToString();
                            break;

                        case '/':
                            sResults += (iTotalSum / iOperand).ToString();
                            
                            break;

                        default:
                            sResults += Environment.NewLine + "Use of invalid operator!";
                            break;
                    }

                    sResults += "**";
                }

                if (iDieNum == 3 && iDieType == 6 && lstDieResults.Count == 3)
                { 
                    //explicitly mentions dragon die 
                    sDragonDie += "```fix" + Environment.NewLine + "Dragon Die = " + lstDieResults[2].ToString() + Environment.NewLine + "```";

                    if (lstDieResults[0] == lstDieResults[1] || lstDieResults[0] == lstDieResults[2] || lstDieResults[1] == lstDieResults[2])
                    {
                        sStuntPoints += "```prolog" + Environment.NewLine + "Stunt Points!" + Environment.NewLine + "```";
                    }
                }
            }

            else
            {
                if (sInput != "?" && sInput.ToLower().Trim() != "help")
                {
                    sResults += "Unable to interpret command";
                }
            }

            sUser = Context.User.ToString();
            sUser = sUser.Substring(0, sUser.IndexOf('#'));

            embedBuilder.AddInlineField("Dice roll for " + sUser, sResults + sDragonDie + sStuntPoints);
            embedBuilder.WithColor(Color.Purple);

            await ReplyAsync("", false, embedBuilder.Build());
        }
    }
}
