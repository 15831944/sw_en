﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DATABASE;
using DATABASE.DTO;

namespace BaseClasses
{
    public class WindowProperties : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string m_sBuildingSide;
        private int m_iBayNumber;
        private List<int> m_Bays;
        private float m_fWindowsHeight;
        private float m_fWindowsWidth;
        private float m_fWindowCoordinateXinBay;
        private float m_fWindowCoordinateZinBay;
        private int m_iNumberOfWindowColumns;

        private CoatingColour m_coatingColor;

        //validationValues
        private float m_WallHeight = float.NaN;
        private float m_L1 = float.NaN;
        private float m_distFrontColumns = float.NaN;
        private float m_distBackColumns = float.NaN;

        public bool IsSetFromCode = false;

        public string sBuildingSide
        {
            get
            {
                return m_sBuildingSide;
            }

            set
            {
                string temp = m_sBuildingSide;
                m_sBuildingSide = value;

                if (!ValidateWindowInsideBay())
                {
                    m_sBuildingSide = temp;
                    MessageBox.Show("Window outside of bay width.");
                }
                else NotifyPropertyChanged("sBuildingSide");
            }
        }

        public int iBayNumber
        {
            get
            {
                return m_iBayNumber;
            }

            set
            {
                m_iBayNumber = value;
                NotifyPropertyChanged("iBayNumber");
            }
        }

        public float fWindowsHeight
        {
            get
            {
                return m_fWindowsHeight;
            }

            set
            {
                m_fWindowsHeight = value;
                NotifyPropertyChanged("fWindowsHeight");
            }
        }

        public float fWindowsWidth
        {
            get
            {
                return m_fWindowsWidth;
            }

            set
            {
                float temp = m_fWindowsWidth;
                m_fWindowsWidth = value;
                if (!ValidateWindowInsideBay())
                {
                    m_fWindowsWidth = temp;
                    MessageBox.Show("Window outside of bay width.");

                }
                else NotifyPropertyChanged("fWindowsWidth");
            }
        }

        public float fWindowCoordinateXinBay
        {
            get
            {
                return m_fWindowCoordinateXinBay;
            }

            set
            {
                float temp = m_fWindowCoordinateXinBay;
                m_fWindowCoordinateXinBay = value;
                if (!ValidateWindowInsideBay()) {
                    m_fWindowCoordinateXinBay = temp;
                    MessageBox.Show("Window outside of bay width.");

                }
                else NotifyPropertyChanged("fWindowCoordinateXinBay");
            }
        }

        public float fWindowCoordinateZinBay
        {
            get
            {
                return m_fWindowCoordinateZinBay;
            }

            set
            {
                m_fWindowCoordinateZinBay = value;
                NotifyPropertyChanged("fWindowCoordinateZinBay");
            }
        }

        public int iNumberOfWindowColumns
        {
            get
            {
                return m_iNumberOfWindowColumns;
            }

            set
            {
                m_iNumberOfWindowColumns = value;
                NotifyPropertyChanged("iNumberOfWindowColumns");
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

        public List<int> Bays
        {
            get
            {
                if (m_Bays == null) m_Bays = new List<int>();
                return m_Bays;
            }

            set
            {
                m_Bays = value;
                if (m_Bays != null) NotifyPropertyChanged("Bays");
            }
        }

        public WindowProperties() { }


        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(m_sBuildingSide)) return false;
            
            if (m_iBayNumber < 0) return false;
            if (m_fWindowsHeight < 0.1f) return false;
            if (m_fWindowsWidth < 0.1f) return false;
            if (m_fWindowCoordinateXinBay < 0f) return false;
            if (m_fWindowCoordinateZinBay < 0f) return false;
            if (m_iNumberOfWindowColumns < 0) return false;
            
            return isValid;
        }

        public bool ValidateWindowInsideBay()
        {
            //TODO implement according to DoorProperties
            float fBayWidth = 0;
            if (sBuildingSide == "Left" || sBuildingSide == "Right")
            {
                fBayWidth = m_L1;
            }
            else if (sBuildingSide == "Front")
            {
                fBayWidth = m_distFrontColumns;

            }
            else if (sBuildingSide == "Back")
            {
                fBayWidth = m_distBackColumns;
            }

            if (float.IsNaN(fBayWidth)) return true;
            if (fBayWidth < m_fWindowsWidth+ m_fWindowCoordinateXinBay) return false;
            
            return true;
        }

        public void SetValidationValues(float wallHeight, float L1, float distFrontColumns, float distBackColumns)
        {
            m_WallHeight = wallHeight;
            m_L1 = L1;
            m_distFrontColumns = distFrontColumns;
            m_distBackColumns = distBackColumns;

        }
    }
}
