﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DATABASE;
using DATABASE.DTO;

namespace BaseClasses
{
    [Serializable]
    public class CrossBracingProperties : INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        private int m_iBayNumber;
        private int m_iBayNumber_old;

        private ObservableCollection<int> m_Bays;

        private bool m_bWallLeftSide;
        private bool m_bWallRightSide;
        private bool m_bRoof;

        private int m_iEveryXXPurlin; // Index of purlin 0 - no bracing 1 - every, 2 - every second purlin, 3 - every third purlin, ...

        private int m_iNumberOfCrossBracingMembers_WallLeftSide;
        private int m_iNumberOfCrossBracingMembers_WallRightSide;

        private int m_iMaximumNoOfCrossesPerRafter;
        private int m_iBayRoofCrossBracingCrossNumberPerRafter;
        private int m_iNumberOfCrossBracingMembers_BayRoof;

        private int iBayIndex;
        private int iRoofCrossBracingEveryXXPurlin;
        private int iNumberOfCrossBracingMembers_Walls;
        private int iRoofCrossBracingCrossNumberPerRafter;




        public bool IsSetFromCode = false;

        public int iBayNumber
        {
            get
            {
                return m_iBayNumber;
            }

            set
            {
                m_iBayNumber_old = m_iBayNumber;
                m_iBayNumber = value;
                NotifyPropertyChanged("iBayNumber");
            }
        }

        public bool bWallLeftSide
        {
            get
            {
                return m_bWallLeftSide;
            }

            set
            {
                m_bWallLeftSide = value;
                NotifyPropertyChanged("bWallLeftSide");
            }
        }

        public bool bWallRightSide
        {
            get
            {
                return m_bWallRightSide;
            }

            set
            {
                m_bWallRightSide = value;
                NotifyPropertyChanged("bWallRightSide");
            }
        }

        public bool bRoof
        {
            get
            {
                return m_bRoof;
            }

            set
            {
                m_bRoof = value;
                NotifyPropertyChanged("bRoof");
            }
        }

        public int iEveryXXPurlin
        {
            get
            {
                return m_iEveryXXPurlin;
            }

            set
            {
                m_iEveryXXPurlin = value;
                NotifyPropertyChanged("iEveryXXPurlin");
            }
        }

        public ObservableCollection<int> Bays
        {
            get
            {
                if (m_Bays == null) m_Bays = new ObservableCollection<int>();
                return m_Bays;
            }

            set
            {
                m_Bays = value;
                if(m_Bays != null) NotifyPropertyChanged("Bays");
            }
        }

        public int iBayNumber_old
        {
            get
            {
                return m_iBayNumber_old;
            }

            set
            {
                m_iBayNumber_old = value;
            }
        }

        public int iNumberOfCrossBracingMembers_WallLeftSide
        {
            get
            {
                return m_iNumberOfCrossBracingMembers_WallLeftSide;
            }

            set
            {
                m_iNumberOfCrossBracingMembers_WallLeftSide = value;
            }
        }

        public int iNumberOfCrossBracingMembers_WallRightSide
        {
            get
            {
                return m_iNumberOfCrossBracingMembers_WallRightSide;
            }

            set
            {
                m_iNumberOfCrossBracingMembers_WallRightSide = value;
            }
        }

        public int iMaximumNoOfCrossesPerRafter
        {
            get
            {
                return m_iMaximumNoOfCrossesPerRafter;
            }

            set
            {
                m_iMaximumNoOfCrossesPerRafter = value;
            }
        }

        public int iBayRoofCrossBracingCrossNumberPerRafter
        {
            get
            {
                return m_iBayRoofCrossBracingCrossNumberPerRafter;
            }

            set
            {
                m_iBayRoofCrossBracingCrossNumberPerRafter = value;
            }
        }

        public int iNumberOfCrossBracingMembers_BayRoof
        {
            get
            {
                return m_iNumberOfCrossBracingMembers_BayRoof;
            }

            set
            {
                m_iNumberOfCrossBracingMembers_BayRoof = value;
            }
        }

        public int BayIndex
        {
            get
            {
                return iBayIndex;
            }

            set
            {
                iBayIndex = value;
            }
        }

        public int RoofCrossBracingEveryXXPurlin
        {
            get
            {
                return iRoofCrossBracingEveryXXPurlin;
            }

            set
            {
                iRoofCrossBracingEveryXXPurlin = value;
            }
        }

        public int NumberOfCrossBracingMembers_Walls
        {
            get
            {
                return iNumberOfCrossBracingMembers_Walls;
            }

            set
            {
                iNumberOfCrossBracingMembers_Walls = value;
            }
        }

        public int RoofCrossBracingCrossNumberPerRafter
        {
            get
            {
                return iRoofCrossBracingCrossNumberPerRafter;
            }

            set
            {
                iRoofCrossBracingCrossNumberPerRafter = value;
            }
        }

        public CrossBracingProperties()
        {
        }

        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Validate()
        {
            bool isValid = true;

            // TODO doplnit

            return isValid;
        }

        public bool ValidateBays()
        {
            if (iBayNumber <= Bays.Count) return true;
            else return false;
        }
    }
}
