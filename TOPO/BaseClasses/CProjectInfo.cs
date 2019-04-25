﻿using System;

namespace BaseClasses
{
    [Serializable]
    public class CProjectInfo
    {
        private string m_ProjectName;
        private string m_Site;
        private string m_ProjectNumber;
        private string m_ProjectPart;
        private DateTime m_Date;

        public string ProjectName
        {
            get
            {
                return m_ProjectName;
            }

            set
            {
                m_ProjectName = value;
            }
        }

        public string Site
        {
            get
            {
                return m_Site;
            }

            set
            {
                m_Site = value;
            }
        }

        public string ProjectNumber
        {
            get
            {
                return m_ProjectNumber;
            }

            set
            {
                m_ProjectNumber = value;
            }
        }

        public string ProjectPart
        {
            get
            {
                return m_ProjectPart;
            }

            set
            {
                m_ProjectPart = value;
            }
        }

        public DateTime Date
        {
            get
            {
                return m_Date;
            }

            set
            {
                m_Date = value;
            }
        }





        //----------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------
        public CProjectInfo(string projectName, string site, string projectNumber, string projectPart, DateTime date)
        {
            m_ProjectName = projectName;
            m_Site = site;
            m_ProjectNumber = projectNumber;
            m_ProjectPart = projectPart;
            m_Date = date;
        }



    }
}