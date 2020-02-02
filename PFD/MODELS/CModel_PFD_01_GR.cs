﻿using BaseClasses;
using CRSC;
using BaseClasses.GraphObj;
using M_EC1.AS_NZS;
using MATERIAL;
using MATH;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Collections.ObjectModel;
using BaseClasses.Helpers;

namespace PFD
{
    public class CModel_PFD_01_GR : CModel_PFD
    {
        public float fRoofPitch_rad;
        public int iFrameNo;

        public float fUpperGirtLimit;
        public float fDist_FrontGirts;
        public float fDist_BackGirts;
        public float fz_UpperLimitForFrontGirts;
        public float fz_UpperLimitForBackGirts;
        public float fFrontFrameRakeAngle_temp_rad;
        public float fBackFrameRakeAngle_temp_rad;
        public float fSlopeFactor;

        public int iFrameNodesNo = 5;

        public int iMainColumnNo;
        public int iRafterNo;
        public int iEavesPurlinNo;
        public int iGirtNoInOneFrame;
        public int iEavesPurlinNoInOneFrame = 2;
        public int iPurlinNoInOneFrame;
        public int iFrontColumnNoInOneFrame;
        public int iBackColumnNoInOneFrame;
        public int iFrontGirtsNoInOneFrame;
        public int iBackGirtsNoInOneFrame;
        public int iGBSideWallsMembersNo = 0;
        public int iPBMembersNo = 0;
        public int iNumberOfGB_FSMembersInOneFrame = 0;
        public int iNumberOfGB_BSMembersInOneFrame = 0;

        int[] iArrNumberOfNodesPerFrontColumn;
        int[] iArrNumberOfNodesPerBackColumn;
        int iOneColumnGirtNo;

        private float fFrontFrameRakeAngle_deg;
        private float fBackFrameRakeAngle_deg;
        private int iMainColumnFlyBracing_EveryXXGirt;
        private int iOneRafterBackColumnNo = 0;
        private int iOneRafterFrontColumnNo = 0;
        private int iRafterFlyBracing_EveryXXPurlin;

        CMemberEccentricity eccentricityColumnFront_Z;
        CMemberEccentricity eccentricityColumnBack_Z;

        CMemberEccentricity eccentricityGirtFront_Y0;
        CMemberEccentricity eccentricityGirtBack_YL;

        public List<CBlock_3D_001_DoorInBay> DoorsModels;
        public List<CBlock_3D_002_WindowInBay> WindowsModels;

        private ObservableCollection<DoorProperties> DoorBlocksProperties; // Pridane kvoli Rebates

        private List<CNode> listOfSupportedNodes_S1;
        private List<CNode> listOfSupportedNodes_S2;

        public CModel_PFD_01_GR
        (
                BuildingGeometryDataInput sGeometryInputData,
                int iFrameNo_temp,
                float fDist_Girt_temp,
                float fDist_Purlin_temp,
                float fDist_FrontColumns_temp,
                float fBottomGirtPosition_temp,
                float fFrontFrameRakeAngle_temp_deg,
                float fBackFrameRakeAngle_temp_deg,
                ObservableCollection<DoorProperties> doorBlocksProperties,
                ObservableCollection<WindowProperties> windowBlocksProperties,
                CComponentListVM componentListVM,
                List<CConnectionJointTypes> joints,
                List<CFoundation> foundations,
                List<CSlab> slabs,
                CPFDViewModel vm
            )
        {
            ObservableCollection<CComponentInfo> componentList = componentListVM?.ComponentList;
            fH1_frame = sGeometryInputData.fH_1;
            fW_frame = sGeometryInputData.fW;
            fL_tot = sGeometryInputData.fL;
            iFrameNo = iFrameNo_temp;
            fH2_frame = sGeometryInputData.fH_2;
            fFrontFrameRakeAngle_deg = fFrontFrameRakeAngle_temp_deg;
            fBackFrameRakeAngle_deg = fBackFrameRakeAngle_temp_deg;

            iFrameNo = iFrameNo_temp;
            fL1_frame = fL_tot / (iFrameNo - 1);

            fDist_Girt = fDist_Girt_temp;
            fDist_Purlin = fDist_Purlin_temp;
            fDist_FrontColumns = fDist_FrontColumns_temp;
            fDist_BackColumns = fDist_FrontColumns; // TODO - docasne, nezadavame zatial rozne vzdialenosti medzi wind post na prednej a zadnej strane

            fBottomGirtPosition = fBottomGirtPosition_temp;
            fDist_FrontGirts = fDist_Girt_temp; // Ak nie je rovnake ako pozdlzne tak su koncove pruty sikmo pretoze sa uvazuje jeden uzol na stlpe pre pozdlzny aj priecny smer nosnikov
            fDist_BackGirts = fDist_Girt_temp;
            fFrontFrameRakeAngle_temp_rad = fFrontFrameRakeAngle_temp_deg * MathF.fPI / 180f;
            fBackFrameRakeAngle_temp_rad = fBackFrameRakeAngle_temp_deg * MathF.fPI / 180f;

            DoorBlocksProperties = doorBlocksProperties;

            m_eSLN = ESLN.e3DD_1D; // 1D members in 3D model
            m_eNDOF = (int)ENDOF.e3DEnv; // DOF in 3D
            m_eGCS = EGCS.eGCSLeftHanded; // Global coordinate system

            fRoofPitch_rad = (float)Math.Atan((fH2_frame - fH1_frame) / (0.5f * fW_frame));

            iEavesPurlinNo = iEavesPurlinNoInOneFrame * (iFrameNo - 1);
            iMainColumnNo = iFrameNo * 2;
            iRafterNo = iFrameNo * 2;

            iOneColumnGirtNo = 0;
            iGirtNoInOneFrame = 0;

            m_arrMat = new CMat[15];
            m_arrCrSc = new CCrSc[15];

            // Materials
            // Materials List - Materials Array - Fill Data of Materials Array
            // TODO - napojit na GUI a na databazu
            m_arrMat[(int)EMemberGroupNames.eMainColumn] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eMainColumn].Material);
            m_arrMat[(int)EMemberGroupNames.eRafter] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eRafter].Material);
            m_arrMat[(int)EMemberGroupNames.eMainColumn_EF] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eMainColumn_EF].Material);
            m_arrMat[(int)EMemberGroupNames.eRafter_EF] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eRafter_EF].Material);
            m_arrMat[(int)EMemberGroupNames.eEavesPurlin] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eEavesPurlin].Material);
            m_arrMat[(int)EMemberGroupNames.eGirtWall] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eGirtWall].Material);
            m_arrMat[(int)EMemberGroupNames.ePurlin] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.ePurlin].Material);
            m_arrMat[(int)EMemberGroupNames.eFrontColumn] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eFrontColumn].Material);
            m_arrMat[(int)EMemberGroupNames.eBackColumn] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eBackColumn].Material);
            m_arrMat[(int)EMemberGroupNames.eFrontGirt] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eFrontGirt].Material);
            m_arrMat[(int)EMemberGroupNames.eBackGirt] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eBackGirt].Material);
            m_arrMat[(int)EMemberGroupNames.eGirtBracing] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eGirtBracing].Material);
            m_arrMat[(int)EMemberGroupNames.ePurlinBracing] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.ePurlinBracing].Material);
            m_arrMat[(int)EMemberGroupNames.eFrontGirtBracing] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eFrontGirtBracing].Material);
            m_arrMat[(int)EMemberGroupNames.eBackGirtBracing] = MaterialFactory.GetMaterial(componentList[(int)EMemberGroupNames.eBackGirtBracing].Material);

            // Cross-sections
            // CrSc List - CrSc Array - Fill Data of Cross-sections Array

            // TODO Ondrej - Nastavit objekt prierezu podla databazy models, tabulka KitsetGableRoofEnclosed alebo KitsetGableRoofEnclosedCrscID
            // Napojit na GUI

            m_arrCrSc[(int)EMemberGroupNames.eMainColumn] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eMainColumn].Section);
            m_arrCrSc[(int)EMemberGroupNames.eRafter] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eRafter].Section);
            m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eMainColumn_EF].Section);
            m_arrCrSc[(int)EMemberGroupNames.eRafter_EF] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eRafter_EF].Section);
            m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eEavesPurlin].Section);
            m_arrCrSc[(int)EMemberGroupNames.eGirtWall] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eGirtWall].Section);
            m_arrCrSc[(int)EMemberGroupNames.ePurlin] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.ePurlin].Section);
            m_arrCrSc[(int)EMemberGroupNames.eFrontColumn] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eFrontColumn].Section);
            m_arrCrSc[(int)EMemberGroupNames.eBackColumn] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eBackColumn].Section);
            m_arrCrSc[(int)EMemberGroupNames.eFrontGirt] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eFrontGirt].Section);
            m_arrCrSc[(int)EMemberGroupNames.eBackGirt] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eBackGirt].Section);
            m_arrCrSc[(int)EMemberGroupNames.eGirtBracing] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eGirtBracing].Section);
            m_arrCrSc[(int)EMemberGroupNames.ePurlinBracing] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.ePurlinBracing].Section);
            m_arrCrSc[(int)EMemberGroupNames.eFrontGirtBracing] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eFrontGirtBracing].Section);
            m_arrCrSc[(int)EMemberGroupNames.eBackGirtBracing] = CrScFactory.GetCrSc(componentList[(int)EMemberGroupNames.eBackGirtBracing].Section);

            for (int i = 0; i < m_arrCrSc.Length; i++)
            {
                m_arrCrSc[i].ID = i + 1;
            }

            m_arrCrSc[(int)EMemberGroupNames.eMainColumn].CSColor = Colors.Chocolate;       //  1 Main Column
            m_arrCrSc[(int)EMemberGroupNames.eRafter].CSColor = Colors.Green;               //  2 Main Rafter
            m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF].CSColor = Colors.DarkOrchid;   //  3 Main Column - Edge Frame
            m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].CSColor = Colors.GreenYellow;      //  4 Main Rafter - Edge Frame
            m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin].CSColor = Colors.DarkCyan;       //  5 Eaves Purlin
            m_arrCrSc[(int)EMemberGroupNames.eGirtWall].CSColor = Colors.Orange;            //  6 Girt - Wall
            m_arrCrSc[(int)EMemberGroupNames.ePurlin].CSColor = Colors.SlateBlue;           //  7 Purlin
            m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].CSColor = Colors.BlueViolet;     //  8 Front Column
            m_arrCrSc[(int)EMemberGroupNames.eBackColumn].CSColor = Colors.BlueViolet;      //  9 Back Column
            m_arrCrSc[(int)EMemberGroupNames.eFrontGirt].CSColor = Colors.Brown;            // 10 Front Girt
            m_arrCrSc[(int)EMemberGroupNames.eBackGirt].CSColor = Colors.YellowGreen;       // 11 Back Girt
            m_arrCrSc[(int)EMemberGroupNames.eGirtBracing].CSColor = Colors.Orange;         // 12 Girt Bracing
            m_arrCrSc[(int)EMemberGroupNames.ePurlinBracing].CSColor = Colors.DarkOrange;   // 13 Purlin Bracing
            m_arrCrSc[(int)EMemberGroupNames.eFrontGirtBracing].CSColor = Colors.LimeGreen; // 14 Girt Bracing - Front Side
            m_arrCrSc[(int)EMemberGroupNames.eBackGirtBracing].CSColor = Colors.LightSeaGreen; // 15 Girt Bracing - Back Side

            // Member Groups
            listOfModelMemberGroups = new List<CMemberGroup>(15);

            //CDatabaseComponents database_temp = new CDatabaseComponents(); // TODO - Ondrej - prerobit triedu na nacitanie z databazy
            // See UC component list

            // TODO - nastavovat v GUI - zaviest databazu pre rozne typy prutov a typy load combinations
            /*
            int iLimitFractionDenominator_PermanentLoad = 250;
            int iLimitFractionDenominator_Total = 150;

            int iLimitFractionDenominator_Total_FrameColumn = 150;
            int iLimitFractionDenominator_Total_FrameRafter = 250;
            */

            // TODO - doplnit potrebne vstupne hodnoty
            float fVerticalDisplacementLimitDenominator_Rafter_PL = vm._designOptionsVM.VerticalDisplacementLimitDenominator_Rafter_PL;
            float fVerticalDisplacementLimitDenominator_Rafter_IL = 150f;
            float fVerticalDisplacementLimitDenominator_Rafter_TL = vm._designOptionsVM.VerticalDisplacementLimitDenominator_Rafter_TL;
            float fHorizontalDisplacementLimitDenominator_Column_PL = 50f;
            float fHorizontalDisplacementLimitDenominator_Column_IL = 100f;
            float fHorizontalDisplacementLimitDenominator_Column_TL = vm._designOptionsVM.HorizontalDisplacementLimitDenominator_Column_TL;
            float fVerticalDisplacementLimitDenominator_Purlin_PL = vm._designOptionsVM.VerticalDisplacementLimitDenominator_Purlin_PL;
            float fVerticalDisplacementLimitDenominator_Purlin_IL = 100f;
            float fVerticalDisplacementLimitDenominator_Purlin_TL = vm._designOptionsVM.VerticalDisplacementLimitDenominator_Purlin_TL;
            float fHorizontalDisplacementLimitDenominator_Girt_PL = 50f;
            float fHorizontalDisplacementLimitDenominator_Girt_IL = 100f;
            float fHorizontalDisplacementLimitDenominator_Girt_TL = vm._designOptionsVM.HorizontalDisplacementLimitDenominator_Girt_TL;
            float fHorizontalDisplacementLimitDenominator_WindPost_PL = 50f;
            float fHorizontalDisplacementLimitDenominator_WindPost_IL = 100f;
            float fHorizontalDisplacementLimitDenominator_WindPost_TL = vm._designOptionsVM.HorizontalDisplacementLimitDenominator_Windpost_TL;

            float fVerticalDisplacementLimit_Rafter_PL = 1f / fVerticalDisplacementLimitDenominator_Rafter_PL;
            float fVerticalDisplacementLimit_Rafter_IL = 1f / fVerticalDisplacementLimitDenominator_Rafter_IL;
            float fVerticalDisplacementLimit_Rafter_TL = 1f / fVerticalDisplacementLimitDenominator_Rafter_TL;
            float fHorizontalDisplacementLimit_Column_PL = 1f / fHorizontalDisplacementLimitDenominator_Column_PL;
            float fHorizontalDisplacementLimit_Column_IL = 1f / fHorizontalDisplacementLimitDenominator_Column_IL;
            float fHorizontalDisplacementLimit_Column_TL = 1f / fHorizontalDisplacementLimitDenominator_Column_TL;
            float fVerticalDisplacementLimit_Purlin_PL = 1f / fVerticalDisplacementLimitDenominator_Purlin_PL;
            float fVerticalDisplacementLimit_Purlin_IL = 1f / fVerticalDisplacementLimitDenominator_Purlin_IL;
            float fVerticalDisplacementLimit_Purlin_TL = 1f / fVerticalDisplacementLimitDenominator_Purlin_TL;
            float fHorizontalDisplacementLimit_Girt_PL = 1f / fHorizontalDisplacementLimitDenominator_Girt_PL;
            float fHorizontalDisplacementLimit_Girt_IL = 1f / fHorizontalDisplacementLimitDenominator_Girt_IL;
            float fHorizontalDisplacementLimit_Girt_TL = 1f / fHorizontalDisplacementLimitDenominator_Girt_TL;
            float fHorizontalDisplacementLimit_WindPost_PL = 1f / fHorizontalDisplacementLimitDenominator_WindPost_PL;
            float fHorizontalDisplacementLimit_WindPost_IL = 1f / fHorizontalDisplacementLimitDenominator_WindPost_IL;
            float fHorizontalDisplacementLimit_WindPost_TL = 1f / fHorizontalDisplacementLimitDenominator_WindPost_TL;

            listOfModelMemberGroups.Add(new CMemberGroup(1, componentList[(int)EMemberGroupNames.eMainColumn].ComponentName, EMemberType_FS.eMC, EMemberType_FS_Position.MainColumn, m_arrCrSc[(int)EMemberGroupNames.eMainColumn], fHorizontalDisplacementLimitDenominator_Column_PL, fHorizontalDisplacementLimitDenominator_Column_IL, fHorizontalDisplacementLimitDenominator_Column_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(2, componentList[(int)EMemberGroupNames.eRafter].ComponentName, EMemberType_FS.eMR, EMemberType_FS_Position.MainRafter, m_arrCrSc[(int)EMemberGroupNames.eRafter], fVerticalDisplacementLimitDenominator_Rafter_PL, fVerticalDisplacementLimitDenominator_Rafter_IL, fVerticalDisplacementLimitDenominator_Rafter_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(3, componentList[(int)EMemberGroupNames.eMainColumn_EF].ComponentName, EMemberType_FS.eEC, EMemberType_FS_Position.EdgeColumn, m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF], fHorizontalDisplacementLimitDenominator_Column_PL, fHorizontalDisplacementLimitDenominator_Column_IL, fHorizontalDisplacementLimitDenominator_Column_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(4, componentList[(int)EMemberGroupNames.eRafter_EF].ComponentName, EMemberType_FS.eER, EMemberType_FS_Position.EdgeRafter, m_arrCrSc[(int)EMemberGroupNames.eRafter_EF], fVerticalDisplacementLimitDenominator_Rafter_PL, fVerticalDisplacementLimitDenominator_Rafter_IL, fVerticalDisplacementLimitDenominator_Rafter_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(5, componentList[(int)EMemberGroupNames.eEavesPurlin].ComponentName, EMemberType_FS.eEP, EMemberType_FS_Position.EdgePurlin, m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin], fVerticalDisplacementLimitDenominator_Purlin_PL, fVerticalDisplacementLimitDenominator_Purlin_IL, fVerticalDisplacementLimitDenominator_Purlin_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(6, componentList[(int)EMemberGroupNames.eGirtWall].ComponentName, EMemberType_FS.eG, EMemberType_FS_Position.Girt, m_arrCrSc[(int)EMemberGroupNames.eGirtWall], fHorizontalDisplacementLimitDenominator_Girt_PL, fHorizontalDisplacementLimitDenominator_Girt_IL, fHorizontalDisplacementLimitDenominator_Girt_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(7, componentList[(int)EMemberGroupNames.ePurlin].ComponentName, EMemberType_FS.eP, EMemberType_FS_Position.Purlin, m_arrCrSc[(int)EMemberGroupNames.ePurlin], fVerticalDisplacementLimitDenominator_Purlin_PL, fVerticalDisplacementLimitDenominator_Purlin_IL, fVerticalDisplacementLimitDenominator_Purlin_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(8, componentList[(int)EMemberGroupNames.eFrontColumn].ComponentName, EMemberType_FS.eC, EMemberType_FS_Position.ColumnFrontSide, m_arrCrSc[(int)EMemberGroupNames.eFrontColumn], fHorizontalDisplacementLimitDenominator_WindPost_PL, fHorizontalDisplacementLimitDenominator_WindPost_IL, fHorizontalDisplacementLimitDenominator_WindPost_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(9, componentList[(int)EMemberGroupNames.eBackColumn].ComponentName, EMemberType_FS.eC, EMemberType_FS_Position.ColumnBackSide, m_arrCrSc[(int)EMemberGroupNames.eBackColumn], fHorizontalDisplacementLimitDenominator_WindPost_PL, fHorizontalDisplacementLimitDenominator_WindPost_IL, fHorizontalDisplacementLimitDenominator_WindPost_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(10, componentList[(int)EMemberGroupNames.eFrontGirt].ComponentName, EMemberType_FS.eG, EMemberType_FS_Position.GirtFrontSide, m_arrCrSc[(int)EMemberGroupNames.eFrontGirt], fHorizontalDisplacementLimitDenominator_Girt_PL, fHorizontalDisplacementLimitDenominator_Girt_IL, fHorizontalDisplacementLimitDenominator_Girt_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(11, componentList[(int)EMemberGroupNames.eBackGirt].ComponentName, EMemberType_FS.eG, EMemberType_FS_Position.GirtBackSide, m_arrCrSc[(int)EMemberGroupNames.eBackGirt], fHorizontalDisplacementLimitDenominator_Girt_PL, fHorizontalDisplacementLimitDenominator_Girt_IL, fHorizontalDisplacementLimitDenominator_Girt_TL, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(12, componentList[(int)EMemberGroupNames.eGirtBracing].ComponentName, EMemberType_FS.eGB, EMemberType_FS_Position.BracingBlockGirts, m_arrCrSc[(int)EMemberGroupNames.eGirtBracing], 0, 0, 0, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(13, componentList[(int)EMemberGroupNames.ePurlinBracing].ComponentName, EMemberType_FS.ePB, EMemberType_FS_Position.BracingBlockPurlins, m_arrCrSc[(int)EMemberGroupNames.ePurlinBracing], 0, 0, 0, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(14, componentList[(int)EMemberGroupNames.eFrontGirtBracing].ComponentName, EMemberType_FS.eGB, EMemberType_FS_Position.BracingBlocksGirtsFrontSide, m_arrCrSc[(int)EMemberGroupNames.eFrontGirtBracing], 0, 0, 0, 0));
            listOfModelMemberGroups.Add(new CMemberGroup(15, componentList[(int)EMemberGroupNames.eBackGirtBracing].ComponentName, EMemberType_FS.eGB, EMemberType_FS_Position.BracingBlocksGirtsBackSide, m_arrCrSc[(int)EMemberGroupNames.eBackGirtBracing], 0, 0, 0, 0));

            // Priradit material prierezov, asi by sa to malo robit uz pri vytvoreni prierezu ale trebalo by upravovat konstruktory :)
            if (m_arrMat.Length >= m_arrCrSc.Length)
            {
                for (int i = 0; i < m_arrCrSc.Length; i++)
                {
                    m_arrCrSc[i].m_Mat = m_arrMat[i];
                }
            }
            else
                throw new Exception("Cross-section material is not defined.");

            // Allignments
            float fallignment_column, fallignment_knee_rafter, fallignment_apex_rafter;
            GetJointAllignments((float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn].h, (float)m_arrCrSc[(int)EMemberGroupNames.eRafter].h, out fallignment_column, out fallignment_knee_rafter, out fallignment_apex_rafter);

            // Member Eccentricities
            // Zadane hodnoty predpokladaju ze prierez je symetricky, je potrebne zobecnit
            CMemberEccentricity eccentricityPurlin = new CMemberEccentricity(0, (float)(0.5 * m_arrCrSc[(int)EMemberGroupNames.eRafter].h - 0.5 * m_arrCrSc[(int)EMemberGroupNames.ePurlin].h));
            CMemberEccentricity eccentricityGirtLeft_X0 = new CMemberEccentricity(0, (float)(-(0.5 * m_arrCrSc[(int)EMemberGroupNames.eMainColumn].h - 0.5 * m_arrCrSc[(int)EMemberGroupNames.eGirtWall].h)));
            CMemberEccentricity eccentricityGirtRight_XB = new CMemberEccentricity(0, (float)(0.5 * m_arrCrSc[(int)EMemberGroupNames.eMainColumn].h - 0.5 * m_arrCrSc[(int)EMemberGroupNames.eGirtWall].h));

            float feccentricityEavePurlin_z = -fallignment_column + (float)m_arrCrSc[(int)EMemberGroupNames.eRafter].h / (float)Math.Cos(fRoofPitch_rad) - (float)m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin].z_max;
            CMemberEccentricity eccentricityEavePurlin = new CMemberEccentricity(-(float)(0.5 * m_arrCrSc[(int)EMemberGroupNames.eMainColumn].h + m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin].y_min), feccentricityEavePurlin_z);

            // Moze byt automaticke alebo uzivatelsky nastavitelne
            //bWindPostEndUnderRafter = m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].h > 0.49f ? true : false; // TODO - nastavovat podla velkosti edge frame rafter // true - stlp konci na spodnej hrane rafter, false - stlp konci na hornej hrane rafter

            if (vm._generalOptionsVM.WindPostUnderRafter)
            {
                eccentricityColumnFront_Z = new CMemberEccentricity(0, -(float)(m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].y_min + m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].z_max));
                eccentricityColumnBack_Z = new CMemberEccentricity(0, -(float)(m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].y_max + m_arrCrSc[(int)EMemberGroupNames.eBackColumn].z_min));

                eccentricityGirtFront_Y0 = new CMemberEccentricity(0, eccentricityColumnFront_Z.MFz_local + (float)(m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].z_max - m_arrCrSc[(int)EMemberGroupNames.eFrontGirt].z_max));
                eccentricityGirtBack_YL = new CMemberEccentricity(0, eccentricityColumnBack_Z.MFz_local + (float)(m_arrCrSc[(int)EMemberGroupNames.eBackColumn].z_min - m_arrCrSc[(int)EMemberGroupNames.eBackGirt].z_min));
            }
            else
            {
                eccentricityColumnFront_Z = new CMemberEccentricity(0, -(float)(m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].y_max + m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].z_max));
                eccentricityColumnBack_Z = new CMemberEccentricity(0, -(float)(m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].y_min + m_arrCrSc[(int)EMemberGroupNames.eBackColumn].z_min));

                eccentricityGirtFront_Y0 = new CMemberEccentricity(0, eccentricityColumnFront_Z.MFz_local + (float)(m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].z_max - m_arrCrSc[(int)EMemberGroupNames.eFrontGirt].z_max + m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].b));
                eccentricityGirtBack_YL = new CMemberEccentricity(0, eccentricityColumnBack_Z.MFz_local + (float)(m_arrCrSc[(int)EMemberGroupNames.eBackColumn].z_min - m_arrCrSc[(int)EMemberGroupNames.eBackGirt].z_min - m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].b));
            }

            // Member Intermediate Supports
            m_arrIntermediateTransverseSupports = new CIntermediateTransverseSupport[1];
            CIntermediateTransverseSupport forkSupport = new CIntermediateTransverseSupport(1, EITSType.eBothFlanges, 0);
            m_arrIntermediateTransverseSupports[0] = forkSupport;

            bool bUseDefaultOrUserDefinedValueForFlyBracing = true; // TODO - zaviest checkbox ci sa maju pouzit hodnoty z databazy / uzivatelom nastavene, alebo sa ma generovat uplne automaticky

            // Frame column fly bracing
            // Index of girt 0 - no bracing 1 - every, 2 - every second girt, 3 - every third girt, ...
            // Poziciu fly bracing - kazdy xx girt nastavovat v GUI, alebo umoznit urcit automaticky, napr. cca tak aby bola vdialenost medzi fly bracing rovna L1

            bool bUseMainColumnFlyBracingPlates = true; // Use fly bracing plates in girt to column joint
            
            if (bUseDefaultOrUserDefinedValueForFlyBracing)
                iMainColumnFlyBracing_EveryXXGirt = sGeometryInputData.iMainColumnFlyBracingEveryXXGirt;
            else
                iMainColumnFlyBracing_EveryXXGirt = Math.Max(0, (int)(fL1_frame / fDist_Girt));

            // Rafter fly bracing
            // Index of purlin 0 - no bracing 1 - every, 2 - every second purlin, 3 - every third purlin, ...
            // Poziciu fly bracing - kazda xx purlin nastavovat v GUI, alebo umoznit urcit automaticky, napr. cca tak aby bola vdialenost medzi fly bracing rovna L1

            bool bUseRafterFlyBracingPlates = true; // Use fly bracing plates in purlin to rafter joint

            if (bUseDefaultOrUserDefinedValueForFlyBracing)
                iRafterFlyBracing_EveryXXPurlin = sGeometryInputData.iRafterFlyBracingEveryXXPurlin;
            else
                iRafterFlyBracing_EveryXXPurlin = Math.Max(0, (int)(fL1_frame / fDist_Purlin));

            // Front and Back Column
            bool bUseFrontColumnFlyBracingPlates = true; // Use fly bracing plates in girt to column joint
            int iFrontColumnFlyBracing_EveryXXGirt = sGeometryInputData.iFrontColumnFlyBracingEveryXXGirt;

            bool bUseBackColumnFlyBracingPlates = true; // Use fly bracing plates in girt to column joint
            int iBackColumnFlyBracing_EveryXXGirt = sGeometryInputData.iBackColumnFlyBracingEveryXXGirt;

            // Transverse bracing - girts, purlins, front girts, back girts
            /*
            bool bUseTransverseBracingBeam_Purlins = true;
            bool bUseTransverseBracingBeam_Girts = true;
            bool bUseTransverseBracingBeam_FrontGirts = true;
            bool bUseTransverseBracingBeam_BackGirts = true;
            */
            int iNumberOfTransverseSupports_EdgePurlins = sGeometryInputData.iEdgePurlin_ILS_Number; // TODO - napojit na generovanie bracing blocks alebo zadavat rucne v GUI
            int iNumberOfTransverseSupports_Purlins = sGeometryInputData.iPurlin_ILS_Number;
            int iNumberOfTransverseSupports_Girts = sGeometryInputData.iGirt_ILS_Number;
            int iNumberOfTransverseSupports_FrontGirts = sGeometryInputData.iGirtFrontSide_ILS_Number;
            int iNumberOfTransverseSupports_BackGirts = sGeometryInputData.iGirtBackSide_ILS_Number;

            // Limit pre poziciu horneho nosnika, mala by to byt polovica suctu vysky edge (eave) purlin h a sirky nosnika b (neberie sa h pretoze nosnik je otoceny o 90 stupnov)
            fUpperGirtLimit = (float)(m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin].h + m_arrCrSc[(int)EMemberGroupNames.eGirtWall].b);

            // Limit pre poziciu horneho nosnika (front / back girt) na prednej alebo zadnej stene budovy
            // Nosnik alebo pripoj nosnika nesmie zasahovat do prievlaku (rafter)
            fz_UpperLimitForFrontGirts = (float)((0.5 * m_arrCrSc[(int)EMemberGroupNames.eRafter].h) / Math.Cos(fRoofPitch_rad) + 0.5f * m_arrCrSc[(int)EMemberGroupNames.eFrontGirt].b);
            fz_UpperLimitForBackGirts = (float)((0.5 * m_arrCrSc[(int)EMemberGroupNames.eRafter].h) / Math.Cos(fRoofPitch_rad) + 0.5f * m_arrCrSc[(int)EMemberGroupNames.eBackGirt].b);

            // Side wall - girts
            bool bGenerateGirts = componentList[(int)EMemberGroupNames.eGirtWall].Generate.Value;
            if (bGenerateGirts)
            {
                iOneColumnGirtNo = (int)((fH1_frame - fUpperGirtLimit - fBottomGirtPosition) / fDist_Girt) + 1;
                iGirtNoInOneFrame = 2 * iOneColumnGirtNo;
            }
            
            componentListVM.SetColumnFlyBracingPosition_Items(iOneColumnGirtNo);  //zakomentovane 20.12.2019 - nechapem naco to tu je

            if (!bGenerateGirts || iMainColumnFlyBracing_EveryXXGirt == 0 || iMainColumnFlyBracing_EveryXXGirt > iGirtNoInOneFrame) // Index 0 means do not use fly bracing, more than number of girts per main column means no fly bracing too
                bUseMainColumnFlyBracingPlates = false;

            float fFirstGirtPosition = fBottomGirtPosition;
            float fFirstPurlinPosition = fDist_Purlin;
            float fRafterLength = MathF.Sqrt(MathF.Pow2(fH2_frame - fH1_frame) + MathF.Pow2(0.5f * fW_frame));

            int iOneRafterPurlinNo = 0;
            iPurlinNoInOneFrame = 0;

            bool bGeneratePurlins = componentList[(int)EMemberGroupNames.ePurlin].Generate.Value;
            if (bGeneratePurlins)
            {
                iOneRafterPurlinNo = (int)((fRafterLength - fFirstPurlinPosition) / fDist_Purlin) + 1;
                iPurlinNoInOneFrame = 2 * iOneRafterPurlinNo;
            }
            componentListVM.SetRafterFlyBracingPosition_Items(iOneRafterPurlinNo); //zakomentovane 20.12.2019 - nechapem naco to tu je

            if (!bGeneratePurlins || iRafterFlyBracing_EveryXXPurlin == 0 || iRafterFlyBracing_EveryXXPurlin > iPurlinNoInOneFrame) // Index 0 means do not use fly bracing, more than number of purlins per rafter means no fly bracing too
                bUseRafterFlyBracingPlates = false;

            iFrontColumnNoInOneFrame = 0;

            bool bGenerateFrontColumns = componentList[(int)EMemberGroupNames.eFrontColumn].Generate.Value;
            if (bGenerateFrontColumns)
            {
                iOneRafterFrontColumnNo = (int)((0.5f * fW_frame) / fDist_FrontColumns);
                iFrontColumnNoInOneFrame = 2 * iOneRafterFrontColumnNo;
                // Update value of distance between columns
                fDist_FrontColumns = (fW_frame / (iFrontColumnNoInOneFrame + 1));
            }

            const int iFrontColumnNodesNo = 2; // Number of Nodes for Front Column
            int iFrontColumninOneRafterNodesNo = iFrontColumnNodesNo * iOneRafterFrontColumnNo; // Number of Nodes for Front Columns under one Rafter
            int iFrontColumninOneFrameNodesNo = 2 * iFrontColumninOneRafterNodesNo; // Number of Nodes for Front Columns under one Frame

            iBackColumnNoInOneFrame = 0;

            fDist_BackColumns = fDist_FrontColumns; // Todo Temporary - umoznit ine roztece medzi zadnymi a prednymi stlpmi

            bool bGenerateBackColumns = componentList[(int)EMemberGroupNames.eBackColumn].Generate.Value;
            if (bGenerateBackColumns)
            {
                iOneRafterBackColumnNo = (int)((0.5f * fW_frame) / fDist_BackColumns);
                iBackColumnNoInOneFrame = 2 * iOneRafterBackColumnNo;
                // Update value of distance between columns
                fDist_BackColumns = (fW_frame / (iBackColumnNoInOneFrame + 1));
            }

            const int iBackColumnNodesNo = 2; // Number of Nodes for Back Column
            int iBackColumninOneRafterNodesNo = iBackColumnNodesNo * iOneRafterBackColumnNo; // Number of Nodes for Back Columns under one Rafter
            int iBackColumninOneFrameNodesNo = 2 * iBackColumninOneRafterNodesNo; // Number of Nodes for Back Columns under one Frame

            // Number of Nodes - Front Girts
            int iFrontIntermediateColumnNodesForGirtsOneRafterNo = 0;
            int iFrontIntermediateColumnNodesForGirtsOneFrameNo = 0;
            iFrontGirtsNoInOneFrame = 0;
            iArrNumberOfNodesPerFrontColumn = new int[iOneRafterFrontColumnNo];

            bool bGenerateFrontGirts = componentList[(int)EMemberGroupNames.eFrontGirt].Generate.Value;

            if (bGenerateFrontGirts)
            {
                iFrontIntermediateColumnNodesForGirtsOneRafterNo = GetNumberofIntermediateNodesInColumnsForOneFrame(iOneRafterFrontColumnNo, fBottomGirtPosition, fDist_FrontColumns, fz_UpperLimitForFrontGirts);
                iFrontIntermediateColumnNodesForGirtsOneFrameNo = 2 * iFrontIntermediateColumnNodesForGirtsOneRafterNo;

                // Number of Girts - Main Frame Column
                //iOneColumnGirtNo = (int)((fH1_frame - fUpperGirtLimit - fBottomGirtPosition) / fDist_Girt) + 1;

                iFrontGirtsNoInOneFrame = iOneColumnGirtNo;

                // Number of girts under one rafter at the frontside of building - middle girts are considered twice
                for (int i = 0; i < iOneRafterFrontColumnNo; i++)
                {
                    int temp = GetNumberofIntermediateNodesInOneColumnForGirts(fBottomGirtPosition, fDist_FrontColumns, fz_UpperLimitForFrontGirts, i);
                    iFrontGirtsNoInOneFrame += temp;
                    iArrNumberOfNodesPerFrontColumn[i] = temp;
                }

                iFrontGirtsNoInOneFrame *= 2;
                // Girts in the middle are considered twice - remove one set
                iFrontGirtsNoInOneFrame -= iArrNumberOfNodesPerFrontColumn[iOneRafterFrontColumnNo - 1];
            }
            componentListVM.SetFrontColumnFlyBracingPosition_Items(iOneColumnGirtNo); //zakomentovane 20.12.2019 - nechapem naco to tu je

            if (!bGenerateFrontGirts || iFrontColumnFlyBracing_EveryXXGirt == 0) // Index 0 means do not use fly bracing
                bUseFrontColumnFlyBracingPlates = false;

            // Number of Nodes - Back Girts
            int iBackIntermediateColumnNodesForGirtsOneRafterNo = 0;
            int iBackIntermediateColumnNodesForGirtsOneFrameNo = 0;
            iBackGirtsNoInOneFrame = 0;
            iArrNumberOfNodesPerBackColumn = new int[iOneRafterBackColumnNo];

            bool bGenerateBackGirts = componentList[(int)EMemberGroupNames.eBackGirt].Generate.Value;

            if (bGenerateBackGirts)
            {
                iBackIntermediateColumnNodesForGirtsOneRafterNo = GetNumberofIntermediateNodesInColumnsForOneFrame(iOneRafterBackColumnNo, fBottomGirtPosition, fDist_BackColumns, fz_UpperLimitForBackGirts);
                iBackIntermediateColumnNodesForGirtsOneFrameNo = 2 * iBackIntermediateColumnNodesForGirtsOneRafterNo;

                // Number of Girts - Main Frame Column
                //iOneColumnGirtNo = (int)((fH1_frame - fUpperGirtLimit - fBottomGirtPosition) / fDist_Girt) + 1;

                iBackGirtsNoInOneFrame = iOneColumnGirtNo;

                // Number of girts under one rafter at the frontside of building - middle girts are considered twice
                for (int i = 0; i < iOneRafterBackColumnNo; i++)
                {
                    int temp = GetNumberofIntermediateNodesInOneColumnForGirts(fBottomGirtPosition, fDist_BackColumns, fz_UpperLimitForBackGirts, i);
                    iBackGirtsNoInOneFrame += temp;
                    iArrNumberOfNodesPerBackColumn[i] = temp;
                }

                iBackGirtsNoInOneFrame *= 2;
                // Girts in the middle are considered twice - remove one set
                iBackGirtsNoInOneFrame -= iArrNumberOfNodesPerBackColumn[iOneRafterBackColumnNo - 1];
            }
            componentListVM.SetBackColumnFlyBracingPosition_Items(iOneColumnGirtNo); //zakomentovane 20.12.2019 - nechapem naco to tu je

            if (!bGenerateBackGirts || iBackColumnFlyBracing_EveryXXGirt == 0) // Index 0 means do not use fly bracing
                bUseBackColumnFlyBracingPlates = false;

            // Sidewall girts bracing blocks
            bool bGenerateGirtBracingSideWalls = true;

            int iNumberOfGBSideWallsNodesInOneBayOneSide = 0;
            int iNumberOfGBSideWallsNodesInOneBay = 0;
            int iGBSideWallsNodesNo = 0;

            int iNumberOfGBSideWallsMembersInOneBayOneSide = 0;
            int iNumberOfGBSideWallsMembersInOneBay = 0;
            
            // TODO 408 - Zapracovat toto nastavenie do GUI - prebrat s Ondrejom a dopracovat funkcionalitu tak ze sa budu generovat len bracing blocks na stenach 
            // alebo pre purlins v kazdom druhom rade (medzera medzi girts alebo purlins)

            bool bUseGBEverySecondGUI = vm._generalOptionsVM.BracingEverySecondRowOfGirts; 
            bool bUseGBEverySecond = bUseGBEverySecondGUI && (iOneColumnGirtNo % 2 != 0); // Nastavena hodnota je true a pocet bracing blocks na vysku steny je neparny

            if (bGenerateGirtBracingSideWalls)
            {
                iNumberOfGBSideWallsNodesInOneBayOneSide = iNumberOfTransverseSupports_Girts * (iOneColumnGirtNo + 1);
                iNumberOfGBSideWallsNodesInOneBay = 2 * iNumberOfGBSideWallsNodesInOneBayOneSide;
                iGBSideWallsNodesNo = iNumberOfGBSideWallsNodesInOneBay * (iFrameNo - 1);

                iNumberOfGBSideWallsMembersInOneBayOneSide = iNumberOfTransverseSupports_Girts * iOneColumnGirtNo;
                iNumberOfGBSideWallsMembersInOneBay = 2 * iNumberOfGBSideWallsMembersInOneBayOneSide;
                iGBSideWallsMembersNo = iNumberOfGBSideWallsMembersInOneBay * (iFrameNo - 1);
            }

            // Purlin bracing blocks
            bool bGeneratePurlinBracing = true;

            int iNumberOfPBNodesInOneBayOneSide = 0;
            int iNumberOfPBNodesInOneBay = 0;
            int iPBNodesNo = 0;

            int iNumberOfPBMembersInOneBayOneSide = 0;
            int iNumberOfPBMembersInOneBay = 0;

            bool bUsePBEverySecondGUI = vm._generalOptionsVM.BracingEverySecondRowOfPurlins;
            bool bUsePBEverySecond = bUsePBEverySecondGUI && (iOneRafterPurlinNo % 2 != 0); // Nastavena hodnota je true a pocet bracing blocks na stranu strechy je neparny

            if (bGeneratePurlinBracing)
            {
                iNumberOfPBNodesInOneBayOneSide = iNumberOfTransverseSupports_Purlins * (iOneRafterPurlinNo + 1);
                iNumberOfPBNodesInOneBay = 2 * iNumberOfPBNodesInOneBayOneSide;
                iPBNodesNo = iNumberOfPBNodesInOneBay * (iFrameNo - 1);

                iNumberOfPBMembersInOneBayOneSide = iNumberOfTransverseSupports_Purlins * iOneRafterPurlinNo;
                iNumberOfPBMembersInOneBay = 2 * iNumberOfPBMembersInOneBayOneSide;
                iPBMembersNo = iNumberOfPBMembersInOneBay * (iFrameNo - 1);
            }

            // Front side girts bracing blocks
            bool bGenerateGirtBracingFrontSide = true;

            int[] iArrGB_FS_NumberOfNodesPerBay = new int[iArrNumberOfNodesPerFrontColumn.Length + 1];
            int[] iArrGB_FS_NumberOfNodesPerBayFirstNode = new int[iArrNumberOfNodesPerFrontColumn.Length + 1];
            int[] iArrGB_FS_NumberOfMembersPerBay = new int[iArrNumberOfNodesPerFrontColumn.Length + 1];
            int iNumberOfGB_FSNodesInOneFrame = 0;

            if (bGenerateGirtBracingFrontSide)
            {
                // First bay - pocet girts urcime podla poctu uzlov pre girts na edge / main column
                iArrGB_FS_NumberOfNodesPerBay[0] = (iOneColumnGirtNo + 1) * iNumberOfTransverseSupports_FrontGirts; // Pridame o jeden rad uzlov viac - nachadzaju sa na edge rafter
                iArrGB_FS_NumberOfNodesPerBayFirstNode[0] = (iOneColumnGirtNo + 1);
                iArrGB_FS_NumberOfMembersPerBay[0] = iOneColumnGirtNo * iNumberOfTransverseSupports_FrontGirts;

                iNumberOfGB_FSNodesInOneFrame = iArrGB_FS_NumberOfNodesPerBay[0];
                iNumberOfGB_FSMembersInOneFrame = iArrGB_FS_NumberOfMembersPerBay[0];

                for (int i = 0; i < iArrNumberOfNodesPerFrontColumn.Length; i++)
                {
                    iArrGB_FS_NumberOfNodesPerBay[i+1] = (iArrNumberOfNodesPerFrontColumn[i] + 1) * iNumberOfTransverseSupports_FrontGirts;
                    iArrGB_FS_NumberOfNodesPerBayFirstNode[i + 1] = iArrNumberOfNodesPerFrontColumn[i] + 1;
                    iArrGB_FS_NumberOfMembersPerBay[i+1] = iArrNumberOfNodesPerFrontColumn[i] * iNumberOfTransverseSupports_FrontGirts;

                    iNumberOfGB_FSNodesInOneFrame += iArrGB_FS_NumberOfNodesPerBay[i + 1];
                    iNumberOfGB_FSMembersInOneFrame += iArrGB_FS_NumberOfMembersPerBay[i + 1];
                }

                iNumberOfGB_FSNodesInOneFrame *= 2;
                iNumberOfGB_FSMembersInOneFrame *= 2;
                // Girt bracing block nodes / members in the middle are considered twice - remove one set

                iNumberOfGB_FSNodesInOneFrame -= iArrGB_FS_NumberOfNodesPerBay[iOneRafterFrontColumnNo];
                iNumberOfGB_FSMembersInOneFrame -= iArrGB_FS_NumberOfMembersPerBay[iOneRafterFrontColumnNo];
            }

            // Back side girts bracing blocks
            bool bGenerateGirtBracingBackSide = true;

            int[] iArrGB_BS_NumberOfNodesPerBay = new int[iArrNumberOfNodesPerBackColumn.Length + 1];
            int[] iArrGB_BS_NumberOfNodesPerBayFirstNode = new int[iArrNumberOfNodesPerBackColumn.Length + 1];
            int[] iArrGB_BS_NumberOfMembersPerBay = new int[iArrNumberOfNodesPerBackColumn.Length + 1];
            int iNumberOfGB_BSNodesInOneFrame = 0;

            if (bGenerateGirtBracingBackSide)
            {
                // First bay - pocet girts urcime podla poctu uzlov pre girts na edge / main column
                iArrGB_BS_NumberOfNodesPerBay[0] = (iOneColumnGirtNo + 1) * iNumberOfTransverseSupports_BackGirts; // Pridame o jeden rad uzlov viac - nachadzaju sa na edge rafter
                iArrGB_BS_NumberOfNodesPerBayFirstNode[0] = (iOneColumnGirtNo + 1);
                iArrGB_BS_NumberOfMembersPerBay[0] = iOneColumnGirtNo * iNumberOfTransverseSupports_BackGirts;

                iNumberOfGB_BSNodesInOneFrame = iArrGB_BS_NumberOfNodesPerBay[0];
                iNumberOfGB_BSMembersInOneFrame = iArrGB_BS_NumberOfMembersPerBay[0];

                for (int i = 0; i < iArrNumberOfNodesPerBackColumn.Length; i++)
                {
                    iArrGB_BS_NumberOfNodesPerBay[i + 1] = (iArrNumberOfNodesPerBackColumn[i] + 1) * iNumberOfTransverseSupports_BackGirts;
                    iArrGB_BS_NumberOfNodesPerBayFirstNode[i + 1] = iArrNumberOfNodesPerBackColumn[i] + 1;
                    iArrGB_BS_NumberOfMembersPerBay[i + 1] = iArrNumberOfNodesPerBackColumn[i] * iNumberOfTransverseSupports_BackGirts;

                    iNumberOfGB_BSNodesInOneFrame += iArrGB_BS_NumberOfNodesPerBay[i + 1];
                    iNumberOfGB_BSMembersInOneFrame += iArrGB_BS_NumberOfMembersPerBay[i + 1];
                }

                iNumberOfGB_BSNodesInOneFrame *= 2;
                iNumberOfGB_BSMembersInOneFrame *= 2;
                // Girt bracing block nodes / members in the middle are considered twice - remove one set

                iNumberOfGB_BSNodesInOneFrame -= iArrGB_BS_NumberOfNodesPerBay[iOneRafterBackColumnNo];
                iNumberOfGB_BSMembersInOneFrame -= iArrGB_BS_NumberOfMembersPerBay[iOneRafterBackColumnNo];
            }

            m_arrNodes = new CNode[iFrameNodesNo * iFrameNo + iFrameNo * iGirtNoInOneFrame + iFrameNo * iPurlinNoInOneFrame + iFrontColumninOneFrameNodesNo + iBackColumninOneFrameNodesNo + iFrontIntermediateColumnNodesForGirtsOneFrameNo + iBackIntermediateColumnNodesForGirtsOneFrameNo + iGBSideWallsNodesNo + iPBNodesNo + iNumberOfGB_FSNodesInOneFrame + iNumberOfGB_BSNodesInOneFrame];
            m_arrMembers = new CMember[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame + iFrontGirtsNoInOneFrame + iBackGirtsNoInOneFrame + iGBSideWallsMembersNo + iPBMembersNo + iNumberOfGB_FSMembersInOneFrame + iNumberOfGB_BSMembersInOneFrame];

            float fCutOffOneSide = 0.005f; // Cut 5 mm from each side of member

            // Allignments (zaporna hodnota skracuje prut)
            float fMainColumnStart = 0.0f; // Dlzka orezu pruta stlpa na zaciatku (pri base plate) (zaporna hodnota skracuje prut)
            float fMainColumnEnd = -fallignment_column - fCutOffOneSide; // Dlzka orezu pruta stlpa na konci (zaporna hodnota skracuje prut)
            float fRafterStart = fallignment_knee_rafter - fCutOffOneSide;
            float fRafterEnd = -fallignment_apex_rafter - fCutOffOneSide;                                                // Calculate according to h of rafter and roof pitch
            float fEavesPurlinStart = -(float)m_arrCrSc[(int)EMemberGroupNames.eRafter].y_max - fCutOffOneSide;
            float fEavesPurlinEnd = (float)m_arrCrSc[(int)EMemberGroupNames.eRafter].y_min - fCutOffOneSide;
            float fGirtStart = -(float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn].y_max - fCutOffOneSide;
            float fGirtEnd = (float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn].y_min - fCutOffOneSide;
            float fPurlinStart = -(float)m_arrCrSc[(int)EMemberGroupNames.eRafter].y_max - fCutOffOneSide;
            float fPurlinEnd = (float)m_arrCrSc[(int)EMemberGroupNames.eRafter].y_min - fCutOffOneSide;

            float fFrontColumnStart = 0.0f;
            float fFrontColumnEnd = (vm._generalOptionsVM.WindPostUnderRafter ? (float)m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].z_min : (float)m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].z_max) / (float)Math.Cos(fRoofPitch_rad) + (float)m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].y_min * (float)Math.Tan(fRoofPitch_rad) /*- fCutOffOneSide*/;
            float fBackColumnStart = 0.0f;
            float fBackColumnEnd = (vm._generalOptionsVM.WindPostUnderRafter ? (float)m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].z_min : (float)m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].z_max) / (float)Math.Cos(fRoofPitch_rad) + (float)m_arrCrSc[(int)EMemberGroupNames.eBackColumn].y_min * (float)Math.Tan(fRoofPitch_rad) /*- fCutOffOneSide*/;

            float fFrontGirtStart = (float)m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].y_min - fCutOffOneSide;    // Just in case that cross-section of column is symmetric about z-z
            float fFrontGirtEnd = (float)m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].y_min - fCutOffOneSide;      // Just in case that cross-section of column is symmetric about z-z
            float fBackGirtStart = (float)m_arrCrSc[(int)EMemberGroupNames.eBackColumn].y_min - fCutOffOneSide;      // Just in case that cross-section of column is symmetric about z-z
            float fBackGirtEnd = (float)m_arrCrSc[(int)EMemberGroupNames.eBackColumn].y_min - fCutOffOneSide;        // Just in case that cross-section of column is symmetric about z-z
            float fFrontGirtStart_MC = (float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF].z_min - fCutOffOneSide;  // Connection to the main frame column (column symmetrical about y-y)
            float fFrontGirtEnd_MC = (float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF].z_min - fCutOffOneSide;    // Connection to the main frame column (column symmetrical about y-y)
            float fBackGirtStart_MC = (float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF].z_min - fCutOffOneSide;   // Connection to the main frame column (column symmetrical about y-y)
            float fBackGirtEnd_MC = (float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF].z_min - fCutOffOneSide;     // Connection to the main frame column (column symmetrical about y-y)

            float fGBSideWallStart = -(float)m_arrCrSc[(int)EMemberGroupNames.eGirtWall].y_max - fCutOffOneSide;
            float fGBSideWallEnd = (float)m_arrCrSc[(int)EMemberGroupNames.eGirtWall].y_min - fCutOffOneSide;

            float fGBFrontSideStart = -(float)m_arrCrSc[(int)EMemberGroupNames.eFrontGirt].y_max - fCutOffOneSide;
            float fGBFrontSideEnd = (float)m_arrCrSc[(int)EMemberGroupNames.eFrontGirt].y_min - fCutOffOneSide;

            float fGBBackSideStart = -(float)m_arrCrSc[(int)EMemberGroupNames.eBackGirt].y_max - fCutOffOneSide;
            float fGBBackSideEnd = (float)m_arrCrSc[(int)EMemberGroupNames.eBackGirt].y_min - fCutOffOneSide;

            float fColumnsRotation = MathF.fPI / 2.0f;
            float fGirtsRotation = MathF.fPI / 2.0f;

            listOfSupportedNodes_S1 = new List<CNode>();
            listOfSupportedNodes_S2 = new List<CNode>();
            // Nodes Automatic Generation
            // Nodes List - Nodes Array

            // Nodes - Frames
            for (int i = 0; i < iFrameNo; i++)
            {
                m_arrNodes[i * iFrameNodesNo + 0] = new CNode(i * iFrameNodesNo + 1, 000000, i * fL1_frame, 00000, 0);
                m_arrNodes[i * iFrameNodesNo + 0].Name = "Main Column Base Node - left";
                listOfSupportedNodes_S1.Add(m_arrNodes[i * iFrameNodesNo + 0]);
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i * iFrameNodesNo + 0]);

                m_arrNodes[i * iFrameNodesNo + 1] = new CNode(i * iFrameNodesNo + 2, 000000, i * fL1_frame, fH1_frame, 0);
                m_arrNodes[i * iFrameNodesNo + 1].Name = "Main Column Top Node - left";
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i * iFrameNodesNo + 1]);

                m_arrNodes[i * iFrameNodesNo + 2] = new CNode(i * iFrameNodesNo + 3, 0.5f * fW_frame, i * fL1_frame, fH2_frame, 0);
                m_arrNodes[i * iFrameNodesNo + 2].Name = "Apex Node";
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i * iFrameNodesNo + 2]);

                m_arrNodes[i * iFrameNodesNo + 3] = new CNode(i * iFrameNodesNo + 4, fW_frame, i * fL1_frame, fH1_frame, 0);
                m_arrNodes[i * iFrameNodesNo + 3].Name = "Main Column Top Node - right";
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i * iFrameNodesNo + 3]);

                m_arrNodes[i * iFrameNodesNo + 4] = new CNode(i * iFrameNodesNo + 5, fW_frame, i * fL1_frame, 00000, 0);
                m_arrNodes[i * iFrameNodesNo + 4].Name = "Main Column Base Node - right";
                listOfSupportedNodes_S1.Add(m_arrNodes[i * iFrameNodesNo + 4]);
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i * iFrameNodesNo + 4]);
            }

            // Members
            for (int i = 0; i < iFrameNo; i++)
            {
                int iCrscColumnIndex = (int)EMemberGroupNames.eMainColumn;
                int iCrscRafterIndex = (int)EMemberGroupNames.eRafter;
                EMemberType_FS eColumnType = EMemberType_FS.eMC;
                EMemberType_FS eRafterType = EMemberType_FS.eMR;
                EMemberType_FS_Position eColumnType_Position = EMemberType_FS_Position.MainColumn;
                EMemberType_FS_Position eRafterType_Position = EMemberType_FS_Position.MainRafter;

                if (i == 0 || i == (iFrameNo - 1))
                {
                    iCrscColumnIndex = (int)EMemberGroupNames.eMainColumn_EF;
                    iCrscRafterIndex = (int)EMemberGroupNames.eRafter_EF;
                    eColumnType = EMemberType_FS.eEC;
                    eRafterType = EMemberType_FS.eER;
                    eColumnType_Position = EMemberType_FS_Position.EdgeColumn;
                    eRafterType_Position = EMemberType_FS_Position.EdgeRafter;
                }

                // Main Column
                m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 0] = new CMember((i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 1, m_arrNodes[i * iFrameNodesNo + 0], m_arrNodes[i * iFrameNodesNo + 1], m_arrCrSc[iCrscColumnIndex], eColumnType, eColumnType_Position, null, null, fMainColumnStart, fMainColumnEnd, 0f, 0);
                CreateAndAssignIrregularTransverseSupportGroupAndLTBsegmentGroup(bUseMainColumnFlyBracingPlates, iMainColumnFlyBracing_EveryXXGirt, fBottomGirtPosition, fDist_Girt, ref m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 0]);

                // Rafters
                m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 1] = new CMember((i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 2, m_arrNodes[i * iFrameNodesNo + 1], m_arrNodes[i * iFrameNodesNo + 2], m_arrCrSc[iCrscRafterIndex], eRafterType, eRafterType_Position, null, null, fRafterStart, fRafterEnd, 0f, 0);
                CreateAndAssignIrregularTransverseSupportGroupAndLTBsegmentGroup(bUseRafterFlyBracingPlates, iRafterFlyBracing_EveryXXPurlin, fFirstPurlinPosition, fDist_Purlin, ref m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 1]);

                m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 2] = new CMember((i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 3, m_arrNodes[i * iFrameNodesNo + 2], m_arrNodes[i * iFrameNodesNo + 3], m_arrCrSc[iCrscRafterIndex], eRafterType, eRafterType_Position, null, null, fRafterEnd, fRafterStart, 0f, 0);
                // Reversed sequence of ILS
                CreateAndAssignReversedIrregularTransverseSupportGroupAndLTBsegmentGroup(bUseRafterFlyBracingPlates, iRafterFlyBracing_EveryXXPurlin, fFirstPurlinPosition, fDist_Purlin, ref m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 2]);

                // Main Column
                m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 3] = new CMember((i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 4, m_arrNodes[i * iFrameNodesNo + 3], m_arrNodes[i * iFrameNodesNo + 4], m_arrCrSc[iCrscColumnIndex], eColumnType, eColumnType_Position, null, null, fMainColumnEnd, fMainColumnStart, 0f, 0);

                // Reversed sequence of ILS
                CreateAndAssignReversedIrregularTransverseSupportGroupAndLTBsegmentGroup(bUseMainColumnFlyBracingPlates, iMainColumnFlyBracing_EveryXXGirt, fBottomGirtPosition, fDist_Girt, ref m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 3]);

                // Eaves Purlins
                if (i < (iFrameNo - 1))
                {
                    // Left - osa z prierezu smeruje dole
                    CMemberEccentricity eccEavePurlinLeft = new CMemberEccentricity(eccentricityEavePurlin.MFy_local, -eccentricityEavePurlin.MFz_local);
                    m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 4] = new CMember((i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 5, m_arrNodes[i * iFrameNodesNo + 1], m_arrNodes[(i + 1) * iFrameNodesNo + 1], m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin], EMemberType_FS.eEP, EMemberType_FS_Position.EdgePurlin, eccEavePurlinLeft, eccEavePurlinLeft, fEavesPurlinStart, fEavesPurlinEnd, (float)Math.PI, 0);

                    // Right - osa z prierezu smeruje hore
                    m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 5] = new CMember((i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 6, m_arrNodes[i * iFrameNodesNo + 3], m_arrNodes[(i + 1) * iFrameNodesNo + 3], m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin], EMemberType_FS.eEP, EMemberType_FS_Position.EdgePurlin, eccentricityEavePurlin, eccentricityEavePurlin, fEavesPurlinStart, fEavesPurlinEnd, 0f, 0);
                    CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 4], iNumberOfTransverseSupports_EdgePurlins);
                    CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 5], iNumberOfTransverseSupports_EdgePurlins);
                }
            }

            // Nodes - Girts
            int i_temp_numberofNodes = iFrameNodesNo * iFrameNo;
            if (bGenerateGirts)
            {
                for (int i = 0; i < iFrameNo; i++)
                {
                    for (int j = 0; j < iOneColumnGirtNo; j++)
                    {
                        m_arrNodes[i_temp_numberofNodes + i * iGirtNoInOneFrame + j] = new CNode(i_temp_numberofNodes + i * iGirtNoInOneFrame + j + 1, 000000, i * fL1_frame, fBottomGirtPosition + j * fDist_Girt, 0);
                        RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + i * iGirtNoInOneFrame + j]);
                    }

                    for (int j = 0; j < iOneColumnGirtNo; j++)
                    {
                        m_arrNodes[i_temp_numberofNodes + i * iGirtNoInOneFrame + iOneColumnGirtNo + j] = new CNode(i_temp_numberofNodes + i * iGirtNoInOneFrame + iOneColumnGirtNo + j + 1, fW_frame, i * fL1_frame, fBottomGirtPosition + j * fDist_Girt, 0);
                        RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + i * iGirtNoInOneFrame + iOneColumnGirtNo + j]);
                    }
                }
            }

            // Members - Girts
            int i_temp_numberofMembers = iMainColumnNo + iRafterNo + iEavesPurlinNoInOneFrame * (iFrameNo - 1);
            float fIntermediateSupportSpacingGirts = fL1_frame / (iNumberOfTransverseSupports_Girts + 1); // number of LTB segments = number of support + 1

            if (bGenerateGirts)
            {
                for (int i = 0; i < (iFrameNo - 1); i++)
                {
                    for (int j = 0; j < iOneColumnGirtNo; j++)
                    {
                        m_arrMembers[i_temp_numberofMembers + i * iGirtNoInOneFrame + j] = new CMember(i_temp_numberofMembers + i * iGirtNoInOneFrame + j + 1, m_arrNodes[i_temp_numberofNodes + i * iGirtNoInOneFrame + j], m_arrNodes[i_temp_numberofNodes + (i + 1) * iGirtNoInOneFrame + j], m_arrCrSc[(int)EMemberGroupNames.eGirtWall], EMemberType_FS.eG, EMemberType_FS_Position.Girt, eccentricityGirtLeft_X0, eccentricityGirtLeft_X0, fGirtStart, fGirtEnd, fGirtsRotation, 0);
                        RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofMembers + i * iGirtNoInOneFrame + j]);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + i * iGirtNoInOneFrame + j], iNumberOfTransverseSupports_Girts);
                    }

                    for (int j = 0; j < iOneColumnGirtNo; j++)
                    {
                        m_arrMembers[i_temp_numberofMembers + i * iGirtNoInOneFrame + iOneColumnGirtNo + j] = new CMember(i_temp_numberofMembers + i * iGirtNoInOneFrame + iOneColumnGirtNo + j + 1, m_arrNodes[i_temp_numberofNodes + i * iGirtNoInOneFrame + iOneColumnGirtNo + j], m_arrNodes[i_temp_numberofNodes + (i + 1) * iGirtNoInOneFrame + iOneColumnGirtNo + j], m_arrCrSc[(int)EMemberGroupNames.eGirtWall], EMemberType_FS.eG, EMemberType_FS_Position.Girt, eccentricityGirtRight_XB, eccentricityGirtRight_XB, fGirtStart, fGirtEnd, fGirtsRotation, 0);
                        RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofMembers + i * iGirtNoInOneFrame + iOneColumnGirtNo + j]);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + i * iGirtNoInOneFrame + iOneColumnGirtNo + j], iNumberOfTransverseSupports_Girts);
                    }
                }
            }

            // Nodes - Purlins
            i_temp_numberofNodes += bGenerateGirts ? (iGirtNoInOneFrame * iFrameNo) : 0;
            float fIntermediateSupportSpacingPurlins = fL1_frame / (iNumberOfTransverseSupports_Purlins + 1); // number of LTB segments = number of support + 1

            if (bGeneratePurlins)
            {
                for (int i = 0; i < iFrameNo; i++)
                {
                    for (int j = 0; j < iOneRafterPurlinNo; j++)
                    {
                        float x_glob, z_glob;
                        CalcPurlinNodeCoord(fFirstPurlinPosition + j * fDist_Purlin, out x_glob, out z_glob);

                        m_arrNodes[i_temp_numberofNodes + i * iPurlinNoInOneFrame + j] = new CNode(i_temp_numberofNodes + i * iPurlinNoInOneFrame + j + 1, x_glob, i * fL1_frame, z_glob, 0);
                        RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + i * iPurlinNoInOneFrame + j]);
                    }

                    for (int j = 0; j < iOneRafterPurlinNo; j++)
                    {
                        float x_glob, z_glob;
                        CalcPurlinNodeCoord(fFirstPurlinPosition + j * fDist_Purlin, out x_glob, out z_glob);

                        m_arrNodes[i_temp_numberofNodes + i * iPurlinNoInOneFrame + iOneRafterPurlinNo + j] = new CNode(i_temp_numberofNodes + i * iPurlinNoInOneFrame + iOneRafterPurlinNo + j + 1, fW_frame - x_glob, i * fL1_frame, z_glob, 0);
                        RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + i * iPurlinNoInOneFrame + iOneRafterPurlinNo + j]);
                    }
                }
            }

            // Members - Purlins
            i_temp_numberofMembers += bGenerateGirts ? (iGirtNoInOneFrame * (iFrameNo - 1)) : 0;
            if (bGeneratePurlins)
            {
                for (int i = 0; i < (iFrameNo - 1); i++)
                {
                    for (int j = 0; j < iOneRafterPurlinNo; j++)
                    {
                        CMemberEccentricity temp = new CMemberEccentricity();
                        float fRotationAngle;

                        bool bOrientationOfLocalZAxisIsUpward = true;

                        if (bOrientationOfLocalZAxisIsUpward)
                        {
                            fRotationAngle = -fRoofPitch_rad;
                            temp.MFz_local = eccentricityPurlin.MFz_local;
                        }
                        else
                        {
                            fRotationAngle = -(fRoofPitch_rad + (float)Math.PI);
                            temp.MFz_local = -eccentricityPurlin.MFz_local; // We need to change sign of eccentrictiy for purlins on the left side because z axis of these purlins is oriented downwards
                        }

                        m_arrMembers[i_temp_numberofMembers + i * iPurlinNoInOneFrame + j] = new CMember(i_temp_numberofMembers + i * iPurlinNoInOneFrame + j + 1, m_arrNodes[i_temp_numberofNodes + i * iPurlinNoInOneFrame + j], m_arrNodes[i_temp_numberofNodes + (i + 1) * iPurlinNoInOneFrame + j], m_arrCrSc[(int)EMemberGroupNames.ePurlin], EMemberType_FS.eP, EMemberType_FS_Position.Purlin, temp/*eccentricityPurlin*/, temp /*eccentricityPurlin*/, fPurlinStart, fPurlinEnd, fRotationAngle, 0);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + i * iPurlinNoInOneFrame + j], iNumberOfTransverseSupports_Purlins);
                    }

                    for (int j = 0; j < iOneRafterPurlinNo; j++)
                    {
                        m_arrMembers[i_temp_numberofMembers + i * iPurlinNoInOneFrame + iOneRafterPurlinNo + j] = new CMember(i_temp_numberofMembers + i * iPurlinNoInOneFrame + iOneRafterPurlinNo + j + 1, m_arrNodes[i_temp_numberofNodes + i * iPurlinNoInOneFrame + iOneRafterPurlinNo + j], m_arrNodes[i_temp_numberofNodes + (i + 1) * iPurlinNoInOneFrame + iOneRafterPurlinNo + j], m_arrCrSc[(int)EMemberGroupNames.ePurlin], EMemberType_FS.eP, EMemberType_FS_Position.Purlin, eccentricityPurlin, eccentricityPurlin, fPurlinStart, fPurlinEnd, fRoofPitch_rad, 0);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + i * iPurlinNoInOneFrame + iOneRafterPurlinNo + j], iNumberOfTransverseSupports_Purlins);
                    }
                }
            }

            // Front Columns
            // Nodes - Front Columns
            i_temp_numberofNodes += bGeneratePurlins ? (iPurlinNoInOneFrame * iFrameNo) : 0;
            if (bGenerateFrontColumns)
            {
                AddColumnsNodes(i_temp_numberofNodes, i_temp_numberofMembers, iOneRafterFrontColumnNo, iFrontColumnNoInOneFrame, fDist_FrontColumns, 0);
            }

            // Members - Front Columns
            i_temp_numberofMembers += bGeneratePurlins ? (iPurlinNoInOneFrame * (iFrameNo - 1)) : 0;
            if (bGenerateFrontColumns)
            {
                AddColumnsMembers(i_temp_numberofNodes, i_temp_numberofMembers, iOneRafterFrontColumnNo, iFrontColumnNoInOneFrame, eccentricityColumnFront_Z, fFrontColumnStart, fFrontColumnEnd, m_arrCrSc[(int)EMemberGroupNames.eFrontColumn], fColumnsRotation, bUseFrontColumnFlyBracingPlates, iFrontColumnFlyBracing_EveryXXGirt, fBottomGirtPosition, fDist_FrontGirts);
            }

            // Back Columns
            // Nodes - Back Columns
            i_temp_numberofNodes += bGenerateFrontColumns ? iFrontColumninOneFrameNodesNo : 0;

            if (bGenerateBackColumns)
            {
                AddColumnsNodes(i_temp_numberofNodes, i_temp_numberofMembers, iOneRafterBackColumnNo, iBackColumnNoInOneFrame, fDist_BackColumns, fL_tot);
            }

            // Members - Back Columns
            i_temp_numberofMembers += bGenerateFrontColumns ? iFrontColumnNoInOneFrame : 0;
            if (bGenerateBackColumns)
            {
                AddColumnsMembers(i_temp_numberofNodes, i_temp_numberofMembers, iOneRafterBackColumnNo, iBackColumnNoInOneFrame, eccentricityColumnBack_Z, fBackColumnStart, fBackColumnEnd, m_arrCrSc[(int)EMemberGroupNames.eBackColumn], fColumnsRotation, bUseBackColumnFlyBracingPlates, iBackColumnFlyBracing_EveryXXGirt, fBottomGirtPosition, fDist_BackGirts);
            }

            // Front Girts
            // Nodes - Front Girts
            i_temp_numberofNodes += bGenerateBackColumns ? iBackColumninOneFrameNodesNo : 0;
            float fIntermediateSupportSpacingGirtsFrontSide = fDist_FrontColumns / (iNumberOfTransverseSupports_FrontGirts + 1); // number of LTB segments = number of support + 1

            if (bGenerateFrontGirts)
            {
                AddFrontOrBackGirtsNodes(iOneRafterFrontColumnNo, iArrNumberOfNodesPerFrontColumn, i_temp_numberofNodes, iFrontIntermediateColumnNodesForGirtsOneRafterNo, fDist_FrontGirts, fDist_FrontColumns, 0);
            }

            // Front Girts
            // Members - Front Girts
            // TODO - doplnit riesenie pre maly rozpon ked neexistuju mezilahle stlpiky, prepojenie mezi hlavnymi stplmi ramu na celu sirku budovy
            // TODO - toto riesenie plati len ak existuju girts v pozdlznom smere, ak budu deaktivovane a nevytvoria sa uzly na stlpoch tak sa musia pruty na celnych stenach generovat uplne inak, musia sa vygenerovat aj uzly na stlpoch ....
            // TODO - pri vacsom sklone strechy (cca > 35 stupnov) by bolo dobre dogenerovat prvky ktore nie su na oboch stranach pripojene k stlpom ale su na jeden strane pripojene na stlp a na druhej strane na rafter, inak vznikaju prilis velke prazdne oblasti bez podpory (trojuhoniky) pod hlavnym ramom

            i_temp_numberofMembers += bGenerateBackColumns ? iBackColumnNoInOneFrame : 0;
            if (bGenerateFrontGirts)
            {
                AddFrontOrBackGirtsMembers(iFrameNodesNo, iOneRafterFrontColumnNo, iArrNumberOfNodesPerFrontColumn, i_temp_numberofNodes, i_temp_numberofMembers, iFrontIntermediateColumnNodesForGirtsOneRafterNo, iFrontIntermediateColumnNodesForGirtsOneFrameNo, 0, fDist_Girt, eccentricityGirtFront_Y0, fFrontGirtStart_MC, fFrontGirtStart, fFrontGirtEnd, m_arrCrSc[(int)EMemberGroupNames.eFrontGirt], EMemberType_FS_Position.GirtFrontSide, fColumnsRotation, iNumberOfTransverseSupports_FrontGirts);
            }

            // Back Girts
            // Nodes - Back Girts

            i_temp_numberofNodes += bGenerateFrontGirts ? iFrontIntermediateColumnNodesForGirtsOneFrameNo : 0;
            float fIntermediateSupportSpacingGirtsBackSide = fDist_BackColumns / (iNumberOfTransverseSupports_BackGirts + 1); // number of LTB segments = number of support + 1

            if (bGenerateBackGirts)
            {
                AddFrontOrBackGirtsNodes(iOneRafterBackColumnNo, iArrNumberOfNodesPerBackColumn, i_temp_numberofNodes, iBackIntermediateColumnNodesForGirtsOneRafterNo, fDist_BackGirts, fDist_BackColumns, fL_tot);
            }

            // Back Girts
            // Members - Back Girts

            i_temp_numberofMembers += bGenerateFrontGirts ? iFrontGirtsNoInOneFrame : 0;
            if (bGenerateBackGirts)
            {
                AddFrontOrBackGirtsMembers(iFrameNodesNo, iOneRafterBackColumnNo, iArrNumberOfNodesPerBackColumn, i_temp_numberofNodes, i_temp_numberofMembers, iBackIntermediateColumnNodesForGirtsOneRafterNo, iBackIntermediateColumnNodesForGirtsOneFrameNo, iGirtNoInOneFrame * (iFrameNo - 1), fDist_Girt, eccentricityGirtBack_YL, fBackGirtStart_MC, fBackGirtStart, fBackGirtEnd, m_arrCrSc[(int)EMemberGroupNames.eBackGirt], EMemberType_FS_Position.GirtBackSide, fColumnsRotation, iNumberOfTransverseSupports_BackGirts);
            }

            // Girt Bracing - Side walls
            // Nodes - Girt Bracing - Side walls

            i_temp_numberofNodes += bGenerateBackGirts ? iBackIntermediateColumnNodesForGirtsOneFrameNo : 0;
            if (bGenerateGirtBracingSideWalls)
            {
                for (int i = 0; i < (iFrameNo-1); i++)
                {
                    for (int j = 0; j < (iOneColumnGirtNo + 1); j++) // Left side
                    {
                        float zCoord = j < iOneColumnGirtNo ? (fBottomGirtPosition + j * fDist_Girt) : fH1_frame;

                        for (int k = 0; k < iNumberOfTransverseSupports_Girts ; k++)
                        {
                            m_arrNodes[i_temp_numberofNodes + i * iNumberOfGBSideWallsNodesInOneBay + j * iNumberOfTransverseSupports_Girts + k] = new CNode(i_temp_numberofNodes + i * iNumberOfGBSideWallsNodesInOneBay + j * iNumberOfTransverseSupports_Girts + k + 1, 000000, i * fL1_frame + (k + 1) * fIntermediateSupportSpacingGirts, zCoord, 0);
                        }
                    }

                    for (int j = 0; j < (iOneColumnGirtNo + 1); j++) // Right side
                    {
                        float zCoord = j < iOneColumnGirtNo ? (fBottomGirtPosition + j * fDist_Girt) : fH1_frame;

                        for (int k = 0; k < iNumberOfTransverseSupports_Girts; k++)
                        {
                            m_arrNodes[i_temp_numberofNodes + i * iNumberOfGBSideWallsNodesInOneBay + iNumberOfGBSideWallsNodesInOneBayOneSide + j * iNumberOfTransverseSupports_Girts + k] = new CNode(i_temp_numberofNodes + i * iNumberOfGBSideWallsNodesInOneBay + iNumberOfGBSideWallsNodesInOneBayOneSide + j * iNumberOfTransverseSupports_Girts + k + 1, fW_frame, i * fL1_frame + (k + 1) * fIntermediateSupportSpacingGirts, zCoord, 0);
                        }
                    }
                }
            }

            // Members - Girt Bracing - Side walls

            i_temp_numberofMembers += bGenerateBackGirts ? iBackGirtsNoInOneFrame : 0;
            if (bGenerateGirtBracingSideWalls)
            {
                for (int i = 0; i < (iFrameNo - 1); i++)
                {
                    for (int j = 0; j < iOneColumnGirtNo; j++) // Left side
                    {
                        bool bDeactivateMember = false;
                        if (bUseGBEverySecond && j % 2 == 1) bDeactivateMember = true;

                        float fGBSideWallEnd_Current = fGBSideWallEnd;

                        if (j == iOneColumnGirtNo - 1) // Last
                            fGBSideWallEnd_Current = (float)m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin].z_min + feccentricityEavePurlin_z - fCutOffOneSide;

                        for (int k = 0; k < iNumberOfTransverseSupports_Girts; k++)
                        {
                            int memberIndex = i_temp_numberofMembers + i * iNumberOfGBSideWallsMembersInOneBay + j * iNumberOfTransverseSupports_Girts + k;
                            int startNodeIndex = i_temp_numberofNodes + i * iNumberOfGBSideWallsNodesInOneBay + j * iNumberOfTransverseSupports_Girts + k;
                            int endNodeIndex = i_temp_numberofNodes + i * iNumberOfGBSideWallsNodesInOneBay + (j + 1) * iNumberOfTransverseSupports_Girts + k;
                            m_arrMembers[memberIndex] = new CMember(memberIndex + 1, m_arrNodes[startNodeIndex], m_arrNodes[endNodeIndex], m_arrCrSc[(int)EMemberGroupNames.eGirtBracing], EMemberType_FS.eGB, EMemberType_FS_Position.BracingBlockGirts, eccentricityGirtLeft_X0, eccentricityGirtLeft_X0, fGBSideWallStart, fGBSideWallEnd_Current, MathF.fPI, 0);

                            if (bDeactivateMember) DeactivateMemberAndItsJoints(ref m_arrMembers[memberIndex]);
                        }
                    }

                    for (int j = 0; j < iOneColumnGirtNo; j++) // Right side
                    {
                        bool bDeactivateMember = false;
                        if (bUseGBEverySecond && j % 2 == 1) bDeactivateMember = true;

                        float fGBSideWallEnd_Current = fGBSideWallEnd;

                        if (j == iOneColumnGirtNo - 1) // Last
                            fGBSideWallEnd_Current = (float)m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin].z_min + feccentricityEavePurlin_z - fCutOffOneSide;

                        for (int k = 0; k < iNumberOfTransverseSupports_Girts; k++)
                        {
                            int memberIndex = i_temp_numberofMembers + i * iNumberOfGBSideWallsMembersInOneBay + iNumberOfGBSideWallsMembersInOneBayOneSide + j * iNumberOfTransverseSupports_Girts + k;
                            int startNodeIndex = i_temp_numberofNodes + i * iNumberOfGBSideWallsNodesInOneBay + iNumberOfGBSideWallsNodesInOneBayOneSide + j * iNumberOfTransverseSupports_Girts + k;
                            int endNodeIndex = i_temp_numberofNodes + i * iNumberOfGBSideWallsNodesInOneBay + +iNumberOfGBSideWallsNodesInOneBayOneSide + (j + 1) * iNumberOfTransverseSupports_Girts + k;
                            m_arrMembers[memberIndex] = new CMember(memberIndex + 1, m_arrNodes[startNodeIndex], m_arrNodes[endNodeIndex], m_arrCrSc[(int)EMemberGroupNames.eGirtBracing], EMemberType_FS.eGB, EMemberType_FS_Position.BracingBlockGirts, eccentricityGirtRight_XB, eccentricityGirtRight_XB, fGBSideWallStart, fGBSideWallEnd_Current, MathF.fPI, 0);

                            if(bDeactivateMember) DeactivateMemberAndItsJoints(ref m_arrMembers[memberIndex]);
                        }
                    }
                }
            }

            // Purlin Bracing
            // Nodes - Purlin Bracing

            i_temp_numberofNodes += bGenerateGirtBracingSideWalls ? iGBSideWallsNodesNo : 0;
            if (bGeneratePurlinBracing)
            {
                for (int i = 0; i < (iFrameNo - 1); i++)
                {
                    for (int j = 0; j < (iOneRafterPurlinNo + 1); j++) // Left side - eave purlin and purlins
                    {
                        float x_glob, z_glob;

                        if (j == 0) // First row of nodes
                        { x_glob = 0; z_glob = fH1_frame; } // Left edge of roof
                        else
                            CalcPurlinNodeCoord(fFirstPurlinPosition + (j - 1) * fDist_Purlin, out x_glob, out z_glob);

                        for (int k = 0; k < iNumberOfTransverseSupports_Purlins; k++)
                        {
                            m_arrNodes[i_temp_numberofNodes + i * iNumberOfPBNodesInOneBay + j * iNumberOfTransverseSupports_Purlins + k] = new CNode(i_temp_numberofNodes + i * iNumberOfPBNodesInOneBay + j * iNumberOfTransverseSupports_Purlins + k + 1, x_glob, i * fL1_frame + (k + 1) * fIntermediateSupportSpacingPurlins, z_glob, 0);
                        }
                    }

                    for (int j = 0; j < (iOneRafterPurlinNo + 1); j++) // Right side - eave purlin and purlins
                    {
                        float x_glob, z_glob;

                        if (j == 0) // First row nodes
                        { x_glob = 0; z_glob = fH1_frame; } // Right edge of roof (x uvazujeme zprava)
                        else
                            CalcPurlinNodeCoord(fFirstPurlinPosition + (j - 1) * fDist_Purlin, out x_glob, out z_glob);

                        for (int k = 0; k < iNumberOfTransverseSupports_Purlins; k++)
                        {
                            m_arrNodes[i_temp_numberofNodes + i * iNumberOfPBNodesInOneBay + iNumberOfPBNodesInOneBayOneSide + j * iNumberOfTransverseSupports_Purlins + k] = new CNode(i_temp_numberofNodes + i * iNumberOfPBNodesInOneBay + iNumberOfPBNodesInOneBayOneSide + j * iNumberOfTransverseSupports_Purlins + k + 1, fW_frame - x_glob, i * fL1_frame + (k + 1) * fIntermediateSupportSpacingPurlins, z_glob, 0);
                        }
                    }
                }
            }

            // Members - Purlin Bracing

            i_temp_numberofMembers += bGenerateGirtBracingSideWalls ? iGBSideWallsMembersNo : 0;

            if (bGeneratePurlinBracing)
            {
                for (int i = 0; i < (iFrameNo - 1); i++)
                {
                    for (int j = 0; j < iOneRafterPurlinNo; j++) // Left side
                    {
                        bool bDeactivateMember = false;
                        if (bUsePBEverySecond && j % 2 == 1) bDeactivateMember = true;

                        float fPBStart = (float)m_arrCrSc[(int)EMemberGroupNames.ePurlin].y_min - fCutOffOneSide;
                        float fPBEnd = -(float)m_arrCrSc[(int)EMemberGroupNames.ePurlin].y_max - fCutOffOneSide;

                        float fPBStart_Current = fPBStart;

                        if (j == 0) // First
                            fPBStart_Current = (-(float)m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin].y_max - eccentricityEavePurlin.MFy_local) / (float)Math.Cos(fRoofPitch_rad) - (float)m_arrCrSc[(int)EMemberGroupNames.ePurlin].z_max * (float)Math.Tan(fRoofPitch_rad) - fCutOffOneSide;

                        for (int k = 0; k < iNumberOfTransverseSupports_Purlins; k++)
                        {
                            int memberIndex = i_temp_numberofMembers + i * iNumberOfPBMembersInOneBay + j * iNumberOfTransverseSupports_Purlins + k;
                            int startNodeIndex = i_temp_numberofNodes + i * iNumberOfPBNodesInOneBay + j * iNumberOfTransverseSupports_Purlins + k;
                            int endNodeIndex = i_temp_numberofNodes + i * iNumberOfPBNodesInOneBay + (j + 1) * iNumberOfTransverseSupports_Purlins + k;
                            m_arrMembers[memberIndex] = new CMember(memberIndex + 1, m_arrNodes[startNodeIndex], m_arrNodes[endNodeIndex], m_arrCrSc[(int)EMemberGroupNames.ePurlinBracing], EMemberType_FS.ePB, EMemberType_FS_Position.BracingBlockPurlins, eccentricityPurlin, eccentricityPurlin, fPBStart_Current, fPBEnd, 0, 0);

                            if (bDeactivateMember) DeactivateMemberAndItsJoints(ref m_arrMembers[memberIndex]);
                        }
                    }

                    for (int j = 0; j < iOneRafterPurlinNo; j++) // Right side
                    {
                        bool bDeactivateMember = false;
                        if (bUsePBEverySecond && j % 2 == 1) bDeactivateMember = true;

                        // Opacna orientacia osi LCS y na pravej strane
                        float fPBStart = -(float)m_arrCrSc[(int)EMemberGroupNames.ePurlin].y_max - fCutOffOneSide;
                        float fPBEnd = (float)m_arrCrSc[(int)EMemberGroupNames.ePurlin].y_min - fCutOffOneSide;

                        float fPBStart_Current = fPBStart;

                        if (j == 0) // First
                            fPBStart_Current = (-(float)m_arrCrSc[(int)EMemberGroupNames.eEavesPurlin].y_max - eccentricityEavePurlin.MFy_local) / (float)Math.Cos(fRoofPitch_rad) - (float)m_arrCrSc[(int)EMemberGroupNames.ePurlin].z_max * (float)Math.Tan(fRoofPitch_rad) - fCutOffOneSide;

                        for (int k = 0; k < iNumberOfTransverseSupports_Purlins; k++)
                        {
                            int memberIndex = i_temp_numberofMembers + i * iNumberOfPBMembersInOneBay + iNumberOfPBMembersInOneBayOneSide + j * iNumberOfTransverseSupports_Purlins + k;
                            int startNodeIndex = i_temp_numberofNodes + i * iNumberOfPBNodesInOneBay + iNumberOfPBNodesInOneBayOneSide + j * iNumberOfTransverseSupports_Purlins + k;
                            int endNodeIndex = i_temp_numberofNodes + i * iNumberOfPBNodesInOneBay + +iNumberOfPBNodesInOneBayOneSide + (j + 1) * iNumberOfTransverseSupports_Purlins + k;
                            m_arrMembers[memberIndex] = new CMember(memberIndex + 1, m_arrNodes[startNodeIndex], m_arrNodes[endNodeIndex], m_arrCrSc[(int)EMemberGroupNames.ePurlinBracing], EMemberType_FS.ePB, EMemberType_FS_Position.BracingBlockPurlins, eccentricityPurlin, eccentricityPurlin, fPBStart_Current, fPBEnd, MathF.fPI, 0);

                            if (bDeactivateMember) DeactivateMemberAndItsJoints(ref m_arrMembers[memberIndex]);
                        }
                    }
                }
            }

            // Girt Bracing - Front side
            // Nodes - Girt Bracing - Front side

            //TO Mato - to co to tu je?  bGeneratePurlinBracing??? asi skor bGenerateGirtBracingFrontSide nie?
            // To Ondrej - funguje to tak, ze sa tu nastavi aktualny pocet existujucich uzlov a to tak, ze sa pripocita pocet, ktory vznikol v predchadzajucom if
            // Mas pravdu, ze by sa to asi malo pripocitat uz v tom predchadzajucom if a tu by potom netrebalo kontrolovat ci je true a ci sa ma nieco pripocitat alebo nic - 0

            i_temp_numberofNodes += bGeneratePurlinBracing ? iPBNodesNo : 0;
            int iNumberOfGB_FSNodesInOneSideAndMiddleBay = 0;

            if (bGenerateGirtBracingFrontSide)
            {
                AddFrontOrBackGirtsBracingBlocksNodes(i_temp_numberofNodes, iArrGB_FS_NumberOfNodesPerBay, iArrGB_FS_NumberOfNodesPerBayFirstNode,
                iNumberOfTransverseSupports_FrontGirts, fIntermediateSupportSpacingGirtsFrontSide, fDist_FrontGirts, fDist_FrontColumns, 0, out iNumberOfGB_FSNodesInOneSideAndMiddleBay);
            }

            // Members - Girt Bracing - Front side
            i_temp_numberofMembers += bGeneratePurlinBracing ? iPBMembersNo : 0;
            if (bGenerateGirtBracingFrontSide)
            {
               float fGBFrontSideEndToRafter = (float)m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].z_min / (float)Math.Cos(fRoofPitch_rad) - (float)m_arrCrSc[(int)EMemberGroupNames.eFrontGirtBracing].y_max * (float)Math.Tan(fRoofPitch_rad) - fCutOffOneSide;

               AddFrontOrBackGirtsBracingBlocksMembers(i_temp_numberofNodes, i_temp_numberofMembers, iArrGB_FS_NumberOfNodesPerBay, iArrGB_FS_NumberOfNodesPerBayFirstNode, iArrGB_FS_NumberOfMembersPerBay,
               iNumberOfGB_FSNodesInOneSideAndMiddleBay, iNumberOfTransverseSupports_FrontGirts, eccentricityGirtFront_Y0, fGBFrontSideStart, fGBFrontSideEnd, fGBFrontSideEndToRafter, m_arrCrSc[(int)EMemberGroupNames.eFrontGirtBracing],
               EMemberType_FS_Position.BracingBlocksGirtsFrontSide, fColumnsRotation, bUseGBEverySecond);
            }

            // Girt Bracing - Back side
            // Nodes - Girt Bracing - Back side
            i_temp_numberofNodes += bGenerateGirtBracingFrontSide ? iNumberOfGB_FSNodesInOneFrame : 0;
            int iNumberOfGB_BSNodesInOneSideAndMiddleBay = 0;

            if (bGenerateGirtBracingBackSide)
            {
                AddFrontOrBackGirtsBracingBlocksNodes(i_temp_numberofNodes, iArrGB_BS_NumberOfNodesPerBay, iArrGB_BS_NumberOfNodesPerBayFirstNode,
                iNumberOfTransverseSupports_BackGirts, fIntermediateSupportSpacingGirtsBackSide, fDist_BackGirts, fDist_BackColumns, fL_tot, out iNumberOfGB_BSNodesInOneSideAndMiddleBay);
            }

            // Members - Girt Bracing - Back side
            i_temp_numberofMembers += bGenerateGirtBracingFrontSide ? iNumberOfGB_FSMembersInOneFrame : 0;
            if (bGenerateGirtBracingBackSide)
            {
                float fGBBackSideEndToRafter = (float)m_arrCrSc[(int)EMemberGroupNames.eRafter_EF].z_min / (float)Math.Cos(fRoofPitch_rad) - (float)m_arrCrSc[(int)EMemberGroupNames.eBackGirtBracing].y_max * (float)Math.Tan(fRoofPitch_rad) - fCutOffOneSide;

                AddFrontOrBackGirtsBracingBlocksMembers(i_temp_numberofNodes, i_temp_numberofMembers, iArrGB_BS_NumberOfNodesPerBay, iArrGB_BS_NumberOfNodesPerBayFirstNode, iArrGB_BS_NumberOfMembersPerBay,
                iNumberOfGB_BSNodesInOneSideAndMiddleBay, iNumberOfTransverseSupports_BackGirts, eccentricityGirtBack_YL, fGBBackSideStart, fGBBackSideEnd, fGBBackSideEndToRafter, m_arrCrSc[(int)EMemberGroupNames.eBackGirtBracing],
                EMemberType_FS_Position.BracingBlocksGirtsBackSide, fColumnsRotation, bUseGBEverySecond);
            }

            // Validacia generovanych prutov a uzlov (overime ci vsetky generovane nodes a members maju ID o 1 vacsie ako je index v poli
            bool bValidateIDs = false;

            if (bValidateIDs)
            {
                for (int i = 0; i < m_arrNodes.Length; i++)
                {
                    if (m_arrNodes[i].ID != i + 1)
                        throw new Exception("Invalid ID - Node Index:" + i.ToString() + " Node ID: " + m_arrNodes[i].ID);
                }

                for (int i = 0; i < m_arrMembers.Length; i++)
                {
                    if (m_arrMembers[i].ID != i + 1)
                        throw new Exception("Invalid ID - Member Index:" + i.ToString() + " Member ID: " + m_arrMembers[i].ID);
                }
            }

            FillIntermediateNodesForMembers();

            #region Joints
            if (joints == null)
            {
                CreateJoints(bGenerateGirts, bUseMainColumnFlyBracingPlates, bGeneratePurlins, bUseRafterFlyBracingPlates, bGenerateFrontColumns, bGenerateBackColumns, bGenerateFrontGirts,
                             bGenerateBackGirts, bGenerateGirtBracingSideWalls, bGeneratePurlinBracing, bGenerateGirtBracingFrontSide, bGenerateGirtBracingBackSide, vm._generalOptionsVM.WindPostUnderRafter);
            }
            else m_arrConnectionJoints = joints;
            #endregion

            CountPlates_ValidationPurpose();

            #region Blocks

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Blocks
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            DoorsModels = new List<CBlock_3D_001_DoorInBay>();
            WindowsModels = new List<CBlock_3D_002_WindowInBay>();
            vm.SetModelBays(iFrameNo);
            bool isChangedFromCode = vm.IsSetFromCode;

            if (doorBlocksProperties != null)
            {
                foreach (DoorProperties dp in doorBlocksProperties.ToList())
                {
                    if (!bGenerateGirts && (dp.sBuildingSide == "Right" || dp.sBuildingSide == "Left")) { if (!isChangedFromCode) vm.IsSetFromCode = true; doorBlocksProperties.Remove(dp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }
                    else if (!bGenerateFrontGirts && dp.sBuildingSide == "Front") { if (!isChangedFromCode) vm.IsSetFromCode = true; doorBlocksProperties.Remove(dp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }
                    else if (!bGenerateBackGirts && dp.sBuildingSide == "Back") { if (!isChangedFromCode) vm.IsSetFromCode = true; doorBlocksProperties.Remove(dp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }

                    if (!dp.ValidateBays()) { if (!isChangedFromCode) vm.IsSetFromCode = true; doorBlocksProperties.Remove(dp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }

                    if (!dp.Validate()) { if (!isChangedFromCode) vm.IsSetFromCode = true; doorBlocksProperties.Remove(dp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }
                    else if (dp.Validate()) // Ak su vlastnosti dveri validne vyrobime blok dveri a nastavime rebates pre floor slab
                    {
                        AddDoorBlock(dp, 0.5f, fH1_frame);

                        // TODO - Ondrej - potrebujem vm.FootingVM.RebateWidth_LRSide a vm.FootingVM.RebateWidth_FBSide
                        // Ale som trosku zacykleny lebo tento model sa vyraba skor nez VM existuje a zase rebate width sa naplna v CSlab, ktora sa vytvara az po vytvoreni bloku dveri
                        // Prosim pomoz mi to nejako usporiadat :)
                        // Mozno by bolo spravnejsie keby sa Rebate width nastavovala v UC_Doors pre Roller Door a tym padom by 
                        //v UC_Footing - Floor uz boli len vlastnosti saw cut, control joints a perimeters
                        // Potom by som vsetko co sa tyka rebates bral z doorBlocksProperties

                        if (dp.sBuildingSide == "Right" || dp.sBuildingSide == "Left")
                            dp.SetRebateProperties((float)DoorsModels.Last().m_arrCrSc[1].b, 0.5f /*vm.FootingVM.RebateWidth_LRSide*/,
                             fL1_frame, fDist_FrontColumns, fDist_BackColumns); // Vlastnosti rebate pre LR Side
                        else
                            dp.SetRebateProperties((float)DoorsModels.Last().m_arrCrSc[1].b, 0.4f /*vm.FootingVM.RebateWidth_FBSide*/,
                            fL1_frame, fDist_FrontColumns, fDist_BackColumns); // Vlastnosti Rebate pre FB Side
                    }
                }

                //refaktoring 24.1.2020
                //for (int i = 0; i < doorBlocksProperties.Count; i++)
                //{
                //    if (!bGenerateGirts && (doorBlocksProperties[i].sBuildingSide == "Right" || doorBlocksProperties[i].sBuildingSide == "Left")) continue;
                //    else if (!bGenerateFrontGirts && doorBlocksProperties[i].sBuildingSide == "Front") continue;
                //    else if (!bGenerateBackGirts && doorBlocksProperties[i].sBuildingSide == "Back") continue;

                //    if (!doorBlocksProperties[i].ValidateBays()) continue;

                //    if (doorBlocksProperties[i].Validate()) // Ak su vlastnosti dveri validne vyrobime blok dveri a nastavime rebates pre floor slab
                //    {
                //        AddDoorBlock(doorBlocksProperties[i], 0.5f, fH1_frame);

                //        // TODO - Ondrej - potrebujem vm.FootingVM.RebateWidth_LRSide a vm.FootingVM.RebateWidth_FBSide
                //        // Ale som trosku zacykleny lebo tento model sa vyraba skor nez VM existuje a zase rebate width sa naplna v CSlab, ktora sa vytvara az po vytvoreni bloku dveri
                //        // Prosim pomoz mi to nejako usporiadat :)
                //        // Mozno by bolo spravnejsie keby sa Rebate width nastavovala v UC_Doors pre Roller Door a tym padom by 
                //        //v UC_Footing - Floor uz boli len vlastnosti saw cut, control joints a perimeters
                //        // Potom by som vsetko co sa tyka rebates bral z doorBlocksProperties

                //        if (doorBlocksProperties[i].sBuildingSide == "Right" || doorBlocksProperties[i].sBuildingSide == "Left")
                //            doorBlocksProperties[i].SetRebateProperties((float)DoorsModels.Last().m_arrCrSc[1].b, 0.5f /*vm.FootingVM.RebateWidth_LRSide*/,
                //             fL1_frame, fDist_FrontColumns, fDist_BackColumns); // Vlastnosti rebate pre LR Side
                //        else
                //            doorBlocksProperties[i].SetRebateProperties((float)DoorsModels.Last().m_arrCrSc[1].b, 0.4f /*vm.FootingVM.RebateWidth_FBSide*/,
                //            fL1_frame, fDist_FrontColumns, fDist_BackColumns); // Vlastnosti Rebate pre FB Side
                //    }
                //}
            }

            if (windowBlocksProperties != null)
            {
                foreach (WindowProperties wp in windowBlocksProperties.ToList())
                {
                    if (!bGenerateGirts && (wp.sBuildingSide == "Right" || wp.sBuildingSide == "Left")) { if (!isChangedFromCode) vm.IsSetFromCode = true; windowBlocksProperties.Remove(wp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }
                    else if (!bGenerateFrontGirts && wp.sBuildingSide == "Front") { if (!isChangedFromCode) vm.IsSetFromCode = true; windowBlocksProperties.Remove(wp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }
                    else if (!bGenerateBackGirts && wp.sBuildingSide == "Back") { if (!isChangedFromCode) vm.IsSetFromCode = true; windowBlocksProperties.Remove(wp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }

                    if (!wp.ValidateBays()) { if (!isChangedFromCode) vm.IsSetFromCode = true; windowBlocksProperties.Remove(wp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }

                    if(!wp.Validate()) { if (!isChangedFromCode) vm.IsSetFromCode = true; windowBlocksProperties.Remove(wp); if (!isChangedFromCode) vm.IsSetFromCode = false; continue; }
                    else if (wp.Validate()) AddWindowBlock(wp, 0.5f);
                }
                //refaktoring 24.1.2020
                //for (int i = 0; i < windowBlocksProperties.Count; i++)
                //{
                //    if (!bGenerateGirts && (windowBlocksProperties[i].sBuildingSide == "Right" || windowBlocksProperties[i].sBuildingSide == "Left")) continue;
                //    else if (!bGenerateFrontGirts && windowBlocksProperties[i].sBuildingSide == "Front") continue;
                //    else if (!bGenerateBackGirts && windowBlocksProperties[i].sBuildingSide == "Back") continue;

                //    if (!windowBlocksProperties[i].ValidateBays()) continue;

                //    if (windowBlocksProperties[i].Validate()) AddWindowBlock(windowBlocksProperties[i], 0.5f);
                //}
            }

            CountPlates_ValidationPurpose();

            // Validation - check that all created joints have assigned Main Member
            // Check all joints after definition of doors and windows members and joints
            for (int i = 0; i < m_arrConnectionJoints.Count; i++)
            {
                if (m_arrConnectionJoints[i].m_MainMember == null)
                {
                    //throw new ArgumentNullException("Main member is not assigned to the joint No.:" + m_arrConnectionJoints[i].ID.ToString() + " Joint index in the list: " + i);

                    // TODO BUG 46 - TO Ondrej // Odstranenie spojov ktore patria k deaktivovanym prutom (pruty boli deaktivovane, pretoze sa nachadazju na miest vlozeneho bloku)
                    // Odstranenie by malo nastavat uz vo funckii ktora generuje bloky okien a dveri

                    // Toto je docasne riesenie - vymazeme spoj zo zoznamu
                    m_arrConnectionJoints.RemoveAt(i); // Remove joint from the list
                }
                //BUG 327
                if (m_arrConnectionJoints[i].m_SecondaryMembers != null)
                {
                    foreach (CMember secMem in m_arrConnectionJoints[i].m_SecondaryMembers)
                    {
                        if (secMem.BIsGenerated == false)
                        {
                            CConnectionJointTypes joint = m_arrConnectionJoints[i];
                            DeactivateJoint(ref joint);
                        }
                    }
                }
            }

            // Opakovana kontrola po odstraneni spojov s MainMember = null
            int iCountOfJoints_NotGenerated = 0; // Number of joints on deactivated members (girts where dorr and window blocks are inserted) // Mozno sa to na nieco pouzije :)
            for (int i = 0; i < m_arrConnectionJoints.Count; i++)
            {
                if (m_arrConnectionJoints[i].m_MainMember == null)
                    throw new ArgumentNullException("Main member is not assigned to the joint No.:" + m_arrConnectionJoints[i].ID.ToString() + " Joint index in the list: " + i);

                if(m_arrConnectionJoints[i].BIsGenerated == false)
                {
                    iCountOfJoints_NotGenerated++;
                }
            }

            // Validation - duplicity of node ID
            for (int i = 0; i < m_arrNodes.Length; i++)
            {
                for (int j = 0; j < m_arrNodes.Length; j++)
                {
                    if ((m_arrNodes[i] != m_arrNodes[j]) && (m_arrNodes[i].ID == m_arrNodes[j].ID))
                        throw new ArgumentNullException("Duplicity in Node ID.\nNode index: " + i + " and Node index: " + j);
                }
            }

            //------------------------------------------------------------
            // Vid TODO 234 - docasne priradenie vlastnosti materialu
            // Pre objekty dveri je potrebne pridat prierezy do Component List - Tab Members a nacitat ich parametre, potom sa moze nacitanie z databazy zmazat
            // Po zapracovani TODO 234 mozno tento kod zmazat
            foreach (CMember member in m_arrMembers)
            {
                if (member.CrScStart.m_Mat is CMat_03_00)
                    DATABASE.CMaterialManager.LoadSteelMaterialProperties((CMat_03_00)member.CrScStart.m_Mat, member.CrScStart.m_Mat.Name);
            }
            //------------------------------------------------------------

            CountPlates_ValidationPurpose();

            // End of blocks
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            AddMembersToMemberGroupsLists();


            vm.SetComponentListAccordingToDoorsAndWindows();

            // Set members Generate, Display, Calculate, Design, MaterialList properties
            CModelHelper.SetMembersAccordingTo(m_arrMembers, componentList);

            #region Supports

            //m_arrNSupports = new CNSupport[2 * iFrameNo];

            // Nodal Supports - fill values

            // Set values
            bool[] bSupport1 = { true, true, true, false, vm.SupportTypeIndex == 0 ? true : false, false }; // Main and Edge Column (fixed / released rotation about Y axis)
            bool[] bSupport2 = { true, true, true, false, false, false }; // Wind Post

            m_arrNSupports = new CNSupport[listOfSupportedNodes_S1.Count + listOfSupportedNodes_S2.Count];

            for (int i = 0; i < m_arrNSupports.Length; i++)
            {
                if(i < listOfSupportedNodes_S1.Count)
                    m_arrNSupports[i] = new CNSupport(6, i + 1, listOfSupportedNodes_S1[i], bSupport1, 0);
                else
                    m_arrNSupports[i] = new CNSupport(6, i + 1, listOfSupportedNodes_S2[i - listOfSupportedNodes_S1.Count], bSupport2, 0);
            }

            // Setridit pole podle ID
            Array.Sort(m_arrNSupports, new CCompare_NSupportID());
            #endregion

            #region Member Releases
            // Member Releases / hinges - fill values

            // Set values
            bool?[] bMembRelase1 = { false, false, false, false, true, false };

            // Create Release / Hinge Objects
            //m_arrMembers[02].CnRelease1 = new CNRelease(6, m_arrMembers[02].NodeStart, bMembRelase1, 0);
            #endregion

            #region Foundations

            if (foundations == null)
            {
                CreateFoundations(bGenerateFrontColumns, bGenerateBackColumns, vm._generalOptionsVM.UseStraightReinforcementBars);
            }
            else
                m_arrFoundations = foundations;
            #endregion

            #region Floor slab, saw cuts and control joints

            if (slabs == null)
            {
                CreateFloorSlab(bGenerateFrontColumns, bGenerateBackColumns, bGenerateFrontGirts, bGenerateBackGirts, vm._generalOptionsVM.WindPostUnderRafter);
            }
            else
                m_arrSlabs = slabs;
            #endregion

            SetJointDefaultParameters();
        }
        
        public override void CalculateLoadValuesAndGenerateLoads(
                CCalcul_1170_1 generalLoad,
                CCalcul_1170_2 wind,
                CCalcul_1170_3 snow,
                CCalcul_1170_5 eq,
                bool bGenerateNodalLoads,
                bool bGenerateLoadsOnGirts,
                bool bGenerateLoadsOnPurlins,
                bool bGenerateLoadsOnColumns,
                bool bGenerateLoadsOnFrameMembers,
                bool bGenerateSurfaceLoads)
        {
            // Loading
            #region Load Cases
            // Load Cases
            CLoadCaseGenerator loadCaseGenerator = new CLoadCaseGenerator();
            m_arrLoadCases = loadCaseGenerator.GenerateLoadCases();
            #endregion

            // Snow load factor - projection on roof
            // Faktor ktory prepocita zatazenie z podorysneho rozmeru premietnute na stresnu rovinu
            fSlopeFactor = ((0.5f * fW_frame) / ((0.5f * fW_frame) / (float)Math.Cos(fRoofPitch_rad))); // Consider projection acc. to Figure 4.1

            #region Surface Loads
            // Surface Loads

            if (bGenerateSurfaceLoads)
            {
                CSurfaceLoadGenerator surfaceLoadGenerator = new CSurfaceLoadGenerator(fH1_frame, fH2_frame, fW_frame, fL_tot, fRoofPitch_rad,
                    fDist_Purlin, fDist_Girt, fDist_FrontGirts, fDist_BackGirts, fDist_FrontColumns, fDist_BackColumns,
                    fSlopeFactor, m_arrLoadCases, generalLoad, wind, snow);
                surfaceLoadGenerator.GenerateSurfaceLoads();
            }

            #endregion

            #region Earthquake - nodal loads
            // Earthquake

            if (bGenerateNodalLoads)
            {
                int iNumberOfLoadsInXDirection = iFrameNo;
                int iNumberOfLoadsInYDirection = 2;

                CNodalLoadGenerator nodalLoadGenerator = new CNodalLoadGenerator(iNumberOfLoadsInXDirection, iNumberOfLoadsInYDirection, m_arrLoadCases, m_arrNodes,/* fL1_frame,*/ eq);
                nodalLoadGenerator.GenerateNodalLoads();
            }
            #endregion

            #region Member Loads
            if (bGenerateLoadsOnGirts || bGenerateLoadsOnPurlins || bGenerateLoadsOnColumns || bGenerateLoadsOnFrameMembers)
            {
                CMemberLoadGenerator loadGenerator =
                new CMemberLoadGenerator(iFrameNo,
                fL1_frame,
                fL_tot,
                fSlopeFactor,
                m_arrCrSc[(int)EMemberGroupNames.eGirtWall],
                m_arrCrSc[(int)EMemberGroupNames.ePurlin],
                fDist_Girt,
                fDist_Purlin,
                m_arrCrSc[(int)EMemberGroupNames.eMainColumn],
                m_arrCrSc[(int)EMemberGroupNames.eRafter],
                m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF],
                m_arrCrSc[(int)EMemberGroupNames.eRafter_EF],
                m_arrLoadCases,
                m_arrMembers,
                generalLoad,
                snow,
                wind);

                #region Secondary Member Loads (girts, purlins, wind posts, door trimmers)
                // Purlins, eave purlins, girts, ....
                LoadCasesMemberLoads memberLoadsOnPurlinsGirtsColumns = new LoadCasesMemberLoads();
                // Generate single member loads
                if (bGenerateLoadsOnGirts || bGenerateLoadsOnPurlins || bGenerateLoadsOnColumns)
                {
                    memberLoadsOnPurlinsGirtsColumns = loadGenerator.GetGeneratedMemberLoads(m_arrLoadCases, m_arrMembers);
                    loadGenerator.AssignMemberLoadListsToLoadCases(memberLoadsOnPurlinsGirtsColumns);
                }
                #endregion

                #region Frame Member Loads (main and edge columns and rafters)
                // Frame Member Loads
                LoadCasesMemberLoads memberLoadsOnFrames = new LoadCasesMemberLoads();
                if (bGenerateLoadsOnFrameMembers)
                {
                    memberLoadsOnFrames = loadGenerator.GetGenerateMemberLoadsOnFrames();
                    loadGenerator.AssignMemberLoadListsToLoadCases(memberLoadsOnFrames);
                }
                #endregion

                #region Merge Member Load Lists
                if ((bGenerateLoadsOnGirts || bGenerateLoadsOnPurlins || bGenerateLoadsOnColumns) && bGenerateLoadsOnFrameMembers)
                {
                    if (memberLoadsOnFrames.Count != memberLoadsOnPurlinsGirtsColumns.Count)
                    {
                        throw new Exception("Not all member load list in all load cases were generated for frames and single members.");
                    }

                    // Merge lists
                    memberLoadsOnFrames.Merge(memberLoadsOnPurlinsGirtsColumns); //Merge both to first LoadCasesMemberLoads
                    // Assign merged list of member loads to the load cases
                    loadGenerator.AssignMemberLoadListsToLoadCases(memberLoadsOnFrames);
                }
                #endregion
            }

            #endregion

            #region Load Groups
            // Create load groups and assigned load cases to the load group
            // Load Case Groups
            m_arrLoadCaseGroups = new CLoadCaseGroup[10];

            // Dead Load
            m_arrLoadCaseGroups[0] = new CLoadCaseGroup(1, "Dead load", ELCGTypeForLimitState.eUniversal, ELCGType.eTogether);
            m_arrLoadCaseGroups[0].MLoadCasesList.Add(m_arrLoadCases[00]);

            // Imposed Load
            m_arrLoadCaseGroups[1] = new CLoadCaseGroup(2, "Imposed load", ELCGTypeForLimitState.eUniversal, ELCGType.eExclusive);
            m_arrLoadCaseGroups[1].MLoadCasesList.Add(m_arrLoadCases[01]);

            // ULS Load Case Groups
            // Snow Load - only one item from group can be in combination
            m_arrLoadCaseGroups[2] = new CLoadCaseGroup(3, "Snow load", ELCGTypeForLimitState.eULSOnly, ELCGType.eExclusive);
            m_arrLoadCaseGroups[2].MLoadCasesList.Add(m_arrLoadCases[02]);
            m_arrLoadCaseGroups[2].MLoadCasesList.Add(m_arrLoadCases[03]);
            m_arrLoadCaseGroups[2].MLoadCasesList.Add(m_arrLoadCases[04]);

            // Wind Load - only one item from group can be in combination
            m_arrLoadCaseGroups[3] = new CLoadCaseGroup(4, "Wind load - Cpi", ELCGTypeForLimitState.eULSOnly, ELCGType.eExclusive);
            m_arrLoadCaseGroups[3].MLoadCasesList.Add(m_arrLoadCases[05]);
            m_arrLoadCaseGroups[3].MLoadCasesList.Add(m_arrLoadCases[06]);
            m_arrLoadCaseGroups[3].MLoadCasesList.Add(m_arrLoadCases[07]);
            m_arrLoadCaseGroups[3].MLoadCasesList.Add(m_arrLoadCases[08]);
            m_arrLoadCaseGroups[3].MLoadCasesList.Add(m_arrLoadCases[09]);
            m_arrLoadCaseGroups[3].MLoadCasesList.Add(m_arrLoadCases[10]);
            m_arrLoadCaseGroups[3].MLoadCasesList.Add(m_arrLoadCases[11]);
            m_arrLoadCaseGroups[3].MLoadCasesList.Add(m_arrLoadCases[12]);

            // Wind Load - only one item from group can be in combination
            m_arrLoadCaseGroups[4] = new CLoadCaseGroup(5, "Wind load - Cpe", ELCGTypeForLimitState.eULSOnly, ELCGType.eExclusive);
            m_arrLoadCaseGroups[4].MLoadCasesList.Add(m_arrLoadCases[13]);
            m_arrLoadCaseGroups[4].MLoadCasesList.Add(m_arrLoadCases[14]);
            m_arrLoadCaseGroups[4].MLoadCasesList.Add(m_arrLoadCases[15]);
            m_arrLoadCaseGroups[4].MLoadCasesList.Add(m_arrLoadCases[16]);
            m_arrLoadCaseGroups[4].MLoadCasesList.Add(m_arrLoadCases[17]);
            m_arrLoadCaseGroups[4].MLoadCasesList.Add(m_arrLoadCases[18]);
            m_arrLoadCaseGroups[4].MLoadCasesList.Add(m_arrLoadCases[19]);
            m_arrLoadCaseGroups[4].MLoadCasesList.Add(m_arrLoadCases[20]);

            // Earthquake Load
            m_arrLoadCaseGroups[5] = new CLoadCaseGroup(6, "Earthquake", ELCGTypeForLimitState.eULSOnly, ELCGType.eExclusive);
            m_arrLoadCaseGroups[5].MLoadCasesList.Add(m_arrLoadCases[21]);
            m_arrLoadCaseGroups[5].MLoadCasesList.Add(m_arrLoadCases[22]);

            // SLS Load Case Groups
            // Snow Load - only one item from group can be in combination
            m_arrLoadCaseGroups[6] = new CLoadCaseGroup(7, "Snow load", ELCGTypeForLimitState.eSLSOnly, ELCGType.eExclusive);
            m_arrLoadCaseGroups[6].MLoadCasesList.Add(m_arrLoadCases[23]);
            m_arrLoadCaseGroups[6].MLoadCasesList.Add(m_arrLoadCases[24]);
            m_arrLoadCaseGroups[6].MLoadCasesList.Add(m_arrLoadCases[25]);

            // Wind Load - only one item from group can be in combination
            m_arrLoadCaseGroups[7] = new CLoadCaseGroup(8, "Wind load - Cpi", ELCGTypeForLimitState.eSLSOnly, ELCGType.eExclusive);
            m_arrLoadCaseGroups[7].MLoadCasesList.Add(m_arrLoadCases[26]);
            m_arrLoadCaseGroups[7].MLoadCasesList.Add(m_arrLoadCases[27]);
            m_arrLoadCaseGroups[7].MLoadCasesList.Add(m_arrLoadCases[28]);
            m_arrLoadCaseGroups[7].MLoadCasesList.Add(m_arrLoadCases[29]);
            m_arrLoadCaseGroups[7].MLoadCasesList.Add(m_arrLoadCases[30]);
            m_arrLoadCaseGroups[7].MLoadCasesList.Add(m_arrLoadCases[31]);
            m_arrLoadCaseGroups[7].MLoadCasesList.Add(m_arrLoadCases[32]);
            m_arrLoadCaseGroups[7].MLoadCasesList.Add(m_arrLoadCases[33]);

            // Wind Load - only one item from group can be in combination
            m_arrLoadCaseGroups[8] = new CLoadCaseGroup(9, "Wind load - Cpe", ELCGTypeForLimitState.eSLSOnly, ELCGType.eExclusive);
            m_arrLoadCaseGroups[8].MLoadCasesList.Add(m_arrLoadCases[34]);
            m_arrLoadCaseGroups[8].MLoadCasesList.Add(m_arrLoadCases[35]);
            m_arrLoadCaseGroups[8].MLoadCasesList.Add(m_arrLoadCases[36]);
            m_arrLoadCaseGroups[8].MLoadCasesList.Add(m_arrLoadCases[37]);
            m_arrLoadCaseGroups[8].MLoadCasesList.Add(m_arrLoadCases[38]);
            m_arrLoadCaseGroups[8].MLoadCasesList.Add(m_arrLoadCases[39]);
            m_arrLoadCaseGroups[8].MLoadCasesList.Add(m_arrLoadCases[40]);
            m_arrLoadCaseGroups[8].MLoadCasesList.Add(m_arrLoadCases[41]);

            // Earthquake Load
            m_arrLoadCaseGroups[9] = new CLoadCaseGroup(10, "Earthquake", ELCGTypeForLimitState.eSLSOnly, ELCGType.eExclusive);
            m_arrLoadCaseGroups[9].MLoadCasesList.Add(m_arrLoadCases[42]);
            m_arrLoadCaseGroups[9].MLoadCasesList.Add(m_arrLoadCases[43]);

            #endregion

            #region Load Combinations
            // Load Combinations
            CLoadCombinationsGenerator generator = new CLoadCombinationsGenerator(m_arrLoadCaseGroups);
            generator.GenerateAll();            
            m_arrLoadCombs = generator.Combinations.ToArray();
            #endregion

            #region Limit states
            // Limit States
            m_arrLimitStates = new CLimitState[3];
            m_arrLimitStates[0] = new CLimitState("Ultimate Limit State - Stability", ELSType.eLS_ULS);
            m_arrLimitStates[1] = new CLimitState("Ultimate Limit State - Strength", ELSType.eLS_ULS);
            m_arrLimitStates[2] = new CLimitState("Serviceability Limit State", ELSType.eLS_SLS);
            #endregion
        }

        private void FillIntermediateNodesForMembers()
        {
            foreach (CMember m in this.m_arrMembers)
            {
                foreach (CNode n in this.m_arrNodes)
                {
                    if (m.IsIntermediateNode(n)) m.IntermediateNodes.Add(n);
                }
            }

            bool debugging = false;
            if (debugging)
            {
                foreach (CMember m in this.m_arrMembers)
                {
                    if (m.IntermediateNodes.Count > 0) System.Diagnostics.Trace.WriteLine($"ID:{m.ID} Name:{m.Name} IntNodesCount: {m.IntermediateNodes.Count}");
                }
            }
        }

        public void CalcPurlinNodeCoord(float x_rel, out float x_global, out float z_global)
        {
            x_global = (float)Math.Cos(fRoofPitch_rad) * x_rel;
            z_global = fH1_frame + (float)Math.Sin(fRoofPitch_rad) * x_rel;
        }

        public void CalcColumnNodeCoord_Z(float x, out float z_global)
        {
            if(x<= 0.5f * fW_frame)
               z_global = fH1_frame + (float)Math.Tan(fRoofPitch_rad) * x;
            else
               z_global = fH1_frame + (float)Math.Tan(fRoofPitch_rad) * (fW_frame - x);
        }

        // Rotate Node in Front or Back Frame about Z (angle between X and Front or Back Frame
        public void RotateFrontOrBackFrameNodeAboutZ(CNode node_temp)
        {
            // Rake Angles - Front and Back Frame
            // Upravi suradnice Y vsetkych uzlov s Y = 0 a s Y = L
            // Prepocita suradnicu podla hodnoty X a uhlu, ktory je nastaveny medzi globalnou osou X a prednym (prvym) alebo zadnym (poslednym) ramom
            // Tato uprava umoznuje zosikmenie prveho / posledneho ramu voci mezilahlym

            if (node_temp != null)
            {
                if (MathF.d_equal(node_temp.Y, 0) && !MathF.d_equal(fFrontFrameRakeAngle_temp_rad, 0)) // Front Frame
                {
                    node_temp.Y += node_temp.X * (float)Math.Tan(fFrontFrameRakeAngle_temp_rad);
                }

                if (MathF.d_equal(node_temp.Y, fL_tot) && !MathF.d_equal(fBackFrameRakeAngle_temp_rad, 0)) // Back Frame
                {
                    node_temp.Y += node_temp.X * (float)Math.Tan(fBackFrameRakeAngle_temp_rad);
                }
            }
        }

        public void RotateAndTranslateNodeAboutZ_CCW(Point3D pControlPoint, ref CNode node, float fAngle_rad)
        {
            Point3D p = node.GetPoint3D();
            RotateAndTranslatePointAboutZ_CCW(pControlPoint, ref p, fAngle_rad);

            node.X = (float)p.X;
            node.Y = (float)p.Y;
        }

        public void RotateAndTranslatePointAboutZ_CCW(Point3D pControlPoint, ref Point3D point, float fAngle_rad)
        {
            // Rotate node
            float fx = (float)Geom2D.GetRotatedPosition_x_CCW_rad(point.X, point.Y, fAngle_rad);
            float fy = (float)Geom2D.GetRotatedPosition_y_CCW_rad(point.X, point.Y, fAngle_rad);

            // Set rotated coordinates
            point.X = fx;
            point.Y = fy;

            // Translate node
            point.X += (float)pControlPoint.X;
            point.Y += (float)pControlPoint.Y;
        }

        public void AddColumnsNodes(int i_temp_numberofNodes, int i_temp_numberofMembers, int iOneRafterColumnNo, int iColumnNoInOneFrame, float fDist_Columns, float fy_Global_Coord)
        {
            float z_glob;

            // Bottom nodes
            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                CalcColumnNodeCoord_Z((i + 1) * fDist_Columns, out z_glob);
                m_arrNodes[i_temp_numberofNodes + i] = new CNode(i_temp_numberofNodes + i + 1, (i + 1) * fDist_Columns, fy_Global_Coord, 0, 0);
                listOfSupportedNodes_S2.Add(m_arrNodes[i_temp_numberofNodes + i]);
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + i]);
            }

            // Bottom nodes
            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                CalcColumnNodeCoord_Z((i + 1) * fDist_Columns, out z_glob);
                m_arrNodes[i_temp_numberofNodes + iOneRafterColumnNo + i] = new CNode(i_temp_numberofNodes + iOneRafterColumnNo + i + 1, fW_frame - ((i + 1) * fDist_Columns), fy_Global_Coord, 0, 0);
                listOfSupportedNodes_S2.Add(m_arrNodes[i_temp_numberofNodes + iOneRafterColumnNo + i]);
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + iOneRafterColumnNo + i]);
            }

            // Top nodes
            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                CalcColumnNodeCoord_Z((i + 1) * fDist_Columns, out z_glob);
                m_arrNodes[i_temp_numberofNodes + iColumnNoInOneFrame + i] = new CNode(i_temp_numberofNodes + 2 * iOneRafterColumnNo + i + 1, (i + 1) * fDist_Columns, fy_Global_Coord, z_glob, 0);
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + iColumnNoInOneFrame + i]);
            }

            // Top nodes
            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                CalcColumnNodeCoord_Z((i + 1) * fDist_Columns, out z_glob);
                m_arrNodes[i_temp_numberofNodes + iColumnNoInOneFrame + iOneRafterColumnNo + i] = new CNode(i_temp_numberofNodes + 3 * iOneRafterColumnNo + i + 1, fW_frame - ((i + 1) * fDist_Columns), fy_Global_Coord, z_glob, 0);
                RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + iColumnNoInOneFrame + iOneRafterColumnNo + i]);
            }
        }

        public void AddColumnsMembers(
            int i_temp_numberofNodes,
            int i_temp_numberofMembers,
            int iOneRafterColumnNo,
            int iColumnNoInOneFrame,
            CMemberEccentricity eccentricityColumn,
            float fColumnAlignmentStart,
            float fColumnAlignmentEnd,
            CCrSc section,
            float fMemberRotation,
            bool bUseFlyBracing,
            int iFlyBracing_Every_XXSupportingMember,
            float fFirstSupportingMemberPositionAbsolute,
            float fSupportingMembersDistance)
        {
            // Members - Columns
            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                m_arrMembers[i_temp_numberofMembers + i] = new CMember(i_temp_numberofMembers + i + 1, m_arrNodes[i_temp_numberofNodes + i], m_arrNodes[i_temp_numberofNodes + iColumnNoInOneFrame + i], section, EMemberType_FS.eC, EMemberType_FS_Position.ColumnFrontSide,  eccentricityColumn, eccentricityColumn, fColumnAlignmentStart, fColumnAlignmentEnd, fMemberRotation, 0);
                CreateAndAssignIrregularTransverseSupportGroupAndLTBsegmentGroup(bUseFlyBracing, iFlyBracing_Every_XXSupportingMember, fFirstSupportingMemberPositionAbsolute, fSupportingMembersDistance, ref m_arrMembers[i_temp_numberofMembers + i]);
            }

            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                m_arrMembers[i_temp_numberofMembers + iOneRafterColumnNo + i] = new CMember(i_temp_numberofMembers + iOneRafterColumnNo + i + 1, m_arrNodes[i_temp_numberofNodes + iOneRafterColumnNo + i], m_arrNodes[i_temp_numberofNodes + iColumnNoInOneFrame + iOneRafterColumnNo + i], section, EMemberType_FS.eC, EMemberType_FS_Position.ColumnBackSide, eccentricityColumn, eccentricityColumn, fColumnAlignmentStart, fColumnAlignmentEnd, fMemberRotation, 0);
                CreateAndAssignIrregularTransverseSupportGroupAndLTBsegmentGroup(bUseFlyBracing, iFlyBracing_Every_XXSupportingMember, fFirstSupportingMemberPositionAbsolute, fSupportingMembersDistance, ref m_arrMembers[i_temp_numberofMembers + iOneRafterColumnNo + i]);
            }
        }

        public int GetNumberofIntermediateNodesInOneColumnForGirts(float fBottomGirtPosition_temp, float fDistBetweenColumns, float fz_UpperLimitForGirts, int iColumnIndex)
        {
            float fz_gcs_column_temp;

            CalcColumnNodeCoord_Z((iColumnIndex + 1) * fDistBetweenColumns, out fz_gcs_column_temp);
            int iNumber_of_segments = (int)((fz_gcs_column_temp - fz_UpperLimitForGirts - fBottomGirtPosition_temp) / fDist_Girt);
            return iNumber_of_segments + 1;
        }

        public int GetNumberofIntermediateNodesInColumnsForOneFrame(int iOneRafterColumnNo, float fBottomGirtPosition_temp, float fDistBetweenColumns, float fz_UpperLimitForGirts)
        {
            int iNo_temp = 0;
            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                iNo_temp += GetNumberofIntermediateNodesInOneColumnForGirts(fBottomGirtPosition_temp, fDistBetweenColumns, fz_UpperLimitForGirts, i);
            }

            return iNo_temp;
        }

        public void AddFrontOrBackGirtsNodes(int iOneRafterColumnNo, int[] iArrNumberOfNodesPerColumn, int i_temp_numberofNodes, int iIntermediateColumnNodesForGirtsOneRafterNo, float fDist_Girts, float fDist_Columns, float fy_Global_Coord)
        {
            int iTemp = 0;

            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                float z_glob;
                CalcColumnNodeCoord_Z((i + 1) * fDist_Columns, out z_glob);

                for (int j = 0; j < iArrNumberOfNodesPerColumn[i]; j++)
                {
                    m_arrNodes[i_temp_numberofNodes + iTemp + j] = new CNode(i_temp_numberofNodes + iTemp + j + 1, (i + 1) * fDist_Columns, fy_Global_Coord, fBottomGirtPosition + j * fDist_Girts, 0);
                    RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + iTemp + j]);
                }

                iTemp += iArrNumberOfNodesPerColumn[i];
            }

            iTemp = 0;

            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                float z_glob;
                CalcColumnNodeCoord_Z((i + 1) * fDist_Columns, out z_glob);

                for (int j = 0; j < iArrNumberOfNodesPerColumn[i]; j++)
                {
                    m_arrNodes[i_temp_numberofNodes + iIntermediateColumnNodesForGirtsOneRafterNo + iTemp + j] = new CNode(i_temp_numberofNodes + iIntermediateColumnNodesForGirtsOneRafterNo + iTemp + j + 1, fW_frame - (i + 1) * fDist_Columns, fy_Global_Coord, fBottomGirtPosition + j * fDist_Girts, 0);
                    RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + iIntermediateColumnNodesForGirtsOneRafterNo + iTemp + j]);
                }

                iTemp += iArrNumberOfNodesPerColumn[i];
            }
        }

        public void AddFrontOrBackGirtsMembers(int iFrameNodesNo, int iOneRafterColumnNo, int[] iArrNumberOfNodesPerColumn, int i_temp_numberofNodes, int i_temp_numberofMembers,
            int iIntermediateColumnNodesForGirtsOneRafterNo, int iIntermediateColumnNodesForGirtsOneFrameNo, int iTempJumpBetweenFrontAndBack_GirtsNumberInLongidutinalDirection,
            float fDist_Girts, CMemberEccentricity eGirtEccentricity, float fGirtStart_MC, float fGirtStart, float fGirtEnd, CCrSc section, EMemberType_FS_Position eMemberType_FS_Position, float fMemberRotation, int iNumberOfTransverseSupports)
        {
            int iTemp = 0;
            int iTemp2 = 0;
            int iOneColumnGirtNo_temp = (int)((fH1_frame - fUpperGirtLimit - fBottomGirtPosition) / fDist_Girt) + 1;

            for (int i = 0; i < iOneRafterColumnNo + 1; i++)
            {
                if (i == 0) // First session depends on number of girts at main frame column
                {
                    for (int j = 0; j < iOneColumnGirtNo_temp; j++)
                    {
                        m_arrMembers[i_temp_numberofMembers + j] = new CMember(i_temp_numberofMembers + j + 1, m_arrNodes[iFrameNodesNo * iFrameNo + iTempJumpBetweenFrontAndBack_GirtsNumberInLongidutinalDirection + j], m_arrNodes[i_temp_numberofNodes + j], section, EMemberType_FS.eG, eMemberType_FS_Position, eGirtEccentricity, eGirtEccentricity, fGirtStart_MC, fGirtEnd, fMemberRotation, 0);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + j], iNumberOfTransverseSupports);
                    }

                    iTemp += iOneColumnGirtNo_temp;
                }
                else if (i < iOneRafterColumnNo) // Other sessions
                {
                    for (int j = 0; j < iArrNumberOfNodesPerColumn[i - 1]; j++)
                    {
                        m_arrMembers[i_temp_numberofMembers + iTemp + j] = new CMember(i_temp_numberofMembers + iTemp + j + 1, m_arrNodes[i_temp_numberofNodes + iTemp2 + j], m_arrNodes[i_temp_numberofNodes + iArrNumberOfNodesPerColumn[i - 1] + iTemp2 + j], section, EMemberType_FS.eG, eMemberType_FS_Position, eGirtEccentricity, eGirtEccentricity, fGirtStart, fGirtEnd, fMemberRotation, 0);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + iTemp + j], iNumberOfTransverseSupports);
                    }

                    iTemp2 += iArrNumberOfNodesPerColumn[i - 1];
                    iTemp += iArrNumberOfNodesPerColumn[i - 1];
                }
                else // Last session - prechadza cez stred budovy
                {
                    for (int j = 0; j < iArrNumberOfNodesPerColumn[i - 1]; j++)
                    {
                        m_arrMembers[i_temp_numberofMembers + iTemp + j] = new CMember(i_temp_numberofMembers + iTemp + j + 1, m_arrNodes[i_temp_numberofNodes + iTemp2 + j], m_arrNodes[i_temp_numberofNodes + iIntermediateColumnNodesForGirtsOneFrameNo - iArrNumberOfNodesPerColumn[iOneRafterColumnNo - 1] + j], section, EMemberType_FS.eG, eMemberType_FS_Position, eGirtEccentricity, eGirtEccentricity, fGirtStart, fGirtEnd, fMemberRotation, 0);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + iTemp + j], iNumberOfTransverseSupports);
                    }

                    iTemp += iArrNumberOfNodesPerColumn[i - 1];
                }
            }

            iTemp = 0;
            iTemp2 = 0;

            for (int i = 0; i < iOneRafterColumnNo; i++)
            {
                int iNumberOfMembers_temp = iOneColumnGirtNo_temp + iIntermediateColumnNodesForGirtsOneRafterNo;

                CMemberEccentricity eGirtEccentricity_temp = new CMemberEccentricity(eGirtEccentricity.MFy_local, -eGirtEccentricity.MFz_local);

                if (i == 0) // First session depends on number of girts at main frame column
                {
                    for (int j = 0; j < iOneColumnGirtNo_temp; j++)
                    {
                        m_arrMembers[i_temp_numberofMembers + iNumberOfMembers_temp + j] = new CMember(i_temp_numberofMembers + iNumberOfMembers_temp + j + 1, m_arrNodes[iFrameNodesNo * iFrameNo + iTempJumpBetweenFrontAndBack_GirtsNumberInLongidutinalDirection + iOneColumnGirtNo_temp + j], m_arrNodes[i_temp_numberofNodes + iIntermediateColumnNodesForGirtsOneRafterNo + j], section, EMemberType_FS.eG, eMemberType_FS_Position, eGirtEccentricity_temp, eGirtEccentricity_temp, fGirtStart_MC, fGirtEnd, -fMemberRotation, 0);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + iNumberOfMembers_temp + j], iNumberOfTransverseSupports);
                    }

                    iTemp += iOneColumnGirtNo_temp;
                }
                else // Other sessions (not in the middle)
                {
                    for (int j = 0; j < iArrNumberOfNodesPerColumn[i - 1]; j++)
                    {
                        m_arrMembers[i_temp_numberofMembers + iNumberOfMembers_temp + iTemp + j] = new CMember(i_temp_numberofMembers + iNumberOfMembers_temp + iTemp + j + 1, m_arrNodes[i_temp_numberofNodes + iIntermediateColumnNodesForGirtsOneRafterNo + iTemp2 + j], m_arrNodes[i_temp_numberofNodes + iIntermediateColumnNodesForGirtsOneRafterNo + iArrNumberOfNodesPerColumn[i - 1] + iTemp2 + j], section, EMemberType_FS.eG, eMemberType_FS_Position, eGirtEccentricity_temp, eGirtEccentricity_temp, fGirtStart, fGirtEnd, -fMemberRotation, 0);
                        CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(m_arrMembers[i_temp_numberofMembers + iNumberOfMembers_temp + iTemp + j], iNumberOfTransverseSupports);
                    }

                    iTemp2 += iArrNumberOfNodesPerColumn[i - 1];
                    iTemp += iArrNumberOfNodesPerColumn[i - 1];
                }
            }
        }

        public void AddFrontOrBackGirtsBracingBlocksNodes(int i_temp_numberofNodes, int [] iArrGB_NumberOfNodesPerBay, int [] iArrGB_NumberOfNodesPerBayFirstNode,
            int iNumberOfTransverseSupports, float fIntermediateSupportSpacing,  float fDist_Girts, float fDist_Columns, float fy_Global_Coord, out int iNumberOfGB_NodesInOneSideAndMiddleBay)
        {
            int iTemp = 0;

            for (int i = 0; i < iArrGB_NumberOfNodesPerBay.Length; i++) // Left side
            {
                for (int j = 0; j < iArrGB_NumberOfNodesPerBayFirstNode[i]; j++) // Bay
                {
                    for (int k = 0; k < iNumberOfTransverseSupports; k++)
                    {
                        float x_glob = i * fDist_FrontColumns + (k + 1) * fIntermediateSupportSpacing;
                        float z_glob;

                        if (j < iArrGB_NumberOfNodesPerBayFirstNode[i] - 1)
                            z_glob = (fBottomGirtPosition + j * fDist_Girts);
                        else
                            CalcColumnNodeCoord_Z(x_glob, out z_glob); // Top bracing blocks under the edge rafter

                        m_arrNodes[i_temp_numberofNodes + iTemp + j * iNumberOfTransverseSupports + k] = new CNode(i_temp_numberofNodes + iTemp + j * iNumberOfTransverseSupports + k + 1, x_glob, fy_Global_Coord, z_glob, 0);
                        RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + iTemp + j * iNumberOfTransverseSupports + k]);
                    }
                }
                iTemp += iArrGB_NumberOfNodesPerBay[i];
            }

            iNumberOfGB_NodesInOneSideAndMiddleBay = iTemp;
            iTemp = 0;

            for (int i = 0; i < iArrGB_NumberOfNodesPerBay.Length - 1; i++) // Right side
            {
                for (int j = 0; j < iArrGB_NumberOfNodesPerBayFirstNode[i]; j++) // Bay
                {
                    for (int k = 0; k < iNumberOfTransverseSupports; k++)
                    {
                        float x_glob = i * fDist_Columns + (k + 1) * fIntermediateSupportSpacing;
                        float z_glob;

                        if (j < iArrGB_NumberOfNodesPerBayFirstNode[i] - 1)
                            z_glob = (fBottomGirtPosition + j * fDist_Girts);
                        else
                            CalcColumnNodeCoord_Z(x_glob, out z_glob); // Top bracing blocks under the edge rafter

                        m_arrNodes[i_temp_numberofNodes + iNumberOfGB_NodesInOneSideAndMiddleBay + iTemp + j * iNumberOfTransverseSupports + k] = new CNode(i_temp_numberofNodes + iNumberOfGB_NodesInOneSideAndMiddleBay + iTemp + j * iNumberOfTransverseSupports + k + 1, fW_frame - x_glob, fy_Global_Coord, z_glob, 0);
                        RotateFrontOrBackFrameNodeAboutZ(m_arrNodes[i_temp_numberofNodes + iNumberOfGB_NodesInOneSideAndMiddleBay + iTemp + j * iNumberOfTransverseSupports + k]);
                    }
                }
                iTemp += iArrGB_NumberOfNodesPerBay[i];
            }
        }

        public void AddFrontOrBackGirtsBracingBlocksMembers(int i_temp_numberofNodes, int i_temp_numberofMembers, int[] iArrGB_NumberOfNodesPerBay, int[] iArrGB_NumberOfNodesPerBayFirstNode, int[] iArrGB_NumberOfMembersPerBay,
            int iNumberOfGB_NodesInOneSideAndMiddleBay, int iNumberOfTransverseSupports, CMemberEccentricity eGirtEccentricity, float fGBAlignmentStart, float fGBAlignmentEnd, float fGBAlignmentEndToRafter, CCrSc section,
            EMemberType_FS_Position eMemberType_FS_Position, float fColumnsRotation, bool bUseBraicingEverySecond)
        {
            float fRealLengthLimit = 0.25f; // Limit pre dlzku pruta, ak je prut kratsi ako limit, nastavimme mu bGenerate na false

            int iTemp = 0;
            int iTemp2 = 0;

            for (int i = 0; i < iArrGB_NumberOfMembersPerBay.Length; i++) // Left side
            {
                for (int j = 0; j < (iArrGB_NumberOfNodesPerBayFirstNode[i] - 1); j++) // Bay
                {
                    bool bDeactivateMember = false;
                    if (bUseBraicingEverySecond && j % 2 == 1) bDeactivateMember = true;

                    float fGBAlignmentEnd_Current = fGBAlignmentEnd;

                    if (j == iArrGB_NumberOfNodesPerBayFirstNode[i] - 1 - 1) // Last
                        fGBAlignmentEnd_Current = fGBAlignmentEndToRafter;

                    for (int k = 0; k < iNumberOfTransverseSupports; k++)
                    {
                        int memberIndex = i_temp_numberofMembers + iTemp2 + j * iNumberOfTransverseSupports + k;
                        int startNodeIndex = i_temp_numberofNodes + iTemp + j * iNumberOfTransverseSupports + k;
                        int endNodeIndex = i_temp_numberofNodes + iTemp + (j + 1) * iNumberOfTransverseSupports + k;
                        m_arrMembers[memberIndex] = new CMember(memberIndex + 1, m_arrNodes[startNodeIndex], m_arrNodes[endNodeIndex], section, EMemberType_FS.eGB, eMemberType_FS_Position, eGirtEccentricity, eGirtEccentricity, fGBAlignmentStart, fGBAlignmentEnd_Current, fColumnsRotation, 0);

                        if (m_arrMembers[memberIndex].FLength_real < fRealLengthLimit)
                            DeactivateMember(ref m_arrMembers[memberIndex]);

                        if(bDeactivateMember) DeactivateMemberAndItsJoints(ref m_arrMembers[memberIndex]);
                    }
                }
                iTemp += iArrGB_NumberOfNodesPerBay[i];
                iTemp2 += iArrGB_NumberOfMembersPerBay[i];
            }

            int iNumberOfGB_MembersInOneSideAndMiddleBay = iTemp2;
            iTemp = 0;
            iTemp2 = 0;

            for (int i = 0; i < iArrGB_NumberOfMembersPerBay.Length - 1; i++) // Right side
            {
                for (int j = 0; j < (iArrGB_NumberOfNodesPerBayFirstNode[i] - 1); j++) // Bay
                {
                    bool bDeactivateMember = false;
                    if (bUseBraicingEverySecond && j % 2 == 1) bDeactivateMember = true;

                    float fGBAlignmentEnd_Current = fGBAlignmentEnd;

                    if (j == iArrGB_NumberOfNodesPerBayFirstNode[i] - 1 - 1) // Last
                        fGBAlignmentEnd_Current = fGBAlignmentEndToRafter;

                    for (int k = 0; k < iNumberOfTransverseSupports; k++)
                    {
                        int memberIndex = i_temp_numberofMembers + iNumberOfGB_MembersInOneSideAndMiddleBay + iTemp2 + j * iNumberOfTransverseSupports + k;
                        int startNodeIndex = i_temp_numberofNodes + iNumberOfGB_NodesInOneSideAndMiddleBay + iTemp + j * iNumberOfTransverseSupports + k;
                        int endNodeIndex = i_temp_numberofNodes + iNumberOfGB_NodesInOneSideAndMiddleBay + iTemp + (j + 1) * iNumberOfTransverseSupports + k;
                        m_arrMembers[memberIndex] = new CMember(memberIndex + 1, m_arrNodes[startNodeIndex], m_arrNodes[endNodeIndex], section, EMemberType_FS.eGB, eMemberType_FS_Position, eGirtEccentricity, eGirtEccentricity, fGBAlignmentStart, fGBAlignmentEnd_Current, fColumnsRotation, 0);

                        if (m_arrMembers[memberIndex].FLength_real < fRealLengthLimit)
                            DeactivateMember(ref m_arrMembers[memberIndex]);

                        if (bDeactivateMember) DeactivateMemberAndItsJoints(ref m_arrMembers[memberIndex]);
                    }
                }
                iTemp += iArrGB_NumberOfNodesPerBay[i];
                iTemp2 += iArrGB_NumberOfMembersPerBay[i];
            }
        }

        public void GetJointAllignments(float fh_column, float fh_rafter, out float allignment_column, out float allignment_knee_rafter, out float allignment_apex_rafter)
        {
            float cosAlpha = (float)Math.Cos(fRoofPitch_rad);
            float sinAlpha = (float)Math.Sin(fRoofPitch_rad);
            float tanAlpha = (float)Math.Tan(fRoofPitch_rad);

            /*
            float y = fh_rafter / cosAlpha;
            float a = sinAlpha * 0.5f * y;
            float x = cosAlpha * 2f * a;
            float x2 = 0.5f * fh_column - x;
            float y2 = tanAlpha * x2;
            allignment_column = 0.5f * y + y2;

            float x3 = 0.5f * x;
            float x4 = 0.5f * fh_column - x3;
            allignment_knee_rafter = x4 / cosAlpha;

            allignment_apex_rafter = a;
            */

            float y = fh_rafter / cosAlpha;
            allignment_apex_rafter = sinAlpha * 0.5f * y;
            float x = cosAlpha * 2f * allignment_apex_rafter;
            allignment_column = 0.5f * y + (tanAlpha * (0.5f * fh_column - x));
            allignment_knee_rafter = (0.5f * fh_column - (0.5f * x)) / cosAlpha;
        }

        public void AddDoorBlock(DoorProperties prop, float fLimitDistanceFromColumn, float fSideWallHeight)
        {
            CMember mReferenceGirt;
            CMember mColumnLeft;
            CMember mColumnRight;
            CMember mEavesPurlin;
            CBlock_3D_001_DoorInBay door;
            Point3D pControlPointBlock;
            float fBayWidth;
            float fBayHeight = fH1_frame; // TODO - spocitat vysku bay v mieste bloku (pre front a back budu dve vysky v mieste vlozenia stlpov bloku
            int iFirstMemberToDeactivate;
            bool bIsReverseSession;
            bool bIsFirstBayInFrontorBackSide;
            bool bIsLastBayInFrontorBackSide;

            DeterminateBasicPropertiesToInsertBlock(prop.sBuildingSide, prop.iBayNumber, out mReferenceGirt, out mColumnLeft, out mColumnRight, out mEavesPurlin, out pControlPointBlock, out fBayWidth, out iFirstMemberToDeactivate, out bIsReverseSession, out bIsFirstBayInFrontorBackSide, out bIsLastBayInFrontorBackSide);

            // Set girt to connect columns / trimmers
            int iNumberOfGirtsToDeactivate = (int)((prop.fDoorsHeight - fBottomGirtPosition) / fDist_Girt) + 1; // Number of intermediate girts + Bottom Girt (prevzate z CBlock_3D_001_DoorInBay)
            CMember mGirtToConnectDoorTrimmers = m_arrMembers[(mReferenceGirt.ID - 1) + iNumberOfGirtsToDeactivate]; // Toto je girt, ku ktoremu sa pripoja stlpy dveri (len v pripade ze sa nepripoja k eave purlin alebo edge rafter) - 1 -index reference girt

            door = new CBlock_3D_001_DoorInBay(
                prop,
                fLimitDistanceFromColumn,
                fBottomGirtPosition,
                fDist_Girt,
                mReferenceGirt,
                mGirtToConnectDoorTrimmers,
                mColumnLeft,
                mColumnRight,
                mEavesPurlin,
                fBayWidth,
                fBayHeight,
                fUpperGirtLimit,
                bIsReverseSession,
                bIsFirstBayInFrontorBackSide,
                bIsLastBayInFrontorBackSide);

            AddDoorOrWindowBlockProperties(pControlPointBlock, iFirstMemberToDeactivate, door);

            DoorsModels.Add(door);
        }

        public void AddWindowBlock(WindowProperties prop, float fLimitDistanceFromColumn)
        {
            CMember mReferenceGirt;
            CMember mColumnLeft;
            CMember mColumnRight;
            CMember mEavesPurlin;
            CBlock_3D_002_WindowInBay window;
            Point3D pControlPointBlock;
            float fBayWidth;
            float fBayHeight = fH1_frame; // TODO - spocitat vysku bay v mieste bloku (pre front a back budu dve vysky v mieste vlozenia stlpov bloku
            int iFirstGirtInBay;
            int iFirstMemberToDeactivate;
            bool bIsReverseSession;
            bool bIsFirstBayInFrontorBackSide;
            bool bIsLastBayInFrontorBackSide;

            DeterminateBasicPropertiesToInsertBlock(prop.sBuildingSide, prop.iBayNumber, out mReferenceGirt, out mColumnLeft, out mColumnRight, out mEavesPurlin, out pControlPointBlock, out fBayWidth, out iFirstGirtInBay, out bIsReverseSession, out bIsFirstBayInFrontorBackSide, out bIsLastBayInFrontorBackSide);

            // Prevzate z CBlock_3D_002_WindowInBay
            int iNumberOfGirtsUnderWindow = (int)((prop.fWindowCoordinateZinBay - fBottomGirtPosition) / fDist_Girt) + 1;
            float fCoordinateZOfGirtUnderWindow = (iNumberOfGirtsUnderWindow - 1) * fDist_Girt + fBottomGirtPosition;

            if (prop.fWindowCoordinateZinBay <= fBottomGirtPosition)
            {
                iNumberOfGirtsUnderWindow = 0;
                fCoordinateZOfGirtUnderWindow = 0f;
            }

            int iNumberOfGirtsToDeactivate = (int)((prop.fWindowsHeight + prop.fWindowCoordinateZinBay - fCoordinateZOfGirtUnderWindow) / fDist_Girt); // Number of intermediate girts to deactivate

            CMember mGirtToConnectWindowColumns_Bottom = null;

            if(iNumberOfGirtsUnderWindow > 0)
               mGirtToConnectWindowColumns_Bottom = m_arrMembers[(mReferenceGirt.ID - 1) + (iNumberOfGirtsUnderWindow - 1)]; // Toto je girt, ku ktoremu sa pripoja stlpiky okna v dolnej casti

            CMember mGirtToConnectWindowColumns_Top = m_arrMembers[(mReferenceGirt.ID - 1) + (iNumberOfGirtsUnderWindow - 1) + iNumberOfGirtsToDeactivate + 1]; // Toto je girt, ku ktoremu sa pripoja stlpiky okna v hornej casti (len v pripade ze sa nepripoja k eave purlin alebo edge rafter)

            window = new CBlock_3D_002_WindowInBay(
                prop,
                fLimitDistanceFromColumn,
                fBottomGirtPosition,
                fDist_Girt,
                mReferenceGirt,
                mGirtToConnectWindowColumns_Bottom,
                mGirtToConnectWindowColumns_Top,
                mColumnLeft,
                mColumnRight,
                mEavesPurlin,
                fBayWidth,
                fBayHeight,
                fUpperGirtLimit,
                bIsReverseSession,
                bIsFirstBayInFrontorBackSide,
                bIsLastBayInFrontorBackSide);

            iFirstMemberToDeactivate = iFirstGirtInBay + window.iNumberOfGirtsUnderWindow;

            AddDoorOrWindowBlockProperties(pControlPointBlock, iFirstMemberToDeactivate, window);

            WindowsModels.Add(window);
        }

        public void DeterminateBasicPropertiesToInsertBlock(
            string sBuildingSide,                     // Identification of building side (left, right, front, back)
            int iBayNumber,                           // Bay number (1-n) in positive X or Y direction
            out CMember mReferenceGirt,               // Reference girt - first girts that needs to be deactivated and replaced by new member (some parameters are same for deactivated and new member)
            out CMember mColumnLeft,                  // Left column of bay
            out CMember mColumnRight,                 // Right column of bay
            out CMember mEavesPurlin,                  // Eave purlin for left and right side
            out Point3D pControlPointBlock,            // Conctrol point to insert block - defined as left column base point
            out float fBayWidth,                      // Width of bay (distance between bay columns)
            out int iFirstMemberToDeactivate,         // Index of first girt in the bay which is in collision with the block and must be deactivated
            out bool bIsReverseSession,               // Front or back wall bay can have reverse direction of girts in X
            out bool bIsFirstBayInFrontorBackSide,
            out bool bIsLastBayInFrontorBackSide
            )
        {
            bIsReverseSession = false;            // Set to true value just for front or back wall (right part of wall)
            bIsFirstBayInFrontorBackSide = false; // Set to true value just for front or back wall (first bay)
            bIsLastBayInFrontorBackSide = false;  // Set to true value just for front or back wall (last bay)

            if (sBuildingSide == "Left" || sBuildingSide == "Right")
            {
                // Left side X = 0, Right Side X = GableWidth
                // Insert after frame ID
                int iSideMultiplier = sBuildingSide == "Left" ? 0 : 1; // 0 left side X = 0, 1 - right side X = Gable Width
                int iBlockFrame = iBayNumber - 1; // ID of frame in the bay, starts with zero

                int iBayColumnLeft = (iBlockFrame * 6) + (iSideMultiplier == 0 ? 0 : (4 - 1)); // (2 columns + 2 rafters + 2 eaves purlins) = 6, For Y = GableWidth + 4 number of members in one frame - 1 (index)
                int iBayColumnRight = ((iBlockFrame + 1) * 6) + (iSideMultiplier == 0 ? 0 : (4 - 1));
                fBayWidth = fL1_frame;
                iFirstMemberToDeactivate = iMainColumnNo + iRafterNo + iEavesPurlinNo + iBlockFrame * iGirtNoInOneFrame + iSideMultiplier * (iGirtNoInOneFrame / 2);

                mReferenceGirt = m_arrMembers[iFirstMemberToDeactivate]; // Deactivated member properties define properties of block girts
                mColumnLeft = m_arrMembers[iBayColumnLeft];
                mColumnRight = m_arrMembers[iBayColumnRight];

                if (sBuildingSide == "Left")
                mEavesPurlin = m_arrMembers[(iBlockFrame * iEavesPurlinNoInOneFrame) + iBlockFrame * (iFrameNodesNo - 1) + 4];
                else
                mEavesPurlin = m_arrMembers[(iBlockFrame * iEavesPurlinNoInOneFrame) + iBlockFrame * (iFrameNodesNo - 1) + 5];
            }
            else // Front or Back Side
            {
                // Insert after sequence ID
                int iNumberOfIntermediateColumns;
                int[] iArrayOfGirtsPerColumnCount;
                //int iNumberOfGirtsInWall;

                if (sBuildingSide == "Front")  // Front side properties
                {
                    iNumberOfIntermediateColumns = iFrontColumnNoInOneFrame;
                    iArrayOfGirtsPerColumnCount = iArrNumberOfNodesPerFrontColumn;
                    //iNumberOfGirtsInWall = iFrontGirtsNoInOneFrame;
                    fBayWidth = fDist_FrontColumns;
                }
                else // Back side properties
                {
                    iNumberOfIntermediateColumns = iBackColumnNoInOneFrame;
                    iArrayOfGirtsPerColumnCount = iArrNumberOfNodesPerBackColumn;
                    //iNumberOfGirtsInWall = iBackGirtsNoInOneFrame;
                    fBayWidth = fDist_BackColumns;
                }

                int iSideMultiplier = sBuildingSide == "Front" ? 0 : 1; // 0 front side Y = 0, 1 - back side Y = Length
                int iBlockSequence = iBayNumber - 1; // ID of sequence, starts with zero
                int iColumnNumberLeft;
                int iColumnNumberRight;
                int iNumberOfFirstGirtInWallToDeactivate = 0;
                int iNumberOfMembers_tempForGirts = iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame + iSideMultiplier * iFrontGirtsNoInOneFrame;
                int iNumberOfMembers_tempForColumns = iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iSideMultiplier * iFrontColumnNoInOneFrame;

                if (iBlockSequence == 0) // Main Column - first bay
                {
                    if (sBuildingSide == "Front")
                    {
                        iColumnNumberLeft = 0;
                        iColumnNumberRight = iNumberOfMembers_tempForColumns + iBlockSequence;
                    }
                    else
                    {
                        iColumnNumberLeft = (iFrameNo - 1) * 6;
                        iColumnNumberRight = iNumberOfMembers_tempForColumns + iBlockSequence;
                    }

                    iFirstMemberToDeactivate = iNumberOfMembers_tempForGirts + iNumberOfFirstGirtInWallToDeactivate;

                    bIsFirstBayInFrontorBackSide = true; // First bay
                }
                else
                {
                    if (iBlockSequence < (int)(iNumberOfIntermediateColumns / 2) + 1) // Left session
                    {
                        iColumnNumberLeft = iNumberOfMembers_tempForColumns + iBlockSequence - 1;
                        iColumnNumberRight = iNumberOfMembers_tempForColumns + iBlockSequence;

                        iNumberOfFirstGirtInWallToDeactivate += iOneColumnGirtNo;

                        for (int i = 0; i < iBlockSequence - 1; i++)
                            iNumberOfFirstGirtInWallToDeactivate += iArrayOfGirtsPerColumnCount[i];
                    }
                    else // Right session
                    {
                        bIsReverseSession = true; // Nodes and members are numbered from right to the left

                        iColumnNumberLeft = iNumberOfMembers_tempForColumns + (int)(iNumberOfIntermediateColumns / 2) + iNumberOfIntermediateColumns - iBlockSequence;
                        iColumnNumberRight = iNumberOfMembers_tempForColumns + (int)(iNumberOfIntermediateColumns / 2) + iNumberOfIntermediateColumns - iBlockSequence - 1;

                        // Number of girts in left session
                        iNumberOfFirstGirtInWallToDeactivate += iOneColumnGirtNo;

                        for (int i = 0; i < (int)(iNumberOfIntermediateColumns / 2); i++)
                            iNumberOfFirstGirtInWallToDeactivate += iArrayOfGirtsPerColumnCount[i];

                        if (iBlockSequence < iNumberOfIntermediateColumns)
                            iNumberOfFirstGirtInWallToDeactivate += iOneColumnGirtNo;

                        for (int i = 0; i < iNumberOfIntermediateColumns - iBlockSequence - 1; i++)
                            iNumberOfFirstGirtInWallToDeactivate += iArrayOfGirtsPerColumnCount[i];

                        if (iBlockSequence == iNumberOfIntermediateColumns) // Last bay
                        {
                            bIsLastBayInFrontorBackSide = true;
                            iColumnNumberRight = iSideMultiplier == 0 ? 3 : (iFrameNo - 1) * 6 + 3;
                        }
                    }

                    iFirstMemberToDeactivate = iNumberOfMembers_tempForGirts + iNumberOfFirstGirtInWallToDeactivate;
                }

                mReferenceGirt = m_arrMembers[iFirstMemberToDeactivate]; // Deactivated member properties define properties of block girts
                mColumnLeft = m_arrMembers[iColumnNumberLeft];
                mColumnRight = m_arrMembers[iColumnNumberRight];
                mEavesPurlin = null; // Not defined for the front and back side
            }

            pControlPointBlock = new Point3D(mColumnLeft.NodeStart.X, mColumnLeft.NodeStart.Y, mColumnLeft.NodeStart.Z);
        }

        //Tuto funkciu mam pozriet - Mato chce:
        //rozsirujem tam velkosti poli a take veci CModel_PFD_01_GR - riadok 1751
        //vlastne tie objekty z objektu CBlock pridavam do celkoveho zoznamu, ale napriklad prerez pre girts som ignoroval aby tam nebol 2x
        //Chce to vymysliet nejaky koncept ako to ma fungovat a chce to programatorsku hlavu 🙂
        //tie moje "patlacky" ako sa to tam dolepuje do poli atd by som nebral velmi vazne
        //Malo by ty to fungovat tak, ze ked pridam prve dvere tak sa tie prierezy pridaju a ked pridavam dalsie, tak uz sa pridavaju len uzly a pruty a prierez sa len nastavi
        //uz by sa nemal vytvarat novy
        public void AddDoorOrWindowBlockProperties(Point3D pControlPointBlock, int iFirstMemberToDeactivate, CBlock block)
        {
            float fBlockRotationAboutZaxis_rad = 0;

            if (block.BuildingSide == "Left" || block.BuildingSide == "Right")
                fBlockRotationAboutZaxis_rad = MathF.fPI / 2.0f; // Parameter of block - depending on side of building (front, back (0 deg), left, right (90 deg))

            //----------------------------------------------------------------------------------------------------------------------------------------------------
            // TODO 405 - TO Ondrej - tu sa znazim pripravit obrysove body otvoru v 3D - GCS
            // Opening definition points
            // Transformation from LCS of block to GCS // Create definition points in 3D

            List<Point3D> openningPointsInGCS = new List<Point3D>();

            foreach (System.Windows.Point p2D in block.openningPoints)
            {
                Point3D p3D = new Point3D(p2D.X, 0, p2D.Y);
                RotateAndTranslatePointAboutZ_CCW(pControlPointBlock, ref p3D, fBlockRotationAboutZaxis_rad);
                openningPointsInGCS.Add(p3D); // Output - s tymito suradnicami by sa mala porovnavat pozicia girt bracing na jednotlivych stranach budovy
            }
            //----------------------------------------------------------------------------------------------------------------------------------------------------

            int arraysizeoriginal;

            // Cross-sections

            // Copy block cross-sections into the model
            for (int i = 1; i < block.m_arrCrSc.Length; i++) // Zacina sa od i = 1 - preskocit prvy prvok v poli doors, pretoze odkaz na girt section uz existuje, nie je potrebne prierez kopirovat znova
            {
                CCrSc foundCrsc = m_arrCrSc.FirstOrDefault(c => c.ID == block.m_arrCrSc[i].ID);
                if (foundCrsc != null) continue;

                arraysizeoriginal = m_arrCrSc.Length;
                Array.Resize(ref m_arrCrSc, arraysizeoriginal + 1); // ( - 1) Prvy prvok v poli blocks crsc ignorujeme
                // Preskocit prvy prvok v poli block crsc, pretoze odkaz na girt section uz existuje, nie je potrebne prierez kopirovat znova
                m_arrCrSc[arraysizeoriginal] = block.m_arrCrSc[i];
                //m_arrCrSc[arraysizeoriginal + i - 1].ID = arraysizeoriginal + i/* -1 + 1*/; // Odcitat index pretoze prvy prierez ignorujeme a pridat 1 pre ID (+ 1)
            }

            //task 405 - je to hotove, ale chcelo by to mozno aj zistit ako je to narocne na pamat, lebo su tam vyhladavacky
            DeactivateBracingBlocksThroughtBlock(block, openningPointsInGCS);

            // Nodes
            arraysizeoriginal = m_arrNodes.Length;
            Array.Resize(ref m_arrNodes, m_arrNodes.Length + block.m_arrNodes.Length);

            int iNumberofMembersToDeactivate = block.INumberOfGirtsToDeactivate;

            // Deactivate already generated members in the bay (space between frames) where is the block inserted
            for (int i = 0; i < iNumberofMembersToDeactivate; i++)
            {
                // Deactivate Members
                // Deactivate Member Joints
                CMember m = m_arrMembers[iFirstMemberToDeactivate + i];
                DeactivateMemberAndItsJoints(ref m);

                // -------------------------------------------------------------------------------------------------
                // Deactivate bracing blocks and joints
                // Find bracing blocks for deactivated girt
                DeactivateMemberBracingBlocks(m, block, openningPointsInGCS);
            }

            // Copy block nodes into the model
            for (int i = 0; i < block.m_arrNodes.Length; i++)
            {
                RotateAndTranslateNodeAboutZ_CCW(pControlPointBlock, ref block.m_arrNodes[i], fBlockRotationAboutZaxis_rad);
                m_arrNodes[arraysizeoriginal + i] = block.m_arrNodes[i];
                m_arrNodes[arraysizeoriginal + i].ID = arraysizeoriginal + i + 1;
            }

            // Members
            arraysizeoriginal = m_arrMembers.Length;
            Array.Resize(ref m_arrMembers, m_arrMembers.Length + block.m_arrMembers.Length);

            // Copy block members into the model
            for (int i = 0; i < block.m_arrMembers.Length; i++)
            {
                // Position of definition nodes was already changed, we dont need to rotate member definition nodes NodeStart and NodeEnd
                // Recalculate basic member data (PointA, PointB, delta projection length)
                block.m_arrMembers[i].Fill_Basic();

                m_arrMembers[arraysizeoriginal + i] = block.m_arrMembers[i];
                m_arrMembers[arraysizeoriginal + i].ID = arraysizeoriginal + i + 1;
            }

            // Add block member connections to the main model connections
            foreach (CConnectionJointTypes joint in block.m_arrConnectionJoints)
                m_arrConnectionJoints.Add(joint);
        }

        private void DeactivateMemberBracingBlocks(CMember m, CBlock block, List<Point3D> openningPointsInGCS)
        {
            float fAdditionalOffset = 0.2f; // Ak nechceme aby brace (bracing blok) bol hned vela door alebo window
            IEnumerable<CMember> bracingBlocks = m_arrMembers.Where(mGB => mGB.EMemberType == EMemberType_FS.eGB && (m.IntermediateNodes.Contains(mGB.NodeStart) || m.IntermediateNodes.Contains(mGB.NodeEnd)));
            foreach (CMember mBB in bracingBlocks)
            {
                if (block.BuildingSide == "Left" || block.BuildingSide == "Right") // Blok je v lavej alebo pravej stene, LCS x bloku odpoveda smer v GCS Y // Porovnavame suradnice GCS Y
                {
                    if (mBB.NodeStart.Y >= openningPointsInGCS[0].Y - fAdditionalOffset && mBB.NodeStart.Y <= openningPointsInGCS[1].Y + fAdditionalOffset)
                    {
                        DeactivateMemberAndItsJoints(mBB);
                    }
                }
                else // Blok je v prednej alebo zadnej stene, LCS x bloku odpoveda smer v GCS X // Porovnavame suradnice GCS X
                {
                    if (mBB.NodeStart.X >= openningPointsInGCS[0].X - fAdditionalOffset && mBB.NodeStart.X <= openningPointsInGCS[1].X + fAdditionalOffset)
                    {
                        DeactivateMemberAndItsJoints(mBB);
                    }
                }
            }
        }

        private void DeactivateBracingBlocksThroughtBlock(CBlock block, List<Point3D> openningPointsInGCS)
        {
            float fAdditionalOffset = 0.2f; // Ak nechceme aby brace (bracing blok) bol hned vela door alebo window
            //vyberieme podla Z suradnice tie,ktore pretinaju okno
            IEnumerable<CMember> bracingBlocks = m_arrMembers.Where(mGB => mGB.EMemberType == EMemberType_FS.eGB && 
                ((mGB.NodeStart.Z <= openningPointsInGCS[0].Z && mGB.NodeEnd.Z >= openningPointsInGCS[3].Z)
                || (mGB.NodeStart.Z >= openningPointsInGCS[0].Z && mGB.NodeEnd.Z <= openningPointsInGCS[3].Z)));
            foreach (CMember mBB in bracingBlocks)
            {
                if (block.BuildingSide == "Left" || block.BuildingSide == "Right") // Blok je v lavej alebo pravej stene, LCS x bloku odpoveda smer v GCS Y // Porovnavame suradnice GCS Y
                {
                    // Bug 411 - pridana prva podmienka - musime kontrolovat aj to na ktorej strane budovy sme
                    if (MathF.d_equal(mBB.NodeStart.X, openningPointsInGCS[0].X) && (mBB.NodeStart.Y >= openningPointsInGCS[0].Y - fAdditionalOffset && mBB.NodeStart.Y <= openningPointsInGCS[1].Y + fAdditionalOffset))
                    {
                        DeactivateMemberAndItsJoints(mBB);
                    }
                }
                else // Blok je v prednej alebo zadnej stene, LCS x bloku odpoveda smer v GCS X // Porovnavame suradnice GCS X
                {
                    // Bug 411 - pridana prva podmienka - musime kontrolovat aj to na ktorej strane budovy sme
                    if (MathF.d_equal(mBB.NodeStart.Y, openningPointsInGCS[0].Y) && (mBB.NodeStart.X >= openningPointsInGCS[0].X - fAdditionalOffset && mBB.NodeStart.X <= openningPointsInGCS[1].X + fAdditionalOffset))
                    {
                        DeactivateMemberAndItsJoints(mBB);
                    }
                }
            }
        }

        // Load model component cross-sections
        public void SetCrossSectionsFromDatabase()
        {
            // Todo - Ondrej
            // Chcel som napojit obsah m_arrCrSc podla 
            // MDBModels tabulka KitsetGableRoofEnclosed alebo KitsetGableRoofEnclosedCrscID
            // Skusis mi aspon jeden prierez napojit prosim ?
            // pole m_arrCrSc by sa malo naplnit prierezmi podla mena prierezu (270115, 27095, 63020, ...) v databaze pre jednotlive typy members (purlin, girt, main column, ...)
            // kazdemu menu prierezu zodpoveda iny objekt z tried, ktore su v CRSC / CrSc_3 / FS
        }

        // Add members to the member group list
        // Base on cross-section ID 
        // TODO - spravnejsie by bolo pridavat member do zoznamu priamo pri vytvoreni
        public void AddMembersToMemberGroupsLists()
        {
            int i = 0;

            // Opravene ID prierezu sa bralo z poradia v databaze a prepisovalo ID prierezu z modelu
            foreach (CMember member in m_arrMembers)
            {
                foreach (CMemberGroup group in listOfModelMemberGroups) // TODO - dalo by sa nahradit napriklad switchom ak pozname presne typy
                {
                    if (member.BIsGenerated && member.CrScStart.ID == group.CrossSection.ID) // In case that cross-section ID is same add member to the list
                    {
                        member.EMemberTypePosition = group.MemberType_FS_Position; // TODO - docasne riesenie, nastavime prutu typ podla pozicie (referencny typ pruta nastaveny v skupine), spravne by sme ho mohli nastavit uz v konstruktore pruta pri jeho vytvoreni
                        group.ListOfMembers.Add(member);
                        //listOfModelMemberGroups[group.CrossSection.ICrSc_ID].ListOfMembers.Add(member);
                        i++;
                        break;
                    }
                }
            }

            // Check
            // TODO - aktivovat po vyrieseni mazania nevygenerovanych prutov zo zoznamu a pridani prutov tvoriacich bloky (dvere, okna, ...)
            /*
            if (i != m_arrMembers.Length)
                throw new Exception("Not all members were added.");
            */
        }

        // TODO - spravnejsie by bolo nastavovat defaultne parametre spoja uz pri vytvoreni
        public void SetJointDefaultParameters()
        {
            foreach (CConnectionJointTypes joint in m_arrConnectionJoints)
            {
                if (joint.m_MainMember != null && joint.m_MainMember.BIsGenerated) // In case that joint main member is generated
                {
                    if (joint.m_MainMember.BIsDisplayed == true) joint.BIsDisplayed = true;
                    if (joint.m_MainMember.BIsSelectedForDesign == true) joint.BIsSelectedForDesign = true;
                    if (joint.m_MainMember.BIsSelectedForMaterialList == true) joint.BIsSelectedForMaterialList = true;
                }
            }
        }

        private List<CSegment_LTB> GenerateIntermediateLTBSegmentsOnMember(List<CIntermediateTransverseSupport> lTransverseSupportGroup, bool bIsRelativeCoordinate_x, float fMemberLength)
        {
            List<CSegment_LTB> LTB_segment_group = null; // TODO - rozhodnut co sa ma generovat ak na prute nie su medzilahle podpory, nic alebo vzdy jeden segment ???

            if (lTransverseSupportGroup != null && lTransverseSupportGroup.Count > 0)
            {
                LTB_segment_group = new List<CSegment_LTB>();

                // Create lateral-torsional buckling segments
                for (int j = 0; j < lTransverseSupportGroup.Count + 1; j++)
                {
                    // Number of segments = number of intermediate supports + 1 - type BothFlanges
                    // TODO - doriesit ako generovat segmenty ak nie su vsetky lateral supports typu BothFlanges
                    // Najprv by sa mal najst pocet podpor s BothFlanges, z toho urcit pocet segmentov a zohladnovat len tie coordinates x,
                    // ktore sa vztahuju na podpory s BothFlanges

                    float fSegmentStart_abs = 0;
                    float fSegmentEnd_abs = fMemberLength;

                    if (j == 0) // First Segment
                    {
                        fSegmentStart_abs = 0f;
                        fSegmentEnd_abs = lTransverseSupportGroup[j].Fx_position_abs;
                    }
                    else if (j < lTransverseSupportGroup.Count)
                    {
                        fSegmentStart_abs = lTransverseSupportGroup[j - 1].Fx_position_abs;
                        fSegmentEnd_abs = lTransverseSupportGroup[j].Fx_position_abs;
                    }
                    else // Last
                    {
                        fSegmentStart_abs = lTransverseSupportGroup[j - 1].Fx_position_abs;
                        fSegmentEnd_abs = fMemberLength;
                    }

                    CSegment_LTB segment = new CSegment_LTB(j + 1, bIsRelativeCoordinate_x, fSegmentStart_abs, fSegmentEnd_abs, fMemberLength);

                    LTB_segment_group.Add(segment);
                }
            }

            return LTB_segment_group;
        }

        private List<CIntermediateTransverseSupport> GenerateRegularIntermediateTransverseSupports(float fMemberLength, int iNumberOfTransverseSupports)
        {
            List<CIntermediateTransverseSupport> TransverseSupportGroup = null;

            float fFirstSupportPosition = fMemberLength / (iNumberOfTransverseSupports + 1); // number of LTB segments = number of support + 1
            float fDistOfSupports = fFirstSupportPosition;

            if (iNumberOfTransverseSupports > 0)
            {
                TransverseSupportGroup = new List<CIntermediateTransverseSupport>();
                for (int j = 0; j < iNumberOfTransverseSupports; j++) // Each suport
                {
                    float fxLocationOfSupport = Math.Min(fFirstSupportPosition + j * fDistOfSupports, fMemberLength);

                    if (fxLocationOfSupport < fMemberLength)
                        TransverseSupportGroup.Add(new CIntermediateTransverseSupport(j + 1, EITSType.eBothFlanges, fxLocationOfSupport / fMemberLength, fxLocationOfSupport, 0));
                    // TODO - To Ondrej, nie som si isty ci mam v kazdej podpore CIntermediateTransverseSupport ukladat jej poziciu (aktualny stav) alebo ma CMember nie list CIntermediateTransverseSupport ale list nejakych struktur (x, CIntermediateTransverseSupport), takze x miesta budu definovane v tejto strukture v objekte CMember a samotny objekt CIntermediateTransverseSupport nebude vediet kde je
                }
            }

            return TransverseSupportGroup;
        }

        private void CreateAndAssignRegularTransverseSupportGroupAndLTBsegmentGroup(CMember member, int iNumberOfTransverseSupports)
        {
            List<CIntermediateTransverseSupport> TransverseSupportGroup = GenerateRegularIntermediateTransverseSupports(member.FLength, iNumberOfTransverseSupports);
            List<CSegment_LTB> LTB_segment_group = GenerateIntermediateLTBSegmentsOnMember(TransverseSupportGroup, false, member.FLength);

            // Assign transverse support group and LTB segment group to the member
            member.IntermediateTransverseSupportGroup = TransverseSupportGroup;
            member.LTBSegmentGroup = LTB_segment_group;
        }

        private void CreateIrregularTransverseSupportGroupAndLTBsegmentGroup(
        bool bUseFlyBracing,
        int iFlyBracing_Every_XXSupportingMember,
        float fFirstSupportingMemberPositionAbsolute,
        float fSupportingMembersDistance,
        ref CMember member,
        out List<CIntermediateTransverseSupport> lTransverseSupportGroup_Member,
        out List<CSegment_LTB> LTB_segment_group_Member)
        {
            lTransverseSupportGroup_Member = null;
            LTB_segment_group_Member = null;

            // Define fly bracing positions and LTB segments on member
            float fMemberLength = member.FLength;

            if (bUseFlyBracing && iFlyBracing_Every_XXSupportingMember > 0)
            {
                    lTransverseSupportGroup_Member = new List<CIntermediateTransverseSupport>();
                    float fFirstFlyBracePosition = fFirstSupportingMemberPositionAbsolute + (iFlyBracing_Every_XXSupportingMember - 1) * fSupportingMembersDistance;
                    int iNumberOfFlyBracesOnMember = fFirstFlyBracePosition < fMemberLength ? (int)((fMemberLength - fFirstFlyBracePosition) / (iFlyBracing_Every_XXSupportingMember * fSupportingMembersDistance)) + 1 : 0;

                    for (int j = 0; j < iNumberOfFlyBracesOnMember; j++) // Each fly brace
                    {
                        float fxLocationOfFlyBrace = fFirstFlyBracePosition + (j * iFlyBracing_Every_XXSupportingMember) * fSupportingMembersDistance;

                        if (fxLocationOfFlyBrace < fMemberLength)
                            lTransverseSupportGroup_Member.Add(new CIntermediateTransverseSupport(j + 1, EITSType.eBothFlanges, fxLocationOfFlyBrace / fMemberLength, fxLocationOfFlyBrace, 0));
                        // TODO - To Ondrej, nie som si isty ci mam v kazdej podpore CIntermediateTransverseSupport ukladat jej poziciu (aktualny stav) alebo ma CMember nie list CIntermediateTransverseSupport ale list nejakych struktur (x, CIntermediateTransverseSupport), takze x miesta budu definovane v tejto strukture v objekte CMember a samotny objekt CIntermediateTransverseSupport nebude vediet kde je
                    }

                    if (lTransverseSupportGroup_Member.Count > 0)
                    {
                        LTB_segment_group_Member = GenerateIntermediateLTBSegmentsOnMember(lTransverseSupportGroup_Member, false, fMemberLength);
                    }
            }
        }

        private void CreateAndAssignIrregularTransverseSupportGroupAndLTBsegmentGroup(
            bool bUseFlyBracing,
            int iFlyBracing_Every_XXSupportingMember,
            float fFirstSupportingMemberPositionAbsolute,
            float fSupportingMembersDistance,
            ref CMember member)
        {
            List<CIntermediateTransverseSupport> lTransverseSupportGroup_Member;
            List<CSegment_LTB> LTB_segment_group_Member;

            CreateIrregularTransverseSupportGroupAndLTBsegmentGroup(
                bUseFlyBracing,
                iFlyBracing_Every_XXSupportingMember,
                fFirstSupportingMemberPositionAbsolute,
                fSupportingMembersDistance,
                ref member,
                out lTransverseSupportGroup_Member,
                out LTB_segment_group_Member);

            if (lTransverseSupportGroup_Member != null && lTransverseSupportGroup_Member.Count > 0)
            {
                // Assign transverse support group to the member
                member.IntermediateTransverseSupportGroup = lTransverseSupportGroup_Member;
                member.LTBSegmentGroup = LTB_segment_group_Member;
            }
        }

        private void CreateAndAssignReversedIrregularTransverseSupportGroupAndLTBsegmentGroup(
            bool bUseFlyBracing,
            int iFlyBracing_Every_XXSupportingMember,
            float fFirstSupportingMemberPositionAbsolute,
            float fSupportingMembersDistance,
            ref CMember member)
        {
            // Suradnice pre member opacne orientovane, takze pouzit L - x
            float fmemberLength = member.FLength;
            List<CIntermediateTransverseSupport> lTransverseSupportGroup_Member = new List<CIntermediateTransverseSupport>();
            List<CSegment_LTB> LTB_segment_group_Member = new List<CSegment_LTB>();
            CreateIrregularTransverseSupportGroupAndLTBsegmentGroup(bUseFlyBracing, iFlyBracing_Every_XXSupportingMember, fBottomGirtPosition, fDist_Girt, ref member, out lTransverseSupportGroup_Member, out LTB_segment_group_Member);

            // Reverse ILS and LTB segments
            List<CIntermediateTransverseSupport> lTransverseSupportGroup_Member_Reverse = new List<CIntermediateTransverseSupport>();
            List<CSegment_LTB> LTB_segment_group_Member_Reverse = new List<CSegment_LTB>();

            if (lTransverseSupportGroup_Member != null && lTransverseSupportGroup_Member.Count != 0) // List is initialized and some intermediate supports exists
            {
                for (int j = 0; j < lTransverseSupportGroup_Member.Count; j++)
                {
                    int iSupportIndex = lTransverseSupportGroup_Member.Count - j - 1;
                    CIntermediateTransverseSupport lTransverseSupport = new CIntermediateTransverseSupport(j + 1, lTransverseSupportGroup_Member[iSupportIndex].Type, 1f - lTransverseSupportGroup_Member[iSupportIndex].Fx_position_rel, fmemberLength - lTransverseSupportGroup_Member[iSupportIndex].Fx_position_abs);
                    lTransverseSupportGroup_Member_Reverse.Add(lTransverseSupport);
                }

                LTB_segment_group_Member_Reverse = GenerateIntermediateLTBSegmentsOnMember(lTransverseSupportGroup_Member_Reverse, false, fmemberLength);

                member.IntermediateTransverseSupportGroup = lTransverseSupportGroup_Member_Reverse;
                member.LTBSegmentGroup = LTB_segment_group_Member_Reverse;
            }
        }

        private void CreateJoints(bool bGenerateGirts, bool bUseMainColumnFlyBracingPlates, bool bGeneratePurlins, bool bUseRafterFlyBracingPlates, 
            bool bGenerateFrontColumns, bool bGenerateBackColumns, bool bGenerateFrontGirts, bool bGenerateBackGirts,
            bool bGenerateGirtBracingSideWalls, bool bGeneratePurlinBracing, bool bGenerateGirtBracingFrontSide, bool bGenerateGirtBracingBackSide, bool bWindPostUnderRafter)
        {
            // Connection Joints
            m_arrConnectionJoints = new List<CConnectionJointTypes>();

            // Frame Main Column Joints to Foundation
            for (int i = 0; i < iFrameNo; i++)
            {
                m_arrConnectionJoints.Add(new CConnectionJoint_TA01(m_arrNodes[i * iFrameNodesNo + 0], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 0], true));
                m_arrConnectionJoints.Add(new CConnectionJoint_TA01(m_arrNodes[i * iFrameNodesNo + 4], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 3], true));
            }

            float ft_rafter_joint_plate = 0.003f; // m

            // Frame Rafter Joints
            for (int i = 0; i < iFrameNo; i++)
            {
                float fRafterDepth = (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 1].CrScStart.h;
                float fa = fRafterDepth * (float)Math.Tan(fRoofPitch_rad);
                float fRafterPart = fRafterDepth + fa;
                float fbPlateHalf = fRafterPart * (float)Math.Cos(fRoofPitch_rad);
                float fPlateWidth = 2 * fbPlateHalf;

                if (i == 0 || i == (iFrameNo - 1)) // Front or Last Frame
                    m_arrConnectionJoints.Add(new CConnectionJoint_A001(m_arrNodes[i * 5 + 2], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 1], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 2], fRoofPitch_rad, fPlateWidth, ft_rafter_joint_plate, i == 0 ? fFrontFrameRakeAngle_deg : fBackFrameRakeAngle_deg, true));
                else //if(i< (iFrameNo - 1) // Intermediate frame

                    m_arrConnectionJoints.Add(new CConnectionJoint_A001(m_arrNodes[i * 5 + 2], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 1], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 2], fRoofPitch_rad, fPlateWidth, ft_rafter_joint_plate, 0, true));
            }

            float ft_knee_joint_plate = 0.003f; // m

            // Knee Joints 1
            for (int i = 0; i < iFrameNo; i++)
            {
                if (i == 0 || i == (iFrameNo - 1)) // Front or Last Frame
                    m_arrConnectionJoints.Add(new CConnectionJoint_B001(m_arrNodes[i * 5 + 1], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 1], fRoofPitch_rad, 1.1f * (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4].CrScStart.h, 2 * (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 1].CrScStart.h, ft_knee_joint_plate, ft_rafter_joint_plate, i == 0 ? fFrontFrameRakeAngle_deg : fBackFrameRakeAngle_deg, true));
                else //if(i< (iFrameNo - 1) // Intermediate frame
                    m_arrConnectionJoints.Add(new CConnectionJoint_B001(m_arrNodes[i * 5 + 1], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 1], fRoofPitch_rad, 1.1f * (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4].CrScStart.h, 2 * (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 1].CrScStart.h, ft_knee_joint_plate, ft_rafter_joint_plate, i == 0 ? fFrontFrameRakeAngle_deg : fBackFrameRakeAngle_deg, true));
            }

            // Knee Joints 2
            for (int i = 0; i < iFrameNo; i++)
            {
                if (i == 0 || i == (iFrameNo - 1)) // Front or Last Frame
                    m_arrConnectionJoints.Add(new CConnectionJoint_B001(m_arrNodes[i * 5 + 3], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 3], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 2], fRoofPitch_rad, 1.1f * (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 3].CrScStart.h, 2 * (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 2].CrScStart.h, ft_knee_joint_plate, ft_rafter_joint_plate, i == 0 ? fFrontFrameRakeAngle_deg : fBackFrameRakeAngle_deg, true));
                else //if(i< (iFrameNo - 1) // Intermediate frame
                    m_arrConnectionJoints.Add(new CConnectionJoint_B001(m_arrNodes[i * 5 + 3], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 3], m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 2], fRoofPitch_rad, 1.1f * (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 3].CrScStart.h, 2 * (float)m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * 4 + 2].CrScStart.h, ft_knee_joint_plate, ft_rafter_joint_plate, i == 0 ? fFrontFrameRakeAngle_deg : fBackFrameRakeAngle_deg, true));
            }

            // Eaves Purlin Joints
            if (iEavesPurlinNo > 0)
            {
                for (int i = 0; i < iEavesPurlinNo / iEavesPurlinNoInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 4];
                    //m_arrConnectionJoints.Add(new CConnectionJoint_C001(current_member.NodeStart, m_arrMembers[0], current_member, true));
                    //m_arrConnectionJoints.Add(new CConnectionJoint_C001(current_member.NodeEnd, m_arrMembers[0], current_member, true));

                    // Eave purlin is connected to the rafter - index i+1 and i+2
                    m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 1], current_member, ft_knee_joint_plate, EPlateNumberAndPositionInJoint.eOneRightPlate, true));
                    m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, m_arrMembers[((i+1) * iEavesPurlinNoInOneFrame) + (i + 1) * (iFrameNodesNo - 1) + 1], current_member, ft_knee_joint_plate, EPlateNumberAndPositionInJoint.eOneRightPlate, true));

                    current_member = m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 5];
                    m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, m_arrMembers[(i * iEavesPurlinNoInOneFrame) + i * (iFrameNodesNo - 1) + 2], current_member, ft_knee_joint_plate, EPlateNumberAndPositionInJoint.eOneRightPlate, true));
                    m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, m_arrMembers[((i+1) * iEavesPurlinNoInOneFrame) + (i + 1) * (iFrameNodesNo - 1) + 2], current_member, ft_knee_joint_plate, EPlateNumberAndPositionInJoint.eOneRightPlate, true));
                }
            }

            // Girt Joints
            if (bGenerateGirts)
            {
                for (int i = 0; i < (iFrameNo - 1) * iGirtNoInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + i];
                    CMember mainMemberForStartJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeStart));
                    CMember mainMemberForEndJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeEnd));

                    int iCurrentFrameIndex = i / iGirtNoInOneFrame;
                    int iFirstGirtInFrameLeftSide = iMainColumnNo + iRafterNo + iEavesPurlinNo + iCurrentFrameIndex * iGirtNoInOneFrame;
                    int iFirstGirtInFrameRightSide = iFirstGirtInFrameLeftSide + iGirtNoInOneFrame / 2;
                    int iCurrentMemberIndex = iMainColumnNo + iRafterNo + iEavesPurlinNo + i;

                    int iFirstGirtOnCurrentSideIndex;
                    if (iCurrentMemberIndex < iFirstGirtInFrameRightSide)
                        iFirstGirtOnCurrentSideIndex = iFirstGirtInFrameLeftSide;
                    else
                        iFirstGirtOnCurrentSideIndex = iFirstGirtInFrameRightSide;

                    if (bUseMainColumnFlyBracingPlates && iMainColumnFlyBracing_EveryXXGirt > 0 && (iCurrentMemberIndex - iFirstGirtOnCurrentSideIndex + 1) % iMainColumnFlyBracing_EveryXXGirt == 0)
                    {
                        bool bTopOfPlateInCrscVerticalAxisPlusDirection = false;

                        if (iFirstGirtInFrameRightSide <= iCurrentMemberIndex && iCurrentMemberIndex < iFirstGirtOnCurrentSideIndex + iGirtNoInOneFrame / 2)
                            bTopOfPlateInCrscVerticalAxisPlusDirection = true;

                        m_arrConnectionJoints.Add(new CConnectionJoint_T003("FB - LH", "FB - RH", current_member.NodeStart, mainMemberForStartJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, bTopOfPlateInCrscVerticalAxisPlusDirection, true));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T003("FB - LH", "FB - RH", current_member.NodeEnd, mainMemberForEndJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, bTopOfPlateInCrscVerticalAxisPlusDirection, true));
                    }
                    else
                    {
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, mainMemberForStartJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, mainMemberForEndJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                    }
                }
            }

            // Purlin Joints
            if (bGeneratePurlins)
            {
                for (int i = 0; i < (iFrameNo - 1) * iPurlinNoInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + i];
                    CMember mainMemberForStartJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeStart));
                    CMember mainMemberForEndJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeEnd));

                    int iCurrentFrameIndex = i / iPurlinNoInOneFrame;
                    int iFirstPurlinInFrameLeftSide = iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + iCurrentFrameIndex * iPurlinNoInOneFrame;
                    int iFirstPurlinInFrameRightSide = iFirstPurlinInFrameLeftSide + iPurlinNoInOneFrame / 2;
                    int iCurrentMemberIndex = iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + i;

                    int iFirstPurlinOnCurrentSideIndex;
                    if (iCurrentMemberIndex < iFirstPurlinInFrameRightSide)
                        iFirstPurlinOnCurrentSideIndex = iFirstPurlinInFrameLeftSide;
                    else
                        iFirstPurlinOnCurrentSideIndex = iFirstPurlinInFrameRightSide;

                    if (bUseRafterFlyBracingPlates && iRafterFlyBracing_EveryXXPurlin > 0 && (iCurrentMemberIndex - iFirstPurlinOnCurrentSideIndex + 1) % iRafterFlyBracing_EveryXXPurlin == 0)
                    {
                        m_arrConnectionJoints.Add(new CConnectionJoint_T003("FB - LH", "FB - RH", current_member.NodeStart, mainMemberForStartJoint, current_member, ft_knee_joint_plate, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T003("FB - LH", "FB - RH", current_member.NodeEnd, mainMemberForEndJoint, current_member, ft_knee_joint_plate, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
                    }
                    else
                    {
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, mainMemberForStartJoint, current_member, ft_knee_joint_plate, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, mainMemberForEndJoint, current_member, ft_knee_joint_plate, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                    }
                }
            }

            // Front Columns Foundation Joints / Top Joint to the rafter
            if (bGenerateFrontColumns)
            {
                for (int i = 0; i < iFrontColumnNoInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + i];
                    m_arrConnectionJoints.Add(new CConnectionJoint_TB01(current_member.NodeStart, current_member, true));

                    if (i < (int)(iFrontColumnNoInOneFrame / 2))
                        m_arrConnectionJoints.Add(new CConnectionJoint_S001(current_member.NodeEnd, m_arrMembers[1], current_member, -fRoofPitch_rad, bWindPostUnderRafter, true, true)); // Front Left Main Rafter (0 to 0.5*W)
                    else
                        m_arrConnectionJoints.Add(new CConnectionJoint_S001(current_member.NodeEnd, m_arrMembers[2], current_member, fRoofPitch_rad, bWindPostUnderRafter, true, true)); // Front Right Main Rafter(0.5*W to W)
                }
            }

            // Back Columns Foundation Joints / Top Joint to the rafter
            if (bGenerateBackColumns)
            {
                for (int i = 0; i < iBackColumnNoInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + i];
                    m_arrConnectionJoints.Add(new CConnectionJoint_TB01(current_member.NodeStart, current_member, true));

                    if (i < (int)(iBackColumnNoInOneFrame / 2))
                        m_arrConnectionJoints.Add(new CConnectionJoint_S001(current_member.NodeEnd, m_arrMembers[(iFrameNo - 1) * 6 + 1], current_member, fRoofPitch_rad, bWindPostUnderRafter, false, true)); // Back Left Main Rafter (0 to 0.5*W)
                    else
                        m_arrConnectionJoints.Add(new CConnectionJoint_S001(current_member.NodeEnd, m_arrMembers[(iFrameNo - 1) * 6 + 2], current_member, -fRoofPitch_rad, bWindPostUnderRafter, false, true)); // Back Right Main Rafter(0.5*W to W)
                }
            }

            // Front Girt Joints
            if (bGenerateFrontGirts)
            {
                for (int i = 0; i < iFrontGirtsNoInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame + i];
                    //m_arrConnectionJoints.Add(new CConnectionJoint_J001(current_member.NodeStart, m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame], current_member, true));
                    //m_arrConnectionJoints.Add(new CConnectionJoint_J001(current_member.NodeEnd, m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame], current_member, true));

                    // Number of girts in the middle
                    int iNumberOfGirtsInMiddle = iArrNumberOfNodesPerFrontColumn[iOneRafterFrontColumnNo - 1];
                    // Number of girts without middle
                    int iNumberOfSymmetricalGirts = iFrontGirtsNoInOneFrame - iNumberOfGirtsInMiddle;
                    // Number of girts in the half of frame (without middle)
                    int iNumberOfSymmetricalGirtsHalf = iNumberOfSymmetricalGirts / 2;

                    // Joint at member start - connected to the first main column
                    if (i < iGirtNoInOneFrame / 2) // First column of girts are connected to the first main column
                    {
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, m_arrMembers[0], current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true)); // Use height (z dimension)
                    }
                    else if ((iNumberOfSymmetricalGirtsHalf + iNumberOfGirtsInMiddle - 1) < i && i < (iNumberOfSymmetricalGirtsHalf + iNumberOfGirtsInMiddle + iOneColumnGirtNo)) // Joint at member start - connected to the second main column
                    {
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, m_arrMembers[3], current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true)); // Use height (z dimension)
                    }
                    else
                    {
                        CMember mainMemberForStartJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeStart));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, mainMemberForStartJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                    }

                    // Joint at member end
                    CMember mainMemberForEndJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeEnd));
                    m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, mainMemberForEndJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                }
            }

            // Back Girt Joints
            if (bGenerateBackGirts)
            {
                for (int i = 0; i < iBackGirtsNoInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame + iFrontGirtsNoInOneFrame + i];
                    //m_arrConnectionJoints.Add(new CConnectionJoint_L001(current_member.NodeStart, m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame], current_member, true));
                    //m_arrConnectionJoints.Add(new CConnectionJoint_L001(current_member.NodeEnd, m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame], current_member, true));

                    // Number of girts in the middle
                    int iNumberOfGirtsInMiddle = iArrNumberOfNodesPerBackColumn[iOneRafterBackColumnNo - 1];
                    // Number of girts without middle
                    int iNumberOfSymmetricalGirts = iBackGirtsNoInOneFrame - iNumberOfGirtsInMiddle;
                    // Number of girts in the half of frame (without middle)
                    int iNumberOfSymmetricalGirtsHalf = iNumberOfSymmetricalGirts / 2;

                    // Joint at member start - connected to the first main column
                    if (i < iGirtNoInOneFrame / 2) // First column of girts are connected to the first main column
                    {
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, m_arrMembers[((iFrameNo - 1) * iEavesPurlinNoInOneFrame) + (iFrameNo - 1) * (iFrameNodesNo - 1)], current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true)); // Use height (z dimension)
                    }
                    else if ((iNumberOfSymmetricalGirtsHalf + iNumberOfGirtsInMiddle - 1) < i && i < (iNumberOfSymmetricalGirtsHalf + iNumberOfGirtsInMiddle + iOneColumnGirtNo)) // Joint at member start - connected to the second main column
                    {
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, m_arrMembers[((iFrameNo - 1) * iEavesPurlinNoInOneFrame) + (iFrameNo - 1) * (iFrameNodesNo - 1) + 3], current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true)); // Use height (z dimension)
                    }
                    else
                    {
                        CMember mainMemberForStartJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeStart));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, mainMemberForStartJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                    }

                    CMember mainMemberForEndJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeEnd));
                    m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, mainMemberForEndJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                }
            }

            // Girt Bracing - Side walls - Joints
            if (bGenerateGirtBracingSideWalls)
            {
                for (int i = 0; i < iGBSideWallsMembersNo; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame + iFrontGirtsNoInOneFrame + iBackGirtsNoInOneFrame + i];

                    //To Mato - toto je podla mna problem preco niekedy pada metoda na disablovanie member Joints - lebo sa proste tie joints pre vypnuty member proste vobec negeneruju
                    // vo vysledku teda musime stale pregenerovat joints, aby to sedelo
                    if (current_member.BIsGenerated)
                    {
                        // Joint at member start
                        CMember mainMemberForStartJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeStart));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, mainMemberForStartJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));

                        // Joint at member end
                        CMember mainMemberForEndJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeEnd));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, mainMemberForEndJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                    }
                }
            }

            // Purlin Bracing - Joints
            if (bGeneratePurlinBracing)
            {
                for (int i = 0; i < iPBMembersNo; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame + iFrontGirtsNoInOneFrame + iBackGirtsNoInOneFrame + iGBSideWallsMembersNo + i];

                    //To Mato - toto je podla mna problem preco niekedy pada metoda na disablovanie member Joints - lebo sa proste tie joints pre vypnuty member proste vobec negeneruju
                    // vo vysledku teda musime stale pregenerovat joints, aby to sedelo
                    if (current_member.BIsGenerated)
                    {
                        // Joint at member start
                        CMember mainMemberForStartJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeStart));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, mainMemberForStartJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));

                        // Joint at member end
                        CMember mainMemberForEndJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeEnd));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, mainMemberForEndJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                    }
                }
            }

            // Girt Bracing - Front Side - Joints
            if (bGenerateGirtBracingFrontSide)
            {
                for (int i = 0; i < iNumberOfGB_FSMembersInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame + iFrontGirtsNoInOneFrame + iBackGirtsNoInOneFrame + iGBSideWallsMembersNo + iPBMembersNo + i];

                    //To Mato - toto je podla mna problem preco niekedy pada metoda na disablovanie member Joints - lebo sa proste tie joints pre vypnuty member proste vobec negeneruju
                    // vo vysledku teda musime stale pregenerovat joints, aby to sedelo
                    if (current_member.BIsGenerated)
                    {
                        // Joint at member start
                        CMember mainMemberForStartJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeStart));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, mainMemberForStartJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));

                        // Joint at member end
                        CMember mainMemberForEndJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeEnd));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, mainMemberForEndJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                    }
                }
            }

            // Girt Bracing - Back Side - Joints
            if (bGenerateGirtBracingBackSide)
            {
                for (int i = 0; i < iNumberOfGB_BSMembersInOneFrame; i++)
                {
                    CMember current_member = m_arrMembers[iMainColumnNo + iRafterNo + iEavesPurlinNo + (iFrameNo - 1) * iGirtNoInOneFrame + (iFrameNo - 1) * iPurlinNoInOneFrame + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame + iFrontGirtsNoInOneFrame + iBackGirtsNoInOneFrame + iGBSideWallsMembersNo + iPBMembersNo + iNumberOfGB_FSMembersInOneFrame + i];

                    //To Mato - toto je podla mna problem preco niekedy pada metoda na disablovanie member Joints - lebo sa proste tie joints pre vypnuty member proste vobec negeneruju
                    // vo vysledku teda musime stale pregenerovat joints, aby to sedelo
                    if (current_member.BIsGenerated)
                    {
                        // Joint at member start
                        CMember mainMemberForStartJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeStart));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, mainMemberForStartJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));

                        // Joint at member end
                        CMember mainMemberForEndJoint = m_arrMembers.FirstOrDefault(m => m.IntermediateNodes.Contains(current_member.NodeEnd));
                        m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, mainMemberForEndJoint, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true));
                    }
                }
            }

            // Validation - check that all created joints have assigned Main Member
            // Check all joints before definition of doors and windows members and joints
            for (int i = 0; i < m_arrConnectionJoints.Count; i++)
            {
                if (m_arrConnectionJoints[i].m_MainMember == null)
                    throw new ArgumentNullException("Main member is not assigned to the joint No.:" + m_arrConnectionJoints[i].ID.ToString() + " Joint index in the list: " + i);
            }

            // Validation - duplicity of node ID
            for (int i = 0; i < m_arrNodes.Length; i++)
            {
                for (int j = 0; j < m_arrNodes.Length; j++)
                {
                    if ((m_arrNodes[i] != m_arrNodes[j]) && (m_arrNodes[i].ID == m_arrNodes[j].ID))
                        throw new ArgumentNullException("Duplicity in Node ID.\nNode index: " + i + " and Node index: " + j);
                }
            }
        }

        public void DeactivateMember(ref CMember m)
        {
            if (m != null)
            {
                m.BIsGenerated = false;
                m.BIsDisplayed = false;
                m.BIsSelectedForIFCalculation = false;
                m.BIsSelectedForDesign = false;
                m.BIsSelectedForMaterialList = false;
            }
        }

        public void DeactivateJoint(ref CConnectionJointTypes j)
        {
            if (j != null)
            {
                j.BIsGenerated = false;
                j.BIsDisplayed = false;
                j.BIsSelectedForIFCalculation = false;
                j.BIsSelectedForDesign = false;
                j.BIsSelectedForMaterialList = false;
            }
        }

        public void DeactivateMemberAndItsJoints(ref CMember m)
        {
            DeactivateMember(ref m);

            try
            {
                CConnectionJointTypes jStart;
                CConnectionJointTypes jEnd;
                GetModelMemberStartEndConnectionJoints(m, out jStart, out jEnd);

                DeactivateJoint(ref jStart);
                DeactivateJoint(ref jEnd);
            }
            catch(Exception ex)
            {
                bool debugging = true;
                if (debugging) System.Diagnostics.Trace.WriteLine($"DeactivateMemberAndItsJoints: Error: [{ex.Message}] m.ID:{m.ID}  Prefix: {m.Prefix}");

            };
        }
        public void DeactivateMemberAndItsJoints(CMember m)
        {
            DeactivateMemberAndItsJoints(ref m);
        }

        private void CreateFoundations(bool bGenerateFrontColumns, bool bGenerateBackColumns, bool bIsReinforcementBarStraight)
        {
            bool bGenerateFoundations = true;
            //bool bIsReinforcementBarStraight = false; // Nastavime bool, aky typ vyztuze chceme vytvorit

            if (bGenerateFoundations)
            {
                // Docasne
                CMat_02_00 materialConcrete = new CMat_02_00();
                materialConcrete.Name = "25";
                materialConcrete.Fck = 25e+6f;
                materialConcrete.m_fRho = 2300f;
                materialConcrete.m_fE = 30e+9f;

                DATABASE.DTO.CMatPropertiesRC concreteProperties = DATABASE.CMaterialManager.LoadMaterialPropertiesRC(materialConcrete.Name);
                materialConcrete.Fck = concreteProperties.Fc;
                materialConcrete.m_fRho = (float)concreteProperties.Rho;
                materialConcrete.m_fE = (float)concreteProperties.E;

                // Foundations
                // Footings
                m_arrFoundations = new List<CFoundation>(iMainColumnNo + iFrontColumnNoInOneFrame + iBackColumnNoInOneFrame);

                // Main Column - Footings
                // TODO - Predbezne doporucene hodnoty velkosti zakladov vypocitane z rozmerov budovy
                // UC - Footings
                float fMainColumnFooting_aX = (float)Math.Round(MathF.Max(0.6f, Math.Min(fW_frame * 0.07f, fL1_frame * 0.35f)), 1);
                float fMainColumnFooting_bY = (float)Math.Round(MathF.Max(0.7f, Math.Min(fW_frame * 0.08f, fL1_frame * 0.40f)), 1);
                float fMainColumnFooting_h = 0.45f; // "AS 2870 - Footing pad size must be between 0.45 and 2 [m]" // TODO napojit na tabulku normy
                float fConcreteCover = 0.075f; // Concrete Cover - UC - Footings

                CReinforcementBar MainColumnFootingReference_Top_Bar_x;
                CReinforcementBar MainColumnFootingReference_Top_Bar_y;
                CReinforcementBar MainColumnFootingReference_Bottom_Bar_x;
                CReinforcementBar MainColumnFootingReference_Bottom_Bar_y;

                int iMainColumnFootingNumberOfBarsTop_x;
                int iMainColumnFootingNumberOfBarsTop_y;
                int iMainColumnFootingNumberOfBarsBottom_x;
                int iMainColumnFootingNumberOfBarsBottom_y;

                CreateReferenceReinforcementBars(
                    bIsReinforcementBarStraight,
                    fMainColumnFooting_aX,
                    fMainColumnFooting_bY,
                    fMainColumnFooting_h,
                    out MainColumnFootingReference_Top_Bar_x,
                    out MainColumnFootingReference_Top_Bar_y,
                    out MainColumnFootingReference_Bottom_Bar_x,
                    out MainColumnFootingReference_Bottom_Bar_y,
                    out iMainColumnFootingNumberOfBarsTop_x,
                    out iMainColumnFootingNumberOfBarsTop_y,
                    out iMainColumnFootingNumberOfBarsBottom_x,
                    out iMainColumnFootingNumberOfBarsBottom_y
                    );

                for (int i = 0; i < iFrameNo; i++)
                {
                    float fMainColumnFooting_Eccentricity_x = 0f;
                    float fMainColumnFooting_Eccentricity_y = 0.5f * (fMainColumnFooting_bY - (float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn].h);

                    EMemberType_FS_Position columnTypePosition = EMemberType_FS_Position.MainColumn;
                    string sName = "A";
                    string sDescriptionText = "PAD TYPE A [MC]";
                    //Color color = Colors.LightSkyBlue;

                    if (i == 0 || i == (iFrameNo - 1)) // First or last frame
                    {
                        columnTypePosition = EMemberType_FS_Position.EdgeColumn;
                        sName = "B";
                        sDescriptionText = "PAD TYPE B [EC]";
                        fMainColumnFooting_Eccentricity_y = 0.5f * (fMainColumnFooting_bY - (float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn_EF].h);

                        // Zistenie hrubky base plate v spoji
                        float fBasePlateThickness = 0.003f;
                        if (m_arrConnectionJoints != null)
                           fBasePlateThickness = (m_arrConnectionJoints.Find(x => x.m_MainMember.EMemberTypePosition == columnTypePosition)).m_arrPlates[0].Ft;

                        // Kedze kotevny plech sa je pripojeny z vonkajsej strany stlpa je potrebne pocitat pri excentricite s tymto rozmerom a zmmensit ju

                        // Front side edge frame columns
                        fMainColumnFooting_Eccentricity_x = 0.5f * (fMainColumnFooting_aX - (float)m_arrCrSc[(int)EMemberGroupNames.eMainColumn].b - 2 * fBasePlateThickness);

                        // Back side edge frame columns
                        if (i == (iFrameNo - 1)) 
                            fMainColumnFooting_Eccentricity_x *= -1;

                        //color = Colors.LightSteelBlue;
                    }

                    // Left
                    CNode node_left = m_arrNodes[i * iFrameNodesNo + 0];
                    //Point3D controlPoint_left = new Point3D(i * 2 + 1, node_left.X - 0.5f * fMainColumnFooting_aX, node_left.Y - 0.5f * fMainColumnFooting_bY, node_left.Z - fMainColumnFooting_h, 0);
                    m_arrFoundations.Add(new CFoundation(i * 2 + 1,
                        EFoundationType.ePad,
                        node_left,
                        materialConcrete,
                        columnTypePosition,
                        sName,
                        sDescriptionText,
                        //controlPoint_left,
                        fMainColumnFooting_aX, 
                        fMainColumnFooting_bY,
                        fMainColumnFooting_h,
                        0,
                        0,
                        fMainColumnFooting_Eccentricity_x,
                        -fMainColumnFooting_Eccentricity_y,
                        90,
                        fConcreteCover,
                        MainColumnFootingReference_Top_Bar_x,
                        MainColumnFootingReference_Top_Bar_y,
                        MainColumnFootingReference_Bottom_Bar_x,
                        MainColumnFootingReference_Bottom_Bar_y,
                        iMainColumnFootingNumberOfBarsTop_x,
                        iMainColumnFootingNumberOfBarsTop_y,
                        iMainColumnFootingNumberOfBarsBottom_x,
                        iMainColumnFootingNumberOfBarsBottom_y,
                        //color,
                        0.5f,
                        true,
                        0));

                    // Right
                    CNode node_right = m_arrNodes[i * iFrameNodesNo + 4];
                    //Point3D controlPoint_right = new Point3D(i * 2 + 2, node_right.X - 0.5f * fMainColumnFooting_aX, node_right.Y - 0.5f * fMainColumnFooting_bY, node_right.Z - fMainColumnFooting_h, 0);
                    m_arrFoundations.Add(new CFoundation(i * 2 + 2,
                        EFoundationType.ePad,
                        node_right,
                        materialConcrete,
                        columnTypePosition,
                        sName,
                        sDescriptionText,
                        //controlPoint_right,
                        fMainColumnFooting_aX,
                        fMainColumnFooting_bY,
                        fMainColumnFooting_h,
                        0,
                        0,
                        -fMainColumnFooting_Eccentricity_x,
                        -fMainColumnFooting_Eccentricity_y,
                        270,
                        fConcreteCover,
                        MainColumnFootingReference_Top_Bar_x,
                        MainColumnFootingReference_Top_Bar_y,
                        MainColumnFootingReference_Bottom_Bar_x,
                        MainColumnFootingReference_Bottom_Bar_y,
                        iMainColumnFootingNumberOfBarsTop_x,
                        iMainColumnFootingNumberOfBarsTop_y,
                        iMainColumnFootingNumberOfBarsBottom_x,
                        iMainColumnFootingNumberOfBarsBottom_y,
                        //color,
                        0.5f,
                        true,
                        0));
                }

                int iLastFoundationIndex = iMainColumnNo;

                // Front and Back Wall Columns - Footings
                if (bGenerateFrontColumns)
                {
                    float fFrontColumnFooting_aX = (float)Math.Round(MathF.Max(0.5f, fDist_FrontColumns * 0.40f), 1);
                    float fFrontColumnFooting_bY = (float)Math.Round(MathF.Max(0.5f, fDist_FrontColumns * 0.40f), 1);
                    float fFrontColumnFooting_h = 0.45f; // "AS 2870 - Footing pad size must be between 0.45 and 2 [m]" // TODO napojit na tabulku normy

                    float fFrontColumnFooting_Eccentricity_y = 0.5f * (fFrontColumnFooting_bY - (float)m_arrCrSc[(int)EMemberGroupNames.eFrontColumn].h);

                    CReinforcementBar FrontColumnFootingReference_Top_Bar_x;
                    CReinforcementBar FrontColumnFootingReference_Top_Bar_y;
                    CReinforcementBar FrontColumnFootingReference_Bottom_Bar_x;
                    CReinforcementBar FrontColumnFootingReference_Bottom_Bar_y;

                    int iFrontColumnFootingNumberOfBarsTop_x;
                    int iFrontColumnFootingNumberOfBarsTop_y;
                    int iFrontColumnFootingNumberOfBarsBottom_x;
                    int iFrontColumnFootingNumberOfBarsBottom_y;

                    CreateReferenceReinforcementBars(
                        bIsReinforcementBarStraight,
                        fFrontColumnFooting_aX,
                        fFrontColumnFooting_bY,
                        fFrontColumnFooting_h,
                        out FrontColumnFootingReference_Top_Bar_x,
                        out FrontColumnFootingReference_Top_Bar_y,
                        out FrontColumnFootingReference_Bottom_Bar_x,
                        out FrontColumnFootingReference_Bottom_Bar_y,
                        out iFrontColumnFootingNumberOfBarsTop_x,
                        out iFrontColumnFootingNumberOfBarsTop_y,
                        out iFrontColumnFootingNumberOfBarsBottom_x,
                        out iFrontColumnFootingNumberOfBarsBottom_y
                        );

                    EMemberType_FS_Position columnTypePosition = EMemberType_FS_Position.ColumnFrontSide;
                    string sName = "C-FRONT";
                    string sDescriptionText = "PAD TYPE C [WP-FRONT]";

                    // Search footings control points
                    List<CNode> listOfControlPoints = new List<CNode>();
                    for (int i = 0; i < m_arrMembers.Length; i++)
                    {
                        // Find foundation definition nodes
                        if (MathF.d_equal(m_arrMembers[i].NodeStart.Z, 0) &&
                            m_arrMembers[i].EMemberType == EMemberType_FS.eC &&
                            m_arrMembers[i].CrScStart.Equals(listOfModelMemberGroups[(int)EMemberGroupNames.eFrontColumn].CrossSection))
                            listOfControlPoints.Add(m_arrMembers[i].NodeStart);
                    }

                    for (int i = 0; i < listOfControlPoints.Count; i++)
                    {
                        //Point3D controlPoint = new Point3D(iLastFoundationIndex + i + 1, listOfControlPoints[i].X - 0.5f * fFrontColumnFooting_aX, listOfControlPoints[i].Y - 0.5f * fFrontColumnFooting_bY, listOfControlPoints[i].Z - fFrontColumnFooting_h, 0);
                        m_arrFoundations.Add(new CFoundation(iLastFoundationIndex + i + 1,
                            EFoundationType.ePad,
                            listOfControlPoints[i],
                            materialConcrete,
                            columnTypePosition,
                            sName,
                            sDescriptionText,
                            //controlPoint,
                            fFrontColumnFooting_aX,
                            fFrontColumnFooting_bY,
                            fFrontColumnFooting_h,
                            0,
                            eccentricityColumnFront_Z.MFz_local,
                            0,
                            -fFrontColumnFooting_Eccentricity_y,
                            180,
                            fConcreteCover,
                            FrontColumnFootingReference_Top_Bar_x,
                            FrontColumnFootingReference_Top_Bar_y,
                            FrontColumnFootingReference_Bottom_Bar_x,
                            FrontColumnFootingReference_Bottom_Bar_y,
                            iFrontColumnFootingNumberOfBarsTop_x,
                            iFrontColumnFootingNumberOfBarsTop_y,
                            iFrontColumnFootingNumberOfBarsBottom_x,
                            iFrontColumnFootingNumberOfBarsBottom_y,
                            //Colors.LightSeaGreen,
                            0.5f,
                            true,
                            0));
                    }

                    iLastFoundationIndex += listOfControlPoints.Count;
                }

                if (bGenerateBackColumns)
                {
                    float fBackColumnFooting_aX = (float)Math.Round(MathF.Max(0.5f, fDist_BackColumns * 0.40f), 1);
                    float fBackColumnFooting_bY = (float)Math.Round(MathF.Max(0.5f, fDist_BackColumns * 0.40f), 1);
                    float fBackColumnFooting_h = 0.45f; // "AS 2870 - Footing pad size must be between 0.45 and 2 [m]" // TODO napojit na tabulku normy

                    float fBackColumnFooting_Eccentricity_y = 0.5f * (fBackColumnFooting_bY - (float)m_arrCrSc[(int)EMemberGroupNames.eBackColumn].h);

                    CReinforcementBar BackColumnFootingReference_Top_Bar_x;
                    CReinforcementBar BackColumnFootingReference_Top_Bar_y;
                    CReinforcementBar BackColumnFootingReference_Bottom_Bar_x;
                    CReinforcementBar BackColumnFootingReference_Bottom_Bar_y;

                    int iBackColumnFootingNumberOfBarsTop_x;
                    int iBackColumnFootingNumberOfBarsTop_y;
                    int iBackColumnFootingNumberOfBarsBottom_x;
                    int iBackColumnFootingNumberOfBarsBottom_y;

                    CreateReferenceReinforcementBars(
                        bIsReinforcementBarStraight,
                        fBackColumnFooting_aX,
                        fBackColumnFooting_bY,
                        fBackColumnFooting_h,
                        out BackColumnFootingReference_Top_Bar_x,
                        out BackColumnFootingReference_Top_Bar_y,
                        out BackColumnFootingReference_Bottom_Bar_x,
                        out BackColumnFootingReference_Bottom_Bar_y,
                        out iBackColumnFootingNumberOfBarsTop_x,
                        out iBackColumnFootingNumberOfBarsTop_y,
                        out iBackColumnFootingNumberOfBarsBottom_x,
                        out iBackColumnFootingNumberOfBarsBottom_y
                        );

                    EMemberType_FS_Position columnTypePosition = EMemberType_FS_Position.ColumnBackSide;
                    string sName = "C-BACK";
                    string sDescriptionText = "PAD TYPE C [WP-BACK]";

                    // Search footings control points
                    List<CNode> listOfControlPoints = new List<CNode>();
                    for (int i = 0; i < m_arrMembers.Length; i++)
                    {
                        // Find foundation definition nodes
                        if (MathF.d_equal(m_arrMembers[i].NodeStart.Z, 0) &&
                            m_arrMembers[i].EMemberType == EMemberType_FS.eC &&
                            m_arrMembers[i].CrScStart.Equals(listOfModelMemberGroups[(int)EMemberGroupNames.eBackColumn].CrossSection))
                            listOfControlPoints.Add(m_arrMembers[i].NodeStart);
                    }

                    for (int i = 0; i < listOfControlPoints.Count; i++)
                    {
                        //Point3D controlPoint = new Point3D(iLastFoundationIndex + i + 1, listOfControlPoints[i].X - 0.5f * fBackColumnFooting_aX, listOfControlPoints[i].Y - 0.5f * fBackColumnFooting_bY, listOfControlPoints[i].Z - fBackColumnFooting_h, 0);
                        m_arrFoundations.Add(new CFoundation(iLastFoundationIndex + i + 1,
                            EFoundationType.ePad,
                            listOfControlPoints[i],
                            materialConcrete,
                            columnTypePosition,
                            sName,
                            sDescriptionText,
                            //controlPoint,
                            fBackColumnFooting_aX,
                            fBackColumnFooting_bY,
                            fBackColumnFooting_h,
                            0,
                            eccentricityColumnBack_Z.MFz_local,
                            0,
                            fBackColumnFooting_Eccentricity_y,
                            180,
                            fConcreteCover,
                            BackColumnFootingReference_Top_Bar_x,
                            BackColumnFootingReference_Top_Bar_y,
                            BackColumnFootingReference_Bottom_Bar_x,
                            BackColumnFootingReference_Bottom_Bar_y,
                            iBackColumnFootingNumberOfBarsTop_x,
                            iBackColumnFootingNumberOfBarsTop_y,
                            iBackColumnFootingNumberOfBarsBottom_x,
                            iBackColumnFootingNumberOfBarsBottom_y,
                            //Colors.Coral,
                            0.5f,
                            true,
                            0));
                    }

                    iLastFoundationIndex += listOfControlPoints.Count;
                }

                // Validation - skontroluje ci je velkost pola zhodna s poctom vygenerovanych prvkov
                if (m_arrFoundations.Count != iLastFoundationIndex)
                    throw new Exception("Incorrect number of generated foundations");
            }
        }

        private void CreateFloorSlab(bool bGenerateFrontColumns, bool bGenerateBackColumns, bool bGenerateFrontGirts, bool bGenerateBackGirts, bool bWindPostUnderRafter)
        {
            bool bGenerateSlabs = true;

            if (bGenerateSlabs)
            {
                // Docasne
                CMat_02_00 materialConcrete = new CMat_02_00();
                materialConcrete.Name = "25";
                materialConcrete.Fck = 25e+6f;
                materialConcrete.m_fRho = 2300f;
                materialConcrete.m_fE = 30e+9f;

                // Ground Floor Slab
                float fFloorSlab_AdditionalOffset_X = 0.005f; // Rozmer o ktory doska presahuje od hrany stlpa
                float fFloorSlab_AdditionalOffset_Y = 0.005f; // Rozmer o ktory doska presahuje od hrany stlpa

                float fFloorSlabOffset_x = -(float)m_arrCrSc[0].z_max - fFloorSlab_AdditionalOffset_X;
                float fFloorSlabOffset_y_Front = (float)m_arrCrSc[0].y_min - fFloorSlab_AdditionalOffset_Y;
                float fFloorSlabOffset_y_Back = (float)m_arrCrSc[0].y_max + fFloorSlab_AdditionalOffset_Y;

                float fFloorSlab_aX = fW_frame + 2 * (-fFloorSlabOffset_x);
                float fFloorSlab_bY = fL_tot + (-fFloorSlabOffset_y_Front) + fFloorSlabOffset_y_Back;
                float fFloorSlab_h = 0.125f;
                float fFloorSlab_eX = fFloorSlabOffset_x;
                float fFloorSlab_eY = fFloorSlabOffset_y_Front;

                float fConcreteCoverTop = 0.05f; // 50 mm
                string sMeshGradeName = "SE92DE";

                CReinforcementMesh mesh = new CReinforcementMesh(sMeshGradeName);

                // Saw Cuts
                CSawCut refSawCut = new CSawCut(0, new Point3D(0, 0, 0), new Point3D(1, 0, 0), 0.01f, 0.03f, true, 0);

                // Create raster of lines in XY-plane
                float fFirstSawCutPositionInDirectionX = 0;
                float fFirstSawCutPositionInDirectionY = 0;
                int iNumberOfSawCutsInDirectionX = 0;
                int iNumberOfSawCutsInDirectionY = 0;
                float fSawCutsSpacingInDirectionX = 0;
                float fSawCutsSpacingInDirectionY = 0;

                float fSawCutMaximumDistanceY = Math.Min(6, fL1_frame); // 6 m // V kazdej bay alebo maximalne 6 metrov od seba
                float fSawCutMaximumDistanceX = 0.5f * fSawCutMaximumDistanceY; // Pomer Y : X = 2:1

                if (fSawCutMaximumDistanceY < fFloorSlab_bY)
                {
                    iNumberOfSawCutsInDirectionY = (int)(fFloorSlab_bY / fSawCutMaximumDistanceY);
                    // Predpoklada sa, ze posledny saw cut je rovnako vzdialeny od konca ako prvy od zaciatku
                    fSawCutsSpacingInDirectionY = fFloorSlab_bY / (iNumberOfSawCutsInDirectionY);
                    fFirstSawCutPositionInDirectionY = fSawCutsSpacingInDirectionY / 2f;
                }

                if (fSawCutMaximumDistanceX < fFloorSlab_aX)
                {
                    iNumberOfSawCutsInDirectionX = (int)(fFloorSlab_aX / fSawCutMaximumDistanceX);
                    // Predpoklada sa, ze posledny saw cut je rovnako vzdialeny od konca ako prvy od zaciatku
                    fSawCutsSpacingInDirectionX = fFloorSlab_aX / (iNumberOfSawCutsInDirectionX);
                    fFirstSawCutPositionInDirectionX = fSawCutsSpacingInDirectionX / 2f;
                }

                // ControlJoints
                CDowel refDowel = new CDowel(new Point3D(0, 0, 0), 0.016f, 0.6f, 0.947f, true);
                CControlJoint refControlJoint = new CControlJoint(0, new Point3D(0, 0, 0), new Point3D(1, 0, 0), refDowel,0.4f, true, 0);
                // Create raster of lines in XY-plane
                float fControlJointMaximumDistance = Math.Min(24, 0.5f * fFloorSlab_bY); // 24 m alebo polovica budovy (vzdy aspon jeden joint v strede)

                float fFirstControlJointPositionInDirectionX = 0;
                float fFirstControlJointPositionInDirectionY = 0;
                int iNumberOfControlJointsInDirectionX = 0;
                int iNumberOfControlJointsInDirectionY = 0;
                float fControlJointsSpacingInDirectionX = 0;
                float fControlJointsSpacingInDirectionY = 0;

                if (fControlJointMaximumDistance < fFloorSlab_bY)
                {
                    iNumberOfControlJointsInDirectionY = (int)(fFloorSlab_bY / fControlJointMaximumDistance);
                    // Predpoklada sa, ze posledny control joint je rovnako vzdialeny od konca ako prvy od zaciatku
                    fControlJointsSpacingInDirectionY = fFloorSlab_bY / (iNumberOfControlJointsInDirectionY);
                    fFirstControlJointPositionInDirectionY = fControlJointsSpacingInDirectionY / 2f;
                }

                if (fControlJointMaximumDistance < fFloorSlab_aX)
                {
                    iNumberOfControlJointsInDirectionX = (int)(fFloorSlab_aX / fControlJointMaximumDistance);
                    // Predpoklada sa, ze posledny control joint je rovnako vzdialeny od konca ako prvy od zaciatku
                    fControlJointsSpacingInDirectionX = fFloorSlab_aX / (iNumberOfControlJointsInDirectionX);
                    fFirstControlJointPositionInDirectionX = fControlJointsSpacingInDirectionX / 2f;
                }

                float fPerimeterDepth_LRSide = 0.45f; // "AS 2870 - Size must be between 0.45 and 2 [m]"; // TODO napojit na tabulku normy
                float fPerimeterWidth_LRSide = 0.25f;
                float fStartersLapLength_LRSide = 0.6f;
                float fStartersSpacing_LRSide = 0.6f;
                float fStarters_Phi_LRSide = 0.012f;
                float fLongitud_Reinf_TopAndBotom_Phi_LRSide = 0.016f;
                float fLongitud_Reinf_Intermediate_Phi_LRSide = 0.012f;
                int fLongitud_Reinf_Intermediate_Count_LRSide = 1;

                float fRebateWidth_LRSide = 0.5f;

                float fPerimeterDepth_FBSide = 0.45f; // "AS 2870 - Size must be between 0.45 and 2 [m]"; // TODO napojit na tabulku normy
                float fPerimeterWidth_FBSide = 0.25f;
                float fStartersLapLength_FBSide = 0.6f;
                float fStartersSpacing_FBSide = 0.6f;
                float fStarters_Phi_FBSide = 0.012f;
                float fLongitud_Reinf_TopAndBotom_Phi_FBSide = 0.016f;
                float fLongitud_Reinf_Intermediate_Phi_FBSide = 0.012f;
                int fLongitud_Reinf_Intermediate_Count_FBSide = 1;

                float fRebateWidth_FBSide = 0.5f;

                m_arrSlabs = new List<CSlab>();
                m_arrSlabs.Add(new CSlab(1,
                            materialConcrete,
                            fFloorSlab_aX,
                            fFloorSlab_bY,
                            fFloorSlab_h,
                            fFloorSlab_eX,
                            fFloorSlab_eY, 0,
                            fConcreteCoverTop,
                            sMeshGradeName,
                            iNumberOfSawCutsInDirectionX,
                            iNumberOfSawCutsInDirectionY,
                            fFirstSawCutPositionInDirectionX,
                            fFirstSawCutPositionInDirectionY,
                            fSawCutsSpacingInDirectionX,
                            fSawCutsSpacingInDirectionY,
                            refSawCut,
                            iNumberOfControlJointsInDirectionX,
                            iNumberOfControlJointsInDirectionY,
                            fFirstControlJointPositionInDirectionX,
                            fFirstControlJointPositionInDirectionY,
                            fControlJointsSpacingInDirectionX,
                            fControlJointsSpacingInDirectionY,
                            refControlJoint,
                            fPerimeterDepth_LRSide,
                            fPerimeterWidth_LRSide,
                            fStartersLapLength_LRSide,
                            fStartersSpacing_LRSide,
                            fStarters_Phi_LRSide,
                            fLongitud_Reinf_TopAndBotom_Phi_LRSide,
                            fLongitud_Reinf_Intermediate_Phi_LRSide,
                            fLongitud_Reinf_Intermediate_Count_LRSide,
                            fRebateWidth_LRSide,
                            fPerimeterDepth_FBSide,
                            fPerimeterWidth_FBSide,
                            fStartersLapLength_FBSide,
                            fStartersSpacing_FBSide,
                            fStarters_Phi_FBSide,
                            fLongitud_Reinf_TopAndBotom_Phi_FBSide,
                            fLongitud_Reinf_Intermediate_Phi_FBSide,
                            fLongitud_Reinf_Intermediate_Count_FBSide,
                            fRebateWidth_FBSide,
                            DoorBlocksProperties,

                            // Temp
                            // TODO Ondrej - tieto parametre sa mi nepacia, sluzia na vypocet polohy zaciatku rebate a dlzky rebate, malo by to podla mna prist do CSlab uz nejako v ramci door properties
                            //fL1_frame, // Vzdialenost ramov
                            //fDist_FrontColumns, // Vzdialenost wind posts (stlpov v prednej stene)
                            //fDist_BackColumns, // Vzdialenost wind posts (stlpov v zadnej stene)
                            //0.14f, // Sirka cross-section typu roller door trimmer

                            //BackColumnFootingReference_Top_Bar_x,
                            //BackColumnFootingReference_Top_Bar_y,
                            //BackColumnFootingReference_Bottom_Bar_x,
                            //BackColumnFootingReference_Bottom_Bar_y,
                            //iBackColumnFootingNumberOfBarsTop_x,
                            //iBackColumnFootingNumberOfBarsTop_y,
                            //iBackColumnFootingNumberOfBarsBottom_x,
                            //iBackColumnFootingNumberOfBarsBottom_y,
                            //Colors.LightGray,
                            0.3f,
                            true,
                            0));
            }
        }

        private int GetDefaultNumberOfReinforcingBars(float footingPadWidth, float fBarDiameter, float fConcreteCover)
        {
            float fDefaultDistanceBetweenReinforcementBars = 0.15f; // 150 mm

            // Number of spacings + 1
            return (int)((footingPadWidth - 2 * fConcreteCover - 3 * fBarDiameter) / fDefaultDistanceBetweenReinforcementBars) + 1;
        }

        private void CreateReferenceReinforcementBars(
            bool bIsReinforcementBarStraight,
            float faX,
            float fbY,
            float fhZ,
            out CReinforcementBar reference_Top_Bar_x,
            out CReinforcementBar reference_Top_Bar_y,
            out CReinforcementBar reference_Bottom_Bar_x,
            out CReinforcementBar reference_Bottom_Bar_y,
            out int iNumberOfBarsTop_x,
            out int iNumberOfBarsTop_y,
            out int iNumberOfBarsBottom_x,
            out int iNumberOfBarsBottom_y,
            float fDiameterTop_Bar_x = 0.012f,
            float fDiameterTop_Bar_y = 0.012f,
            float fDiameterBottom_Bar_x = 0.012f,
            float fDiameterBottom_Bar_y = 0.012f,
            float fConcreteCover = 0.075f
            )
        {
            // For each pad recalculate lengths of reference bars
            float fLengthTop_Bar_x = bIsReinforcementBarStraight ? faX - 2 * fConcreteCover : faX - 2 * fConcreteCover - fDiameterTop_Bar_x;
            float fLengthBottom_Bar_x = bIsReinforcementBarStraight ? faX - 2 * fConcreteCover : faX - 2 * fConcreteCover - fDiameterBottom_Bar_x;
            float fLengthTop_Bar_y = bIsReinforcementBarStraight ? fbY - 2 * fConcreteCover : fbY - 2 * fConcreteCover - fDiameterTop_Bar_y;
            float fLengthBottom_Bar_y = bIsReinforcementBarStraight ? fbY - 2 * fConcreteCover : fbY - 2 * fConcreteCover - fDiameterBottom_Bar_y;

            iNumberOfBarsTop_x = GetDefaultNumberOfReinforcingBars(fbY, fDiameterTop_Bar_x, fConcreteCover);
            iNumberOfBarsTop_y = GetDefaultNumberOfReinforcingBars(faX, fDiameterTop_Bar_y, fConcreteCover);
            iNumberOfBarsBottom_x = GetDefaultNumberOfReinforcingBars(fbY, fDiameterBottom_Bar_x, fConcreteCover);
            iNumberOfBarsBottom_y = GetDefaultNumberOfReinforcingBars(faX, fDiameterBottom_Bar_y, fConcreteCover);

            // Reference / first bar coordinates
            double cp_Top_x_coordX = bIsReinforcementBarStraight ? fConcreteCover : fConcreteCover + 0.5f * fDiameterTop_Bar_x;
            double cp_Top_x_coordY = bIsReinforcementBarStraight ? fConcreteCover + 0.5f * fDiameterTop_Bar_x : fConcreteCover + fDiameterTop_Bar_y + 0.5f * fDiameterTop_Bar_x;
            double cp_Top_y_coordX = bIsReinforcementBarStraight ? fConcreteCover + 0.5f * fDiameterTop_Bar_y : fConcreteCover + fDiameterTop_Bar_x + 0.5f * fDiameterTop_Bar_y;
            double cp_Top_y_coordY = bIsReinforcementBarStraight ? fConcreteCover : fConcreteCover + 0.5f * fDiameterTop_Bar_y;
            double cp_Bottom_x_coordX = bIsReinforcementBarStraight ? fConcreteCover : fConcreteCover + 0.5f * fDiameterBottom_Bar_x;
            double cp_Bottom_x_coordY = bIsReinforcementBarStraight ? fConcreteCover + 0.5f * fDiameterBottom_Bar_x : fConcreteCover + fDiameterBottom_Bar_y + 0.5f * fDiameterBottom_Bar_x;
            double cp_Bottom_y_coordX = bIsReinforcementBarStraight ? fConcreteCover + 0.5f * fDiameterBottom_Bar_y : fConcreteCover + fDiameterBottom_Bar_x + 0.5f * fDiameterBottom_Bar_y;
            double cp_Bottom_y_coordY = bIsReinforcementBarStraight ? fConcreteCover : fConcreteCover + 0.5f * fDiameterBottom_Bar_y;

            Point3D cp_Top_x = new Point3D(cp_Top_x_coordX, cp_Top_x_coordY, fhZ - fConcreteCover - fDiameterTop_Bar_y - 0.5f * fDiameterTop_Bar_x);
            Point3D cp_Top_y = new Point3D(cp_Top_y_coordX, cp_Top_y_coordY, fhZ - fConcreteCover - 0.5f * fDiameterTop_Bar_y);
            Point3D cp_Bottom_x = new Point3D(cp_Bottom_x_coordX, cp_Bottom_x_coordY, fConcreteCover + fDiameterBottom_Bar_y + 0.5f * fDiameterBottom_Bar_x);
            Point3D cp_Bottom_y = new Point3D(cp_Bottom_y_coordX, cp_Bottom_y_coordY, fConcreteCover + 0.5f * fDiameterBottom_Bar_y);

            if(!bIsReinforcementBarStraight)
            {
                cp_Top_x = new Point3D(cp_Top_x_coordX, cp_Top_x_coordY, fConcreteCover + fDiameterTop_Bar_y);
                cp_Top_y = new Point3D(cp_Top_y_coordX, cp_Top_y_coordY, fConcreteCover);

                // Kedze sa vertikalne casti hornych a spodnych prutov prekryvaju posunieme horne pruty o sucet polovic priemeru
                cp_Top_x.Y = cp_Top_x_coordY + 0.5 * fDiameterTop_Bar_x + 0.5 * fDiameterBottom_Bar_x;
                cp_Top_y.X = cp_Top_y_coordX + 0.5 * fDiameterTop_Bar_y + 0.5 * fDiameterBottom_Bar_y;

                cp_Bottom_x = new Point3D(cp_Bottom_x_coordX, cp_Bottom_x_coordY, fhZ - fConcreteCover - fDiameterBottom_Bar_y);
                cp_Bottom_y = new Point3D(cp_Bottom_y_coordX, cp_Bottom_y_coordY, fhZ - fConcreteCover);
            }

            if (bIsReinforcementBarStraight)
            {
                reference_Top_Bar_x = new CReinforcementBarStraight(1, "500E", "Top x", true, cp_Top_x, fLengthTop_Bar_x, fDiameterTop_Bar_x, /*Colors.CadetBlue,*/ 0.5f, true, 0);
                reference_Top_Bar_y = new CReinforcementBarStraight(2, "500E", "Top y", false, cp_Top_y, fLengthTop_Bar_y, fDiameterTop_Bar_y, /*Colors.Coral,*/ 0.5f,  true, 0);
                reference_Bottom_Bar_x = new CReinforcementBarStraight(3, "500E", "Bottom x", true, cp_Bottom_x, fLengthBottom_Bar_x, fDiameterBottom_Bar_x, /*Colors.YellowGreen,*/ 0.5f, true, 0);
                reference_Bottom_Bar_y = new CReinforcementBarStraight(4, "500E", "Bottom y", false, cp_Bottom_y, fLengthBottom_Bar_y, fDiameterBottom_Bar_y, /*Colors.Purple,*/ 0.5f, true, 0);
            }
            else
            {
                float fArcRadiusNetTop_Bar_x = 3f * fDiameterTop_Bar_x;
                float fArcRadiusNetTop_Bar_y = 3f * fDiameterTop_Bar_y;
                float fArcRadiusNetBottom_Bar_x = 3f * fDiameterBottom_Bar_x;
                float fArcRadiusNetBottom_Bar_y = 3f * fDiameterBottom_Bar_y;

                reference_Top_Bar_x = new CReinforcementBar_U(1, "500E", "Top x", true, cp_Top_x, fLengthTop_Bar_x, fArcRadiusNetTop_Bar_x, fDiameterTop_Bar_x, /*Colors.CadetBlue,*/ 0.5f, true, true, 0);
                reference_Top_Bar_y = new CReinforcementBar_U(2, "500E", "Top y", false, cp_Top_y, fLengthTop_Bar_y, fArcRadiusNetTop_Bar_y, fDiameterTop_Bar_y, /*Colors.Coral,*/ 0.5f, true, true, 0);
                reference_Bottom_Bar_x = new CReinforcementBar_U(3, "500E", "Bottom x", true, cp_Bottom_x, fLengthBottom_Bar_x, fArcRadiusNetBottom_Bar_x, fDiameterBottom_Bar_x, /*Colors.YellowGreen,*/ 0.5f, false, true, 0);
                reference_Bottom_Bar_y = new CReinforcementBar_U(4, "500E", "Bottom y", false, cp_Bottom_y, fLengthBottom_Bar_y, fArcRadiusNetBottom_Bar_y, fDiameterBottom_Bar_y, /*Colors.Purple,*/ 0.5f, false, true, 0);
            }
        }

        private void CountPlates_ValidationPurpose()
        {
            int iNumberOfJoints = 0;
            int iNumberOfJointsGenerateTrue = 0;
            int iNumberOfJointsGenerateFalse = 0;

            int iNumberOfPlates = 0;
            int iNumberOfPlateGenerateTrue = 0;
            int iNumberOfPlateGenerateFalse = 0;
            int iNumberOfPlateMatListTrue = 0;
            int iNumberOfPlateMatlistFalse = 0;

            foreach (CConnectionJointTypes joint in m_arrConnectionJoints)
            {
                iNumberOfJoints++;
                if (joint.BIsGenerated) iNumberOfJointsGenerateTrue++; else iNumberOfJointsGenerateFalse++;

                foreach (CPlate plate in joint.m_arrPlates)
                {
                    iNumberOfPlates++;

                    if (plate.BIsGenerated) iNumberOfPlateGenerateTrue++; else iNumberOfPlateGenerateFalse++;
                    if (plate.BIsSelectedForMaterialList) iNumberOfPlateMatListTrue++; else iNumberOfPlateMatlistFalse++;

                    if (plate is CConCom_Plate_B_basic)
                    {
                        CConCom_Plate_B_basic basePlate = (CConCom_Plate_B_basic)plate;

                        foreach (CAnchor anchor in basePlate.AnchorArrangement.Anchors)
                        {
                            iNumberOfPlates++; // anchor.WasherBearing
                            iNumberOfPlates++; // anchor.WasherPlateTop

                            if (anchor.WasherBearing.BIsGenerated) iNumberOfPlateGenerateTrue++; else iNumberOfPlateGenerateFalse++;
                            if (anchor.WasherBearing.BIsSelectedForMaterialList) iNumberOfPlateMatListTrue++; else iNumberOfPlateMatlistFalse++;

                            if (anchor.WasherPlateTop.BIsGenerated) iNumberOfPlateGenerateTrue++; else iNumberOfPlateGenerateFalse++;
                            if (anchor.WasherPlateTop.BIsSelectedForMaterialList) iNumberOfPlateMatListTrue++; else iNumberOfPlateMatlistFalse++;
                        }
                    }
                }
            }

            Console.WriteLine(
                "Total number of joints: " + iNumberOfJoints.ToString() + "\n" +
                "Number of joints - Generate - True: " + iNumberOfJointsGenerateTrue.ToString() + "\n" +
                "Number of joints - Generate - False: " + iNumberOfJointsGenerateFalse.ToString() + "\n" +
                "\n" +
                "Total number of plates: " + iNumberOfPlates.ToString() + "\n" +
                "Number of plates - Generate - True: " + iNumberOfPlateGenerateTrue.ToString() + "\n" +
                "Number of plates - Generate - False: " + iNumberOfPlateGenerateFalse.ToString() + "\n" +
                "Number of plates - Material List - True: " + iNumberOfPlateMatListTrue.ToString() + "\n" +
                "Number of plates - Material List - False: " + iNumberOfPlateMatlistFalse.ToString() + "\n");
        }
    }
}
