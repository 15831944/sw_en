﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATH;
using BaseClasses;

namespace FEM_CALC_BASE
{
    public class SimpleBeamCalculation
    {
        public SimpleBeamCalculation() { }

        // SBD funkcie (zahrnaju kombinacie)
        public void CalculateInternalForcesOnSimpleBeam_SBD(int iNumberOfLoadCombinations, int iNumberOfDesignSections, CMember member, float[] fx_positions,
            float[] fE_d_load_values_LCS_y, float[] fE_d_load_values_LCS_z, out basicInternalForces[,] sBIF_x, out designBucklingLengthFactors[] sBucklingLengthFactors, out designMomentValuesForCb[] sMomentValuesforCb)
        {
            sBucklingLengthFactors = new designBucklingLengthFactors[iNumberOfLoadCombinations];
            sMomentValuesforCb = new designMomentValuesForCb[iNumberOfLoadCombinations];
            sBIF_x = new basicInternalForces[iNumberOfLoadCombinations, iNumberOfDesignSections];

            // Temporary calculation of internal forces - each combination
            for (int i = 0; i < iNumberOfLoadCombinations; i++)
            {
                CExample_2D_51_SB memberModel_qy = new CExample_2D_51_SB(member.FLength, fE_d_load_values_LCS_y[i]);
                CExample_2D_51_SB memberModel_qz = new CExample_2D_51_SB(member.FLength, fE_d_load_values_LCS_z[i]);

                float fM_abs_max = 0;

                for (int j = 0; j < iNumberOfDesignSections; j++)
                {
                    sBIF_x[i, j].fV_yu = memberModel_qy.m_arrMLoads[0].Get_SSB_V_x(fx_positions[j], member.FLength);
                    sBIF_x[i, j].fM_zv = memberModel_qy.m_arrMLoads[0].Get_SSB_M_x(fx_positions[j], member.FLength);

                    sBIF_x[i, j].fV_zv = memberModel_qz.m_arrMLoads[0].Get_SSB_V_x(fx_positions[j], member.FLength);
                    sBIF_x[i, j].fM_yu = memberModel_qz.m_arrMLoads[0].Get_SSB_M_x(fx_positions[j], member.FLength);

                    sBIF_x[i, j].fN = 0f; // TODO - doplnit vypocet
                    sBIF_x[i, j].fT = 0f; // TODO - doplnit vypocet

                    if (Math.Abs(sBIF_x[i, j].fM_yu) > Math.Abs(fM_abs_max))
                        fM_abs_max = sBIF_x[i, j].fM_yu;
                }

                sBucklingLengthFactors[i].fBeta_x_FB_fl_ex = 1f; // TODO - nastavit pre member - moze zavisiet od zatazenia
                sBucklingLengthFactors[i].fBeta_y_FB_fl_ey = 1f; // TODO - nastavit pre member - moze zavisiet od zatazenia
                sBucklingLengthFactors[i].fBeta_z_TB_TFB_l_ez = 1f; // TODO - nastavit pre member - moze zavisiet od zatazenia
                sBucklingLengthFactors[i].fBeta_LTB_fl_LTB = 1f; // TODO - nastavit pre member - moze zavisiet od zatazenia

                sMomentValuesforCb[i].fM_max = fM_abs_max;
                sMomentValuesforCb[i].fM_14 = memberModel_qz.m_arrMLoads[0].Get_SSB_M_x(0.25f * member.FLength, member.FLength);
                sMomentValuesforCb[i].fM_24 = memberModel_qz.m_arrMLoads[0].Get_SSB_M_x(0.50f * member.FLength, member.FLength);
                sMomentValuesforCb[i].fM_34 = memberModel_qz.m_arrMLoads[0].Get_SSB_M_x(0.75f * member.FLength, member.FLength);
            }
        }

        public void CalculateInternalForcesOnSimpleBeam_SBD(int iNumberOfDesignSections, CMember member, float[] fx_positions, 
            float fE_d_load_value_LCS_y, float fE_d_load_value_LCS_z, out basicInternalForces[,] sBIF_x, out designBucklingLengthFactors[] sBucklingLengthFactors, out designMomentValuesForCb[] sMomentValuesforCb)
        {
            int iNumberOfLoadCombinations = 1;
            float[] fE_d_load_values_LCS_y = new float[1] { fE_d_load_value_LCS_y };
            float[] fE_d_load_values_LCS_z = new float[1] { fE_d_load_value_LCS_z };

            CalculateInternalForcesOnSimpleBeam_SBD(iNumberOfLoadCombinations, iNumberOfDesignSections, member, fx_positions, fE_d_load_values_LCS_y, fE_d_load_values_LCS_z, out sBIF_x, out sBucklingLengthFactors, out sMomentValuesforCb);
        }

        public void CalculateInternalForcesOnSimpleBeam_SBD(int iNumberOfDesignSections, float[] fx_positions, CMember member,
        CMLoad_21 memberload, out basicInternalForces[,] sBIF_x, out designBucklingLengthFactors[] sBucklingLengthFactors,  out designMomentValuesForCb[] sMomentValuesforCb)
        {
            int iNumberOfLoadCombinations = 1;
            float[] fE_d_load_values_LCS_y = new float[1] { memberload.ELoadDir == ELoadDirection.eLD_Y ? memberload.Fq : 0};
            float[] fE_d_load_values_LCS_z = new float[1] { memberload.ELoadDir == ELoadDirection.eLD_Z ? memberload.Fq : 0};

            CalculateInternalForcesOnSimpleBeam_SBD(iNumberOfLoadCombinations, iNumberOfDesignSections, member, fx_positions, fE_d_load_values_LCS_y, fE_d_load_values_LCS_z, out sBIF_x, out sBucklingLengthFactors, out sMomentValuesforCb);
        }

        // PFD - nove funkcie (nezahrnaju kombinacie)

        // Internal Forces
        public void CalculateInternalForcesOnSimpleBeam_PFD(bool bUseCRSCGeometricalAxes, int iNumberOfDesignSections, float[] fx_positions, CMember member,
           CLoadCase lc, out basicInternalForces[] sBIF_x, out designBucklingLengthFactors[] sBucklingLengthFactors/*, out designMomentValuesForCb[] sMomentValuesforCb*/)
        {
            sBucklingLengthFactors = new designBucklingLengthFactors[iNumberOfDesignSections];
            //sMomentValuesforCb = new designMomentValuesForCb[iNumberOfDesignSections];
            sBIF_x = new basicInternalForces[iNumberOfDesignSections];

            foreach (CMLoad cmload in lc.MemberLoadsList) // Each member load in load case assigned to the member
            {
                if (cmload.Member.ID == member.ID)
                {
                    CExample_2D_51_SB memberModel = new CExample_2D_51_SB(member, cmload);

                    for (int j = 0; j < iNumberOfDesignSections; j++)
                    {
                        //designMomentValuesForCb sMomentValuesforCb_temp = new designMomentValuesForCb();
                        basicInternalForces sBIF_x_temp = new basicInternalForces();

                        if (cmload.ELoadDir == ELoadDirection.eLD_X)
                        {
                            sBIF_x_temp.fN = memberModel.m_arrMLoads[0].Get_SSB_N_x(fx_positions[j]);
                        }
                        else if (cmload.ELoadDir == ELoadDirection.eLD_Y)
                        {
                            if (bUseCRSCGeometricalAxes)
                            {
                                sBIF_x_temp.fV_yy = memberModel.m_arrMLoads[0].Get_SSB_V_x(fx_positions[j], member.FLength);
                                sBIF_x_temp.fM_zz = memberModel.m_arrMLoads[0].Get_SSB_M_x(fx_positions[j], member.FLength);
                            }
                            else
                            {
                                sBIF_x_temp.fV_yu = memberModel.m_arrMLoads[0].Get_SSB_V_x(fx_positions[j], member.FLength);
                                sBIF_x_temp.fM_zv = memberModel.m_arrMLoads[0].Get_SSB_M_x(fx_positions[j], member.FLength);
                            }
                        }
                        else
                        {
                            if (bUseCRSCGeometricalAxes)
                            {
                                sBIF_x_temp.fV_zz = memberModel.m_arrMLoads[0].Get_SSB_V_x(fx_positions[j], member.FLength);
                                sBIF_x_temp.fM_yy = memberModel.m_arrMLoads[0].Get_SSB_M_x(fx_positions[j], member.FLength);
                            }
                            else
                            {
                                sBIF_x_temp.fV_zv = memberModel.m_arrMLoads[0].Get_SSB_V_x(fx_positions[j], member.FLength);
                                sBIF_x_temp.fM_yu = memberModel.m_arrMLoads[0].Get_SSB_M_x(fx_positions[j], member.FLength);
                            }
                        }

                        sBIF_x_temp.fT = 0f; // TODO - doplnit vypocet

                        // Add load results
                        sBIF_x[j].fN += sBIF_x_temp.fN;
                        sBIF_x[j].fV_yu += sBIF_x_temp.fV_yu;
                        sBIF_x[j].fV_yy += sBIF_x_temp.fV_yy;
                        sBIF_x[j].fV_zv += sBIF_x_temp.fV_zv;
                        sBIF_x[j].fV_zz += sBIF_x_temp.fV_zz;
                        sBIF_x[j].fM_yu += sBIF_x_temp.fM_yu;
                        sBIF_x[j].fM_yy += sBIF_x_temp.fM_yy;
                        sBIF_x[j].fM_zv += sBIF_x_temp.fM_zv;
                        sBIF_x[j].fM_zz += sBIF_x_temp.fM_zz;

                        if (cmload.ELoadDir == ELoadDirection.eLD_Z)
                        {
                            float fSegmentStart_abs;
                            float fSegmentEnd_abs;

                            GetSegmentStartAndEndFor_xLocation(fx_positions[j], member, out fSegmentStart_abs, out fSegmentEnd_abs);

                            float fSegmentLength = fSegmentEnd_abs - fSegmentStart_abs;
  
                            //sMomentValuesforCb_temp.fM_14 = memberModel.m_arrMLoads[0].Get_SSB_M_x(fSegmentStart_abs + 0.25f * fSegmentLength, member.FLength);
                            //sMomentValuesforCb_temp.fM_24 = memberModel.m_arrMLoads[0].Get_SSB_M_x(fSegmentStart_abs + 0.50f * fSegmentLength, member.FLength);
                            //sMomentValuesforCb_temp.fM_34 = memberModel.m_arrMLoads[0].Get_SSB_M_x(fSegmentStart_abs + 0.75f * fSegmentLength, member.FLength);

                            //sMomentValuesforCb_temp.fM_max = 0;

                            //int iNumberOfDesignSegments = iNumberOfDesignSections - 1;

                            //for (int i = 0; i < iNumberOfDesignSections; i++)
                            //{
                            //    float fx = fSegmentStart_abs + ((float)i / (float)iNumberOfDesignSegments) * fSegmentLength;
                            //    float fM_max_temp = memberModel.m_arrMLoads[0].Get_SSB_M_x(fx, member.FLength);
                            //
                            //if (Math.Abs(fM_max_temp) > Math.Abs(sMomentValuesforCb_temp.fM_max))
                            //        sMomentValuesforCb_temp.fM_max = fM_max_temp;
                            //}

                            // Add load results
                            //sMomentValuesforCb[j].fM_max += sMomentValuesforCb_temp.fM_max;
                            //sMomentValuesforCb[j].fM_14 += sMomentValuesforCb_temp.fM_14;
                            //sMomentValuesforCb[j].fM_24 += sMomentValuesforCb_temp.fM_24;
                            //sMomentValuesforCb[j].fM_34 += sMomentValuesforCb_temp.fM_34;

                            // Check that M_max is more or equal to the maximum from (M_14, M_24, M_34) - symbols M_3, M_4, M_5 used in exception message
                            //if (Math.Abs(sMomentValuesforCb[j].fM_max) < MathF.Max(Math.Abs(sMomentValuesforCb[j].fM_14), Math.Abs(sMomentValuesforCb[j].fM_24), Math.Abs(sMomentValuesforCb[j].fM_34)))
                            //{
                            //    throw new Exception("Maximum value of bending moment doesn't correspond with values of bending moment at segment M₃, M₄, M₅.");
                            //}
                        }

                        sBucklingLengthFactors[j] = GetSegmentBucklingFactors_xLocation(fx_positions[j], member, lc.ID);
                    }
                }
            }
        }

        // Deflections
        public void CalculateDeflectionsOnSimpleBeam_PFD(bool bUseCRSCGeometricalAxes, int iNumberOfDesignSections, float[] fx_positions, CMember member,
        CLoadCase lc, out basicDeflections[] sBDeflections_x)
        {
            sBDeflections_x = new basicDeflections[iNumberOfDesignSections];

            foreach (CMLoad cmload in lc.MemberLoadsList) // Each member load in load case assigned to the member
            {
                if (cmload.Member.ID == member.ID)
                {
                    CExample_2D_51_SB memberModel = new CExample_2D_51_SB(member, cmload);

                    for (int j = 0; j < iNumberOfDesignSections; j++)
                    {
                        basicDeflections[] sBDeflections_x_temp = new basicDeflections[iNumberOfDesignSections];

                        if (cmload.ELoadDir == ELoadDirection.eLD_Y)
                        {
                            if (bUseCRSCGeometricalAxes)
                                sBDeflections_x_temp[j].fDelta_yy = memberModel.m_arrMLoads[0].Get_SSB_Delta_x(fx_positions[j], member.FLength, member.CrScStart.m_Mat.m_fE, (float)member.CrScStart.I_z);
                            else
                                sBDeflections_x_temp[j].fDelta_yu = memberModel.m_arrMLoads[0].Get_SSB_Delta_x(fx_positions[j], member.FLength, member.CrScStart.m_Mat.m_fE, (float)member.CrScStart.I_mikro);
                        }
                        else if(cmload.ELoadDir == ELoadDirection.eLD_Z)
                        {
                            if (bUseCRSCGeometricalAxes)
                                sBDeflections_x_temp[j].fDelta_zz = memberModel.m_arrMLoads[0].Get_SSB_Delta_x(fx_positions[j], member.FLength, member.CrScStart.m_Mat.m_fE, (float)member.CrScStart.I_y);
                            else
                                sBDeflections_x_temp[j].fDelta_zv = memberModel.m_arrMLoads[0].Get_SSB_Delta_x(fx_positions[j], member.FLength, member.CrScStart.m_Mat.m_fE, (float)member.CrScStart.I_epsilon);
                        }
                        else // x - no deflection
                        {
                            sBDeflections_x_temp[j].fDelta_yy = 0f;
                            sBDeflections_x_temp[j].fDelta_yu = 0f;
                            sBDeflections_x_temp[j].fDelta_zz = 0f;
                            sBDeflections_x_temp[j].fDelta_zv = 0f;
                        }

                        /* // nepotrebujeme pocitat - zrata sa z vyslednych hodnot v jednotlivych smeroch
                        if (bUseCRSCGeometricalAxes)
                            sBDeflections_x_temp[j].fDelta_tot = MathF.Sqrt(MathF.Pow2(sBDeflections_x_temp[j].fDelta_yy) + MathF.Pow2(sBDeflections_x_temp[j].fDelta_zz)); // Vektorovy sucin pre vyslednicu
                        else
                            sBDeflections_x_temp[j].fDelta_tot = MathF.Sqrt(MathF.Pow2(sBDeflections_x_temp[j].fDelta_yu) + MathF.Pow2(sBDeflections_x_temp[j].fDelta_zv)); // Vektorovy sucin pre vyslednicu
                        */

                        // Add load results
                        sBDeflections_x[j].fDelta_yu += sBDeflections_x_temp[j].fDelta_yu;
                        sBDeflections_x[j].fDelta_yy += sBDeflections_x_temp[j].fDelta_yy;
                        sBDeflections_x[j].fDelta_zv += sBDeflections_x_temp[j].fDelta_zv;
                        sBDeflections_x[j].fDelta_zz += sBDeflections_x_temp[j].fDelta_zz;

                        if (bUseCRSCGeometricalAxes && (!MathF.d_equal(sBDeflections_x[j].fDelta_yy, 0) || !MathF.d_equal(sBDeflections_x[j].fDelta_zz, 0)))
                            sBDeflections_x[j].fDelta_tot = MathF.Sqrt(MathF.Pow2(sBDeflections_x[j].fDelta_yy) + MathF.Pow2(sBDeflections_x[j].fDelta_zz)); // Vektorovy sucin pre vyslednicu
                        else if (!MathF.d_equal(sBDeflections_x[j].fDelta_yu, 0) || !MathF.d_equal(sBDeflections_x[j].fDelta_zv, 0))
                            sBDeflections_x[j].fDelta_tot = MathF.Sqrt(MathF.Pow2(sBDeflections_x[j].fDelta_yu) + MathF.Pow2(sBDeflections_x[j].fDelta_zv)); // Vektorovy sucin pre vyslednicu
                        else
                            sBDeflections_x[j].fDelta_tot = 0;
                    }
                }
            }
        }

        // TODO Refaktorovat s metodou v projekte PFD
        private void GetSegmentStartAndEndFor_xLocation(float fx, CMember member, out float fSegmentStart_abs, out float fSegmentEnd_abs)
        {
            fSegmentStart_abs = 0f;
            fSegmentEnd_abs = member.FLength;

            if (member.LTBSegmentGroup != null && member.LTBSegmentGroup.Count > 1) // More than one LTB segment exists
            {
                for (int i = 0; i < member.LTBSegmentGroup.Count; i++)
                {
                    if (fx >= member.LTBSegmentGroup[i].SegmentStartCoord_Abs && fx <= member.LTBSegmentGroup[i].SegmentEndCoord_Abs)
                    {
                        fSegmentStart_abs = member.LTBSegmentGroup[i].SegmentStartCoord_Abs;
                        fSegmentEnd_abs = member.LTBSegmentGroup[i].SegmentEndCoord_Abs;
                    }
                }
            }
        }

        // TODO Refaktorovat s metodou v projekte PFD
        public designBucklingLengthFactors GetSegmentBucklingFactors_xLocation(float fx, CMember member, int lcombID)
        {
            designBucklingLengthFactors bucklingLengthFactors = new designBucklingLengthFactors();
            bucklingLengthFactors.fBeta_x_FB_fl_ex = 1.0f;
            bucklingLengthFactors.fBeta_y_FB_fl_ey = 1.0f;
            bucklingLengthFactors.fBeta_z_TB_TFB_l_ez = 1.0f;
            bucklingLengthFactors.fBeta_LTB_fl_LTB = 1.0f;

            if (member.LTBSegmentGroup != null && member.LTBSegmentGroup.Count > 1) // More than one LTB segment exists
            {
                for (int i = 0; i < member.LTBSegmentGroup.Count; i++)
                {
                    if (fx >= member.LTBSegmentGroup[i].SegmentStartCoord_Abs && fx <= member.LTBSegmentGroup[i].SegmentEndCoord_Abs)
                    {
                        if (member.LTBSegmentGroup[i].BucklingLengthFactors == null || member.LTBSegmentGroup[i].BucklingLengthFactors.Count == 0) // Default
                        {
                            bucklingLengthFactors = new designBucklingLengthFactors();
                            bucklingLengthFactors.fBeta_x_FB_fl_ex = 1.0f;
                            bucklingLengthFactors.fBeta_y_FB_fl_ey = 1.0f;
                            bucklingLengthFactors.fBeta_z_TB_TFB_l_ez = 1.0f;
                            bucklingLengthFactors.fBeta_LTB_fl_LTB = 1.0f;
                        }
                        else if (member.LTBSegmentGroup[i].BucklingLengthFactors.Count == 1) // Defined only once - prut ma rovnake faktory pre vzperne dlzky pre vsetky kombinacie.
                        {
                            bucklingLengthFactors = member.LTBSegmentGroup[i].BucklingLengthFactors[0];
                        }
                        else // if(bucklingLengthFactors.Count > 1) // Different values for load combinations
                            bucklingLengthFactors = member.LTBSegmentGroup[i].BucklingLengthFactors[lcombID - 1];
                    }
                }
            }

            return bucklingLengthFactors;
        }
    }
}
