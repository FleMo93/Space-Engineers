using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        

        class LCDArray
        {
            IMyTextPanel[,] lcdArray;
            public LCDArray(IMyGridTerminalSystem gts, string lcdArrayName)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                gts.SearchBlocksOfName("[LCDArray]", blocks);
                lcdArray = new IMyTextPanel[2, 1];


                lcdArray[0, 0] = blocks.Where(x => x.CustomName == "[LCDArray][0,0]").FirstOrDefault() as IMyTextPanel;
                lcdArray[1, 0] = blocks.Where(x => x.CustomName == "[LCDArray][1,0]").FirstOrDefault() as IMyTextPanel;

                foreach (IMyTextPanel tp in lcdArray)
                {
                    tp.ShowPublicTextOnScreen();
                    tp.Font = "Monospace";
                }
            }

            public void Main(string argument, UpdateType updateSource)
            {
                //1 2
                string[] arguments = argument.Split(' ');

                if (arguments.Contains("showPlacing"))
                {
                    for (int x = 0; x < lcdArray.GetLength(1); x++)
                    {
                        for (int y = 0; y < lcdArray.GetLength(0); y++)
                        {
                            lcdArray[y, x].WritePublicText("[" + y + ", " + x + "]");

                        }
                    }
                }
            }
        }

        FontStyles[] styles = new FontStyles[] {
            new FontStyles { Size = 1, CharWidth = 27 },
            new FontStyles { Size = 2, CharWidth = 13 }};

        class FontStyles
        {
            public float Size;
            public int CharWidth;
        }
    }
}