﻿using System;
using System.Windows.Media.Media3D;

namespace BaseClasses
{
    public class CMLoad_23 : CMLoad
    {
        //----------------------------------------------------------------------------
        private float m_fq; // Force Value
        private float m_fa; // Distance from Member Start (distance between load start point and member start)
        private float m_fb; // Distance along which load acts (distance between load start point and member end)
        //----------------------------------------------------------------------------
        public float Fq
        {
            get { return m_fq; }
            set { m_fq = value; }
        }
        public float Fa
        {
            get { return m_fa; }
            set { m_fa = value; }
        }
        public float Fb
        {
            get { return m_fb; }
            set { m_fb = value; }
        }

        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        //----------------------------------------------------------------------------
        public CMLoad_23()
        {


        }
        public CMLoad_23(float fq, float fb)
        {
            Fq = fq;
            Fb = fb;
        }

        public CMLoad_23(int id_temp,
            float fq,
            float fb,
            CMember member_aux,
            EMLoadTypeDistr mLoadTypeDistr,
            ELoadType mLoadType,
            ELoadCoordSystem eLoadCS,
            ELoadDirection eLoadDir,
            bool bIsDislayed,
            int fTime)
        {
            ID = id_temp;
            Fq = fq;
            Fb = fb;
            Fa = member_aux.FLength - fb;
            Member = member_aux;
            MLoadTypeDistr = mLoadTypeDistr;
            MLoadType = mLoadType;
            ELoadCS = eLoadCS;
            ELoadDir = eLoadDir;
            BIsDisplayed = bIsDislayed;
            FTime = fTime;
        }

        public override Model3DGroup CreateM_3D_G_Load(bool bConsiderCrossSectionDimensions, float fDisplayin3D_ratio)
        {
            return CreateUniformLoadSequence(Fq, Fa, Fb, bConsiderCrossSectionDimensions, fDisplayin3D_ratio);
        }

        // Simply supported beam
        // Docasne, hodnoty reakcii zavisia od typu podopretia pruta - rozpracovane v projkte FEM_CALC
        public override float Get_SSB_SupportReactionValue_RA_Start(float fL)
        {
            throw new NotImplementedException();
        }
        public override float Get_SSB_SupportReactionValue_RB_End(float fL)
        {
            throw new NotImplementedException();
        }
        public override float Get_SSB_V_max(float fL)
        {
            throw new NotImplementedException();
        }
        public override float Get_SSB_M_max(float fL)
        {
            throw new NotImplementedException();
        }
        public override float Get_SSB_N_x(float fx)
        {
            throw new NotImplementedException();
        }
        public override float Get_SSB_V_x(float fx, float fL)
        {
            throw new NotImplementedException();
        }
        public override float Get_SSB_M_x(float fx, float fL)
        {
            throw new NotImplementedException();
        }
        public override float Get_SSB_Delta_max(float fL, float fE, float fI)
        {
            throw new NotImplementedException();
        }
        public override float Get_SSB_Delta_x(float fx, float fL, float fE, float fI)
        {
            throw new NotImplementedException();
        }
    }
}
