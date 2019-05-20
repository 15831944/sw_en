﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Configuration;
using BaseClasses;
using FEM_CALC_BASE;
using System.Data;
using MATH;
using M_BASE;
using M_EC1.AS_NZS;
using BriefFiniteElementNet.CodeProjectExamples;
using BriefFiniteElementNet;
using PFD.Infrastructure;
using System.Collections.ObjectModel;
using DATABASE.DTO;
using EXPIMP;
using M_AS4600;

namespace PFD
{
    public class CPFDViewModel : INotifyPropertyChanged
    {
        private bool debugging = true;
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public MainWindow PFDMainWindow;
        public Solver SolverWindow;
        //-------------------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsSetFromCode = false;

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        private int MModelIndex;
        private float MGableWidth;
        private float MLength;
        private float MWallHeight;
        private float MRoofPitch_deg;
        private int MFrames;
        private float MGirtDistance;
        private float MPurlinDistance;
        private float MColumnDistance;
        private float MBottomGirtPosition;
        private float MFrontFrameRakeAngle;
        private float MBackFrameRakeAngle;
        private int MRoofCladdingIndex;
        private int MRoofCladdingColorIndex;
        private int MRoofCladdingThicknessIndex;
        private int MWallCladdingIndex;
        private int MWallCladdingColorIndex;
        private int MWallCladdingThicknessIndex;

        private int MMainColumnFlyBracingPositionIndex;
        private int MRafterFlyBracingPositionIndex;
        private int MEdgePurlin_ILS_Number;
        private int MGirt_ILS_Number;
        private int MPurlin_ILS_Number;
        private int MFrontColumnFlyBracingPositionIndex;
        private int MBackColumnFlyBracingPositionIndex;
        private int MGirtFrontSide_ILS_Number;
        private int MGirtBackSide_ILS_Number;

        private int MLoadCaseIndex;

        private int iFrontColumnNoInOneFrame;

        private bool m_LightDirectional;
        private bool m_LightPoint;
        private bool m_LightSpot;
        private bool m_LightAmbient;
        private bool m_MaterialDiffuse;
        private bool m_MaterialEmissive;
        private bool m_DisplayMembers;
        private bool m_DisplayJoints;
        private bool m_DisplayPlates;
        private bool m_DisplayConnectors;
        private bool m_DisplayFoundations;
        private bool m_DisplayFloorSlab;
        private bool m_DisplayNodalSupports;
        private bool m_DisplayMembersCenterLines;
        private bool m_DisplaySolidModel;
        private bool m_DisplayWireFrameModel;
        private bool m_DisplayDistinguishedColorMember;
        private bool m_DisplayTransparentModelMember;

        // Load Case - display options
        private bool MShowLoads;
        private bool MShowNodalLoads;
        private bool MShowLoadsOnMembers;
        private bool MShowLoadsOnGirts;
        private bool MShowLoadsOnPurlins;
        private bool MShowLoadsOnColumns;
        private bool MShowLoadsOnFrameMembers;
        private bool MShowSurfaceLoads;

        // Loads - generate options
        private bool MGenerateNodalLoads;
        private bool MGenerateLoadsOnGirts;
        private bool MGenerateLoadsOnPurlins;
        private bool MGenerateLoadsOnColumns;
        private bool MGenerateLoadsOnFrameMembers;
        private bool MGenerateSurfaceLoads;

        // Labels and axes
        private bool MShowLoadsLabels;
        private bool MShowLoadsLabelsUnits;
        private bool MShowGlobalAxis;
        private bool MShowLocalMembersAxis;
        private bool MIsEnabledLocalMembersAxis;
        private bool MShowSurfaceLoadsAxis;
        private bool MIsEnabledSurfaceLoadsAxis;

        // Member description options
        private bool MShowMemberDescription;
        private bool MShowMemberID;
        private bool MShowMemberPrefix;
        private bool MShowMemberCrossSectionStartName;
        private bool MShowMemberRealLength;

        private float MDisplayIn3DRatio;

        // Load Combination - options
        private bool MDeterminateCombinationResultsByFEMSolver;
        private bool MUseFEMSolverCalculationForSimpleBeam;
        private bool MDeterminateMemberLocalDisplacementsForULS;

        // Local member load direction used for load definition, calculation of internal forces and design
        // Use geometrical or principal axes of cross-section to define load direction etc.
        private bool MUseCRSCGeometricalAxes = true;

        //Color display options
        private bool m_ColorsAccordingToMembers;
        private bool m_ColorsAccordingToSections;

        private ObservableCollection<DoorProperties> MDoorBlocksProperties;
        private ObservableCollection<WindowProperties> MWindowBlocksProperties;
        private List<string> MBuildingSides;
        private List<string> MDoorsTypes;

        private ObservableCollection<CComponentInfo> MComponentList;
        private bool MModelCalculatedResultsValid;

        // Popis pre Ondreja - doors and windows
        // GUI

        // Personnel door su len z jedneho prierezu BOX 10075
        // Roller door mozu mat dva prierezy - door trimmer C270115btb a niekedy aj door lintel z C27095 (podla toho aka je vzdialenost medzi hornou hranou dveri a najblizsim girt nad tym)
        // U blokov sa momentalne moze a nemusi vytvorit crsc pre girt, ten sa potom ale nepridava do celkoveho pola m_arrCrSc pretoze tam uz je

        // Window - podobne ako door, posledna polozka je combobox s 1-20?? - pocet stlpikov okna, ktore rozdelia okno na viacero casti

        // Ked pridam do modelu prve dvere alebo okno mali by sa do Component list pridat prierezy, ktore tam este nie su (trimmer, lintel), tieto prierezy by sa mali vyrobit len raz a ked budem pridavat dalsie dvere alebo okna, tak by sa mali len priradzovat
        // pri pridani dveri sa mozu vygenerovat rozne prierezy podla typu dveri a geometrie (C270115btb-trimmer pre roller door, C27095-lintel pre roller door, BOX 10075 pre personnel door), pri pridani window sa generuje len BOX 10075

        // Prierezy bloku sa momentalne pridavaju do zoznamu vsetkych prierezov pre kazde dvere/okno samostatne
        // vid CBlock_3D_001_DoorInBay, line 41, takze ked pridam 10 dveri s BOX 10075 tak ten BOX 10075 je tam 10 krat samostatny pre kazde dvere co je zbytocne
        // v CModel_PFD_01_GR sa vo funkcii AddDoorOrWindowBlockProperties menia velkosti povodnych poli a pridavaju sa do nich nove objekty z blokov
        // prierez pre GIRT z bloku sa nepridava lebo uz v zozname 3D modelu existuje

        // Je potrebne skontrolovat ci sa prutom z blokov priradia vsetky "bool" parametre a ci sa dane pruty priradia do group of members vid CModel_PFD_01_GR AddMembersToMemberGroupsLists()

        // Potrebujeme opravit 3D grafiku
        // a otocit vsetky lokane osy prutov bloku rovnakym smerom
        // nastavit spravne excentricity a pootocenia
        // "vymysliet" algoritmus ako sa tieto udaje zmenia ak blok otocim a presuniem z prednej steny na lavu, pravu, zadnu (toto je poriadny oriesok)

        // Door trimmers bude potrebne pridat do zoznamov typov prutov ktore su zatazene surface loads, tu je komplikacia ze kazdy prut moze mat inu zatazovaciu sirku
        // vid FreeSurfaceLoadsMemberTypeData


        private CModel_PFD MModel;
        //-------------------------------------------------------------------------------------------------------------
        //tieto treba spracovat nejako
        public float fL1;
        public float fh2;
        public float fRoofPitch_radians;
        public float fMaterial_density = 7850f; // [kg /m^3] (malo by byt zadane v databaze materialov)

        public CCalcul_1170_1 GeneralLoad;
        public CCalcul_1170_2 Wind;
        public CCalcul_1170_3 Snow;
        public CCalcul_1170_5 Eq;
        public CPFDLoadInput Loadinput;

        public List<CMemberInternalForcesInLoadCases> MemberInternalForcesInLoadCases;
        public List<CMemberDeflectionsInLoadCases> MemberDeflectionsInLoadCases;

        public List<CMemberInternalForcesInLoadCombinations> MemberInternalForcesInLoadCombinations;
        public List<CMemberDeflectionsInLoadCombinations> MemberDeflectionsInLoadCombinations;

        public List<CMemberLoadCombinationRatio_ULS> MemberDesignResults_ULS = new List<CMemberLoadCombinationRatio_ULS>();
        public List<CMemberLoadCombinationRatio_SLS> MemberDesignResults_SLS = new List<CMemberLoadCombinationRatio_SLS>();
        public List<CJointLoadCombinationRatio_ULS> JointDesignResults_ULS;

        public sDesignResults sDesignResults_ULSandSLS = new sDesignResults();
        public sDesignResults sDesignResults_ULS = new sDesignResults();
        public sDesignResults sDesignResults_SLS = new sDesignResults();

        public List<CFrame> frameModels;
        public List<CBeam_Simple> beamSimpleModels;

        //-------------------------------------------------------------------------------------------------------------
        public int ModelIndex
        {
            get
            {
                return MModelIndex;
            }

            set
            {
                MModelIndex = value;

                //dolezite je volat private fields a nie Properties pokial nechceme aby sa volali setter metody
                CDatabaseModels dmodel = new CDatabaseModels(MModelIndex);
                IsSetFromCode = true;
                GableWidth = dmodel.fb;
                Length = dmodel.fL;
                WallHeight = dmodel.fh;
                Frames = dmodel.iFrNo;
                RoofPitch_deg = dmodel.fRoof_Pitch_deg;
                GirtDistance = dmodel.fdist_girt;
                PurlinDistance = dmodel.fdist_purlin;
                ColumnDistance = dmodel.fdist_frontcolumn;
                BottomGirtPosition = dmodel.fdist_girt_bottom;
                FrontFrameRakeAngle = dmodel.fRakeAngleFrontFrame_deg;
                BackFrameRakeAngle = dmodel.fRakeAngleBackFrame_deg;
                _componentVM.SetModelComponentListProperties(dmodel.MembersSectionsDict); //set default components sections

                // Set default position of fly bracing point and numbers of lateral intermediate supports
                MainColumnFlyBracingPositionIndex = dmodel.iMainColumnFlyBracingEveryXXGirt; // 0 - no fly bracing, 1 - every girt, 2 - every second girt, 3 - every third girt
                RafterFlyBracingPositionIndex = dmodel.iRafterFlyBracingEveryXXPurlin; // 0 - no fly bracing, 1 - every purlin, 2 - every second purlin, 3 - every third purlin
                EdgePurlin_ILS_Number = dmodel.iEdgePurlin_ILS_Number;
                Girt_ILS_Number = dmodel.iGirt_ILS_Number;
                Purlin_ILS_Number = dmodel.iPurlin_ILS_Number;
                FrontColumnFlyBracingPositionIndex = dmodel.iFrontColumnFlyBracingEveryXXGirt;
                BackColumnFlyBracingPositionIndex = dmodel.iBackColumnFlyBracingEveryXXGirt;
                GirtFrontSide_ILS_Number = dmodel.iGirtFrontSide_ILS_Number;
                GirtBackSide_ILS_Number = dmodel.iGirtBackSide_ILS_Number;

                SetResultsAreNotValid();
                IsSetFromCode = false;

                //tieto riadky by som tu najradsej nemal, resp. ich nejako spracoval ako dalsie property
                fL1 = MLength / (MFrames - 1);
                fRoofPitch_radians = MRoofPitch_deg * MathF.fPI / 180f;
                fh2 = MWallHeight + 0.5f * MGableWidth * (float)Math.Tan(fRoofPitch_radians);

                RoofCladdingIndex = 1;
                RoofCladdingColorIndex = 22;
                WallCladdingIndex = 0;
                WallCladdingColorIndex = 22;
                ModelCalculatedResultsValid = false;

                NotifyPropertyChanged("ModelIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float GableWidth
        {
            get
            {
                return MGableWidth;
            }
            set
            {
                if (value < 3 || value > 100)
                    throw new ArgumentException("Gable Width must be between 3 and 100 [m]");
                MGableWidth = value;

                if (MModelIndex != 0)
                {
                    // UHOL ZACHOVAME ROVNAKY - V OPACNOM PRIPADE SA NEUPDATOVALA SPRAVNE VYSKA h2

                    // Recalculate roof pitch
                    //fRoofPitch_radians = (float)Math.Atan((fh2 - MWallHeight) / (0.5f * MGableWidth));
                    // Set new value in GUI
                    //MRoofPitch_deg = (fRoofPitch_radians * 180f / MathF.fPI);
                    // Recalculate roof height
                    fh2 = MWallHeight + 0.5f * MGableWidth * (float)Math.Tan(fRoofPitch_radians);

                    // Re-calculate value of distance between columns (number of columns per frame is always even
                    int iOneRafterFrontColumnNo = (int)((0.5f * MGableWidth) / MColumnDistance);
                    iFrontColumnNoInOneFrame = 2 * iOneRafterFrontColumnNo;
                }
                SetResultsAreNotValid();
                NotifyPropertyChanged("GableWidth");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float Length
        {
            get
            {
                return MLength;
            }

            set
            {
                if (value < 3 || value > 150)
                    throw new ArgumentException("Length must be between 3 and 150 [m]");
                MLength = value;

                if (MModelIndex != 0)
                {
                    // Recalculate fL1
                    fL1 = MLength / (MFrames - 1);
                }
                SetResultsAreNotValid();
                NotifyPropertyChanged("Length");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float WallHeight
        {
            get
            {
                return MWallHeight;
            }

            set
            {
                if (value < 2 || value > 30)
                    throw new ArgumentException("Wall Height must be between 2 and 30 [m]");
                MWallHeight = value;

                if (MModelIndex != 0)
                {
                    // Recalculate roof heigth
                    fh2 = MWallHeight + 0.5f * MGableWidth * (float)Math.Tan(fRoofPitch_radians);
                }
                SetResultsAreNotValid();
                NotifyPropertyChanged("WallHeight");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float RoofPitch_deg
        {
            get
            {
                return MRoofPitch_deg;
            }

            set
            {
                // Todo - Ondrej - pada ak zadam napr 60 stupnov, tato kontrola ma prebehnut len ak je hodnota zadana do text boxu uzivatelsky, 
                // ak sa prepocita z inych hodnot (napr. b), tak by sa mala zobrazit
                // aj ked je nevalidna a malo by vypisat nizsie uvedene varovanie, model by sa nemal prekreslit kym nie su vsetky hodnoty validne

                if (value < 3 || value > 35)
                    throw new ArgumentException("Roof Pitch must be between 3 and 35 degrees");
                MRoofPitch_deg = value;

                if (MModelIndex != 0)
                {
                    fRoofPitch_radians = MRoofPitch_deg * MathF.fPI / 180f;
                    // Recalculate h2
                    fh2 = MWallHeight + 0.5f * MGableWidth * (float)Math.Tan(fRoofPitch_radians);
                }
                SetResultsAreNotValid();
                NotifyPropertyChanged("RoofPitch");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int Frames
        {
            get
            {
                return MFrames;
            }

            set
            {
                if (value < 2 || value > 50)
                    throw new ArgumentException("Number of frames must be between 2 and 50");
                MFrames = value;

                if (MModelIndex != 0)
                {
                    // Recalculate L1
                    fL1 = MLength / (MFrames - 1);
                }
                SetResultsAreNotValid();
                NotifyPropertyChanged("Frames");
            }
        }



        //-------------------------------------------------------------------------------------------------------------
        public float GirtDistance
        {
            get
            {
                return MGirtDistance;
            }

            set
            {
                if (value < 0.5 || value > 5)
                    throw new ArgumentException("Girt distance must be between 0.5 and 5 [m]");

                MGirtDistance = (float)Math.Round(value, 3); //Display only limited number of decimal places - Todo - Ondrej Review
                SetResultsAreNotValid();
                NotifyPropertyChanged("GirtDistance");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float PurlinDistance
        {
            get
            {
                return MPurlinDistance;
            }

            set
            {
                if (value < 0.5 || value > 5)
                    throw new ArgumentException("Purlin distance must be between 0.5 and 5 [m]");

                MPurlinDistance = (float)Math.Round(value, 3); //Display only limited number of decimal places - Todo - Ondrej Review
                SetResultsAreNotValid();
                NotifyPropertyChanged("PurlinDistance");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float ColumnDistance
        {
            get
            {
                return MColumnDistance;
            }

            set
            {
                if (value < 1 || value > 10)
                    throw new ArgumentException("Column distance must be between 1 and 10 [m]");
                MColumnDistance = value;

                if (MModelIndex != 0)
                {
                    // Re-calculate value of distance between columns (number of columns per frame is always even
                    int iOneRafterFrontColumnNo = (int)((0.5f * MGableWidth) / MColumnDistance);
                    iFrontColumnNoInOneFrame = 2 * iOneRafterFrontColumnNo;
                    // Update value of distance between columns

                    // Todo
                    // Nie je to trosku na hlavu? Pouzivatel zada Column distance a ono sa mu to nejako prepocita a zmeni???
                    // Odpoved: Spravnejsie by bolo zadat pocet stlpov, ale je to urobene tak ze to musi byt parne cislo aby boli stlpiky symetricky voci stredu
                    // Pripadalo mi lepsie ak si uzivatel zada nejaku vzdialenost ktoru priblizne vie, pocet stlpikov by si musel spocitat 
                    // alebo tam musi byt dalsi textbox pre column distance kde sa ta dopocitana vzdialenost bude zobrazovat, ale je to dalsi readonly riadok na vstupe navyse
                    // chcel som tam mat toho co najmenej a len najnutnejsie hodnoty
                    // Mozes to tak upravit ak je to logickejsie a spravnejsie

                    MColumnDistance = (MGableWidth / (iFrontColumnNoInOneFrame + 1));
                }
                SetResultsAreNotValid();
                NotifyPropertyChanged("ColumnDistance");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float BottomGirtPosition
        {
            get
            {
                return MBottomGirtPosition;
            }

            set
            {
                if (value < 0.2 || value > 0.8 * MWallHeight) // Limit is 80% of main column height, could be more but it is 
                    throw new ArgumentException("Bottom Girt Position between 0.2 and " + Math.Round(0.8 * MWallHeight, 3) + " [m]");
                MBottomGirtPosition = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("BottomGirtPosition");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float FrontFrameRakeAngle
        {
            get
            {
                return MFrontFrameRakeAngle;
            }

            set
            {
                float frontFrameRakeAngle_limit_rad = (float)(Math.Atan(fL1 / MGableWidth) - (Math.PI / 180)); // minus 1 radian
                float frontFrameRakeAngle_limit_deg = frontFrameRakeAngle_limit_rad * 180f / MathF.fPI;

                if (value < -frontFrameRakeAngle_limit_deg || value > frontFrameRakeAngle_limit_deg)
                {
                    string s = "Front frame angle must be between " + (-Math.Round(frontFrameRakeAngle_limit_deg, 1)) + " and " + Math.Round(frontFrameRakeAngle_limit_deg, 1) + " degrees";
                    throw new ArgumentException(s);
                }
                MFrontFrameRakeAngle = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("FrontFrameRakeAngle");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float BackFrameRakeAngle
        {
            get
            {
                return MBackFrameRakeAngle;
            }

            set
            {
                float backFrameRakeAngle_limit_rad = (float)(Math.Atan(fL1 / MGableWidth) - (Math.PI / 180)); // minus 1 radian
                float backFrameRakeAngle_limit_deg = backFrameRakeAngle_limit_rad * 180f / MathF.fPI;

                if (value < -backFrameRakeAngle_limit_deg || value > backFrameRakeAngle_limit_deg)
                {
                    string s = "Back frame angle must be between " + (-Math.Round(backFrameRakeAngle_limit_deg, 1)) + " and " + Math.Round(backFrameRakeAngle_limit_deg, 1) + " degrees";
                    throw new ArgumentException(s);
                }
                MBackFrameRakeAngle = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("BackFrameRakeAngle");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int RoofCladdingIndex
        {
            get
            {
                return MRoofCladdingIndex;
            }

            set
            {
                MRoofCladdingIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("RoofCladdingIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int RoofCladdingColorIndex
        {
            get
            {
                return MRoofCladdingColorIndex;
            }

            set
            {
                MRoofCladdingColorIndex = value;

                NotifyPropertyChanged("RoofCladdingColorIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int RoofCladdingThicknessIndex
        {
            get
            {
                return MRoofCladdingThicknessIndex;
            }

            set
            {
                MRoofCladdingThicknessIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("RoofCladdingThicknessIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int WallCladdingIndex
        {
            get
            {
                return MWallCladdingIndex;
            }

            set
            {
                MWallCladdingIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("WallCladdingIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int WallCladdingColorIndex
        {
            get
            {
                return MWallCladdingColorIndex;
            }

            set
            {
                MWallCladdingColorIndex = value;

                NotifyPropertyChanged("WallCladdingColorIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int WallCladdingThicknessIndex
        {
            get
            {
                return MWallCladdingThicknessIndex;
            }

            set
            {
                MWallCladdingThicknessIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("WallCladdingThicknessIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // 0 - no fly bracing, 1 - every girt, 2 - every second girt, 3 - every third girt
        public int MainColumnFlyBracingPositionIndex
        {
            get
            {
                return MMainColumnFlyBracingPositionIndex;
            }

            set
            {
                MMainColumnFlyBracingPositionIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("MainColumnFlyBracingPositionIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // 0 - no fly bracing, 1 - every purlin, 2 - every second purlin, 3 - every third purlin
        public int RafterFlyBracingPositionIndex
        {
            get
            {
                return MRafterFlyBracingPositionIndex;
            }

            set
            {
                MRafterFlyBracingPositionIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("RafterFlyBracingPositionIndex"); // RafterFlyBracingEveryXXPurlin
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int EdgePurlin_ILS_Number
        {
            get
            {
                return MEdgePurlin_ILS_Number;
            }

            set
            {
                MEdgePurlin_ILS_Number = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("EdgePurlin_ILS_Number");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int Girt_ILS_Number
        {
            get
            {
                return MGirt_ILS_Number;
            }

            set
            {
                MGirt_ILS_Number = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("Girt_ILS_Number");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int Purlin_ILS_Number
        {
            get
            {
                return MPurlin_ILS_Number;
            }

            set
            {
                MPurlin_ILS_Number = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("Purlin_ILS_Number");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int FrontColumnFlyBracingPositionIndex
        {
            get
            {
                return MFrontColumnFlyBracingPositionIndex;
            }

            set
            {
                MFrontColumnFlyBracingPositionIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("FrontColumnFlyBracingPositionIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int BackColumnFlyBracingPositionIndex
        {
            get
            {
                return MBackColumnFlyBracingPositionIndex;
            }

            set
            {
                MBackColumnFlyBracingPositionIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("BackColumnFlyBracingPositionIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int GirtFrontSide_ILS_Number
        {
            get
            {
                return MGirtFrontSide_ILS_Number;
            }

            set
            {
                MGirtFrontSide_ILS_Number = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("GirtFrontSide_ILS_Number");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int GirtBackSide_ILS_Number
        {
            get
            {
                return MGirtBackSide_ILS_Number;
            }

            set
            {
                MGirtBackSide_ILS_Number = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("GirtBackSide_ILS_Number");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int LoadCaseIndex
        {
            get
            {
                return MLoadCaseIndex;
            }

            set
            {
                MLoadCaseIndex = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("LoadCaseIndex");
            }
        }

        public CModel_PFD Model
        {
            get
            {
                return MModel;
            }

            set
            {
                MModel = value;
                SetModelBays();
            }
        }

        public bool ShowLoads
        {
            get
            {
                return MShowLoads;
            }

            set
            {
                MShowLoads = value;
                SetIsEnabledSurfaceLoadsAxis();
                NotifyPropertyChanged("ShowLoads");
            }
        }

        public bool ShowNodalLoads
        {
            get
            {
                return MShowNodalLoads;
            }

            set
            {
                MShowNodalLoads = value;
                NotifyPropertyChanged("ShowNodalLoads");
            }
        }

        public bool ShowLoadsOnMembers
        {
            get
            {
                return MShowLoadsOnMembers;
            }

            set
            {
                MShowLoadsOnMembers = value;
                NotifyPropertyChanged("ShowLoadsOnMembers");
            }
        }

        public bool ShowLoadsOnGirts
        {
            get
            {
                return MShowLoadsOnGirts;
            }

            set
            {
                MShowLoadsOnGirts = value;
                //if (MShowLoadsOnPurlinsAndGirts && MShowLoadsOnFrameMembers) ShowLoadsOnFrameMembers = false; // Umoznit zobrazit aj single members a frames spolocne
                NotifyPropertyChanged("ShowLoadsOnGirts");
            }
        }

        public bool ShowLoadsOnPurlins
        {
            get
            {
                return MShowLoadsOnPurlins;
            }

            set
            {
                MShowLoadsOnPurlins = value;
                //if (MShowLoadsOnPurlinsAndGirts && MShowLoadsOnFrameMembers) ShowLoadsOnFrameMembers = false; // Umoznit zobrazit aj single members a frames spolocne
                NotifyPropertyChanged("ShowLoadsOnPurlins");
            }
        }

        public bool ShowLoadsOnColumns
        {
            get
            {
                return MShowLoadsOnColumns;
            }

            set
            {
                MShowLoadsOnColumns = value;
                //if (MShowLoadsOnPurlinsAndGirts && MShowLoadsOnFrameMembers) ShowLoadsOnFrameMembers = false; // Umoznit zobrazit aj single members a frames spolocne
                NotifyPropertyChanged("ShowLoadsOnColumns");
            }
        }

        public bool ShowLoadsOnFrameMembers
        {
            get
            {
                return MShowLoadsOnFrameMembers;
            }

            set
            {
                MShowLoadsOnFrameMembers = value;
                //if (MShowLoadsOnPurlinsAndGirts && MShowLoadsOnFrameMembers) ShowLoadsOnPurlinsAndGirts = false; // Umoznit zobrazit aj single members a frames spolocne
                NotifyPropertyChanged("ShowLoadsOnFrameMembers");
            }
        }

        public bool ShowSurfaceLoads
        {
            get
            {
                return MShowSurfaceLoads;
            }

            set
            {
                MShowSurfaceLoads = value;
                if (!MShowSurfaceLoads && MShowSurfaceLoadsAxis) ShowSurfaceLoadsAxis = false;
                SetIsEnabledSurfaceLoadsAxis();
                NotifyPropertyChanged("ShowSurfaceLoads");
            }
        }

        public bool DeterminateCombinationResultsByFEMSolver
        {
            get
            {
                return MDeterminateCombinationResultsByFEMSolver;
            }

            set
            {
                MDeterminateCombinationResultsByFEMSolver = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("DeterminateCombinationResultsByFEMSolver");
            }
        }

        public bool UseFEMSolverCalculationForSimpleBeam
        {
            get
            {
                return MUseFEMSolverCalculationForSimpleBeam;
            }

            set
            {
                MUseFEMSolverCalculationForSimpleBeam = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("UseFEMSolverCalculationForSimpleBeam");
            }
        }

        public bool DeterminateMemberLocalDisplacementsForULS
        {
            get
            {
                return MDeterminateMemberLocalDisplacementsForULS;
            }

            set
            {
                MDeterminateMemberLocalDisplacementsForULS = value;
                SetResultsAreNotValid();
                NotifyPropertyChanged("DeterminateMemberLocalDisplacementsForULS");
            }
        }

        public bool UseCRSCGeometricalAxes
        {
            get
            {
                return MUseCRSCGeometricalAxes;
            }

            set
            {
                MUseCRSCGeometricalAxes = value;
                NotifyPropertyChanged("UseCRSCGeometricalAxes");
            }
        }

        public bool ShowMemberDescription
        {
            get
            {
                return MShowMemberDescription;
            }

            set
            {
                MShowMemberDescription = value;
                NotifyPropertyChanged("ShowMemberDescription");
            }
        }

        public bool ShowMemberID
        {
            get
            {
                return MShowMemberID;
            }

            set
            {
                MShowMemberID = value;
                NotifyPropertyChanged("ShowMemberID");
            }
        }

        public bool ShowMemberPrefix
        {
            get
            {
                return MShowMemberPrefix;
            }

            set
            {
                MShowMemberPrefix = value;
                NotifyPropertyChanged("ShowMemberPrefix");
            }
        }

        public bool ShowMemberCrossSectionStartName
        {
            get
            {
                return MShowMemberCrossSectionStartName;
            }

            set
            {
                MShowMemberCrossSectionStartName = value;
                NotifyPropertyChanged("ShowMemberCrossSectionStartName");
            }
        }

        public bool ShowMemberRealLength
        {
            get
            {
                return MShowMemberRealLength;
            }

            set
            {
                MShowMemberRealLength = value;
                NotifyPropertyChanged("ShowMemberRealLength");
            }
        }

        public bool ShowLoadsLabels
        {
            get
            {
                return MShowLoadsLabels;
            }

            set
            {
                MShowLoadsLabels = value;
                NotifyPropertyChanged("ShowLoadsLabels");
            }
        }

        public bool ShowLoadsLabelsUnits
        {
            get
            {
                return MShowLoadsLabelsUnits;
            }

            set
            {
                MShowLoadsLabelsUnits = value;
                NotifyPropertyChanged("ShowLoadsLabelsUnits");
            }
        }

        public float DisplayIn3DRatio
        {
            get
            {
                return MDisplayIn3DRatio;
            }

            set
            {
                MDisplayIn3DRatio = value;
                NotifyPropertyChanged("MDisplayIn3DRatio");
            }
        }


        public bool ShowGlobalAxis
        {
            get { return MShowGlobalAxis; }
            set { MShowGlobalAxis = value; NotifyPropertyChanged("ShowGlobalAxis"); }
        }
        public bool ShowLocalMembersAxis
        {
            get
            {
                return MShowLocalMembersAxis;
            }

            set
            {
                MShowLocalMembersAxis = value;
                NotifyPropertyChanged("ShowLocalMembersAxis");
            }
        }
        public bool IsEnabledLocalMembersAxis
        {
            get
            {
                return MIsEnabledLocalMembersAxis;
            }

            set
            {
                MIsEnabledLocalMembersAxis = value;
                NotifyPropertyChanged("IsEnabledLocalMembersAxis");
            }
        }
        public bool ShowSurfaceLoadsAxis
        {
            get
            {
                return MShowSurfaceLoadsAxis;
            }

            set
            {
                MShowSurfaceLoadsAxis = value;
                NotifyPropertyChanged("ShowSurfaceLoadsAxis");
            }
        }
        public bool IsEnabledSurfaceLoadsAxis
        {
            get
            {
                return MIsEnabledSurfaceLoadsAxis;
            }

            set
            {
                MIsEnabledSurfaceLoadsAxis = value;
                NotifyPropertyChanged("IsEnabledSurfaceLoadsAxis");
            }
        }

        public bool GenerateNodalLoads
        {
            get
            {
                return MGenerateNodalLoads;
            }

            set
            {
                MGenerateNodalLoads = value;
                NotifyPropertyChanged("GenerateNodalLoads");
            }
        }

        public bool GenerateLoadsOnGirts
        {
            get
            {
                return MGenerateLoadsOnGirts;
            }

            set
            {
                MGenerateLoadsOnGirts = value;
                NotifyPropertyChanged("GenerateLoadsOnGirts");
            }
        }

        public bool GenerateLoadsOnPurlins
        {
            get
            {
                return MGenerateLoadsOnPurlins;
            }

            set
            {
                MGenerateLoadsOnPurlins = value;
                NotifyPropertyChanged(" GenerateLoadsOnPurlins");
            }
        }

        public bool GenerateLoadsOnColumns
        {
            get
            {
                return MGenerateLoadsOnColumns;
            }

            set
            {
                MGenerateLoadsOnColumns = value;
                NotifyPropertyChanged(" GenerateLoadsOnColumns");
            }
        }

        public bool GenerateLoadsOnFrameMembers
        {
            get
            {
                return MGenerateLoadsOnFrameMembers;
            }

            set
            {
                MGenerateLoadsOnFrameMembers = value;
                NotifyPropertyChanged("ShowLoadsOnFrameMembers");
            }
        }

        public bool GenerateSurfaceLoads
        {
            get
            {
                return MGenerateSurfaceLoads;
            }

            set
            {
                MGenerateSurfaceLoads = value;
                NotifyPropertyChanged("GenerateSurfaceLoads");
            }
        }

        public ObservableCollection<DoorProperties> DoorBlocksProperties
        {
            get
            {
                if (MDoorBlocksProperties == null) MDoorBlocksProperties = new ObservableCollection<DoorProperties>();
                return MDoorBlocksProperties;
            }

            set
            {
                MDoorBlocksProperties = value;
                if (MDoorBlocksProperties == null) return;
                MDoorBlocksProperties.CollectionChanged += DoorBlocksProperties_CollectionChanged;
                foreach (DoorProperties d in MDoorBlocksProperties)
                {
                    d.PropertyChanged += HandleDoorPropertiesPropertyChangedEvent;
                }
                NotifyPropertyChanged("DoorBlocksProperties");
            }
        }

        private void DoorBlocksProperties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                DoorProperties d = MDoorBlocksProperties.LastOrDefault();
                if (d != null)
                {
                    CDoorsAndWindowsHelper.SetDefaultDoorParams(d);
                    d.PropertyChanged += HandleDoorPropertiesPropertyChangedEvent;
                    NotifyPropertyChanged("DoorBlocksProperties_Add");
                    SetResultsAreNotValid();
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NotifyPropertyChanged("DoorBlocksProperties_CollectionChanged");
                SetResultsAreNotValid();
            }
            SetComponentListAccordingToDoors();
        }

        public ObservableCollection<WindowProperties> WindowBlocksProperties
        {
            get
            {
                if (MWindowBlocksProperties == null) MWindowBlocksProperties = new ObservableCollection<WindowProperties>();
                return MWindowBlocksProperties;
            }

            set
            {
                MWindowBlocksProperties = value;
                if (MWindowBlocksProperties == null) return;
                MWindowBlocksProperties.CollectionChanged += WindowBlocksProperties_CollectionChanged;
                foreach (WindowProperties w in MWindowBlocksProperties)
                {
                    w.PropertyChanged += HandleWindowPropertiesPropertyChangedEvent;
                }
                NotifyPropertyChanged("WindowBlocksProperties");
            }
        }
        private void WindowBlocksProperties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                WindowProperties w = MWindowBlocksProperties.LastOrDefault();
                if (w != null)
                {
                    CDoorsAndWindowsHelper.SetDefaultWindowParams(w);
                    w.PropertyChanged += HandleWindowPropertiesPropertyChangedEvent;
                    NotifyPropertyChanged("WindowBlocksProperties_Add");
                    SetResultsAreNotValid();
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NotifyPropertyChanged("WindowBlocksProperties_CollectionChanged");
                SetResultsAreNotValid();
            }
            SetComponentListAccordingToWindows();
        }
        public List<string> BuildingSides
        {
            get
            {
                if (MBuildingSides == null) MBuildingSides = new List<string>() { "Left", "Right", "Front", "Back" };
                return MBuildingSides;
            }
        }
        public List<string> DoorsTypes
        {
            get
            {
                if (MDoorsTypes == null) MDoorsTypes = new List<string>() { "Personnel Door", "Roller Door" };
                return MDoorsTypes;
            }
        }
        public List<int> WindowColumns
        {
            get
            {
                return new List<int>() { 2, 3, 4, 5, 6, 7, 8, 9, 10 };
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

                if (MComponentList == null) return;

                foreach (CComponentInfo c in MComponentList)
                {
                    c.PropertyChanged += HandleComponentInfoPropertyChangedEvent;
                }
                NotifyPropertyChanged("ComponentList");
            }
        }

        public bool ModelCalculatedResultsValid
        {
            get
            {
                return MModelCalculatedResultsValid;
            }

            set
            {
                MModelCalculatedResultsValid = value;
                NotifyPropertyChanged("ModelCalculatedResultsValid");
            }
        }

        public bool LightDirectional
        {
            get
            {
                return m_LightDirectional;
            }

            set
            {
                m_LightDirectional = value;
                NotifyPropertyChanged("LightDirectional");
            }
        }

        public bool LightPoint
        {
            get
            {
                return m_LightPoint;
            }

            set
            {
                m_LightPoint = value;
                NotifyPropertyChanged("LightPoint");
            }
        }

        public bool LightSpot
        {
            get
            {
                return m_LightSpot;
            }

            set
            {
                m_LightSpot = value;
                NotifyPropertyChanged("LightSpot");
            }
        }

        public bool LightAmbient
        {
            get
            {
                return m_LightAmbient;
            }

            set
            {
                m_LightAmbient = value;
                NotifyPropertyChanged("LightAmbient");
            }
        }

        public bool MaterialDiffuse
        {
            get
            {
                return m_MaterialDiffuse;
            }

            set
            {
                m_MaterialDiffuse = value;
                if (!m_MaterialDiffuse && !m_MaterialEmissive) MaterialEmissive = true;
                NotifyPropertyChanged("MaterialDiffuse");
            }
        }

        public bool MaterialEmissive
        {
            get
            {
                return m_MaterialEmissive;
            }

            set
            {
                m_MaterialEmissive = value;
                if (!m_MaterialEmissive && !m_MaterialDiffuse) MaterialDiffuse = true;
                NotifyPropertyChanged("MaterialEmissive");
            }
        }

        public bool DisplayMembers
        {
            get
            {
                return m_DisplayMembers;
            }

            set
            {
                m_DisplayMembers = value;
                if (!m_DisplayMembers && MShowLocalMembersAxis) ShowLocalMembersAxis = false;
                SetIsEnabledLocalMembersAxis();
                NotifyPropertyChanged("DisplayMembers");
            }
        }

        public bool DisplayJoints
        {
            get
            {
                return m_DisplayJoints;
            }

            set
            {
                m_DisplayJoints = value;
                NotifyPropertyChanged("DisplayJoints");
            }
        }

        public bool DisplayPlates
        {
            get
            {
                return m_DisplayPlates;
            }

            set
            {
                m_DisplayPlates = value;
                if (m_DisplayPlates) DisplayJoints = true;
                NotifyPropertyChanged("DisplayPlates");
            }
        }

        public bool DisplayConnectors
        {
            get
            {
                return m_DisplayConnectors;
            }

            set
            {
                m_DisplayConnectors = value;
                if (m_DisplayConnectors) DisplayJoints = true;
                NotifyPropertyChanged("DisplayConnectors");
            }
        }

        public bool DisplayFoundations
        {
            get
            {
                return m_DisplayFoundations;
            }

            set
            {
                m_DisplayFoundations = value;
                NotifyPropertyChanged("DisplayFoundations");
            }
        }

        public bool DisplayFloorSlab
        {
            get
            {
                return m_DisplayFloorSlab;
            }

            set
            {
                m_DisplayFloorSlab = value;
                NotifyPropertyChanged("DisplayFloorSlab");
            }
        }

        public bool DisplayNodalSupports
        {
            get
            {
                return m_DisplayNodalSupports;
            }

            set
            {
                m_DisplayNodalSupports = value;
                NotifyPropertyChanged("DisplayNodalSupports");
            }
        }

        public bool DisplayMembersCenterLines
        {
            get
            {
                return m_DisplayMembersCenterLines;
            }

            set
            {
                m_DisplayMembersCenterLines = value;
                SetIsEnabledLocalMembersAxis();
                NotifyPropertyChanged("DisplayMembersCenterLines");
            }
        }

        public bool DisplaySolidModel
        {
            get
            {
                return m_DisplaySolidModel;
            }

            set
            {
                m_DisplaySolidModel = value;
                SetIsEnabledLocalMembersAxis();
                NotifyPropertyChanged("DisplaySolidModel");
            }
        }

        public bool DisplayWireFrameModel
        {
            get
            {
                return m_DisplayWireFrameModel;
            }

            set
            {
                m_DisplayWireFrameModel = value;
                SetIsEnabledLocalMembersAxis();
                NotifyPropertyChanged("DisplayWireFrameModel");
            }
        }

        public bool DisplayDistinguishedColorMember
        {
            get
            {
                return m_DisplayDistinguishedColorMember;
            }

            set
            {
                m_DisplayDistinguishedColorMember = value;
                NotifyPropertyChanged("DisplayDistinguishedColorMember");
            }
        }

        public bool DisplayTransparentModelMember
        {
            get
            {
                return m_DisplayTransparentModelMember;
            }

            set
            {
                m_DisplayTransparentModelMember = value;
                NotifyPropertyChanged("DisplayTransparentModelMember");
            }
        }

        public bool ColorsAccordingToMembers
        {
            get
            {
                return m_ColorsAccordingToMembers;
            }

            set
            {
                if (m_ColorsAccordingToMembers != value)
                {
                    m_ColorsAccordingToMembers = value;
                    NotifyPropertyChanged("ColorsAccordingToMembers");
                }
            }
        }

        public bool ColorsAccordingToSections
        {
            get
            {
                return m_ColorsAccordingToSections;
            }

            set
            {
                m_ColorsAccordingToSections = value;
                //NotifyPropertyChanged("ColorsAccordingToSections");
            }
        }

        private List<int> frontBays;
        private List<int> backBays;
        private List<int> leftRightBays;
        private void SetModelBays()
        {
            CModel_PFD_01_GR model = (CModel_PFD_01_GR)this.Model;
            frontBays = new List<int>();
            backBays = new List<int>();
            leftRightBays = new List<int>();

            int iFrameNo = model != null ? model.iFrameNo : 4;
            int i = 0;
            while (i < iFrameNo - 1)
            {
                leftRightBays.Add((++i));
            }
            i = 0;
            while (i < iFrontColumnNoInOneFrame + 1)
            {
                frontBays.Add((++i));
            }
            i = 0;
            while (i < iFrontColumnNoInOneFrame + 1)
            {
                backBays.Add((++i));
            }

            SetDoorsBays();
            SetWindowsBays();
            SetDoorsWindowsValidationProperties();
        }

        private void SetDoorsBays()
        {
            foreach (DoorProperties d in MDoorBlocksProperties)
            {
                if (d.sBuildingSide == "Front" && !d.Bays.SequenceEqual(frontBays)) d.Bays = frontBays;
                else if (d.sBuildingSide == "Back" && !d.Bays.SequenceEqual(backBays)) d.Bays = backBays;
                else if (d.sBuildingSide == "Left" && !d.Bays.SequenceEqual(leftRightBays)) d.Bays = leftRightBays;
                else if (d.sBuildingSide == "Right" && !d.Bays.SequenceEqual(leftRightBays)) d.Bays = leftRightBays;
            }
            CheckDoorsBays();
        }
        private void SetDoorsBays(DoorProperties d)
        {
            if (d.sBuildingSide == "Front" && !d.Bays.SequenceEqual(frontBays)) d.Bays = frontBays;
            else if (d.sBuildingSide == "Back" && !d.Bays.SequenceEqual(backBays)) d.Bays = backBays;
            else if (d.sBuildingSide == "Left" && !d.Bays.SequenceEqual(leftRightBays)) d.Bays = leftRightBays;
            else if (d.sBuildingSide == "Right" && !d.Bays.SequenceEqual(leftRightBays)) d.Bays = leftRightBays;

            CheckDoorsBays(d);
        }
        private void CheckDoorsBays()
        {
            foreach (DoorProperties d in MDoorBlocksProperties)
            {
                if (d.iBayNumber > d.Bays.Count) d.iBayNumber = 1;
                if (MDoorBlocksProperties.Where(x => x.iBayNumber == d.iBayNumber && x.sBuildingSide == d.sBuildingSide).Count() > 1)
                {
                    //d.iBayNumber++; //tu by sa dala napisat funkcia na najdenie volneho bay na umiesnenie dveri                    
                    int bayNum = GetFreeBayFor(d);
                    if (bayNum == -1) PFDMainWindow.ShowMessageBoxInPFDWindow($"Not possible to find free bay on this side. [{d.sBuildingSide}]");
                    else d.iBayNumber = bayNum;
                }
            }
        }
        private void CheckDoorsBays(DoorProperties d)
        {
            if (d.iBayNumber > d.Bays.Count) d.iBayNumber = 1;
            if (MDoorBlocksProperties.Where(x => x.iBayNumber == d.iBayNumber && x.sBuildingSide == d.sBuildingSide).Count() > 1)
            {
                //d.iBayNumber++; //tu by sa dala napisat funkcia na najdenie volneho bay na umiesnenie dveri                    
                int bayNum = GetFreeBayFor(d);
                if (bayNum == -1) PFDMainWindow.ShowMessageBoxInPFDWindow($"Not possible to find free bay on this side. [{d.sBuildingSide}]");
                else d.iBayNumber = bayNum;
            }
        }

        private void SetWindowsBays()
        {
            foreach (WindowProperties w in MWindowBlocksProperties)
            {
                if (w.sBuildingSide == "Front" && !w.Bays.SequenceEqual(frontBays)) w.Bays = frontBays;
                else if (w.sBuildingSide == "Back" && !w.Bays.SequenceEqual(backBays)) w.Bays = backBays;
                else if (w.sBuildingSide == "Left" && !w.Bays.SequenceEqual(leftRightBays)) w.Bays = leftRightBays;
                else if (w.sBuildingSide == "Right" && !w.Bays.SequenceEqual(leftRightBays)) w.Bays = leftRightBays;
            }
            CheckWindowsBays();
        }
        private void SetWindowsBays(WindowProperties w)
        {
            if (w.sBuildingSide == "Front" && !w.Bays.SequenceEqual(frontBays)) w.Bays = frontBays;
            else if (w.sBuildingSide == "Back" && !w.Bays.SequenceEqual(backBays)) w.Bays = backBays;
            else if (w.sBuildingSide == "Left" && !w.Bays.SequenceEqual(leftRightBays)) w.Bays = leftRightBays;
            else if (w.sBuildingSide == "Right" && !w.Bays.SequenceEqual(leftRightBays)) w.Bays = leftRightBays;

            CheckWindowsBays(w);
        }
        private void CheckWindowsBays()
        {
            foreach (WindowProperties w in MWindowBlocksProperties)
            {
                if (w.iBayNumber > w.Bays.Count) w.iBayNumber = 1;
                if (MWindowBlocksProperties.Where(x => x.iBayNumber == w.iBayNumber && x.sBuildingSide == w.sBuildingSide).Count() > 1)
                {
                    //w.iBayNumber++; //tu by sa dala napisat funkcia na najdenie volneho bay na umiesnenie okna
                    int bayNum = GetFreeBayFor(w);
                    if (bayNum == -1) PFDMainWindow.ShowMessageBoxInPFDWindow($"Not possible to find free bay on this side. [{w.sBuildingSide}]");
                    else w.iBayNumber = bayNum;
                }
            }
        }
        private void CheckWindowsBays(WindowProperties w)
        {
            if (w.iBayNumber > w.Bays.Count) w.iBayNumber = 1;
            if (MWindowBlocksProperties.Where(x => x.iBayNumber == w.iBayNumber && x.sBuildingSide == w.sBuildingSide).Count() > 1)
            {
                //w.iBayNumber++; //tu by sa dala napisat funkcia na najdenie volneho bay na umiesnenie okna
                int bayNum = GetFreeBayFor(w);
                if (bayNum == -1) PFDMainWindow.ShowMessageBoxInPFDWindow($"Not possible to find free bay on this side. [{w.sBuildingSide}]");
                else w.iBayNumber = bayNum;
            }
        }
        private int GetFreeBayFor(WindowProperties win)
        {
            foreach (int bayNum in win.Bays)
            {
                if (MWindowBlocksProperties.Where(x => x.iBayNumber == bayNum && x.sBuildingSide == win.sBuildingSide).Count() == 0) return bayNum;
            }
            return -1;
        }
        private int GetFreeBayFor(DoorProperties d)
        {
            foreach (int bayNum in d.Bays)
            {
                if (MDoorBlocksProperties.Where(x => x.iBayNumber == bayNum && x.sBuildingSide == d.sBuildingSide).Count() == 0) return bayNum;
            }
            return -1;
        }

        private void SetDoorsWindowsValidationProperties()
        {
            SetDoorsValidationProperties();
            SetWindowsValidationProperties();
        }
        private void SetDoorsValidationProperties()
        {
            CModel_PFD_01_GR model = (CModel_PFD_01_GR)this.Model;
            foreach (DoorProperties d in MDoorBlocksProperties)
            {
                d.SetValidationValues(MWallHeight, model.fL1_frame, model.fDist_FrontColumns, model.fDist_BackColumns);
            }
        }
        private void SetWindowsValidationProperties()
        {
            CModel_PFD_01_GR model = (CModel_PFD_01_GR)this.Model;
            foreach (WindowProperties w in MWindowBlocksProperties)
            {
                w.SetValidationValues(MWallHeight, model.fL1_frame, model.fDist_FrontColumns, model.fDist_BackColumns);
            }
        }

        private CComponentListVM _componentVM;
        private CPFDLoadInput _loadInput;
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public CPFDViewModel(int modelIndex, ObservableCollection<DoorProperties> doorBlocksProperties, ObservableCollection<WindowProperties> windowBlocksProperties, CComponentListVM componentVM, CPFDLoadInput loadInput)
        {
            IsSetFromCode = true;
            DoorBlocksProperties = doorBlocksProperties;
            WindowBlocksProperties = windowBlocksProperties;

            _componentVM = componentVM;
            SetComponentListAccordingToDoorsAndWindows();

            _componentVM.PropertyChanged += ComponentVM_PropertyChanged;
            ComponentList = _componentVM.ComponentList;

            _loadInput = loadInput;
            _loadInput.PropertyChanged += _loadInput_PropertyChanged;

            LightDirectional = false;
            LightPoint = false;
            LightSpot = false;
            LightAmbient = true;
            MaterialDiffuse = true;
            MaterialEmissive = false;
            DisplayMembers = true;
            DisplayJoints = false;
            DisplayPlates = false;
            DisplayConnectors = false;
            DisplayFoundations = false;
            DisplayFloorSlab = false;
            DisplayNodalSupports = false;
            DisplayMembersCenterLines = false;
            DisplaySolidModel = true;
            DisplayWireFrameModel = false;
            DisplayDistinguishedColorMember = false;
            DisplayTransparentModelMember = false;
            ColorsAccordingToMembers = true;
            ColorsAccordingToSections = false;

            ShowMemberID = true;
            ShowMemberRealLength = true;

            ShowLoads = false;
            ShowLoadsOnMembers = false;
            ShowLoadsOnGirts = true;
            ShowLoadsOnPurlins = true;
            ShowLoadsOnColumns = true;
            ShowLoadsOnFrameMembers = true;
            ShowNodalLoads = false;
            ShowSurfaceLoads = false;
            ShowLoadsLabels = false;
            ShowLoadsLabelsUnits = false;
            ShowGlobalAxis = true;
            ShowLocalMembersAxis = false;
            ShowSurfaceLoadsAxis = false;
            DisplayIn3DRatio = 0.003f;

            GenerateNodalLoads = true;
            GenerateLoadsOnGirts = true;
            GenerateLoadsOnPurlins = true;
            GenerateLoadsOnColumns = true;
            GenerateLoadsOnFrameMembers = true;
            GenerateSurfaceLoads = true;

            DeterminateCombinationResultsByFEMSolver = false;
            UseFEMSolverCalculationForSimpleBeam = false;
            DeterminateMemberLocalDisplacementsForULS = false;

            //nastavi sa default model type a zaroven sa nastavia vsetky property ViewModelu (samozrejme sa updatuje aj View) 
            //vid setter metoda pre ModelIndex
            ModelIndex = modelIndex;

            MModelCalculatedResultsValid = false;

            IsSetFromCode = false;

            _worker.DoWork += CalculateInternalForces;
            _worker.WorkerSupportsCancellation = true;
        }

        private void _loadInput_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetResultsAreNotValid();
        }

        private void ComponentVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedComponentIndex") return;
            else if (e.PropertyName == "ComponentDetailsList") return;
            else if (e.PropertyName == "MaterialDetailsList") return;

            if (e.PropertyName == "Section" || e.PropertyName == "Material" ||
                e.PropertyName == "Generate" || e.PropertyName == "Calculate" ||
                e.PropertyName == "Design")
            {
                SetResultsAreNotValid();
            }

            if (PropertyChanged != null) PropertyChanged(sender, e);
        }

        //tato metoda sa pouzila iba raz...podla mna je zbytocna a staci volat UpdateAll()
        public void CreateModel()
        {
            BuildingGeometryDataInput sBuildingGeometryData;

            // Centerline dimenions
            sBuildingGeometryData.fW = GableWidth;
            sBuildingGeometryData.fL = Length;
            sBuildingGeometryData.fH_1 = WallHeight;
            sBuildingGeometryData.fH_2 = fh2;

            sBuildingGeometryData.fRoofPitch_deg = RoofPitch_deg;

            // Total dimensions
            sBuildingGeometryData.fWidthTotal = GableWidth;
            sBuildingGeometryData.fLengthTotal = Length;
            sBuildingGeometryData.fEaveHeight = WallHeight;
            sBuildingGeometryData.fRidgeHeight = fh2;

            sBuildingGeometryData.iMainColumnFlyBracingEveryXXGirt = MainColumnFlyBracingPositionIndex;
            sBuildingGeometryData.iRafterFlyBracingEveryXXPurlin = RafterFlyBracingPositionIndex;
            sBuildingGeometryData.iEdgePurlin_ILS_Number = EdgePurlin_ILS_Number;
            sBuildingGeometryData.iGirt_ILS_Number = Girt_ILS_Number;
            sBuildingGeometryData.iPurlin_ILS_Number = Purlin_ILS_Number;
            sBuildingGeometryData.iFrontColumnFlyBracingEveryXXGirt = FrontColumnFlyBracingPositionIndex;
            sBuildingGeometryData.iBackColumnFlyBracingEveryXXGirt = BackColumnFlyBracingPositionIndex;
            sBuildingGeometryData.iGirtFrontSide_ILS_Number = GirtFrontSide_ILS_Number;
            sBuildingGeometryData.iGirtBackSide_ILS_Number = GirtBackSide_ILS_Number;

            MGenerateSurfaceLoads =
                MShowSurfaceLoadsAxis ||
                MGenerateLoadsOnGirts ||
                MGenerateLoadsOnPurlins ||
                MGenerateLoadsOnColumns ||
                MGenerateLoadsOnFrameMembers;

            // Create 3D model of structure including loading
            MModel = new CModel_PFD_01_GR(
                    sBuildingGeometryData,
                    //WallHeight,
                    //GableWidth,
                    //fL1,
                    Frames,
                    //fh2,
                    GirtDistance,
                    PurlinDistance,
                    ColumnDistance,
                    BottomGirtPosition,
                    FrontFrameRakeAngle,
                    BackFrameRakeAngle,
                    DoorBlocksProperties,
                    WindowBlocksProperties,
                    ComponentList,
                    GeneralLoad,
                    Wind,
                    Snow,
                    Eq,
                    MGenerateNodalLoads,
                    MGenerateLoadsOnGirts,
                    MGenerateLoadsOnPurlins,
                    MGenerateLoadsOnColumns,
                    MGenerateLoadsOnFrameMembers,
                    MGenerateSurfaceLoads);
        }

        public void Run()
        {
            if (!_worker.IsBusy) _worker.RunWorkerAsync();
        }

        private void CalculateInternalForces(object sender, DoWorkEventArgs e)
        {
            Calculate();
        }

        public void GenerateMemberLoadsIfNotGenerated()
        {
            CModel_PFD_01_GR model = (CModel_PFD_01_GR)Model;
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Generate loads if they are not generated
            DateTime start = DateTime.Now;
            if (debugging) System.Diagnostics.Trace.WriteLine("GenerateMemberLoadsIfNotGenerated: " + (DateTime.Now - start).TotalMilliseconds);
            if (!GenerateLoadsOnFrameMembers ||
                !GenerateLoadsOnGirts ||
                !GenerateLoadsOnPurlins ||
                !GenerateLoadsOnColumns)
            {
                //Toto tu je blbost, pretoze sa to aj tak zrube z nejakeho dovodu
                CMemberLoadGenerator loadGenerator = new CMemberLoadGenerator(model, GeneralLoad, Snow, Wind);
                if (debugging) System.Diagnostics.Trace.WriteLine("After CMemberLoadGenerator: " + (DateTime.Now - start).TotalMilliseconds);

                LoadCasesMemberLoads memberLoadsOnFrames = new LoadCasesMemberLoads();

                if (!GenerateLoadsOnFrameMembers)
                    memberLoadsOnFrames = loadGenerator.GetGenerateMemberLoadsOnFrames();
                if (debugging) System.Diagnostics.Trace.WriteLine("After GetListOfGenerateMemberLoadsOnFrames: " + (DateTime.Now - start).TotalMilliseconds);

                LoadCasesMemberLoads memberLoadsOnGirtsPurlinsColumns = new LoadCasesMemberLoads();

                if (!GenerateLoadsOnGirts || !GenerateLoadsOnPurlins || !GenerateLoadsOnColumns)
                    memberLoadsOnGirtsPurlinsColumns = loadGenerator.GetGeneratedMemberLoads(model.m_arrLoadCases, model.m_arrMembers);
                if (debugging) System.Diagnostics.Trace.WriteLine("After GetListOfGeneratedMemberLoads: " + (DateTime.Now - start).TotalMilliseconds);

                #region Merge Member Load Lists
                if ((GenerateLoadsOnGirts ||
                    GenerateLoadsOnPurlins ||
                    GenerateLoadsOnColumns) && GenerateLoadsOnFrameMembers)
                {
                    if (memberLoadsOnFrames.Count != memberLoadsOnGirtsPurlinsColumns.Count)
                    {
                        throw new Exception("Not all member load list in all load cases were generated for frames and single members.");
                    }

                    // Merge lists
                    memberLoadsOnFrames.Merge(memberLoadsOnGirtsPurlinsColumns); // Merge both to first LoadCasesMemberLoads
                    // Assign merged list of member loads to the load cases
                    loadGenerator.AssignMemberLoadListsToLoadCases(memberLoadsOnFrames);
                }
                if (debugging) System.Diagnostics.Trace.WriteLine("After AssignMemberLoadListsToLoadCases: " + (DateTime.Now - start).TotalMilliseconds);
                #endregion
            }
            if (debugging) System.Diagnostics.Trace.WriteLine("END OF GenerateMemberLoadsIfNotGenerated: " + (DateTime.Now - start).TotalMilliseconds);
        }

        private void Calculate()
        {
            SolverWindow.SetInputData();
            SolverWindow.Progress = 1;
            SolverWindow.UpdateProgress();

            DateTime start = DateTime.Now;
            if (debugging) System.Diagnostics.Trace.WriteLine("STARTING CALCULATE: " + (DateTime.Now - start).TotalMilliseconds);

            CModel_PFD_01_GR model = (CModel_PFD_01_GR)Model;

            CModelHelper.SetMembersAccordingTo(model.m_arrMembers, ComponentList);

            //tieto texty budeme zobrazovat vsetky members alebo len tie ktore su urcene pre vypocet?
            int membersCalcCount = CModelHelper.GetMembersSetForCalculationsCount(model.m_arrMembers);

            SolverWindow.SetCountsLabels(model.m_arrNodes.Length, membersCalcCount/*model.m_arrMembers.Length*/, model.m_arrConnectionJoints.Count, model.m_arrLoadCases.Length, model.m_arrLoadCombs.Length);
            SolverWindow.SetMemberDesignLoadCaseProgress(0, membersCalcCount);
            SolverWindow.SetMemberDesignLoadCombinationProgress(0, membersCalcCount);

            // Validate model before calculation (compare IDs)
            CModelHelper.ValidateModel(model);

            SolverWindow.Progress = 2;
            SolverWindow.UpdateProgress();

            if (debugging) System.Diagnostics.Trace.WriteLine("After validation: " + (DateTime.Now - start).TotalMilliseconds);
            // Tu by sa mal napojit 3D FEM vypocet v pripade ze budeme pocitat vsetko v 3D
            //RunFEMSOlver();

            SolverWindow.SetFrames();
            SolverWindow.SetFramesProgress(0, 0);
            if (!_componentVM.NoFrameMembersForCalculate())
            {
                // Calculation of frame model
                frameModels = model.GetFramesFromModel(); // Create models of particular frames
                if (debugging) System.Diagnostics.Trace.WriteLine("After frameModels = model.GetFramesFromModel(); " + (DateTime.Now - start).TotalMilliseconds);
                CFramesCalculations.RunFramesCalculations(frameModels, !DeterminateCombinationResultsByFEMSolver, SolverWindow);
                if (debugging) System.Diagnostics.Trace.WriteLine("After frameModels: " + (DateTime.Now - start).TotalMilliseconds);
            }

            if (DeterminateCombinationResultsByFEMSolver || UseFEMSolverCalculationForSimpleBeam)
            {
                SolverWindow.SetBeams();
                // Calculation of simple beam model
                beamSimpleModels = model.GetMembersFromModel(); // Create models of particular beams
                if (debugging) System.Diagnostics.Trace.WriteLine("After beamSimpleModels = model.GetMembersFromModel(); " + (DateTime.Now - start).TotalMilliseconds);
                CBeamsCalculations.RunBeamsCalculations(beamSimpleModels, !DeterminateCombinationResultsByFEMSolver, SolverWindow);
                if (debugging) System.Diagnostics.Trace.WriteLine("After beamSimpleModels: " + (DateTime.Now - start).TotalMilliseconds);
            }

            CMemberDesignCalculations memberDesignCalculations = new CMemberDesignCalculations(SolverWindow, model, UseCRSCGeometricalAxes, DeterminateCombinationResultsByFEMSolver, UseFEMSolverCalculationForSimpleBeam, DeterminateMemberLocalDisplacementsForULS, frameModels, beamSimpleModels);
            memberDesignCalculations.CalculateAll();
            SetDesignMembersLists(memberDesignCalculations);

            System.Diagnostics.Trace.WriteLine("end of calculations: " + (DateTime.Now - start).TotalMilliseconds);

            PFDMainWindow.UpdateResults();
        }

        private void GetMinAndMaxValueInTheArray(float[] array, out float min, out float max)
        {
            if (array != null)
            {
                min = max = array[0];

                foreach (float f in array)
                {
                    if (Math.Abs(f) > Math.Abs(min))
                        min = f;

                    if (Math.Abs(f) > Math.Abs(max))
                        max = f;
                }
            }
            else // Exception
            {
                min = max = float.MaxValue;
            }
        }

        private void GetMinAndMaxValueInTheArray(float[,] array, out float min, out float max)
        {
            if (array != null)
            {
                min = max = array[0, 0];

                foreach (float f in array)
                {
                    if (Math.Abs(f) > Math.Abs(min))
                        min = f;

                    if (Math.Abs(f) > Math.Abs(max))
                        max = f;
                }
            }
            else // Exception
            {
                min = max = float.MaxValue;
            }
        }

        private void SetDesignMembersLists(CMemberDesignCalculations mdc)
        {
            // TODO - toto budeme asi potrebovat vycistit, ukladame a nastavujeme toho zbytocne prilis vela
            // TODO pokial by sme mali private members a public properties, tak by sa dalo hned pozriet,ci sa pouziva property
            MemberInternalForcesInLoadCombinations = mdc.MemberInternalForcesInLoadCombinations;
            MemberDeflectionsInLoadCombinations = mdc.MemberDeflectionsInLoadCombinations;

            MemberInternalForcesInLoadCases = mdc.MemberInternalForcesInLoadCases;
            MemberDeflectionsInLoadCases = mdc.MemberDeflectionsInLoadCases;

            MemberDesignResults_ULS = mdc.MemberDesignResults_ULS;
            MemberDesignResults_SLS = mdc.MemberDesignResults_SLS;
            JointDesignResults_ULS = mdc.JointDesignResults_ULS;

            sDesignResults_ULSandSLS = mdc.sDesignResults_ULSandSLS;
            sDesignResults_ULS = mdc.sDesignResults_ULS;
            sDesignResults_SLS = mdc.sDesignResults_SLS;
        }

        private void SetComponentListAccordingToDoorsAndWindows()
        {
            SetComponentListAccordingToDoors();
            SetComponentListAccordingToWindows();
        }

        private void SetComponentListAccordingToDoors()
        {
            if (ModelHasPersonelDoor()) _componentVM.AddPersonelDoor();
            else _componentVM.RemovePersonelDoor();

            if (ModelHasRollerDoor()) _componentVM.AddRollerDoor();
            else _componentVM.RemoveRollerDoor();

        }

        private void SetComponentListAccordingToWindows()
        {
            if (ModelHasWindow()) _componentVM.AddWindow();
            else _componentVM.RemoveWindow();
        }

        private bool ModelHasPersonelDoor()
        {
            foreach (DoorProperties d in DoorBlocksProperties)
            {
                if (d.sDoorType == "Personnel Door") return true;
            }
            return false;
        }

        private bool ModelHasRollerDoor()
        {
            foreach (DoorProperties d in DoorBlocksProperties)
            {
                if (d.sDoorType == "Roller Door") return true;
            }
            return false;
        }

        private bool ModelHasWindow()
        {
            if (WindowBlocksProperties == null) return false;

            return WindowBlocksProperties.Count > 0;
        }

        private void SetIsEnabledLocalMembersAxis()
        {
            //ak su zapnute Members, ale nie je ziaden z checkboxov Display Members Centerline, Solid Model, Wireframe Model zapnuty, 
            //tak by malo byt zobrazenie os Local Member Axis disabled.            
            if (m_DisplayMembers)
            {
                if (m_DisplayMembersCenterLines || m_DisplaySolidModel || m_DisplayWireFrameModel) IsEnabledLocalMembersAxis = true;
                else { IsEnabledLocalMembersAxis = false; }
            }
            else { IsEnabledLocalMembersAxis = false; }

            if (!IsEnabledLocalMembersAxis && ShowLocalMembersAxis) ShowLocalMembersAxis = false;
        }

        private void SetIsEnabledSurfaceLoadsAxis()
        {
            //Podobne ak su sice zapnute Surface loads, ale nie su zapnute Loads ako celok, tak by Surface Loads Axis malo byt disabled.
            if (MShowSurfaceLoads && MShowLoads) IsEnabledSurfaceLoadsAxis = true;
            else IsEnabledSurfaceLoadsAxis = false;

            if (!IsEnabledSurfaceLoadsAxis && ShowSurfaceLoadsAxis) ShowSurfaceLoadsAxis = false;
        }

        private void SetResultsAreNotValid()
        {
            ModelCalculatedResultsValid = false;
        }

        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleDoorPropertiesPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "sBuildingSide")
            {
                SetResultsAreNotValid();
                if (sender is DoorProperties) SetDoorsBays(sender as DoorProperties);
                if (sender is WindowProperties) SetWindowsBays(sender as WindowProperties);
            }
            else if (e.PropertyName == "iBayNumber")
            {
                SetResultsAreNotValid();
                if (sender is DoorProperties) CheckDoorsBays(sender as DoorProperties);
                if (sender is WindowProperties) CheckWindowsBays(sender as WindowProperties);
            }
            else if (e.PropertyName == "sDoorType")
            {
                SetResultsAreNotValid();
                SetComponentListAccordingToDoors();
            }

            if (e.PropertyName == "fDoorsHeight" || e.PropertyName == "fDoorsWidth" ||
                e.PropertyName == "fDoorCoordinateXinBlock")
            {
                SetResultsAreNotValid();
            }

            this.PropertyChanged(sender, e);
        }

        private void HandleWindowPropertiesPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            SetResultsAreNotValid();
            this.PropertyChanged(sender, e);
        }

        private void HandleComponentInfoPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) this.PropertyChanged(sender, e);
        }

        public CModelData GetModelData()
        {
            CModelData data = new CModelData();
            data.ModelIndex = ModelIndex;
            data.GableWidth = MGableWidth;
            data.Length = MLength;
            data.WallHeight = MWallHeight;
            data.RoofPitch_deg = MRoofPitch_deg;
            data.Frames = MFrames;
            data.GirtDistance = MGirtDistance;
            data.PurlinDistance = MPurlinDistance;
            data.ColumnDistance = MColumnDistance;
            data.BottomGirtPosition = MBottomGirtPosition;
            data.FrontFrameRakeAngle = MFrontFrameRakeAngle;
            data.BackFrameRakeAngle = MBackFrameRakeAngle;
            data.RoofCladdingIndex = MRoofCladdingIndex;
            data.RoofCladdingColorIndex = MRoofCladdingColorIndex;
            data.RoofCladdingThicknessIndex = MRoofCladdingThicknessIndex;
            data.WallCladdingIndex = MWallCladdingIndex;
            data.WallCladdingColorIndex = MWallCladdingColorIndex;
            data.WallCladdingThicknessIndex = MWallCladdingThicknessIndex;
            data.LoadCaseIndex = MLoadCaseIndex;
            data.IFrontColumnNoInOneFrame = iFrontColumnNoInOneFrame;
            data.UseCRSCGeometricalAxes = UseCRSCGeometricalAxes;

            data.GeneralLoad = GeneralLoad;
            data.Wind = Wind;
            data.Snow = Snow;
            data.Eq = Eq;

            data.DoorBlocksProperties = DoorBlocksProperties;
            data.WindowBlocksProperties = WindowBlocksProperties;

            data.ComponentList = ComponentList;
            data.Model = Model;

            //Load input
            data.Location = _loadInput.ListLocations[_loadInput.LocationIndex];
            data.DesignLife = _loadInput.ListDesignLife[_loadInput.DesignLifeIndex];
            data.ImportanceClass = _loadInput.ListImportanceClass[_loadInput.ImportanceClassIndex];
            data.SnowRegion = _loadInput.ListSnowRegion[_loadInput.SnowRegionIndex];
            data.ExposureCategory = _loadInput.ListExposureCategory[_loadInput.ExposureCategoryIndex];
            data.WindRegion = _loadInput.ListWindRegion[_loadInput.WindRegionIndex];
            data.TerrainCategory = _loadInput.ListTerrainCategory[_loadInput.TerrainCategoryIndex];
            data.SiteSubSoilClass = _loadInput.ListSiteSubSoilClass[_loadInput.SiteSubSoilClassIndex];
            data.SiteElevation = _loadInput.SiteElevation;
            data.R_SLS = _loadInput.R_SLS;
            data.AnnualProbabilitySLS = _loadInput.AnnualProbabilitySLS;
            data.AdditionalDeadActionWall = _loadInput.AdditionalDeadActionWall;
            data.AdditionalDeadActionRoof = _loadInput.AdditionalDeadActionRoof;
            data.ImposedActionRoof = _loadInput.ImposedActionRoof;
            data.AnnualProbabilityULS_Snow = _loadInput.AnnualProbabilityULS_Snow;
            data.R_ULS_Snow = _loadInput.R_ULS_Snow;
            data.AnnualProbabilityULS_Wind = _loadInput.AnnualProbabilityULS_Wind;
            data.R_ULS_Wind = _loadInput.R_ULS_Wind;
            data.AnnualProbabilityULS_EQ = _loadInput.AnnualProbabilityULS_EQ;
            data.R_ULS_EQ = _loadInput.R_ULS_EQ;
            data.FaultDistanceDmin = _loadInput.FaultDistanceDmin;
            data.FaultDistanceDmax = _loadInput.FaultDistanceDmax;
            data.ZoneFactorZ = _loadInput.ZoneFactorZ;
            data.PeriodAlongXDirectionTx = _loadInput.PeriodAlongXDirectionTx;
            data.PeriodAlongYDirectionTy = _loadInput.PeriodAlongYDirectionTy;
            data.SpectralShapeFactorChTx = _loadInput.SpectralShapeFactorChTx;
            data.SpectralShapeFactorChTy = _loadInput.SpectralShapeFactorChTy;

            data.sDesignResults_ULSandSLS = sDesignResults_ULSandSLS;
            //data.sDesignResults_ULS = sDesignResults_ULS;
            //data.sDesignResults_SLS = sDesignResults_SLS;
            
            data.dictULSDesignResults = GetDesignResultsULS();
            data.dictSLSDesignResults = GetDesignResultsSLS();
            GetGoverningMemberJointsDesignDetails(out data.dictStartJointResults, out data.dictEndJointResults);

            data.MemberInternalForcesInLoadCombinations = MemberInternalForcesInLoadCombinations;
            data.MemberDeflectionsInLoadCombinations = MemberDeflectionsInLoadCombinations;
            data.frameModels = frameModels;

            return data;
        }

        private Dictionary<EMemberType_FS_Position, CCalculMember> GetDesignResultsULS()
        {
            Dictionary<EMemberType_FS_Position, CCalculMember> dictULSDesignResults = new Dictionary<EMemberType_FS_Position, CCalculMember>();

            foreach (CMemberGroup mGr in Model.listOfModelMemberGroups)
            {
                CLoadCombination governingLoadComb = sDesignResults_ULS.DesignResults[mGr.MemberType_FS_Position].GoverningLoadCombination;
                if (governingLoadComb == null) continue;
                CMember governingMember = sDesignResults_ULS.DesignResults[mGr.MemberType_FS_Position].MemberWithMaximumDesignRatio;
                if (governingMember == null) continue;
                CCalculMember cGoverningMemberResultsULS;
                CalculateGoverningMemberDesignDetails(UseCRSCGeometricalAxes, MemberDesignResults_ULS, governingMember, governingLoadComb.ID, out cGoverningMemberResultsULS);
                dictULSDesignResults.Add(mGr.MemberType_FS_Position, cGoverningMemberResultsULS);
            }
            return dictULSDesignResults;
        }

        private Dictionary<EMemberType_FS_Position, CCalculMember> GetDesignResultsSLS()
        {
            Dictionary<EMemberType_FS_Position, CCalculMember> dictSLSDesignResults = new Dictionary<EMemberType_FS_Position, CCalculMember>();

            foreach (CMemberGroup mGr in Model.listOfModelMemberGroups)
            {
                CLoadCombination governingLoadComb = sDesignResults_SLS.DesignResults[mGr.MemberType_FS_Position].GoverningLoadCombination;
                if (governingLoadComb == null) continue;
                CMember governingMember = sDesignResults_SLS.DesignResults[mGr.MemberType_FS_Position].MemberWithMaximumDesignRatio;
                if (governingMember == null) continue;
                CCalculMember cGoverningMemberResultsSLS;
                CalculateGoverningMemberDesignDetails(UseCRSCGeometricalAxes, MemberDesignResults_SLS, governingMember, governingLoadComb.ID, mGr, out cGoverningMemberResultsSLS);
                dictSLSDesignResults.Add(mGr.MemberType_FS_Position, cGoverningMemberResultsSLS);
            }
            return dictSLSDesignResults;
        }

        // Calculate governing member design ratio
        private void CalculateGoverningMemberDesignDetails(bool bUseCRSCGeometricalAxes, List<CMemberLoadCombinationRatio_ULS> DesignResults, CMember m, int loadCombID, out CCalculMember cGoverningMemberResults)
        {
            CMemberLoadCombinationRatio_ULS res = DesignResults.FirstOrDefault(i => i.Member.ID == m.ID  && i.LoadCombination.ID == loadCombID);
            cGoverningMemberResults = new CCalculMember(false, bUseCRSCGeometricalAxes, res.DesignInternalForces, m, res.DesignBucklingLengthFactors, res.DesignMomentValuesForCb);
        }

        public void CalculateGoverningMemberDesignDetails(bool bUseCRSCGeometricalAxes, List<CMemberLoadCombinationRatio_SLS> DesignResults, CMember m, int loadCombID, CMemberGroup GroupOfMembersWithSelectedType, out CCalculMember cGoverningMemberResults)
        {
            CMemberLoadCombinationRatio_SLS res = DesignResults.FirstOrDefault(i => i.Member.ID == m.ID && i.LoadCombination.ID == loadCombID);

            // Limit zavisi od typu zatazenia (load combination) a typu pruta
            int iDelfectionLimitFraction_Denominator = GroupOfMembersWithSelectedType.DeflectionLimitFraction_Denominator_Total;
            float fDeflectionLimit = GroupOfMembersWithSelectedType.DeflectionLimit_Total;

            //Mato??? toto nechapem, potrebujem vysvetlit
            // TO Ondrej: Ak je v kombinacii len load case typu permanent load, potrebujem nastavit do fDeflectionLimit hodnotu DeflectionLimit_PermanentLoad definovanu pre group, pre ine kombinacie pouzijem total
            // V nasom pripade mame v SLS len jednu taku kombinaciu, ta ma cislo 41, TO 41 by malo byt nahradene niecim inym, nie len ID, ked sa zmeni pocet kombinacii tak sa to pokazi
            // Da sa pouzit metoda z triedy CLoadCombination IsCombinationOfPermanentLoadCasesOnly()

            if (loadCombID == 41) // TODO Combination of permanent load (TODO - nacitat/zistit spravne parametre kombinacie (je typu SLS a obsahuje len load cases typu permanent), neurcovat podla cisla ID)
            {
                iDelfectionLimitFraction_Denominator = GroupOfMembersWithSelectedType.DeflectionLimitFraction_Denominator_PermanentLoad;
                fDeflectionLimit = GroupOfMembersWithSelectedType.DeflectionLimit_PermanentLoad;
            }

            cGoverningMemberResults = new CCalculMember(false, bUseCRSCGeometricalAxes, res.DesignDeflections, m, iDelfectionLimitFraction_Denominator, fDeflectionLimit);
        }

        public void GetGoverningMemberJointsDesignDetails(out Dictionary<EMemberType_FS_Position, CCalculJoint> dictStartJointResults, out Dictionary<EMemberType_FS_Position, CCalculJoint> dictEndJointResults)
        {
            dictStartJointResults = new Dictionary<EMemberType_FS_Position, CCalculJoint>();
            dictEndJointResults = new Dictionary<EMemberType_FS_Position, CCalculJoint>();

            if (JointDesignResults_ULS == null) return;

            foreach (CMemberGroup mGr in Model.listOfModelMemberGroups)
            {
                CLoadCombination governingLoadComb = sDesignResults_ULS.DesignResults[mGr.MemberType_FS_Position].GoverningLoadCombination;
                if (governingLoadComb == null) continue;
                CMember governingMember = sDesignResults_ULS.DesignResults[mGr.MemberType_FS_Position].MemberWithMaximumDesignRatio;
                if (governingMember == null) continue;

                CConnectionJointTypes cjStart = null;
                CConnectionJointTypes cjEnd = null;
                Model.GetModelMemberStartEndConnectionJoints(governingMember, out cjStart, out cjEnd);

                CJointLoadCombinationRatio_ULS resStart = JointDesignResults_ULS.FirstOrDefault(i => i.Member.ID == governingMember.ID && i.LoadCombination.ID == governingLoadComb.ID && i.Joint.m_Node.ID == cjStart.m_Node.ID);
                CJointLoadCombinationRatio_ULS resEnd = JointDesignResults_ULS.FirstOrDefault(i => i.Member.ID == governingMember.ID && i.LoadCombination.ID == governingLoadComb.ID && i.Joint.m_Node.ID == cjEnd.m_Node.ID);
                if (resStart == null) continue;
                if (resEnd == null) continue;

                CCalculJoint cGoverningMemberStartJointResults = new CCalculJoint(false, UseCRSCGeometricalAxes, cjStart, resStart.DesignInternalForces, true);
                CCalculJoint cGoverningMemberEndJointResults = new CCalculJoint(false, UseCRSCGeometricalAxes, cjEnd, resEnd.DesignInternalForces, true);

                dictStartJointResults.Add(mGr.MemberType_FS_Position, cGoverningMemberStartJointResults);
                dictEndJointResults.Add(mGr.MemberType_FS_Position, cGoverningMemberEndJointResults);
            }
        }
    }
}
