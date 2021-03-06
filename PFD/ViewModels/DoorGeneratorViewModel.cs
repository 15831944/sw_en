﻿using BaseClasses;
using BaseClasses.GraphObj;
using BaseClasses.Helpers;
using DATABASE;
using DATABASE.DTO;
using MATH;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace PFD
{
    public class DoorGeneratorViewModel : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        private string m_sDoorType;
        private float m_fDoorsHeight;
        private float m_fDoorsWidth;
        private float m_fDoorCoordinateXinBlock;
        private CoatingColour m_coatingColor;
        private List<CoatingColour> m_CoatingColors;

        private List<string> m_Series;
        private string m_Serie;
        private bool m_SerieEnabled;

        private bool m_AddDoors;
        private bool m_DeleteDoors;

        private List<string> m_DoorsTypes;
        public bool IsSetFromCode = false;

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        
        public List<string> DoorsTypes
        {
            get
            {
                if (m_DoorsTypes == null) m_DoorsTypes = new List<string>() { "Personnel Door", "Roller Door" };
                return m_DoorsTypes;
            }
        }
        

        public string DoorType
        {
            get
            {
                return m_sDoorType;
            }

            set
            {
                m_sDoorType = value;
                if (m_sDoorType == "Roller Door")
                {
                    Series = new List<string>() { "Domestic", "Roller Shutter" };
                    Serie = "Domestic";
                    SerieEnabled = true;
                }
                else
                {
                    Series = new List<string>();
                    SerieEnabled = false;
                }
                NotifyPropertyChanged("DoorType");
            }
        }

        public float DoorsHeight
        {
            get
            {
                return m_fDoorsHeight;
            }

            set
            {
                m_fDoorsHeight = value;
                NotifyPropertyChanged("DoorsHeight");
            }
        }

        public float DoorsWidth
        {
            get
            {
                return m_fDoorsWidth;
            }

            set
            {
                m_fDoorsWidth = value;
                NotifyPropertyChanged("DoorsWidth");
            }
        }

        public float DoorCoordinateXinBlock
        {
            get
            {
                return m_fDoorCoordinateXinBlock;
            }

            set
            {
                m_fDoorCoordinateXinBlock = value;
                NotifyPropertyChanged("DoorCoordinateXinBlock");
            }
        }
        public CoatingColour CoatingColor
        {
            get
            {
                return m_coatingColor;
            }

            set
            {
                m_coatingColor = value;
                NotifyPropertyChanged("CoatingColor");
            }
        }
        public List<CoatingColour> CoatingColors
        {
            get
            {
                if (m_CoatingColors == null) m_CoatingColors = CCoatingColorManager.LoadColours("AccessoriesSQLiteDB");
                return m_CoatingColors;
            }

            set
            {
                m_CoatingColors = value;
            }
        }

        public bool AddDoors
        {
            get
            {
                return m_AddDoors;
            }

            set
            {
                m_AddDoors = value;
            }
        }
        public bool DeleteDoors
        {
            get
            {
                return m_DeleteDoors;
            }

            set
            {
                m_DeleteDoors = value;
            }
        }

        public List<string> Series
        {
            get
            {
                if (m_Series == null)
                {
                    m_Series = new List<string>();
                }
                return m_Series;
            }

            set
            {
                m_Series = value;
                NotifyPropertyChanged("Series");
            }
        }

        public string Serie
        {
            get
            {
                return m_Serie;
            }

            set
            {
                m_Serie = value;
                NotifyPropertyChanged("Serie");
            }
        }
        public bool SerieEnabled
        {
            get
            {
                return m_SerieEnabled;
            }

            set
            {
                m_SerieEnabled = value;
                NotifyPropertyChanged("SerieEnabled");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public DoorGeneratorViewModel()
        {
            IsSetFromCode = true;
            AddDoors = false;
            DeleteDoors = false;
            DoorType = "Roller Door";
            DoorsHeight = 2.1f;
            DoorsWidth = 0.6f;
            DoorCoordinateXinBlock = 0.5f;
            CoatingColor = CoatingColors.First();

            IsSetFromCode = false;
        }

        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}