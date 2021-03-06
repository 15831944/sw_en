﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Configuration;
using System.Globalization;
using BaseClasses;
using System.Collections.ObjectModel;

namespace PFD
{
    public class CPFDFootingDesign : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        private int MLimitStateIndex;
        private int MComponentTypeIndex;
        private int MSelectedLoadCombinationID;

        private List<string> MComponentList;
        private List<CLimitState> MLimitStates;
        private List<ComboItem> MLoadCombinations;

        public bool IsSetFromCode = false;

        //-------------------------------------------------------------------------------------------------------------
        public int LimitStateIndex
        {
            get
            {
                return MLimitStateIndex;
            }

            set
            {
                MLimitStateIndex = value;
                SetLoadCombinations();
                //TODO No. 68
                NotifyPropertyChanged("LimitStateIndex");
            }
        }
        
        //-------------------------------------------------------------------------------------------------------------
        public int ComponentTypeIndex
        {
            get
            {
                return MComponentTypeIndex;
            }

            set
            {
                MComponentTypeIndex = value;
                //TODO No. 68
                NotifyPropertyChanged("ComponentTypeIndex");
            }
        }
        
        public List<CLimitState> LimitStates
        {
            get
            {
                return MLimitStates;
            }

            set
            {
                MLimitStates = value;
                NotifyPropertyChanged("LimitStates");
            }
        }

        public List<ComboItem> LoadCombinations
        {
            get
            {
                return MLoadCombinations;
            }

            set
            {
                MLoadCombinations = value;
                NotifyPropertyChanged("LoadCombinations");
                SelectedLoadCombinationID = MLoadCombinations[0].ID;
            }
        }
        public int SelectedLoadCombinationID
        {
            get
            {
                return MSelectedLoadCombinationID;
            }

            set
            {
                MSelectedLoadCombinationID = value;
                NotifyPropertyChanged("SelectedLoadCombinationID");
            }
        }

        private CLoadCombination[] m_allLoadCombinations;

        public List<string> ComponentList
        {
            get
            {
                return MComponentList;
            }

            set
            {
                MComponentList = value;
                NotifyPropertyChanged("ComponentList");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public CPFDFootingDesign(CLimitState[] limitStates, CLoadCombination[] allLoadCombinations, ObservableCollection<CComponentInfo> componentList)
        {
            SetLimitStates(limitStates);
            SetComponentList(componentList);
            m_allLoadCombinations = allLoadCombinations;

            // Set default
            LimitStateIndex = 0;
            ComponentTypeIndex = 0;

            IsSetFromCode = false;
        }

        public void SetComponentList(ObservableCollection<CComponentInfo> componentList)
        {
            //Task 335: Pre Component Type staci Main Column, Edge Column, Wind Post (alebo Column) -front side, Wind Post (alebo Column) -back side.
            ComponentList = componentList.Where(s => (s.MemberTypePosition == EMemberType_FS_Position.MainColumn || s.MemberTypePosition == EMemberType_FS_Position.EdgeColumn 
                        || s.MemberTypePosition == EMemberType_FS_Position.WindPostFrontSide || s.MemberTypePosition == EMemberType_FS_Position.WindPostBackSide) 
                        && s.Generate == true && s.Calculate == true && s.Design == true).Select(s => s.ComponentName).ToList();

            if(ComponentList.Count > 0) ComponentList.Add("All");
        }

        private void SetLimitStates(CLimitState[] modelLimitStates)
        {
            MLimitStates = new List<CLimitState>();
            foreach (CLimitState ls in modelLimitStates)
            {
                if (ls.eLS_Type == ELSType.eLS_SLS) continue;  //task 213 = vynechanie SLS

                MLimitStates.Add(ls);
            }
        }
 
        private void SetLoadCombinations()
        {
            CLimitState limitState = LimitStates[LimitStateIndex];

            List<ComboItem> loadCombinations = new List<ComboItem>();
            foreach (CLoadCombination lc in m_allLoadCombinations)
            {
                if (lc.eLComType == limitState.eLS_Type) loadCombinations.Add(new ComboItem(lc.ID, $"{lc.Name}\t{lc.CombinationKey}"));
            }

            loadCombinations.Add(new ComboItem(-1, "Envelope"));
            LoadCombinations = loadCombinations;
        }

        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
