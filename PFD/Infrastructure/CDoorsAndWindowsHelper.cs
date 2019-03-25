﻿using BaseClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFD.Infrastructure
{
    public static class CDoorsAndWindowsHelper
    {
        public static ObservableCollection<DoorProperties> GetDefaultDoorProperties()
        {
            ObservableCollection<DoorProperties> DoorBlocks = new ObservableCollection<DoorProperties>();
            DoorProperties doorProps = new DoorProperties();
            doorProps.sBuildingSide = "Front";
            doorProps.iBayNumber = 2;
            doorProps.sDoorType = "Personnel Door";
            doorProps.fDoorsHeight = 2.1f;
            doorProps.fDoorsWidth = 0.8f;
            doorProps.fDoorCoordinateXinBlock = 0.5f;
            DoorBlocks.Add(doorProps);

            return DoorBlocks;
        }

        public static void SetDefaultDoorParams(DoorProperties d)
        {
            d.sBuildingSide = "Front";
            d.iBayNumber = 1;
            d.sDoorType = "Personnel Door";
            d.fDoorsWidth = 0.8f;
            d.fDoorsHeight = 2.1f;
            d.fDoorCoordinateXinBlock = 0.4f;
        }
        public static void SetDefaultWindowParams(WindowProperties w)
        {
            w.sBuildingSide = "Front";
            w.iBayNumber = 1;
            w.fWindowsHeight = 0.6f;
            w.fWindowsWidth = 0.6f;
            w.fWindowCoordinateXinBay = 0.4f;
            w.fWindowCoordinateZinBay = 2f;
            w.iNumberOfWindowColumns = 2;
        }

        public static ObservableCollection<WindowProperties> GetDefaultWindowsProperties()
        {
            ObservableCollection<WindowProperties> WindowBlocks = new ObservableCollection<WindowProperties>();
            WindowProperties windowProps = new WindowProperties();
            windowProps.sBuildingSide = "Front";
            windowProps.iBayNumber = 1;
            windowProps.fWindowsHeight = 0.6f;
            windowProps.fWindowsWidth = 0.6f;
            windowProps.fWindowCoordinateXinBay = 0.4f;
            windowProps.fWindowCoordinateZinBay = 0.8f;
            windowProps.iNumberOfWindowColumns = 2;
            WindowBlocks.Add(windowProps);

            return WindowBlocks;
        }
    }
}
