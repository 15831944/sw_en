﻿using DATABASE;
using DATABASE.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace PFD
{
    public class CFootingInputVM : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------

        private Dictionary<string, CMatPropertiesRC> m_ConcreteGrades;
        private Dictionary<string, CMatPropertiesRF> m_ReinforcementGrades;
        private Dictionary<int, CReinforcementBarProperties> m_ReinforcementBars;

        private List<string> m_ConcreteGradesList;
        private List<string> m_ReinforcementGradesList;
        private List<string> m_ReinforcementBarsList;
        private List<string> m_ReinforcementBarsCountList;

        private int m_FootingPadMemberTypeIndex;
        private string m_ConcreteGrade;
        private float m_ConcreteDensity;
        private string m_ReinforcementGrade;

        private string m_LongReinTop_x_No;
        private string m_LongReinTop_x_Phi;
        private float m_LongReinTop_x_distance_s_y;

        private string m_LongReinTop_y_No;
        private string m_LongReinTop_y_Phi;
        private float m_LongReinTop_y_distance_s_x;

        private string m_LongReinBottom_x_No;
        private string m_LongReinBottom_x_Phi;
        private float m_LongReinBottom_x_distance_s_y;

        private string m_LongReinBottom_y_No;
        private string m_LongReinBottom_y_Phi;
        private float m_LongReinBottom_y_distance_s_x;

        private float m_FootingPadSize_x_Or_a;
        private float m_FootingPadSize_y_Or_b;
        private float m_FootingPadSize_z_Or_h;

        private float m_SoilReductionFactor_Phi;
        private float m_SoilReductionFactorEQ_Phi;

        private float m_SoilBearingCapacity;
        private float m_ConcreteCover;
        private float m_FloorSlabThickness;

        public bool IsSetFromCode = false;

        //-------------------------------------------------------------------------------------------------------------
        public Dictionary<string, CMatPropertiesRC> ConcreteGrades
        {
            get
            {
                return m_ConcreteGrades;
            }

            set
            {
                m_ConcreteGrades = value;
                NotifyPropertyChanged("ConcreteGrades");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public Dictionary<string, CMatPropertiesRF> ReinforcementGrades
        {
            get
            {
                return m_ReinforcementGrades;
            }

            set
            {
                m_ReinforcementGrades = value;
                NotifyPropertyChanged("ReinforcementGrades");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public Dictionary<int, CReinforcementBarProperties> ReinforcementBars
        {
            get
            {
                return m_ReinforcementBars;
            }

            set
            {
                m_ReinforcementBars = value;
                NotifyPropertyChanged("ReinforcementBars");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public List<string> ConcreteGradesList
        {
            get
            {
                return m_ConcreteGradesList;
            }

            set
            {
                m_ConcreteGradesList = value;
                NotifyPropertyChanged("ConcreteGradesList");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public List<string> ReinforcementGradesList
        {
            get
            {
                return m_ReinforcementGradesList;
            }

            set
            {
                m_ReinforcementGradesList = value;
                NotifyPropertyChanged("ReinforcementGradesList");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public List<string> ReinforcementBarsList
        {
            get
            {
                return m_ReinforcementBarsList;
            }

            set
            {
                m_ReinforcementBarsList = value;
                NotifyPropertyChanged("ReinforcementBarsList");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public List<string> ReinforcementBarsCountList
        {
            get
            {
                return m_ReinforcementBarsCountList;
            }

            set
            {
                m_ReinforcementBarsCountList = value;
                NotifyPropertyChanged("ReinforcementBarsCountList");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int FootingPadMemberTypeIndex
        {
            get
            {
                return m_FootingPadMemberTypeIndex;
            }

            set
            {
                m_FootingPadMemberTypeIndex = value;
                NotifyPropertyChanged("FootingPadMemberTypeIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string ConcreteGrade
        {
            get
            {
                return m_ConcreteGrade;
            }

            set
            {
                m_ConcreteGrade = value;
                NotifyPropertyChanged("ConcreteGrade");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float ConcreteDensity
        {
            get
            {
                return m_ConcreteDensity;
            }

            set
            {
                if (value < 1800 || value > 2800)
                    throw new ArgumentException("Concrete density must be between 1800 and 2800 [kg/m³]");

                m_ConcreteDensity = value;
                NotifyPropertyChanged("ConcreteDensity");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string ReinforcementGrade
        {
            get
            {
                return m_ReinforcementGrade;
            }

            set
            {
                m_ReinforcementGrade = value;
                NotifyPropertyChanged("ReinforcementGrade");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string LongReinTop_x_No
        {
            get
            {
                return m_LongReinTop_x_No;
            }

            set
            {
                m_LongReinTop_x_No = value;
                NotifyPropertyChanged("LongReinTop_x_No");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string LongReinTop_x_Phi
        {
            get
            {
                return m_LongReinTop_x_Phi;
            }

            set
            {
                m_LongReinTop_x_Phi = value;
                NotifyPropertyChanged("LongReinTop_x_Phi");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float LongReinTop_x_distance_s_y
        {
            get
            {
                return m_LongReinTop_x_distance_s_y;
            }

            set
            {
                m_LongReinTop_x_distance_s_y = value;
                NotifyPropertyChanged("LongReinTop_x_distance_s_y");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string LongReinTop_y_No
        {
            get
            {
                return m_LongReinTop_y_No;
            }

            set
            {
                m_LongReinTop_y_No = value;
                NotifyPropertyChanged("LongReinTop_y_No");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string LongReinTop_y_Phi
        {
            get
            {
                return m_LongReinTop_y_Phi;
            }

            set
            {
                m_LongReinTop_y_Phi = value;
                NotifyPropertyChanged("LongReinTop_y_Phi");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float LongReinTop_y_distance_s_x
        {
            get
            {
                return m_LongReinTop_y_distance_s_x;
            }

            set
            {
                m_LongReinTop_y_distance_s_x = value;
                NotifyPropertyChanged("LongReinTop_y_distance_s_x");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string LongReinBottom_x_No
        {
            get
            {
                return m_LongReinBottom_x_No;
            }

            set
            {
                m_LongReinBottom_x_No = value;
                NotifyPropertyChanged("LongReinBottom_x_No");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string LongReinBottom_x_Phi
        {
            get
            {
                return m_LongReinBottom_x_Phi;
            }

            set
            {
                m_LongReinBottom_x_Phi = value;
                NotifyPropertyChanged("LongReinBottom_x_Phi");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float LongReinBottom_x_distance_s_y
        {
            get
            {
                return m_LongReinBottom_x_distance_s_y;
            }

            set
            {
                m_LongReinBottom_x_distance_s_y = value;
                NotifyPropertyChanged("LongReinBottom_x_distance_s_y");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string LongReinBottom_y_No
        {
            get
            {
                return m_LongReinBottom_y_No;
            }

            set
            {
                m_LongReinBottom_y_No = value;
                NotifyPropertyChanged("LongReinBottom_y_No");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public string LongReinBottom_y_Phi
        {
            get
            {
                return m_LongReinBottom_y_Phi;
            }

            set
            {
                m_LongReinBottom_y_Phi = value;
                NotifyPropertyChanged("LongReinBottom_y_Phi");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float LongReinBottom_y_distance_s_x
        {
            get
            {
                return m_LongReinBottom_y_distance_s_x;
            }

            set
            {
                m_LongReinBottom_y_distance_s_x = value;
                NotifyPropertyChanged("LongReinBottom_y_distance_s_x");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float FootingPadSize_x_Or_a
        {
            get
            {
                return m_FootingPadSize_x_Or_a;
            }

            set
            {
                if (value < 0.4f || value > 5f)
                    throw new ArgumentException("Footing pad size must be between 0.4 and 5 [m]");

                m_FootingPadSize_x_Or_a = value;
                NotifyPropertyChanged("FootingPadSize_x_Or_a");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float FootingPadSize_y_Or_b
        {
            get
            {
                return m_FootingPadSize_y_Or_b;
            }

            set
            {
                if (value < 0.4f || value > 5f)
                    throw new ArgumentException("Footing pad size must be between 0.4 and 5 [m]");

                m_FootingPadSize_y_Or_b = value;
                NotifyPropertyChanged("FootingPadSize_y_Or_b");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float FootingPadSize_z_Or_h
        {
            get
            {
                return m_FootingPadSize_z_Or_h;
            }

            set
            {
                if (value < 0.1f || value > 2f)
                    throw new ArgumentException("Footing pad size must be between 0.1 and 2 [m]");

                m_FootingPadSize_z_Or_h = value;
                NotifyPropertyChanged("FootingPadSize_z_Or_h");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float SoilReductionFactor_Phi
        {
            get
            {
                return m_SoilReductionFactor_Phi;
            }

            set
            {
                if (value < 0.3f || value > 1f)
                    throw new ArgumentException("Soil reduction factor must be between 0.3 and 1 [-]");

                m_SoilReductionFactor_Phi = value;
                NotifyPropertyChanged("SoilReductionFactor_Phi");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float SoilReductionFactorEQ_Phi
        {
            get
            {
                return m_SoilReductionFactorEQ_Phi;
            }

            set
            {
                if (value < 0.3f || value > 1f)
                    throw new ArgumentException("Soil reduction factor must be between 0.3 and 1 [-]");

                m_SoilReductionFactorEQ_Phi = value;
                NotifyPropertyChanged("SoilReductionFactorEQ_Phi");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float SoilBearingCapacity
        {
            get
            {
                return m_SoilBearingCapacity;
            }

            set
            {
                if (value < 30f || value > 800f)
                    throw new ArgumentException("Soil bearing capacity must be between 30 and 800 [kPa]");

                m_SoilBearingCapacity = value;
                NotifyPropertyChanged("SoilBearingCapacity");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float ConcreteCover
        {
            get
            {
                return m_ConcreteCover;
            }

            set
            {
                if (value < 10f || value > 200f)
                    throw new ArgumentException("Concrete cover must be between 10 and 200 [mm]");

                m_ConcreteCover = value;
                NotifyPropertyChanged("ConcreteCover");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float FloorSlabThickness
        {
            get
            {
                return m_FloorSlabThickness;
            }

            set
            {
                if (value < 50f || value > 500f)
                    throw new ArgumentException("Floor slab thickness must be between 50 and 500 [mm]");

                m_FloorSlabThickness = value;
                NotifyPropertyChanged("FloorSlabThickness");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public CFootingInputVM()
        {
            // Fill dictionaries
            ConcreteGrades = CMaterialManager.LoadMaterialPropertiesRC();
            ReinforcementGrades = CMaterialManager.LoadMaterialPropertiesRF();
            ReinforcementBars = CReinforcementBarManager.LoadReiforcementBarProperties();

            // To Ondrej - asi by som mal urobit zoznamy objektov vlastnosti/properties priamo v Database Manager
            // v Database Manager mame niekde dictionary, niekde list of properties, neviem ci nam to fakt oboje treba a ci by to nemalo byt jednotne vsade jedno alebo druhe (dictionary alebo list of properties objects)
            // Convert dictionary keys to list of strings - used for combobox items
            ConcreteGradesList = ConcreteGrades.Keys.ToList();
            ReinforcementGradesList = ReinforcementGrades.Keys.ToList();
            List<int> rcBarsDiameters = ReinforcementBars.Keys.ToList();
            ReinforcementBarsList = rcBarsDiameters.ConvertAll<string>(x => x.ToString());

            // Zoznam poctov vyztuznych tyci pre jeden smer (None alebo 2 - 30)
            ReinforcementBarsCountList = GetReinforcementBarsCountList();







            IsSetFromCode = false;
        }

        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<string> GetReinforcementBarsCountList()
        {
            List<string> list = new List<string>();

            list.Add("None"); // count of bars = 0, unused reinforcement

            int iMinimumNumberOfBars = 2;
            int iMaximumNumberOfBars = 30;

            for (int i = iMinimumNumberOfBars; i <= iMaximumNumberOfBars; i++)
                list.Add(i.ToString());

            return list;
        }

        // TO Ondrej - neviem ci ma byt toho vo viewmodeli alebo UC_FootingInputxaml.cs
        // Jedna sa o prepocitanie hodnot a zobrazv GUI pri zmene niektorej hodnoty v GUI
        private void UpdateValuesInGUI()
        {




        }
    }
}