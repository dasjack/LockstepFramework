﻿using UnityEngine;
using System.Collections;
using Lockstep.UI;
using Lockstep.Data;

namespace Lockstep
{
    public class RTSPlayerInterfacingHelper : PlayerInterfacingHelper
    {
        public static GUIManager GUIManager;

        private static AbilityInterfacer _currentInterfacer;

        public static AbilityInterfacer CurrentInterfacer
        {
            get { return _currentInterfacer;}
            set
            {
                if (value .IsNotNull())
                {
                    IsGathering = true;
                }
                _currentInterfacer = value;
            }
        }

        private static AbilityInterfacer QuickPos;
        private static AbilityInterfacer QuickTarget;

        public static bool IsGathering { get; private set; }

        static void Setup()
        {
            QuickPos = AbilityInterfacer.FindInterfacer ("Move");
            QuickTarget = AbilityInterfacer.FindInterfacer("Scan");

            Setted = true;
        }
        static bool Setted = false;

        protected override void OnInitialize()
        {
            if (!Setted)
                Setup ();
            SelectionManager.Initialize();

            RTSInterfacing.Initialize();
            IsGathering = false;
            CurrentInterfacer = null;
        }

        static Command curCom;
        protected override void OnVisualize()
        {
            SelectionManager.Update();

            if (CommandManager.sendType == SendState.None)
                return;
            RTSInterfacing.Visualize();

            if (IsGathering)
            {
                if (InputManager.GetQuickDown())
                {
                    IsGathering = false;
                    return;
                }

                if (InputManager.GetInformationDown() || CurrentInterfacer.InformationGather == InformationGatherType.None)
                {
                    ProcessInterfacer(CurrentInterfacer);
                }
            } else
            {
                if (Selector.MainSelectedAgent != null) {
                if (InputManager.GetQuickDown())
                {
                    LSAgent target;
                    if (RTSInterfacing.MousedAgent.IsNotNull() &&
                        PlayerManager.GetAllegiance(RTSInterfacing.MousedAgent) == AllegianceType.Enemy && 
                        Selector.MainSelectedAgent.Scanner != null)
                    {
                        ProcessInterfacer((QuickTarget));
                    } else
                    {
                        ProcessInterfacer((QuickPos));
                    }
                }
                }
            }
        }

        private static void ProcessInterfacer(AbilityInterfacer facer)
        {
            switch (facer.InformationGather)
            {
                case InformationGatherType.Position:
                    curCom = new Command(facer.ListenInput);
                    curCom.Position = RTSInterfacing.GetWorldPosD(Input.mousePosition);
                    break;
                case InformationGatherType.Target:
                    curCom = new Command(facer.ListenInput);
                    if (RTSInterfacing.MousedAgent .IsNotNull())
                    {
                        curCom.Target = RTSInterfacing.MousedAgent.LocalID;
                    }
                    break;
                case InformationGatherType.PositionOrTarget:
                    curCom = new Command(facer.ListenInput);
                    if (RTSInterfacing.MousedAgent .IsNotNull())
                    {
                        curCom.Target = RTSInterfacing.MousedAgent.GlobalID;
                    } else
                    {
                        curCom.Position = RTSInterfacing.GetWorldPosD(Input.mousePosition);
                    }
                    break;
                case InformationGatherType.None:
                    curCom = new Command(facer.ListenInput);
                    break;
            }
            if (facer.MarkType != MarkerType.None)
            {
                RTSInterfacing.ActivateMarkerOnMouse(facer.MarkType);
            }
            Send(curCom);
        }

        public void DrawGUI () {

        }
        protected virtual void OnDrawGUI () {
            SelectionManager.DrawBox(GUIStyle.none);
        }
        
        private static void Send(Command com)
        {
            IsGathering = false;
            PlayerManager.SendCommand(com);
        }
    }
}