﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClasses
{
    // TODO 201
    // TO Ondrej - tato trieda by mohla dedit od CModel alebo CExample a obsahovat to co je teraz v CExample_2D_15_PF
    public class CFrame
    {
        List<CMember> m_Members;
        public List<CMember> Members
        {
            get
            {
                return m_Members;
            }

            set
            {
                m_Members = value;
            }
        }
        public CFrame()
        {
            m_Members = new List<CMember>();
        }

        
    }
}