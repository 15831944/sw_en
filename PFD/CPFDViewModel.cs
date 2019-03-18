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
        private int MLoadCaseIndex;

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
        private bool MShowSurfaceLoadsAxis;

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
        // Local member load direction used for load definition, calculation of internal forces and design
        // Use geometrical or principal axes of cross-section to define load direction etc.
        private bool MUseCRSCGeometricalAxes = true;

        private ObservableCollection<DoorProperties> MDoorBlocksProperties;
        private ObservableCollection<WindowProperties> MWindowBlocksProperties;
        private List<string> MBuildingSides;
        private List<string> MDoorsTypes;
        
        // Popis pre Ondreja - doors and windows

        // GUI
        // Building Side - combobox s polozkami "Left", "Right", "Front", "Back"
        // Bay Number - combobox s polozkami 1 - n_lefright alebo 1 - n_front alebo 1 - n_back, takze v kazdom riadku moze byt v comboboxe iny zoznam podla toho ktora strana ma kolko bays (bays - segmenty medzi stlpami, cislovane v smere kladnej osi X, resp. Y)

        // v CModel_PFD_01_GR su premenne z ktorych sa maximalne cislo bay da urcit
        // (iFrameNo - 1)
        // (iFrontColumnNoInOneFrame + 1)
        // (iBackColumnNoInOneFrame + 1)

        // Door Type - combobox s polozkami Personnel Door, Roller Door
        // Personnel door su len z jedneho prierezu BOX 10075
        // Roller door mozu mat dva prierezy - door trimmer C270115btb a niekedy aj door lintel z C27095 (podla toho aka je vzdialenost medzi hornou hranou dveri a najblizsim girt nad tym)
        // U blokov sa momentalne moze a nemusi vytvorit crsc pre girt, ten sa potom ale nepridava do celkoveho pola m_arrCrSc pretoze tam uz je

        // dalsie polozky v tabulke su float, moze tam byt validacia ze napriklad poloha dveri v bay + sirka dveri nemoze byt vacsia nez sirka bay (L1) alebo ze vyska dveri nemoze byt vacsia nez vyska steny budovy atd

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

                IsSetFromCode = false;

                //tieto riadky by som tu najradsej nemal, resp. ich nejako spracoval ako dalsie property
                fL1 = MLength / (MFrames - 1);
                fRoofPitch_radians = MRoofPitch_deg * MathF.fPI / 180f;
                fh2 = MWallHeight + 0.5f * MGableWidth * (float)Math.Tan(fRoofPitch_radians);

                RoofCladdingIndex = 1;
                RoofCladdingColorIndex = 22;
                WallCladdingIndex = 0;
                WallCladdingColorIndex = 22;

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
                }

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
                    int iFrontColumnNoInOneFrame = 2 * iOneRafterFrontColumnNo;
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

                NotifyPropertyChanged("WallCladdingThicknessIndex");
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
                NotifyPropertyChanged("UseFEMSolverCalculationForSimpleBeam");
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
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NotifyPropertyChanged("DoorBlocksProperties_CollectionChanged");
            }            
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
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                NotifyPropertyChanged("WindowBlocksProperties_CollectionChanged");
            }
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


        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public CPFDViewModel(int modelIndex, ObservableCollection<DoorProperties> doorBlocksProperties, ObservableCollection<WindowProperties> windowBlocksProperties)
        {
            IsSetFromCode = true;

            DoorBlocksProperties = doorBlocksProperties;
            WindowBlocksProperties = windowBlocksProperties;

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

            //nastavi sa default model type a zaroven sa nastavia vsetky property ViewModelu (samozrejme sa updatuje aj View) 
            //vid setter metoda pre ModelIndex
            ModelIndex = modelIndex;

            IsSetFromCode = false;

            _worker.DoWork += CalculateInternalForces;
            _worker.WorkerSupportsCancellation = true;
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

            SolverWindow.SetCountsLabels(model.m_arrNodes.Length, model.m_arrMembers.Length, model.m_arrConnectionJoints.Count, model.m_arrLoadCases.Length, model.m_arrLoadCombs.Length);
            SolverWindow.SetMemberDesignLoadCaseProgress(0, model.m_arrMembers.Length);
            SolverWindow.SetMemberDesignLoadCombinationProgress(0, model.m_arrMembers.Length);

            // Validate model before calculation (compare IDs)
            CModelHelper.ValidateModel(model);

            SolverWindow.Progress = 2;
            SolverWindow.UpdateProgress();

            if (debugging) System.Diagnostics.Trace.WriteLine("After validation: " + (DateTime.Now - start).TotalMilliseconds);
            // Tu by sa mal napojit 3D FEM vypocet v pripade ze budeme pocitat vsetko v 3D
            //RunFEMSOlver();

            SolverWindow.SetFrames();
            // Calculation of frame model
            frameModels = model.GetFramesFromModel(); // Create models of particular frames
            if (debugging) System.Diagnostics.Trace.WriteLine("After frameModels = model.GetFramesFromModel(); " + (DateTime.Now - start).TotalMilliseconds);
            CFramesCalculations.RunFramesCalculations(frameModels, !DeterminateCombinationResultsByFEMSolver, SolverWindow);
            if (debugging) System.Diagnostics.Trace.WriteLine("After frameModels: " + (DateTime.Now - start).TotalMilliseconds);

            if (DeterminateCombinationResultsByFEMSolver || UseFEMSolverCalculationForSimpleBeam)
            {
                SolverWindow.SetBeams();
                // Calculation of simple beam model
                beamSimpleModels = model.GetMembersFromModel(); // Create models of particular beams
                if (debugging) System.Diagnostics.Trace.WriteLine("After beamSimpleModels = model.GetMembersFromModel(); " + (DateTime.Now - start).TotalMilliseconds);
                CBeamsCalculations.RunBeamsCalculations(beamSimpleModels, !DeterminateCombinationResultsByFEMSolver, SolverWindow);
                if (debugging) System.Diagnostics.Trace.WriteLine("After beamSimpleModels: " + (DateTime.Now - start).TotalMilliseconds);
            }

            CMemberDesignCalculations memberDesignCalculations = new CMemberDesignCalculations(SolverWindow, model, UseCRSCGeometricalAxes, DeterminateCombinationResultsByFEMSolver, UseFEMSolverCalculationForSimpleBeam, frameModels, beamSimpleModels);
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
            MemberInternalForcesInLoadCases = mdc.MemberInternalForcesInLoadCases;
            MemberDeflectionsInLoadCases = mdc.MemberDeflectionsInLoadCases;
            MemberDesignResults_ULS = mdc.MemberDesignResults_ULS;
            MemberDesignResults_SLS = mdc.MemberDesignResults_SLS;
            JointDesignResults_ULS = mdc.JointDesignResults_ULS;
        }

        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleDoorPropertiesPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            this.PropertyChanged(sender, e);
        }
        private void HandleWindowPropertiesPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            this.PropertyChanged(sender, e);
        }
    }
}
