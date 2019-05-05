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
using DATABASE;
using DATABASE.DTO;
using System.Windows.Media;

namespace PFD
{
    public class CComponentListVM : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<CComponentInfo> MComponentList;
        private int MSelectedComponentIndex;

        private List<string> MSections;
        private List<string> MColors;
        private List<string> MSectionsForColumnsOrRafters;
        private List<string> MSectionsForGirtsOrPurlins;
        private List<string> MSectionsForDoorOrWindowFrame;
        private List<string> MSectionsForRollerDoorTrimmer;
        private List<string> MSectionsForRollerDoorLintel;

        private List<string> MMaterials;
        private List<CSectionPropertiesText> m_ComponentDetailsList;
        private List<CMaterialPropertiesText> m_MaterialDetailsList;

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public ObservableCollection<CComponentInfo> ComponentList
        {
            get
            {
                return MComponentList;
            }

            set
            {
                MComponentList = value;
                foreach (CComponentInfo ci in ComponentList)
                {
                    ci.PropertyChanged += ComponentListItem_PropertyChanged;
                }
                NotifyPropertyChanged("ComponentList");
            }
        }

        private void ComponentListItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CComponentInfo)
            {
                CComponentInfo cInfo = sender as CComponentInfo;
                if (cInfo.IsSetFromCode) return;
                if (e.PropertyName == "GenerateIsEnabled") return;

                if (e.PropertyName == "Material") SetComponentDetails();
                else if (e.PropertyName == "Section") SetComponentDetails();

                if (e.PropertyName == "Generate")
                {
                    if (!ValidateGirts()) { cInfo.IsSetFromCode = true; cInfo.Generate = !cInfo.Generate; ValidateGirts(); cInfo.IsSetFromCode = false; return; }
                    SetGirtsAndColumns(cInfo);
                }
            }

            PropertyChanged(sender, e);
        }



        private bool ValidateGirts()
        {
            //To Mato - ja osobne tejto podmienke proste nerozumiem
            //Ak je zaskrtnute generovanie front girts alebo back girts musia byt zaskrtnute aj girt (teda side wall)
            CComponentInfo girt = ComponentList.First(c => c.ComponentName == "Girt");
            CComponentInfo girtFront = ComponentList.First(c => c.ComponentName == "Girt - Front Side");
            CComponentInfo girtBack = ComponentList.First(c => c.ComponentName == "Girt - Back Side");

            //ak je front aj back false tak vtedy dovolit editovat girt
            if (!girtFront.Generate.Value && !girtBack.Generate.Value) { girt.GenerateIsEnabled = true; girt.GenerateIsReadonly = false; }
            else { girt.GenerateIsEnabled = false; girt.GenerateIsReadonly = true; }

            if (girt.GenerateIsEnabled && !girt.Generate.Value) { girtFront.GenerateIsEnabled = false; girtFront.GenerateIsReadonly = true; girtBack.GenerateIsEnabled = false; girtBack.GenerateIsReadonly = true; }
            else { girtFront.GenerateIsEnabled = true; girtFront.GenerateIsReadonly = false; girtBack.GenerateIsEnabled = true; girtBack.GenerateIsReadonly = false; }

            if (girt.Generate.Value)
            {
                return true;
            }
            else
            {
                if (girtFront.Generate.Value || girtBack.Generate.Value) return false;
            }

            return true;
        }

        private void SetGirtsAndColumns(CComponentInfo cInfo)
        {
            //Ak je zaskrtnute generovanie girts front side musia byt zapnute columns front side, a podobne pre back side girts a back side columns.
            if (cInfo.ComponentName == "Column - Front Side")
            {
                CComponentInfo girtFront = ComponentList.First(c => c.ComponentName == "Girt - Front Side");
                if (girtFront.Generate != cInfo.Generate) girtFront.Generate = cInfo.Generate;
            }
            else if (cInfo.ComponentName == "Column - Back Side")
            {
                CComponentInfo girtBack = ComponentList.First(c => c.ComponentName == "Girt - Back Side");
                if (girtBack.Generate != cInfo.Generate) girtBack.Generate = cInfo.Generate;
            }
            else if (cInfo.ComponentName == "Girt - Front Side" && cInfo.Generate.Value) //iba ked zapnem Girt tak sa musi zapnut aj column
            {
                CComponentInfo columnFront = ComponentList.First(c => c.ComponentName == "Column - Front Side");
                if (columnFront.Generate != cInfo.Generate) columnFront.Generate = cInfo.Generate;
            }
            else if (cInfo.ComponentName == "Girt - Back Side" && cInfo.Generate.Value) //iba ked zapnem Girt tak sa musi zapnut aj column
            {
                CComponentInfo columnBack = ComponentList.First(c => c.ComponentName == "Column - Back Side");
                if (columnBack.Generate != cInfo.Generate) columnBack.Generate = cInfo.Generate;
            }
        }

        public List<CSectionPropertiesText> ComponentDetailsList
        {
            get
            {
                if (m_ComponentDetailsList == null)
                {
                    m_ComponentDetailsList = CSectionManager.LoadSectionPropertiesNamesSymbolsUnits();
                }
                return m_ComponentDetailsList;
            }

            set
            {
                m_ComponentDetailsList = value;
                NotifyPropertyChanged("ComponentDetailsList");
            }
        }
        public List<CMaterialPropertiesText> MaterialDetailsList
        {
            get
            {
                if (m_MaterialDetailsList == null)
                {
                    m_MaterialDetailsList = CMaterialManager.LoadMaterialPropertiesNamesSymbolsUnits();
                }
                return m_MaterialDetailsList;
            }

            set
            {
                m_MaterialDetailsList = value;
                NotifyPropertyChanged("MaterialDetailsList");
            }
        }

        private List<string> Sections
        {
            get
            {
                if (MSections == null)
                {
                    MSections = CDatabaseManager.GetStringList("SectionsSQLiteDB", "tableSections_m", "sectionName_short");
                }
                return MSections;
            }
        }

        // Nastavit pre rozne typy component iny obsah comboboxu "sections"
        // Mozno by to mohlo byt predpripravene v nejakom zozname sectionID v databaze pre kazdy prefix,
        // vid MDBModels - tabulka componentPrefixes a MDBSections, kazdy prefix (typ pruta) by potom obsahoval zoznam ID prierezov ktore mu mozu byt priradene

        /*
        -----------------------------------------
        ID  section   sectionName
        -----------------------------------------
        1   10075     Box 10075
        2   27055     C 27055
        3   27095     C 27095
        4   27095n    C 27095n
        5   270115    C 270115
        6   270115btb C 270115 back to back
        7   270115n   C 270115 nested
        8   50020     C 50020
        9   50020n    C 50020 nested
        10  63020     Box 63020
        11  63020s1   Box 63020 single stiffener
        12  63020s2   Box 63020 double stiffener
        */

        public List<string> SectionsForColumnsOrRafters
        {
            get
            {
                if (MSectionsForColumnsOrRafters == null)
                {
                    MSectionsForColumnsOrRafters = new List<string>(8);
                    MSectionsForColumnsOrRafters.Add(Sections[3]);   // DB ID 4
                    MSectionsForColumnsOrRafters.Add(Sections[4]);   // DB ID 5
                    MSectionsForColumnsOrRafters.Add(Sections[6]);   // DB ID 7
                    MSectionsForColumnsOrRafters.Add(Sections[7]);   // DB ID 8
                    MSectionsForColumnsOrRafters.Add(Sections[8]);   // DB ID 9
                    MSectionsForColumnsOrRafters.Add(Sections[9]);   // DB ID 10
                    MSectionsForColumnsOrRafters.Add(Sections[10]);  // DB ID 11
                    MSectionsForColumnsOrRafters.Add(Sections[11]);  // DB ID 12
                }
                return MSectionsForColumnsOrRafters;
            }
        }

        public List<string> SectionsForGirtsOrPurlins
        {
            get
            {
                if (MSectionsForGirtsOrPurlins == null)
                {
                    MSectionsForGirtsOrPurlins = new List<string>(6);
                    MSectionsForGirtsOrPurlins.Add(Sections[2]);   // DB ID 3
                    MSectionsForGirtsOrPurlins.Add(Sections[3]);   // DB ID 4
                    MSectionsForGirtsOrPurlins.Add(Sections[4]);   // DB ID 5
                    MSectionsForGirtsOrPurlins.Add(Sections[6]);   // DB ID 7
                    MSectionsForGirtsOrPurlins.Add(Sections[7]);   // DB ID 8
                    MSectionsForGirtsOrPurlins.Add(Sections[8]);   // DB ID 9
                }
                return MSectionsForGirtsOrPurlins;
            }
        }

        public List<string> SectionsForDoorOrWindowFrame
        {
            get
            {
                if (MSectionsForDoorOrWindowFrame == null)
                {
                    MSectionsForDoorOrWindowFrame = new List<string>(1);
                    MSectionsForDoorOrWindowFrame.Add(Sections[0]); // DB ID 1

                }
                return MSectionsForDoorOrWindowFrame;
            }
        }

        public List<string> SectionsForRollerDoorTrimmer
        {
            get
            {
                if (MSectionsForRollerDoorTrimmer == null)
                {
                    MSectionsForRollerDoorTrimmer = new List<string>(1);
                    MSectionsForRollerDoorTrimmer.Add(Sections[5]); // DB ID 6

                }
                return MSectionsForRollerDoorTrimmer;
            }
        }
        public List<string> SectionsForRollerDoorLintel
        {
            get
            {
                if (MSectionsForRollerDoorLintel == null)
                {
                    MSectionsForRollerDoorLintel = new List<string>(1);
                    MSectionsForRollerDoorLintel.Add(Sections[2]); // DB ID 3

                }
                return MSectionsForRollerDoorLintel;
            }
        }

        public int SelectedComponentIndex
        {
            get
            {
                return MSelectedComponentIndex;
            }

            set
            {
                MSelectedComponentIndex = value;
                SetComponentDetails();
                NotifyPropertyChanged("SelectedComponentIndex");
            }
        }

        public List<string> Materials
        {
            get
            {
                if (MMaterials == null) MMaterials = CMaterialManager.GetMaterialTypesList();
                return MMaterials;
            }
        }

        public List<string> Colors
        {
            get
            {
                // TODO - zaviest farby do databazy prierezov a databazy component (pre komponenty je uz v databaze definicia podla RGB)
                // TODO - pridame este jeden stlpec read-only pre farbu prierezu, bude sa menit podla comboboxu cross-section
                
                if (MColors == null) MColors = new List<string>() {
                "AliceBlue",
                "AntiqueWhite",
                "Aqua",
                "Aquamarine",
                "Azure",
                "Beige",
                "Bisque",
                "Black",
                "BlanchedAlmond",
                "Blue",
                "BlueViolet",
                "Brown",
                "BurlyWood",
                "CadetBlue",
                "Chartreuse",
                "Chocolate",
                "Coral",
                "CornflowerBlue",
                "Cornsilk",
                "Crimson",
                "Cyan",
                "DarkBlue",
                "DarkCyan",
                "DarkGoldenrod",
                "DarkGray",
                "DarkGreen",
                "DarkKhaki",
                "DarkMagenta",
                "DarkOliveGreen",
                "DarkOrange",
                "DarkOrchid",
                "DarkRed",
                "DarkSalmon",
                "DarkSeaGreen",
                "DarkSlateBlue",
                "DarkSlateGray",
                "DarkTurquoise",
                "DarkViolet",
                "DeepPink",
                "DeepSkyBlue",
                "DimGray",
                "DodgerBlue",
                "Firebrick",
                "FloralWhite",
                "ForestGreen",
                "Fuchsia",
                "Gainsboro",
                "GhostWhite",
                "Gold",
                "Goldenrod",
                "Gray",
                "Green",
                "GreenYellow",
                "Honeydew",
                "HotPink",
                "IndianRed",
                "Indigo",
                "Ivory",
                "Khaki",
                "Lavender",
                "LavenderBlush",
                "LawnGreen",
                "LemonChiffon",
                "LightBlue",
                "LightCoral",
                "LightCyan",
                "LightGoldenrodYellow",
                "LightGray",
                "LightGreen",
                "LightPink",
                "LightSalmon",
                "LightSeaGreen",
                "LightSkyBlue",
                "LightSlateGray",
                "LightSteelBlue",
                "LightYellow",
                "Lime",
                "LimeGreen",
                "Linen",
                "Magenta",
                "Maroon",
                "MediumAquamarine",
                "MediumBlue",
                "MediumOrchid",
                "MediumPurple",
                "MediumSeaGreen",
                "MediumSlateBlue",
                "MediumSpringGreen",
                "MediumTurquoise",
                "MediumVioletRed",
                "MidnightBlue",
                "MintCream",
                "MistyRose",
                "Moccasin",
                "NavajoWhite",
                "Navy",
                "OldLace",
                "Olive",
                "OliveDrab",
                "Orange",
                "OrangeRed",
                "Orchid",
                "PaleGoldenrod",
                "PaleGreen",
                "PaleTurquoise",
                "PaleVioletRed",
                "PapayaWhip",
                "PeachPuff",
                "Peru",
                "Pink",
                "Plum",
                "PowderBlue",
                "Purple",
                "Red",
                "RosyBrown",
                "RoyalBlue",
                "SaddleBrown",
                "Salmon",
                "SandyBrown",
                "SeaGreen",
                "SeaShell",
                "Sienna",
                "Silver",
                "SkyBlue",
                "SlateBlue",
                "SlateGray",
                "Snow",
                "SpringGreen",
                "SteelBlue",
                "Tan",
                "Teal",
                "Thistle",
                "Tomato",
                "Transparent",
                "Turquoise",
                "Olive",
                "Wheat",
                "White",
                "WhiteSmoke",
                "Yellow",
                "YellowGreen" };

                return MColors;
            }

            set
            {
                MColors = value;
            }
        }

        //public List<Color> Colors
        //{
        //    get
        //    {
        //        if (MColors == null) MColors = new List<Color>() { Color.FromArgb(1, 255,0,0), Color.FromArgb(0, 0, 255, 0), Color.FromArgb(1, 0, 0, 255) };
        //        return MColors;
        //    }

        //    set
        //    {
        //        MColors = value;
        //    }
        //}

        private List<CComponentPrefixes> list_CompPref;
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public CComponentListVM()
        {
            MComponentList = new ObservableCollection<CComponentInfo>();
            
            list_CompPref = CComponentManager.LoadComponentsPrefixes();

            

            CComponentInfo ci = null;
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eMC].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eMC].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eMC].ComponentName, "63020", "Green", "G550‡", true, true, true, true, true, SectionsForColumnsOrRafters, Colors, EMemberType_FS_Position.MainColumn);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eMR].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eMR].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eMR].ComponentName, "63020", "Green", "G550‡", true, true, true, true, true, SectionsForColumnsOrRafters, Colors, EMemberType_FS_Position.MainRafter);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eEC].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eEC].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eEC].ComponentName, "63020", "Green", "G550‡", true, true, true, true, true, SectionsForColumnsOrRafters, Colors, EMemberType_FS_Position.EdgeColumn);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eER].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eER].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eER].ComponentName, "63020", "Green", "G550‡", true, true, true, true, true, SectionsForColumnsOrRafters, Colors, EMemberType_FS_Position.EdgeRafter);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eEP].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eEP].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eEP].ComponentName, "50020", "Green", "G550‡", true, true, true, true, true, SectionsForGirtsOrPurlins, Colors, EMemberType_FS_Position.EdgePurlin);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eG].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eG].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eG].ComponentName, "27095", "Green", "G550‡", true, true, true, true, true, SectionsForGirtsOrPurlins, Colors, EMemberType_FS_Position.Girt);
            ci.GenerateIsEnabled = false; ci.GenerateIsReadonly = true;
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eP].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eP].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eP].ComponentName, "270115", "Green", "G550‡", true, true, true, true, true, SectionsForGirtsOrPurlins, Colors, EMemberType_FS_Position.Purlin);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eC].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eC].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eC].ComponentName + " - Front Side", "270115n", "Green", "G550‡", true, true, true, true, true, SectionsForColumnsOrRafters, Colors, EMemberType_FS_Position.ColumnFrontSide);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eC].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eC].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eC].ComponentName + " - Back Side", "270115n", "Green", "G550‡", true, true, true, true, true, SectionsForColumnsOrRafters, Colors, EMemberType_FS_Position.ColumnBackSide);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eG].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eG].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eG].ComponentName + " - Front Side", "27095", "Green", "G550‡", true, true, true, true, true, SectionsForGirtsOrPurlins, Colors, EMemberType_FS_Position.GirtFrontSide);
            MComponentList.Add(ci);
            ci = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eG].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eG].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eG].ComponentName + " - Back Side", "27095", "Green", "G550‡", true, true, true, true, true, SectionsForGirtsOrPurlins, Colors, EMemberType_FS_Position.GirtBackSide);
            MComponentList.Add(ci);

            SetComponentSectionsColors();

            foreach (CComponentInfo cInfo in MComponentList)
            {
                cInfo.PropertyChanged += ComponentListItem_PropertyChanged;
            }
        }

        private void SetComponentSectionsColors()
        {
            foreach (CComponentInfo cInfo in MComponentList)
            {
                CrScProperties prop = CSectionManager.GetSectionProperties(cInfo.Section);
                cInfo.SectionColor = prop.colorName;
            }
        }

        public void SetModelComponentListProperties(Dictionary<EMemberType_FS_Position, string> MembersSectionsDict)
        {
            foreach (CComponentInfo cInfo in ComponentList)
            {
                cInfo.IsSetFromCode = true;
                cInfo.Section = MembersSectionsDict[cInfo.MemberTypePosition];
                cInfo.IsSetFromCode = false;
            }
        }


        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetComponentDetails()
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cross-section details
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            CComponentInfo cInfo = ComponentList[MSelectedComponentIndex];
            if (cInfo == null) return;

            // Cross-section properties
            List<string> listSectionPropertyValue = CSectionManager.LoadSectionPropertiesStringList(cInfo.Section);

            for (int i = 0; i < ComponentDetailsList.Count; i++)
            {
                ComponentDetailsList[i].Value = listSectionPropertyValue[i];
            }
            ComponentDetailsList = new List<CSectionPropertiesText>(ComponentDetailsList);

            // Material properties
            List<string> listMaterialPropertyValue = CMaterialManager.LoadMaterialPropertiesStringList(cInfo.Material);

            for (int i = 0; i < MaterialDetailsList.Count; i++)
            {
                MaterialDetailsList[i].Value = listMaterialPropertyValue[i];
            }
            MaterialDetailsList = new List<CMaterialPropertiesText>(MaterialDetailsList);
        }

        public void AddPersonelDoor()
        {
            CComponentInfo cInfo = ComponentList.FirstOrDefault(c => c.MemberTypePosition == EMemberType_FS_Position.DoorFrame);
            if (cInfo != null) return; //already exist in the collection

            CrScProperties prop = CSectionManager.GetSectionProperties("10075");

            cInfo = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eDF].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eDF].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eDF].ComponentName, "10075", prop.colorName, "G550‡", null, true, false, false, true, SectionsForDoorOrWindowFrame, Colors, EMemberType_FS_Position.DoorFrame);
            cInfo.PropertyChanged += ComponentListItem_PropertyChanged;
            ComponentList.Add(cInfo);

        }
        public void RemovePersonelDoor()
        {
            CComponentInfo cInfo = ComponentList.FirstOrDefault(c => c.MemberTypePosition == EMemberType_FS_Position.DoorFrame);
            if (cInfo != null) ComponentList.Remove(cInfo);
        }

        public void AddRollerDoor()
        {
            CComponentInfo cDT = ComponentList.FirstOrDefault(c => c.MemberTypePosition == EMemberType_FS_Position.DoorTrimmer);
            if (cDT == null)
            {
                CrScProperties prop = CSectionManager.GetSectionProperties("270115btb");

                cDT = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eDT].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eDT].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eDT].ComponentName, "270115btb", prop.colorName, "G550‡", null, true, false, false, true, SectionsForRollerDoorTrimmer, Colors, EMemberType_FS_Position.DoorTrimmer);
                cDT.PropertyChanged += ComponentListItem_PropertyChanged;
                ComponentList.Add(cDT);
            }

            CComponentInfo cDL = ComponentList.FirstOrDefault(c => c.MemberTypePosition == EMemberType_FS_Position.DoorLintel);
            if (cDL == null)
            {
                CrScProperties prop = CSectionManager.GetSectionProperties("27095");

                cDL = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eDL].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eDL].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eDL].ComponentName, "27095", prop.colorName, "G550‡", null, true, false, false, true, SectionsForRollerDoorLintel, Colors, EMemberType_FS_Position.DoorLintel);
                cDL.PropertyChanged += ComponentListItem_PropertyChanged;
                ComponentList.Add(cDL);
            }
        }
        public void RemoveRollerDoor()
        {
            CComponentInfo cDT = ComponentList.FirstOrDefault(c => c.MemberTypePosition == EMemberType_FS_Position.DoorTrimmer);
            if (cDT != null) ComponentList.Remove(cDT);

            CComponentInfo cDL = ComponentList.FirstOrDefault(c => c.MemberTypePosition == EMemberType_FS_Position.DoorLintel);
            if (cDL != null) ComponentList.Remove(cDL);
        }

        public void AddWindow()
        {
            CComponentInfo cInfo = ComponentList.FirstOrDefault(c => c.MemberTypePosition == EMemberType_FS_Position.WindowFrame);
            if (cInfo != null) return; //already exist in the collection

            CrScProperties prop = CSectionManager.GetSectionProperties("10075");

            cInfo = new CComponentInfo(list_CompPref[(int)EMemberType_FS.eWF].ComponentPrefix, list_CompPref[(int)EMemberType_FS.eWF].ComponentColorName,
                list_CompPref[(int)EMemberType_FS.eWF].ComponentName, "10075", prop.colorName, "G550‡", null, true, false, false, true, SectionsForDoorOrWindowFrame, MColors, EMemberType_FS_Position.WindowFrame);
            cInfo.PropertyChanged += ComponentListItem_PropertyChanged;
            ComponentList.Add(cInfo);
        }
        public void RemoveWindow()
        {
            CComponentInfo cInfo = ComponentList.FirstOrDefault(c => c.MemberTypePosition == EMemberType_FS_Position.WindowFrame);
            if (cInfo != null) ComponentList.Remove(cInfo);
        }

        public bool NoFrameMembersForCalculate()
        {
            return !ComponentList.Any(c => (c.MemberTypePosition == EMemberType_FS_Position.MainColumn ||
            c.MemberTypePosition == EMemberType_FS_Position.MainRafter ||
            c.MemberTypePosition == EMemberType_FS_Position.EdgeColumn ||
            c.MemberTypePosition == EMemberType_FS_Position.EdgeRafter) && c.Calculate == true);
        }

        public bool NoCompomentsForCalculate()
        {
            return !ComponentList.Any(c => c.Calculate == true);
        }
        public bool NoCompomentsForDesign()
        {
            return !ComponentList.Any(c => c.Design == true);
        }
        public bool NoCompomentsForMaterialList()
        {
            return !ComponentList.Any(c => c.MaterialList == true);
        }
    }
}
