﻿using System;
using System.Collections.Generic;

namespace BaseClasses
{
    [Serializable]
    public class CLoadCaseGroup
    {
        private int m_ID;

        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        private string m_Name;

        public string MName
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        private ELCGTypeForLimitState m_Type_LS;

        public ELCGTypeForLimitState MType_LS
        {
            get { return m_Type_LS; }
            set { m_Type_LS = value; }
        }

        private ELCGType m_Type;

        public ELCGType MType
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        private List<CLoadCase> m_loadCasesList;

        public List<CLoadCase> MLoadCasesList
        {
            get { return m_loadCasesList; }
            set { m_loadCasesList = value; }
        }

        public CLoadCaseGroup(int id_temp, string name_temp, ELCGTypeForLimitState type_LS_temp, ELCGType type_temp)
        {
            ID = id_temp;
            MName = name_temp;
            MType_LS = type_LS_temp;
            MType = type_temp;
            MLoadCasesList = new List<CLoadCase>(1);
        }

        public CLoadCaseGroup(int id_temp, string name_temp, ELCGTypeForLimitState type_LS_temp, ELCGType type_temp, List<CLoadCase> listLoadCases_temp)
        {
            ID = id_temp;
            MName = name_temp;
            MType_LS = type_LS_temp;
            MType = type_temp;
            MLoadCasesList = listLoadCases_temp;
        }
    }
}
