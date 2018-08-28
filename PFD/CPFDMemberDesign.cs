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
    public class CPFDMemberDesign : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        private int MLimitStateIndex;
        private int MLoadCombinationIndex;
        private int MComponentTypeIndex;

        private ObservableCollection<CComponentInfo> MComponentList;
        private CLimitState[] MLimitStates;
        private CLoadCombination[] MLoadCombinations;

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

                // TODO - Pri zmene LimitState nacitat load combinations, ktora patria k danemu limit state a maju spocitane vysledky
                // PODOBNE PRE INTERNAL FORCES

                NotifyPropertyChanged("LimitStateIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int LoadCombinationIndex
        {
            get
            {
                return MLoadCombinationIndex;
            }

            set
            {
                MLoadCombinationIndex = value;

                // TODO - Pri zmene load combination, spustit vypocet pre vsetky pruty vybraneho typu v component type a zobrazit vysledky pre najnevyhodnejsi z nich, max design ratio

                NotifyPropertyChanged("LoadCombinationIndex");
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

                // TODO - Pri zmene typpu pruta, spustit vypocet pre vsetky pruty vybraneho typu pre vybranu kombinaciu a zobrazit vysledky pre najnevyhodnejsi z nich, max design ratio

                NotifyPropertyChanged("ComponentTypeIndex");
            }
        }
        
        public CLimitState[] LimitStates
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

        public CLoadCombination[] LoadCombinations
        {
            get
            {
                return MLoadCombinations;
            }

            set
            {
                MLoadCombinations = value;
                NotifyPropertyChanged("LoadCombinations");
            }
        }

        public ObservableCollection<CComponentInfo> ComponentList
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
        public CPFDMemberDesign(CLimitState[] limitStates, CLoadCombination[] loadCombinations, ObservableCollection<CComponentInfo> componentList)
        {
            MLimitStates = limitStates;
            MLoadCombinations = loadCombinations;
            MComponentList = componentList;

            // Set default
            LimitStateIndex = 0;
            LoadCombinationIndex = 0;
            ComponentTypeIndex = 0;
            
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
