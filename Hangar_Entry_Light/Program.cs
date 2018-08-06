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
        Doors doors;
        Lights lights;
        DangerLights dangerLights;
        LCD lcd;
        bool isRunning = false;

        public Program()
        {
            lcd = new LCD(GridTerminalSystem);
            doors = new Doors(GridTerminalSystem);
            lights = new Lights(GridTerminalSystem);
            dangerLights = new DangerLights(GridTerminalSystem);
        }

        public void Main(string arguments)
        {
            string[] args = arguments.Split(' ');

            if (args.Contains("open"))
            {
                doors.OpenLowerDoors();
                doors.OpenUpperDoors();
                dangerLights.TurnOnLowerLights();
                dangerLights.TurnOnUpperLights();

                if (!lights.LightsOn)
                {
                    lights.SwitchLights(false);
                }
            }
            else if (args.Contains("close"))
            {
                doors.CloseLowerDoors();
                doors.CloseUpperDoors();
                dangerLights.TurnOnLowerLights();
                dangerLights.TurnOnUpperLights();

                if (lights.LightsOn)
                {
                    lights.SwitchLights(false);
                }
            }
            else
            {
                if (args.Contains("openDoors"))
                {
                    doors.OpenLowerDoors();
                    doors.OpenUpperDoors();
                }
                else if (args.Contains("closeDoors"))
                {
                    doors.CloseLowerDoors();
                    doors.CloseUpperDoors();
                }
                else if (args.Contains("openUpperDoors"))
                {
                    doors.OpenUpperDoors();
                }
                else if (args.Contains("closeUpperDoors"))
                {
                    doors.CloseUpperDoors();
                }
                else if (args.Contains("openLowerDoors"))
                {
                    doors.OpenLowerDoors();
                }
                else if (args.Contains("closeLowerDoors"))
                {
                    doors.CloseLowerDoors();
                }


                if (args.Contains("switchLights"))
                {
                    lights.SwitchLights(false);
                }
                else if (args.Contains("switchLightsReverse"))
                {
                    lights.SwitchLights(true);
                }

                if(args.Contains("upperDangerLightsOn"))
                {
                    dangerLights.TurnOnUpperLights();
                }
                
                if(args.Contains("lowerDangerLightsOn"))
                {
                    dangerLights.TurnOnLowerLights();
                }

                if(args.Contains("upperDangerLightsOff"))
                {
                    dangerLights.TurnOffUpperLights();
                }

                if(args.Contains("lowerDangerLightsOff"))
                {
                    dangerLights.TurnOffLowerLights();
                }
            }

            if(lights.LightsSwitching)
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update10;
            }
            else
            {
                dangerLights.TurnOffUpperLights();
                dangerLights.TurnOffLowerLights();
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }

            lights.Main(arguments);
        }


        class Doors
        {
            IMyMotorStator tlM, trM, blM, brm;
            float rpm = 2;

            public Doors(IMyGridTerminalSystem gts)
            {
                tlM = gts.GetBlockWithName("[Base]tlM") as IMyMotorStator;
                trM = gts.GetBlockWithName("[Base]trM") as IMyMotorStator;
                blM = gts.GetBlockWithName("[Base]blM") as IMyMotorStator;
                brm = gts.GetBlockWithName("[Base]brM") as IMyMotorStator;
            }

            public void OpenUpperDoors()
            {
                tlM.TargetVelocityRPM = rpm;
                trM.TargetVelocityRPM = -rpm;
            }

            public void CloseUpperDoors()
            {
                tlM.TargetVelocityRPM = -rpm;
                trM.TargetVelocityRPM = rpm;
                
            }

            public void OpenLowerDoors()
            {
                brm.TargetVelocityRPM = rpm;
                blM.TargetVelocityRPM = -rpm;
            }

            public void CloseLowerDoors()
            {
                brm.TargetVelocityRPM = -rpm;
                blM.TargetVelocityRPM = rpm;
            }
        }

        class Lights
        {
            private LightGroup[] lightGroups;
            private int lightGroupCount = 37;
            private int lightGroupSize = 6;
            private int lightGroupsLeft = 0;
            private bool lightState = false;
            private bool reverseDirection = false;
            private int lightsDelay = 5;
            private int lightsDelayLeft;

            public bool LightsSwitching
            {
                get
                {
                    return lightGroupsLeft > 0;
                }
            }
            public bool LightsOn
            {
                get
                {
                    return lightState;
                }
            }

            public Lights(IMyGridTerminalSystem gts)
            {

                lightGroups = new LightGroup[lightGroupCount];

                for (int i = 0; i < lightGroupCount; i++)
                {
                    List<IMyInteriorLight> lightGroup = new List<IMyInteriorLight>();

                    for (int j = 0; j < lightGroupSize; j++)
                    {
                        IMyInteriorLight light = gts.GetBlockWithName("Licht" + i + "." + j) as IMyInteriorLight;

                        if (light == null)
                        {
                            string error = "Licht " + i + "." + j + " nicht gefunden.";
                        }
                        else
                        {
                            lightGroup.Add(light);
                            light.Enabled = lightState;
                        }
                    }

                    lightGroups[i] = new LightGroup(lightGroup.ToArray());
                }
            }

            public void SwitchLights(bool reverseDirection)
            {
                if(LightsSwitching)
                {
                    return;
                }

                if (lightGroupsLeft == 0)
                {
                    lightGroupsLeft = lightGroups.Length;
                    lightsDelayLeft = lightsDelay;
                }

                this.reverseDirection = reverseDirection;
            }



            public void Main(string direction)
            {
                if(lightGroupsLeft == 0)
                {
                    return;
                }

                if(lightsDelayLeft > 0)
                {
                    lightsDelayLeft--;
                    return;
                }

                int lightToSwitch = lightGroupsLeft - 1;

                if (reverseDirection)
                {
                    lightGroups[lightGroups.Length - 1 - lightToSwitch].Enable(!lightState);
                }
                else
                {
                    lightGroups[lightToSwitch].Enable(!lightState);
                }

                lightGroupsLeft--;


                if (lightGroupsLeft == 0)
                {
                    lightState = !lightState;
                    reverseDirection = false;
                }
            }


            class LightGroup
            {
                private IMyInteriorLight[] lights;

                public LightGroup(IMyInteriorLight[] lights)
                {
                    this.lights = lights;
                }

                public void Enable(bool enable)
                {
                    foreach (IMyInteriorLight light in lights)
                    {
                        light.Enabled = enable;
                    }
                }
            }
        }

        class DangerLights
        {
            int dangerUpperMotorCount = 1;
            int dangerUpperLightCount = 2;
            int dangerLowerMotorCount = 0;
            int dangerLowerLightCount = 0;

            DangerLightGroup upperLightGroup;
            DangerLightGroup lowerLightGroup;


            public DangerLights(IMyGridTerminalSystem mts)
            {
                IMyMotorStator[] upperMotors = new IMyMotorStator[dangerUpperMotorCount];
                IMyLightingBlock[] upperLights = new IMyLightingBlock[dangerUpperLightCount];
                IMyMotorStator[] lowerMotors = new IMyMotorStator[dangerLowerMotorCount];
                IMyLightingBlock[] lowerLights = new IMyLightingBlock[dangerLowerLightCount];

                for (int i = 0; i < dangerUpperMotorCount; i++)
                {
                    upperMotors[i] = mts.GetBlockWithName("[Base]DoorDangerUpperMotor" + i) as IMyMotorStator;
                    upperMotors[i].TargetVelocityRPM = 0;
                }

                for (int i = 0; i < dangerUpperLightCount; i++)
                {
                    upperLights[i] = mts.GetBlockWithName("[Base]DoorDangerUpperLight" + i) as IMyLightingBlock;
                    upperLights[i].Enabled = false;
                }

                for (int i = 0; i < dangerLowerMotorCount; i++)
                {
                    lowerMotors[i] = mts.GetBlockWithName("[Base]DoorDangerLowerMotor" + i) as IMyMotorStator;
                    lowerMotors[i].TargetVelocityRPM = 0;
                }

                for (int i = 0; i < dangerLowerLightCount; i++)
                {
                    lowerLights[i] = mts.GetBlockWithName("[Base]DoorDangerLowerLight" + i) as IMyLightingBlock;
                    lowerLights[i].Enabled = false;
                }

                upperLightGroup = new DangerLightGroup(upperMotors, upperLights);
                lowerLightGroup = new DangerLightGroup(lowerMotors, lowerLights);
            }

            public void TurnOnUpperLights()
            {
                upperLightGroup.SwitchState(true);
            }

            public void TurnOffUpperLights()
            {
                upperLightGroup.SwitchState(false);
            }

            public void TurnOnLowerLights()
            {
                lowerLightGroup.SwitchState(true);
            }

            public void TurnOffLowerLights()
            {
                lowerLightGroup.SwitchState(false);
            }

            class DangerLightGroup
            {
                IMyMotorStator[] motors;
                IMyLightingBlock[] lights;
                float rpm = 20;

                public DangerLightGroup(IMyMotorStator[] motors, IMyLightingBlock[] lights)
                {
                    this.motors = motors;
                    this.lights = lights;
                }

                public void SwitchState(bool state)
                {
                    foreach(IMyMotorStator motor in motors)
                    {
                        if(state)
                        {
                            motor.TargetVelocityRPM = rpm;
                        }
                        else
                        {
                            motor.TargetVelocityRPM = 0;
                        }
                    }

                    foreach(IMyLightingBlock light in lights)
                    {
                        light.Enabled = state;
                    }
                }
            }
        }

        class DoorProgress
        {
            IMySensorBlock[] blocks;

            public DoorProgress(IMyGridTerminalSystem gts)
            {
                
            }
        }

        class LCD
        {
            IMyTextPanel[] textPanels;
            public LCD(IMyGridTerminalSystem gts)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                gts.SearchBlocksOfName("[Base]HangarDoorLCD", blocks);
                
                textPanels = blocks.Where(x => x as IMyTextPanel != null).Select(x => x as IMyTextPanel).ToArray();

                foreach(var x in textPanels)
                {
                    x.ShowPublicTextOnScreen();
                }

                SetText("Hallo");
            }

            public void SetText(string text)
            {
                foreach(var textPanel in textPanels)
                {
                    textPanel.WritePublicText(text);
                }
            }
        }
    }
}