﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using BaseClasses;
using DATABASE;
using MATH;
using System.ComponentModel;

namespace PFD
{
    /// <summary>
    /// Interaction logic for ComponentList.xaml
    /// </summary>
    public partial class UC_ComponentList : UserControl
    {
        DataSet ds;
        CDatabaseComponents database = new CDatabaseComponents(); // System components database

        List<string> listMemberPrefix = new List<string>(0);
        List<string> listMemberComponentName = new List<string>();
        List<string> listMemberCrScName = new List<string>();
        List<string> listMemberMaterialName = new List<string>();

        List<bool> listMemberGenerate = new List<bool>();
        List<bool> listMemberDisplay = new List<bool>();
        List<bool> listMemberCalculate = new List<bool>();
        List<bool> listMemberDesign = new List<bool>();
        List<bool> listMemberMaterialList = new List<bool>();

        public List<string> MemberComponentName
        {
            get { return listMemberComponentName; }
            set { listMemberComponentName = value; }
        }

        List<string> listSectionPropertyName = new List<string>(1);
        List<string> listSectionPropertySymbol = new List<string>(1);
        List<string> listSectionPropertyValue = new List<string>(1);
        List<string> listSectionPropertyUnit = new List<string>(1);

        int iSelectedComponentRowIndex = 1;
        int iSelectedComponentCrossSectionIndex = 1;

        public UC_ComponentList()
        {
            InitializeComponent();

            // Clear all lists
            //DeleteAllLists();

            // TODO Ondrej - zrusit napojenie na objekt DatabaseComponents database a nahradit vstupmi z SQL databazy - MDBModels, tabulka componentPrefixes
            // Obsah tejto tabulky co sa tyka prierezov by sa mal prebrat z modelu CExample_3D_901_PF - m_arrSections
            // Material prevziat z prierezov

            // TODO - Ondrej - obsah tohto zoznamu preberat z CExample_3D_901_PF - zoznam listOfModelMemberGroups
            CComponentInfo ci = null;
            CComponentListVM cl = new CComponentListVM();
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eMC, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eMC, 1], "Box 63020", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eMR, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eMR, 1], "Box 63020", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eEC, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eEC, 1], "Box 63020", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eER, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eER, 1], "Box 63020", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eEP, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eEP, 1], "C 50020", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG, 1], "C 27095", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eP, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eP, 1], "C 270115", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eC, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eC, 1] + " - Front Side", "Box 10075", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eC, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eC, 1] + " - Back Side", "Box 10075", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG, 1] + " - Front Side", "C 27095", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);
            ci = new CComponentInfo(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG, 0], database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG, 1] + " - Back Side", "C 27095", "G550", true, true, true, true, true);
            cl.ComponentList.Add(ci);

            cl.PropertyChanged += HandleComponentListPropertyChangedEvent;
            this.DataContext = cl;

            // For each component add one row
            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eMC, 0]);   // "MC"
            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eMR, 0]);   // "MR"
            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eEP, 0]);   // "EP"
            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG,  0]);   // "G"
            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eP,  0]);   // "P"

            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eC,  0]);   // "C"
            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eC,  0]);   // "C"
            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG,  0]);   // "G"
            //listMemberPrefix.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG,  0]);   // "G"

            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eMC, 1]);
            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eMR, 1]);
            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eEP, 1]);
            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG,  1]);
            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eP,  1]);

            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eC, 1] + " - Front Side");
            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eC, 1] + " - Back Side");
            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG, 1] + " - Front Side");
            //listMemberComponentName.Add(database.arr_Member_Types_Prefix[(int)EMemberType_FS.eG, 1] + " - Back Side");

            //// Creating new DataGridComboBoxColumn
            //DataGridComboBoxColumn myDGCBC_crsc = new DataGridComboBoxColumn();
            //myDGCBC_crsc.Header = "Section";
            //// Binding DataGridComboBoxColumn.ItemsSource and DataGridComboBoxColumn.SelectedItem
            //myDGCBC_crsc.ItemsSource = database.arr_Crsc_Types;
            //myDGCBC_crsc.SelectedItemBinding = new Binding("SelectedValue");
            //// Adding DataGridComboBoxColumn to the DataGrid
            ////Datagrid_Components.Columns.Add(myDGCBC_crsc);

            //listMemberCrScName.Add("Box 63020");   // Main Column
            //listMemberCrScName.Add("Box 63020");   // Rafter
            //listMemberCrScName.Add("C 50020");     // Eaves Purlin
            //listMemberCrScName.Add("C 27095");     // Girt - Wall
            //listMemberCrScName.Add("C 270115");    // Purlin

            //listMemberCrScName.Add("Box 10075");   // Front Column
            //listMemberCrScName.Add("Box 10075");   // Back Column
            //listMemberCrScName.Add("C 27095");     // Front Girt
            //listMemberCrScName.Add("C 27095");     // Back Girt

            //// Default - other properties
            //foreach(string sprefix in listMemberPrefix)
            //{
            //    listMemberMaterialName.Add("G550");
            //    listMemberGenerate.Add(true);
            //    listMemberDisplay.Add(true);
            //    listMemberCalculate.Add(true);
            //    listMemberDesign.Add(true);
            //    listMemberMaterialList.Add(true);
            //}

            //// Create Table
            //DataTable table = new DataTable("Table");

            //// Create Table Rows
            //table.Columns.Add("Prefix", typeof(String));
            //table.Columns.Add("ComponentName", typeof(String));
            //table.Columns.Add("Section", typeof(String));
            //table.Columns.Add("Section", typeof(String));
            //table.Columns.Add("Material", typeof(String));
            //table.Columns.Add("Generate", typeof(Boolean));
            //table.Columns.Add("Display", typeof(Boolean));
            //table.Columns.Add("Calculate", typeof(Boolean));
            //table.Columns.Add("Design", typeof(Boolean));
            //table.Columns.Add("MaterialList", typeof(Boolean));

            //// Set Column Caption
            //table.Columns["Prefix"].Caption = "Prefix";
            //table.Columns["ComponentName"].Caption = "ComponentName";
            //table.Columns["Section"].Caption = "Section";
            //table.Columns["Material"].Caption = "Material";
            //table.Columns["Generate"].Caption = "Generate";
            //table.Columns["Display"].Caption = "Display";
            //table.Columns["Calculate"].Caption = "Calculate";
            //table.Columns["Design"].Caption = "Design";
            //table.Columns["MaterialList"].Caption = "MaterialList";

            //// Create Datases
            //ds = new DataSet();
            //// Add Table to Dataset
            //ds.Tables.Add(table);

            //for (int i = 0; i < listMemberPrefix.Count; i++)
            //{
            //    DataRow row = table.NewRow();

            //    try
            //    {
            //        row["Prefix"] = listMemberPrefix[i];
            //        row["ComponentName"] = listMemberComponentName[i];
            //        row["Section"] = listMemberCrScName[i];
            //        row["Material"] = listMemberMaterialName[i];
            //        row["Generate"] = listMemberGenerate[i];
            //        row["Display"] =  listMemberDisplay[i];
            //        row["Calculate"] = listMemberCalculate[i];
            //        row["Design"] = listMemberDesign[i];
            //        row["MaterialList"] = listMemberMaterialList[i];
            //    }
            //    catch (ArgumentOutOfRangeException) { }
            //    table.Rows.Add(row);
            //}

            //Datagrid_Components.ItemsSource = ds.Tables[0].AsDataView();  //draw the table to datagridview

            //// Adding DataGridComboBoxColumn to the DataGrid
            //Datagrid_Components.Columns.Add(myDGCBC_crsc);

            // TODO Ondrej - vlozit stlpec section pred stlpec material (teraz sa vklada na zaciatok ako prvy stlpec) a inicializovat defaultne hodnoty v comboboxoch

            /*
             "Box 63020"
             "Box 63020"
             "C 50020"
             "C 27095"
             "C 270115"
             
             "Box 10075"
             "Box 10075"
             "C 27095"
             "C 27095"
             */

            // Set default indexes of cross-sections

            //(Datagrid_Components.Items[0] as DataRowView).Row.ItemArray[0] = database.arr_Crsc_Types[0];
            //(Datagrid_Components.Items[1] as DataRowView).Row.ItemArray[0] = database.arr_Crsc_Types[1];

            // Set Column Header
            /*
            Datagrid_Components.Columns[0].Header = "Prefix";
            Datagrid_Components.Columns[1].Header = "ComponentName";
            Datagrid_Components.Columns[2].Header = "Section";
            Datagrid_Components.Columns[3].Header = "Material";
            Datagrid_Components.Columns[4].Header = "Generate";
            Datagrid_Components.Columns[5].Header = "Display";
            Datagrid_Components.Columns[6].Header = "Calculate";
            Datagrid_Components.Columns[7].Header = "Design";
            Datagrid_Components.Columns[8].Header = "MaterialList";
            */

            // Set Column Width
            /*
            Datagrid_Components.Columns[0].Width = 100;
            Datagrid_Components.Columns[1].Width = 100;
            Datagrid_Components.Columns[2].Width = 100;
            Datagrid_Components.Columns[3].Width = 100;
            Datagrid_Components.Columns[4].Width = 100;
            Datagrid_Components.Columns[5].Width = 100;
            Datagrid_Components.Columns[6].Width = 100;
            Datagrid_Components.Columns[7].Width = 100;
            */
        }

        protected void HandleComponentListPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            if (sender == null) return;
        }

        private void DeleteAllLists()
        {
            //Todo - asi sa to da jednoduchsie
            DeleteLists();

            //Datagrid_Components.ItemsSource = null;
            //Datagrid_Components.Items.Clear();
            //Datagrid_Components.Items.Refresh();
        }

        // Deleting lists for updating actual values
        private void DeleteLists()
        {
            listMemberPrefix.Clear();
            listMemberComponentName.Clear();
            listMemberCrScName.Clear();
            listMemberMaterialName.Clear();
            listMemberGenerate.Clear();
            listMemberDisplay.Clear();
            listMemberCalculate.Clear();
            listMemberDesign.Clear();
            listMemberMaterialList.Clear();
        }

        private void DeleteDetails()
        {
            listSectionPropertyName.Clear();
            listSectionPropertySymbol.Clear();
            listSectionPropertyValue.Clear();
            listSectionPropertyUnit.Clear();
        }

        private void Datagrid_Components_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO Ondrej - zapracovat update obsahu Datagrid_ComponentDetails podla indexu v comboboxe
            // na vybranom riadku v Datagrid_Components
            // TERAZ JE TO NESPRAVNE, neviem ako vydolovat z comboboxe v gride selectedindex na prislusnom riadku

            var rowIndex = this.Datagrid_Components.Items.IndexOf(this.Datagrid_Components.SelectedItem);

            if(rowIndex > 0)
                iSelectedComponentCrossSectionIndex = rowIndex + 1;

            int sectionID = iSelectedComponentCrossSectionIndex; // iSelectedComponentCrossSectionIndex; // TODO Ondrej Nastavovat obsah Datagrid_ComponentDetails podla prierezu na vybranom riadku v Datagrid_Components

            SetComponentDetails(sectionID);
        }

        private void SetComponentDetails(int sectionID)
        {

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cross-section details
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // TODO Ondrej - zapracovat do CComponentListVM

            // TO Ondrej, viem ze som to spravil nespravne starym sposobom, ale nejako sa mi s tym VM nedarilo, nevedel som kde mam co presne dat,
            // asi som to este dostatocne nepochopil, prosim o refaktoring

            DeleteDetails(); // Clear previous data

            List<DATABASE.DTO.CSectionPropertiesText> sectionTexts = CSectionManager.LoadSectionPropertiesNamesSymbolsUnits();
            listSectionPropertyValue = CSectionManager.LoadSectionPropertiesStringList(sectionID);

            // Default - other properties
            foreach (DATABASE.DTO.CSectionPropertiesText textRow in sectionTexts)
            {
                listSectionPropertyName.Add(textRow.text);
                listSectionPropertySymbol.Add(textRow.symbol);
                listSectionPropertyUnit.Add(textRow.unit_NmmMpa);
            }

            // Create Table
            DataTable table = new DataTable("Table");

            // Create Table Rows
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("Symbol", typeof(String));
            table.Columns.Add("Value", typeof(String));
            table.Columns.Add("Unit", typeof(String));

            // Set Column Caption
            table.Columns["Name"].Caption = "Name";
            table.Columns["Symbol"].Caption = "Symbol";
            table.Columns["Value"].Caption = "Value";
            table.Columns["Unit"].Caption = "Unit";

            // Create Datases
            ds = new DataSet();
            // Add Table to Dataset
            ds.Tables.Add(table);

            for (int i = 0; i < listSectionPropertyName.Count; i++)
            {
                DataRow row = table.NewRow();

                try
                {
                    row["Name"] = listSectionPropertyName[i];
                    row["Symbol"] = listSectionPropertySymbol[i];
                    row["Value"] = listSectionPropertyValue[i];
                    row["Unit"] = listSectionPropertyUnit[i];
                }
                catch (ArgumentOutOfRangeException) { }
                table.Rows.Add(row);
            }

            Datagrid_ComponentDetails.ItemsSource = ds.Tables[0].AsDataView();  //draw the table to datagridview
        }
    }
}
