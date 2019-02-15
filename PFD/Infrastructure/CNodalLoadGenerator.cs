﻿using BaseClasses;
using M_EC1.AS_NZS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFD
{
    public class CNodalLoadGenerator
    {
        private int iNumberOfLoadsInXDirection;
        private int iNumberOfLoadsInYDirection;
        CNode[] m_arrNodes;
        CCalcul_1170_5 eq;

        public List<CNLoad> nodalLoadEQ_ULS_PlusX;
        public List<CNLoad> nodalLoadEQ_ULS_PlusY;
        public List<CNLoad> nodalLoadEQ_SLS_PlusX;
        public List<CNLoad> nodalLoadEQ_SLS_PlusY;

        public CNodalLoadGenerator(int numberOfLoadsInXDirection, int numberOfLoadsInYDirection, CNode[] arrNodes, CCalcul_1170_5 calc_eq)
        {
            iNumberOfLoadsInXDirection = numberOfLoadsInXDirection;
            iNumberOfLoadsInYDirection = numberOfLoadsInYDirection;
            m_arrNodes = arrNodes;
            eq = calc_eq;
        }

        public void GenerateNodalLoads()
        {
            nodalLoadEQ_ULS_PlusX = new List<CNLoad>(iNumberOfLoadsInXDirection);
            nodalLoadEQ_ULS_PlusY = new List<CNLoad>(iNumberOfLoadsInYDirection);

            nodalLoadEQ_SLS_PlusX = new List<CNLoad>(iNumberOfLoadsInXDirection);
            nodalLoadEQ_SLS_PlusY = new List<CNLoad>(iNumberOfLoadsInYDirection);

            for (int i = 0; i < iNumberOfLoadsInXDirection; i++)
            {
                nodalLoadEQ_ULS_PlusX.Add(new CNLoadSingle(m_arrNodes[i * 5 + 1], ENLoadType.eNLT_Fx, eq.fV_x_ULS_stregnth, true, 0));
                nodalLoadEQ_SLS_PlusX.Add(new CNLoadSingle(m_arrNodes[i * 5 + 1], ENLoadType.eNLT_Fx, eq.fV_x_SLS, true, 0));
            }

            for (int i = 0; i < iNumberOfLoadsInYDirection; i++)
            {
                nodalLoadEQ_ULS_PlusY.Add(new CNLoadSingle(m_arrNodes[i * 2 + 1], ENLoadType.eNLT_Fy, eq.fV_y_ULS_stregnth, true, 0));
                nodalLoadEQ_SLS_PlusY.Add(new CNLoadSingle(m_arrNodes[i * 2 + 1], ENLoadType.eNLT_Fy, eq.fV_y_SLS, true, 0));
            }
        }
    }
}