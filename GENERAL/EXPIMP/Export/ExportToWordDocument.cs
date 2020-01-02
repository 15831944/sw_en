﻿using _3DTools;
using BaseClasses;
using BaseClasses.Helpers;
using DATABASE;
using DATABASE.DTO;
using M_AS4600;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Words.NET;

namespace EXPIMP
{
    public static class ExportToWordDocument
    {
        public enum EViewType3D
        {
            MEMBER_CENTERLINES = 0,
            MEMBER_SOLID = 1,
        }

        private const string resourcesFolderPath = "./../../Resources/";
        private const double fontSizeInTable = 8;
        private const int imageMaxWidth = 720;

        private const float quotationTableWidth = 520;

        public static void ReportAllDataToWordDoc(CModelData modelData)
        {
            string fileName = GetReportName();
            // Create a new document.
            using (DocX document = DocX.Create(fileName))
            {
                // The path to a template document,
                string templatePath = resourcesFolderPath + "TemplateReport.docx";
                // Apply a template to the document based on a path.
                document.ApplyTemplate(templatePath);

                //DrawModel3DToDoc(document, viewPort);
                DrawModel3DToDoc(document, modelData, EViewType3D.MEMBER_SOLID);
                DrawModel3DToDoc(document, modelData, EViewType3D.MEMBER_CENTERLINES);

                DrawProjectInfo(document, modelData.ProjectInfo);
                DrawBasicGeometry(document, modelData);
                DrawMaterial(document, modelData);
                DrawCrossSections(document, modelData);
                DrawComponentList(document, modelData);

                DrawLoad(document, modelData);
                DrawLoadCases(document, modelData);
                DrawLoadCombinations(document, modelData);

                DrawMemberDesign(document, modelData);
                DrawJointDesign(document, modelData);
                DrawFootingDesign(document, modelData);

                //DrawLogoAndProjectInfoTable(document);
                //DrawLogo(document);
                //DrawModel3D(document, viewPort);
                //DrawBasicGeometry(document, null);
                //DrawLoad(document, null);

                //CreateChapterWithBuletedList(document);

                CreateTOC(document);

                // Save this document to disk.
                document.Save();

            }
            Process.Start(fileName);
        }

        public static void ReportQuotationToWordDoc(List<DataTable> tables, QuotationData data)
        {
            string fileName = GetQuotationName();
            // Create a new document.
            using (DocX document = DocX.Create(fileName))
            {
                // The path to a template document,
                string templatePath = resourcesFolderPath + "TemplateQuotation.docx";
                // Apply a template to the document based on a path.
                document.ApplyTemplate(templatePath);

                DrawProjectInfo(document, data.ProjectInfo);

                // Replace parameter identificators by values in .doc
                //-----------------------------------------------------------------------------------------------------------
                document.ReplaceText("[Length]", data.Length.ToString("F2"));
                document.ReplaceText("[GableWidth]", data.GableWidth.ToString("F2"));
                document.ReplaceText("[WallHeight]", data.WallHeight.ToString("F2"));
                document.ReplaceText("[ApexHeight_H2]",data.ApexHeight_H2.ToString("F2"));
                document.ReplaceText("[RoofPitch_deg]", data.RoofPitch_deg.ToString());
                document.ReplaceText("[BayWidth]", data.BayWidth.ToString("F2"));

                document.ReplaceText("[roofCladding]", $"{data.RoofCladding}-{data.RoofCladdingThickness_mm}");
                document.ReplaceText("[wallCladding]", $"{data.WallCladding}-{data.WallCladdingThickness_mm}");

                document.ReplaceText("[roofCoating]", data.RoofCladdingCoating);
                document.ReplaceText("[wallCoating]", data.WallCladdingCoating);

                document.ReplaceText("[Location]", data.Location);
                document.ReplaceText("[WindRegion]", data.WindRegion);

                document.ReplaceText("[numberRollerDoors]", data.NumberOfRollerDoors.ToString());
                document.ReplaceText("[numberPersonnelDoors]", data.NumberOfPersonnelDoors.ToString());

                document.ReplaceText("[price_WithMargin_WithoutGST]", data.BuildingPrice_WithMargin_WithoutGST.ToString("F2"));
                //-----------------------------------------------------------------------------------------------------------

                Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[Quotation]"));                

                if (tables.Count == 0)
                {
                    par.Remove(false);
                    par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("APPENDIX – MATERIAL LIST"));
                    par.Remove(false);
                }
                else
                {
                    par.RemoveText(0);
                    par = par.InsertParagraphAfterSelf("");
                }

                foreach (DataTable dt in tables)
                {
                    Table t = GetTableFromDataTable(document, dt);
                    par.InsertText(dt.TableName);
                    par.StyleName = "NoSpacing";
                    par = par.InsertParagraphAfterSelf("");
                    par.InsertTableBeforeSelf(t);
                }

                // Save this document to disk.
                document.Save();
            }
            Process.Start(fileName);
        }

        private static string GetReportName()
        {
            int count = 0;
            string fileName = null;
            bool nameOK = false;
            while (!nameOK)
            {
                fileName = $"Report_{++count}.docx";

                if (!System.IO.File.Exists(fileName)) nameOK = true;
            }
            return fileName;
        }
        private static string GetQuotationName()
        {
            int count = 0;
            string fileName = null;
            bool nameOK = false;
            while (!nameOK)
            {
                fileName = $"Quotation_{++count}.docx";

                if (!System.IO.File.Exists(fileName)) nameOK = true;
            }
            return fileName;
        }

        //private static CProjectInfo GetProjectInfo()
        //{
        //    CProjectInfo pInfo = new CProjectInfo("New self storage", "8 Forest Road, Stoke", "B6351", "Building 1", DateTime.Now);
        //    return pInfo;
        //}

        private static void DrawProjectInfo(DocX document, CProjectInfo pInfo)
        {
            if (pInfo == null) return;

            document.ReplaceText("[ProjectName]", pInfo.ProjectName);
            document.ReplaceText("[ProjectSite]", pInfo.Site);
            document.ReplaceText("[ProjectNumber]", pInfo.ProjectNumber);
            document.ReplaceText("[ProjectPart]", pInfo.ProjectPart);

            document.ReplaceText("[ProjectName_UC]", pInfo.ProjectName.ToUpper()); // Upper Case
            document.ReplaceText("[SalesPerson_UC]", pInfo.SalesPerson.ToUpper()); // Upper Case

            document.ReplaceText("[Date]", pInfo.Date.ToString("dd/MM/yyyy"));

            document.ReplaceText("[CustomerName]", pInfo.CustomerName);
            document.ReplaceText("[CustomerContactPerson]", pInfo.CustomerContactPerson);

            document.ReplaceText("[SalesPerson]", pInfo.SalesPerson);
            document.ReplaceText("[SalesPersonPhone]", pInfo.SalesPersonPhone);
            document.ReplaceText("[SalesPersonEmail]", pInfo.SalesPersonEmail);
        }

        private static void DrawBasicGeometry(DocX document, CModelData data)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            DrawBasicGeometryParameters(document);

            document.ReplaceText("[GableWidth]", data.GableWidth.ToString(nfi));
            document.ReplaceText("[Length]", data.Length.ToString(nfi));
            document.ReplaceText("[WallHeight]", data.WallHeight.ToString(nfi));
            document.ReplaceText("[RoofPitch_deg]", data.RoofPitch_deg.ToString(nfi));
            document.ReplaceText("[GirtDistance]", data.GirtDistance.ToString(nfi));
            document.ReplaceText("[PurlinDistance]", data.PurlinDistance.ToString(nfi));
            document.ReplaceText("[ColumnDistance]", data.ColumnDistance.ToString(nfi));
        }

        private static void DrawMaterial(DocX document, CModelData data)
        {
            // Steel
            var diffMaterials = data.ComponentList.Select(c => c.Material).Distinct();
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[MaterialProperties]"));
            par.RemoveText(0);
            foreach (string material in diffMaterials)
            {
                par = par.InsertParagraphAfterSelf("Material grade: ").Bold().Append(material);

                // Material properties
                List<string> listMaterialPropertyValue = CMaterialManager.LoadSteelMaterialPropertiesStringList(material);

                for (int i = 0; i < data.MaterialDetailsList.Count; i++)
                {
                    data.MaterialDetailsList[i].Value = listMaterialPropertyValue[i];
                }
                data.MaterialDetailsList = new List<CMaterialPropertiesText>(data.MaterialDetailsList);

                par = DrawMaterialTable(document, par, "steelcoil01.png", data.MaterialDetailsList);
            }

            // TODO 376
            // Concrete (uvazovat z GUI - UC Footings alebo z objektov zakladu ???
            var diffMaterialsConcrete = data.Model.m_arrFoundations.Select(c => c.m_Mat.Name).Distinct();

            // TODO Ondrej - for review, moze sa refaktorovat so steel (tabulky podla toho kolko je roznych druhov ocele 
            // + tabulky podla toho kolko je roznych druhov betonu pre footing pads (len teoreticky, v praxi bude beton len jeden)
            // Este by sa mohol pridat beton pre floor slab a steel pre plates, ale to asi zatial neriesime

            if(diffMaterialsConcrete != null)
            {
                foreach (string material in diffMaterialsConcrete)
                {
                    par = par.InsertParagraphAfterSelf("Concrete material grade: ").Bold().Append(material);

                    // Material properties
                    List<string> listMaterialPropertyValue = CMaterialManager.LoadMaterialPropertiesStringList_RC(material);

                    for (int i = 0; i < data.MaterialDetailsList_RC.Count; i++)
                    {
                        data.MaterialDetailsList_RC[i].Value = listMaterialPropertyValue[i];
                    }
                    data.MaterialDetailsList_RC = new List<CMaterialPropertiesText>(data.MaterialDetailsList_RC);

                    par = DrawMaterialTable(document, par, "concretecube01.png", data.MaterialDetailsList_RC);
                }
            }
        }

        // TO Ondrej - Moje uvahy o jednom bazovom rieseni pre tabulky :-)

        // Dalo by sa metodu DrawMaterialTable zobecnit tak, ze List<CMaterialPropertiesText> a List<CSectionPropertiesText>
        // pripadne aj dalsie zobrazovane hodnoty v datagridoch, ktore maju rovnake (podobne) nazvy stlpcov (Load parameters, Member Design, Joint Design) by mali nejakeho spolocneho predka
        // V tom by boli zadefinovane stlpce

        /*
        0 - Text (alebo Name, alebo Description)
        1 - Symbol
        2 - Value
        3 - Unit
        4 - Formula (alebo Equation)
        5 - Note
        */

        // A sprava zobrazovania v Datagridoch GUI a pri exporte do Wordu by bola pre vsetky tieto zoznamy spolocna
        // Zobrazenie pre jednotlive stlpce by sa dalo vypinat a zapinat
        // Dalo by sa pocet riadkov tabulky zmensit tak, ze by sa mohli vlozit napriklad dve alebo tri sekvencie (1-5) vedla seba,
        // takze z 9 riadkov x 5 stlpcov by sme teoreticky mohli urobit 3 riadky x 15 stlpcov (nieco podobne ako je teraz v Member Design Details)
        // Samozrejme by to bolo nastavitelne podla sirky datagridu v GUI alebo sirky stranky A4 bez okrajov
        // Spolocne by sa riesili horne a dolne indexy a jednotky

        private static Paragraph DrawMaterialTable(DocX document, Paragraph p, string pictureName, List<CMaterialPropertiesText> details)
        {
            var t = document.AddTable(1, 5);
            t.Design = TableDesign.TableGrid;
            t.Alignment = Alignment.left;
            t.AutoFit = AutoFit.Window;

            //var imagePath = $"{resourcesFolderPath}steelcoil01.png";
            var imagePath = $"{resourcesFolderPath}" + pictureName;
            var picWidth = 250;
            var picHeight = 250;
            var image = document.AddImage(imagePath);
            // Set Picture Height and Width.
            var picture = image.CreatePicture(picHeight, picWidth);

            t.Rows[0].Cells[0].Paragraphs[0].AppendPicture(picture);
            t.Rows[0].Cells[1].Paragraphs[0].InsertText("Text");
            t.Rows[0].Cells[2].Paragraphs[0].InsertText("Symbol");
            t.Rows[0].Cells[3].Paragraphs[0].InsertText("Value");
            t.Rows[0].Cells[4].Paragraphs[0].InsertText("Unit");
            t.Rows[0].Cells[1].Paragraphs[0].Bold();
            t.Rows[0].Cells[2].Paragraphs[0].Bold();
            t.Rows[0].Cells[3].Paragraphs[0].Bold();
            t.Rows[0].Cells[4].Paragraphs[0].Bold();

            t.Rows[0].Cells[0].Width = picWidth;
            t.Rows[0].Cells[1].Width = (document.PageWidth - picWidth) * 0.7;
            t.Rows[0].Cells[2].Width = (document.PageWidth - picWidth) * 0.1;
            t.Rows[0].Cells[3].Width = (document.PageWidth - picWidth) * 0.1;
            t.Rows[0].Cells[4].Width = (document.PageWidth - picWidth) * 0.1;

            foreach (CMaterialPropertiesText prop in details)
            {
                if (string.IsNullOrEmpty(prop.Value)) continue;

                Row row = t.InsertRow();
                row.Cells[1].Paragraphs[0].InsertText(prop.Text);
                row.Cells[2].Paragraphs[0].InsertText(prop.Symbol);
                row.Cells[3].Paragraphs[0].InsertText(prop.Value);
                row.Cells[4].Paragraphs[0].InsertText(prop.Unit_NmmMpa);
            }
            t.MergeCellsInColumn(0, 0, t.Rows.Count - 1);

            p = p.InsertParagraphAfterSelf(p);
            p.RemoveText(0);
            p.InsertTableBeforeSelf(t);

            SetFontSizeForTable(t);

            return p;
        }

        private static void DrawCrossSections(DocX document, CModelData data)
        {
            var diffCrsc = data.ComponentList.Select(c => c.Section).Distinct();
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[CrossSections]"));
            par.RemoveText(0);
            foreach (string crsc in diffCrsc)
            {
                par = par.InsertParagraphAfterSelf("Cross-section name: ").Bold().Append(crsc);

                // Cross-section properties
                List<string> listSectionPropertyValue = CSectionManager.LoadSectionPropertiesStringList(crsc);

                for (int i = 0; i < data.ComponentDetailsList.Count; i++)
                {
                    data.ComponentDetailsList[i].Value = listSectionPropertyValue[i];
                }
                data.ComponentDetailsList = new List<CSectionPropertiesText>(data.ComponentDetailsList);

                par = DrawCrossSectionTable(document, par, crsc, data.ComponentDetailsList);
            }

        }

        private static Paragraph DrawCrossSectionTable(DocX document, Paragraph p, string crsc, List<CSectionPropertiesText> details)
        {
            var t = document.AddTable(1, 5);
            t.Design = TableDesign.TableGrid;
            t.Alignment = Alignment.left;
            t.AutoFit = AutoFit.Contents;

            var picWidth = 200;
            var picHeight = 400;
            var image = document.AddImage($"{resourcesFolderPath}crsc{crsc}.png"); //zmenil som na PNG - aby bolo vidno spodny okraj tabulky a tiez optimalizacia velkosti
            // Set Picture Height and Width.
            var picture = image.CreatePicture(picHeight, picWidth);

            // Insert Picture in paragraph.
            t.Rows[0].Cells[0].Paragraphs[0].AppendPicture(picture);
            t.Rows[0].Cells[1].Paragraphs[0].InsertText("Text");
            t.Rows[0].Cells[2].Paragraphs[0].InsertText("Symbol");
            t.Rows[0].Cells[3].Paragraphs[0].InsertText("Value");
            t.Rows[0].Cells[4].Paragraphs[0].InsertText("Unit");
            t.Rows[0].Cells[1].Paragraphs[0].Bold();
            t.Rows[0].Cells[2].Paragraphs[0].Bold();
            t.Rows[0].Cells[3].Paragraphs[0].Bold();
            t.Rows[0].Cells[4].Paragraphs[0].Bold();

            t.Rows[0].Cells[0].Width = picWidth;
            t.Rows[0].Cells[1].Width = (document.PageWidth - picWidth) * 0.7;
            t.Rows[0].Cells[2].Width = (document.PageWidth - picWidth) * 0.1;
            t.Rows[0].Cells[3].Width = (document.PageWidth - picWidth) * 0.1;
            t.Rows[0].Cells[4].Width = (document.PageWidth - picWidth) * 0.1;

            foreach (CSectionPropertiesText prop in details)
            {
                if (!prop.VisibleInReport) continue;
                if (string.IsNullOrEmpty(prop.Value)) continue;
                if (prop.Value == "NaN") continue;

                Row row = t.InsertRow();
                row.Cells[1].Paragraphs[0].InsertText(prop.Text);
                row.Cells[2].Paragraphs[0].InsertText(prop.Symbol);
                row.Cells[3].Paragraphs[0].InsertText(prop.Value);
                row.Cells[4].Paragraphs[0].InsertText(prop.Unit_NmmMpa);
            }

            t.MergeCellsInColumn(0, 0, t.Rows.Count - 1);

            p = p.InsertParagraphAfterSelf(p);
            p.RemoveText(0);
            p.InsertTableBeforeSelf(t);

            SetFontSizeForTable(t);

            return p;
        }

        private static void DrawComponentList(DocX document, CModelData data)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[MemberTypes]"));
            par.RemoveText(0);

            var t = document.AddTable(1, 7);
            t.Design = TableDesign.TableGrid;
            t.Alignment = Alignment.left; 

            t.Rows[0].Cells[0].Paragraphs[0].InsertText("Prefix");
            t.Rows[0].Cells[1].Paragraphs[0].InsertText("Color");
            t.Rows[0].Cells[2].Paragraphs[0].InsertText("Component Name");
            t.Rows[0].Cells[3].Paragraphs[0].InsertText("Section");
            t.Rows[0].Cells[4].Paragraphs[0].InsertText("Section Color");
            t.Rows[0].Cells[5].Paragraphs[0].InsertText("Material");
            t.Rows[0].Cells[6].Paragraphs[0].InsertText("ILS");
            t.Rows[0].Cells[0].Paragraphs[0].Bold();
            t.Rows[0].Cells[1].Paragraphs[0].Bold();
            t.Rows[0].Cells[2].Paragraphs[0].Bold();
            t.Rows[0].Cells[3].Paragraphs[0].Bold();
            t.Rows[0].Cells[4].Paragraphs[0].Bold();
            t.Rows[0].Cells[5].Paragraphs[0].Bold();
            t.Rows[0].Cells[6].Paragraphs[0].Bold();
            
            foreach (CComponentInfo cInfo in data.ComponentList)
            {
                Row row = t.InsertRow();
                row.Cells[0].Paragraphs[0].InsertText(cInfo.Prefix);
                row.Cells[1].Paragraphs[0].InsertText("");
                row.Cells[1].FillColor = System.Drawing.Color.FromName(cInfo.Color.Name);
                row.Cells[1].Paragraphs[0].Color(System.Drawing.Color.FromName(cInfo.Color.Name));
                row.Cells[2].Paragraphs[0].InsertText(cInfo.ComponentName);
                row.Cells[3].Paragraphs[0].InsertText(cInfo.Section);
                row.Cells[4].Paragraphs[0].InsertText("");
                row.Cells[4].FillColor = System.Drawing.Color.FromName(cInfo.SectionColor);
                row.Cells[4].Paragraphs[0].Color(System.Drawing.Color.FromName(cInfo.SectionColor));
                row.Cells[5].Paragraphs[0].InsertText(cInfo.Material);
                row.Cells[6].Paragraphs[0].InsertText(cInfo.ILS);
            }

            SetFontSizeForTable(t);

            float tableWidth = 525f;
            t.AutoFit = AutoFit.ColumnWidth;
            t.SetWidthsPercentage(new[] { 7f, 12f, 28f, 13f, 12f, 13f, 15f }, tableWidth);

            par.InsertTableBeforeSelf(t);
        }

        private static void DrawLoad(DocX document, CModelData data)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            DrawBasicLoadParameters(document);
            DrawDeadLoadParameters(document);
            DrawImposedLoadParameters(document);
            DrawSnowLoadParameters(document);
            DrawWindLoadParameters(document);
            DrawSeismicLoadParameters(document);

            Dictionary<string, DataExportTables> allItems = CStringsManager.GetAllDict();
            Dictionary<string, QuantityLibraryItem> quantityLibrary = CQuantityLibrary.GetQuantityLibrary();
            ValueDisplayHelper vdh = new ValueDisplayHelper(allItems, quantityLibrary, nfi);

            // TO Ondrej - tu mi nie je jasne preco nieco vytvarame cez ValueDisplayHelper vdh.GetStringReport(data.R_SLS, "R_SLS")); a nieco priamo konvertujeme z cisla ToString data.AnnualProbabilitySLS.ToString(nfi));
            // Pre AnnualProbabilitySLS to cez vdh nefungovalo, tak som to zmenil

            // Basic parameters
            document.ReplaceText("[Location]", data.Location);
            document.ReplaceText("[DesignLife_Value]", data.DesignLife);
            document.ReplaceText("[ImportanceClass]", data.ImportanceClass);
            document.ReplaceText("[AnnualProbabilitySLS]", data.AnnualProbabilitySLS.ToString(nfi)); // vdh.GetStringReport(data.AnnualProbabilitySLS, "AnnualProbabilitySLS")); ??? Toto nefungovalo ???
            document.ReplaceText("[R_SLS]", vdh.GetStringReport(data.R_SLS, "R_SLS"));
            document.ReplaceText("[SiteElevation]", vdh.GetStringReport(data.SiteElevation, "SiteElevation"));

            // Dead Load
            document.ReplaceText("[CCalcul_1170_1.DeadLoad_Wall]", vdh.GetStringReport(data.GeneralLoad.fDeadLoad_Wall, "CCalcul_1170_1.DeadLoad_Wall"));
            document.ReplaceText("[CCalcul_1170_1.DeadLoad_Roof]", vdh.GetStringReport(data.GeneralLoad.fDeadLoad_Roof, "CCalcul_1170_1.DeadLoad_Roof"));
            document.ReplaceText("[AdditionalDeadActionWall]", vdh.GetStringReport(data.AdditionalDeadActionWall, "AdditionalDeadActionWall"));
            document.ReplaceText("[AdditionalDeadActionRoof]", vdh.GetStringReport(data.AdditionalDeadActionRoof, "AdditionalDeadActionRoof"));
            document.ReplaceText("[CCalcul_1170_1.DeadLoadTotal_Wall]", vdh.GetStringReport(data.GeneralLoad.fDeadLoadTotal_Wall, "CCalcul_1170_1.DeadLoadTotal_Wall"));
            document.ReplaceText("[CCalcul_1170_1.DeadLoadTotal_Roof]", vdh.GetStringReport(data.GeneralLoad.fDeadLoadTotal_Roof, "CCalcul_1170_1.DeadLoadTotal_Roof"));

            // Imposed Load
            document.ReplaceText("[ImposedActionRoof]", data.ImposedActionRoof.ToString(nfi));

            // Snow Load
            document.ReplaceText("[AnnualProbabilityULS_Snow]", data.AnnualProbabilityULS_Snow.ToString(nfi));
            document.ReplaceText("[R_ULS_Snow]", data.R_ULS_Snow.ToString(nfi));

            document.ReplaceText("[CCalcul_1170_3.eSnowElevationRegion]", data.Snow.eSnowElevationRegion.GetFriendlyName());
            document.ReplaceText("[CCalcul_1170_3.s_g_ULS]", data.Snow.fs_g_ULS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_3.s_g_SLS]", data.Snow.fs_g_SLS.ToString(nfi));
            document.ReplaceText("[ExposureCategory]", data.ExposureCategory);
            document.ReplaceText("[CCalcul_1170_3.C_e]", data.Snow.fC_e.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_3.Nu1_Alpha1]", data.Snow.fNu1_Alpha1.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_3.Nu2_Alpha1]", data.Snow.fNu2_Alpha1.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_3.s_ULS]", data.Snow.fs_ULS_Nu_1.ToString(nfi)); //???
            document.ReplaceText("[CCalcul_1170_3.s_SLS]", data.Snow.fs_SLS_Nu_1.ToString(nfi)); //???

            // Wind Load
            document.ReplaceText("[AnnualProbabilityULS_Wind]", data.AnnualProbabilityULS_Wind.ToString(nfi));
            document.ReplaceText("[R_ULS_Wind]", data.R_ULS_Wind.ToString(nfi));
            document.ReplaceText("[EWindRegion]", data.WindRegion);
            document.ReplaceText("[TerrainCategory]", data.TerrainCategory);
            document.ReplaceText("[CCalcul_1170_2.z]", data.Wind.fz.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.h]", data.Wind.fh.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.V_R_ULS]", data.Wind.fV_R_ULS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.V_R_SLS]", data.Wind.fV_R_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.M_z_cat]", data.Wind.fM_z_cat.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.M_s]", data.Wind.fM_s.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.M_t]", data.Wind.fM_t.ToString(nfi));

            // Hodnoty sa daju pocitat pre rozne smery vetra N, W, E, S. Zatial budeme zobrazovat len hodnoty s indexom[0]

            document.ReplaceText("[CCalcul_1170_2.fM_D_array_values_9[0]]", data.Wind.fM_D_array_values_9[0].ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.V_sit_ULS_Theta_9[0]]", data.Wind.fV_sit_ULS_Theta_9[0].ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.V_sit_ULS_Theta_9[0]]", data.Wind.fV_sit_ULS_Theta_9[0].ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.V_sit_SLS_Theta_9[0]]", data.Wind.fV_sit_SLS_Theta_9[0].ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.V_des_ULS_Theta_4[0]]", data.Wind.fV_des_ULS_Theta_4[0].ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.V_des_SLS_Theta_4[0]]", data.Wind.fV_des_SLS_Theta_4[0].ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.Rho_air]", data.Wind.fRho_air.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.C_dyn]", data.Wind.fC_dyn.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.p_basic_ULS_Theta_4[0]]", data.Wind.fp_basic_ULS_Theta_4[0].ToString(nfi));
            document.ReplaceText("[CCalcul_1170_2.p_basic_SLS_Theta_4[0]]", data.Wind.fp_basic_SLS_Theta_4[0].ToString(nfi));

            // External and internal pressure coefficients
            document.ReplaceText("[Cpi_max]", data.InternalPressureCoefficientCpiMaximumPressure.ToString(nfi));
            document.ReplaceText("[Cpi_min]", data.InternalPressureCoefficientCpiMaximumSuction.ToString(nfi));

            // Seismic Load
            document.ReplaceText("[AnnualProbabilityULS_EQ]", data.AnnualProbabilityULS_EQ.ToString(nfi));
            document.ReplaceText("[R_ULS_EQ]", data.R_ULS_EQ.ToString(nfi));
            document.ReplaceText("[ESiteSubSoilClass]", data.SiteSubSoilClass);
            document.ReplaceText("[FaultDistanceDmin]", data.FaultDistanceDmin.ToString(nfi));
            document.ReplaceText("[FaultDistanceDmax]", data.FaultDistanceDmax.ToString(nfi));
            document.ReplaceText("[ZoneFactorZ]", data.ZoneFactorZ.ToString(nfi));

            document.ReplaceText("[CCalcul_1170_5.G_tot_x]", data.Eq.fG_tot_x.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.G_tot_y]", data.Eq.fG_tot_y.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.PeriodAlongXDirectionTx]", data.Eq.fPeriodAlongXDirection_Tx.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.PeriodAlongYDirectionTy]", data.Eq.fPeriodAlongYDirection_Ty.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.SpectralShapeFactorChTx]", data.Eq.fSpectralShapeFactor_Ch_Tx.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.SpectralShapeFactorChTy]", data.Eq.fSpectralShapeFactor_Ch_Ty.ToString(nfi));

            // ULS
            // TO Ondrej - tu by to chcelo do tabulky riadok s textom Ultimiate Limit State
            document.ReplaceText("[CCalcul_1170_5.Nu_ULS]", data.Eq.fNu_ULS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.S_p_ULS_strength]", data.Eq.fS_p_ULS_strength.ToString(nfi));
            // X-direction
            document.ReplaceText("[CCalcul_1170_5.N_TxD_ULS]", data.Eq.fN_TxD_ULS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.C_Tx_ULS]", data.Eq.fC_Tx_ULS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.k_Nu_Tx_ULS]", data.Eq.fk_Nu_Tx_ULS_strength.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.C_d_T1x_ULS_strength]", data.Eq.fC_d_T1x_ULS_strength.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.V_x_ULS_strength]", data.Eq.fV_x_ULS_strength.ToString(nfi));
            // Y-direction
            document.ReplaceText("[CCalcul_1170_5.N_TyD_ULS]", data.Eq.fN_TyD_ULS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.C_Ty_ULS]", data.Eq.fC_Ty_ULS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.k_Nu_Ty_ULS]", data.Eq.fk_Nu_Ty_ULS_strength.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.C_d_T1y_ULS_strength]", data.Eq.fC_d_T1y_ULS_strength.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.V_y_ULS_strength]", data.Eq.fV_y_ULS_strength.ToString(nfi));

            // SLS
            // TO Ondrej - tu by to chcelo do tabulky riadok s textom Serviceability Limit State
            // alebo tu tabulku rozdelit na dve tabulky a tie texty tam dat ako nazov tabulky

            document.ReplaceText("[CCalcul_1170_5.Nu_SLS]", data.Eq.fNu_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.S_p_SLS]", data.Eq.fS_p_SLS.ToString(nfi));
            // X-direction
            document.ReplaceText("[CCalcul_1170_5.N_TxD_SLS]", data.Eq.fN_TxD_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.C_Tx_SLS]", data.Eq.fC_Tx_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.k_Nu_Tx_SLS]", data.Eq.fk_Nu_Tx_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.C_d_T1x_SLS]", data.Eq.fC_d_T1x_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.V_x_SLS]", data.Eq.fV_x_SLS.ToString(nfi));
            // Y-direction
            document.ReplaceText("[CCalcul_1170_5.N_TyD_SLS]", data.Eq.fN_TyD_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.C_Ty_SLS]", data.Eq.fC_Ty_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.k_Nu_Ty_SLS]", data.Eq.fk_Nu_Ty_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.C_d_T1y_SLS]", data.Eq.fC_d_T1y_SLS.ToString(nfi));
            document.ReplaceText("[CCalcul_1170_5.V_y_SLS]", data.Eq.fV_y_SLS.ToString(nfi));
        }

        private static void DrawBasicGeometryParameters(DocX document)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[BasicGeometryParameters]"));
            par.RemoveText(0);

            par = DrawDataExportTable(document, par, CStringsManager.LoadBasicGeometryParameters());
        }

        private static void DrawBasicLoadParameters(DocX document)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[BasicLoadParameters]"));
            par.RemoveText(0);

            par = DrawDataExportTable(document, par, CStringsManager.LoadBasicLoadParameters());
        }

        private static void DrawDeadLoadParameters(DocX document)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[DeadLoad]"));
            par.RemoveText(0);

            par = DrawDataExportTable(document, par, CStringsManager.LoadDeadLoadParameters_AS1170_1());
        }

        private static void DrawImposedLoadParameters(DocX document)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[ImposedLoad]"));
            par.RemoveText(0);

            par = DrawDataExportTable(document, par, CStringsManager.LoadImposedLoadParameters_AS1170_1());
        }

        private static void DrawSnowLoadParameters(DocX document)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[SnowLoad]"));
            par.RemoveText(0);

            par = DrawDataExportTable(document, par, CStringsManager.LoadSnowLoadParameters_AS1170_3());
        }

        private static void DrawWindLoadParameters(DocX document)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[WindLoad]"));
            par.RemoveText(0);

            par = DrawDataExportTable(document, par, CStringsManager.LoadWindLoadParameters_AS1170_2());
        }

        private static void DrawSeismicLoadParameters(DocX document)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[SeismicLoad]"));
            par.RemoveText(0);

            par = DrawDataExportTable(document, par, CStringsManager.LoadSeismicLoadParameters_NZS1170_5());
        }

        private static Paragraph DrawDataExportTable(DocX document, Paragraph p, List<DataExportTables> items)
        {
            if (items == null || items.Count < 1) return p;

            var t = document.AddTable(items.Count, 4);
            t.Design = TableDesign.TableGrid;
            t.Alignment = Alignment.left;

            //temp
            string language = "en";

            int i = 0;
            foreach (DataExportTables item in items)
            {
                Row row = t.Rows[i];
                i++;
                if (language == "cz") row.Cells[0].Paragraphs[0].InsertText(item.Description_CSY);
                else if (language == "sk") row.Cells[0].Paragraphs[0].InsertText(item.Description_SVK);
                else row.Cells[0].Paragraphs[0].InsertText(item.Description_ENU_USA);

                row.Cells[1].Paragraphs[0].InsertText(item.Symbol);
                row.Cells[2].Paragraphs[0].InsertText($"[{item.Identificator}]");
                row.Cells[3].Paragraphs[0].InsertText(CQuantityLibrary.GetReportUnit(item.UnitIdentificator));
            }

            p = p.InsertParagraphAfterSelf(p);
            p.InsertTableBeforeSelf(t);

            SetFontSizeForTable(t);

            float tableWidth = 480f;
            t.AutoFit = AutoFit.ColumnWidth;
            t.SetWidthsPercentage(new[] { 50f, 15f, 20f, 15f }, tableWidth);

            return p;
        }

        private static void DrawModel3D(DocX document, Viewport3D viewPort)
        {
            document.InsertParagraph("Structural model in 3D environment: ");

            ExportHelper.SaveViewPortContentAsImage(viewPort);

            // Add a simple image from disk.
            var image = document.AddImage("ViewPort.png");

            // Set Picture Height and Width.
            var picture = image.CreatePicture((int)document.PageWidth, (int)document.PageWidth);
            // Insert Picture in paragraph.
            var p = document.InsertParagraph();
            p.AppendPicture(picture);
            p.InsertPageBreakAfterSelf();
        }

        private static void DrawModel3DToDoc(DocX document, CModelData data, EViewType3D eViewtype)
        {
            string sParagraphName;
            string sImageName;

            if(eViewtype == EViewType3D.MEMBER_CENTERLINES)
            {
                sParagraphName = "[3DModelImage_MemberCenterlines]";
                sImageName = "ViewPort2.png";
            }
            else
            {
                sParagraphName = "[3DModelImage_MemberSolidModel]";
                sImageName = "ViewPort1.png";
            }

            DisplayOptions opts = ExportHelper.GetDisplayOptionsForMainModelExport(data, eViewtype == EViewType3D.MEMBER_CENTERLINES);
            opts.bCreateHorizontalGridlines = false;
            opts.bCreateVerticalGridlinesFront = false;
            opts.bCreateVerticalGridlinesBack = false;
            opts.bCreateVerticalGridlinesLeft = false;
            opts.bCreateVerticalGridlinesRight = false;

            CModel filteredModel = null;
            Trackport3D trackport = null;            
            Viewport3D viewPort = ExportHelper.GetBaseModelViewPort(opts, data, out filteredModel, out trackport);
            viewPort.UpdateLayout();

            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains(sParagraphName));

            //ExportHelper.SaveViewPortContentAsImage(viewPort);
            RenderTargetBitmap bmp = ExportHelper.RenderVisual(viewPort);
            ExportHelper.SaveBitmapImage(bmp, sImageName);
            bmp = null;

            double ratio = imageMaxWidth / viewPort.ActualWidth;

            // Add a simple image from disk.
            var image = document.AddImage(sImageName);
            // Set Picture Height and Width.
            var picture = image.CreatePicture((int)(viewPort.ActualHeight * ratio), imageMaxWidth);
            // Insert Picture in paragraph.
            par.RemoveText(0);
            par.AppendPicture(picture);
            //par.InsertPageBreakAfterSelf();
            picture = null;
            viewPort.Dispose();
            trackport.Dispose();
        }

        private static void DrawLoadCases(DocX document, CModelData data)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[LoadCases]"));
            par.RemoveText(0);

            var t = document.AddTable(1, 3);
            t.Design = TableDesign.TableGrid;
            t.Alignment = Alignment.left;
            t.AutoFit = AutoFit.Window;

            t.Rows[0].Cells[0].Paragraphs[0].InsertText("Load Case ID");
            t.Rows[0].Cells[1].Paragraphs[0].InsertText("Load Case Name");
            t.Rows[0].Cells[2].Paragraphs[0].InsertText("Load Case Type");
            t.Rows[0].Cells[0].Paragraphs[0].Bold();
            t.Rows[0].Cells[1].Paragraphs[0].Bold();
            t.Rows[0].Cells[2].Paragraphs[0].Bold();
            t.Rows[0].Cells[0].Width = document.PageWidth * 0.1;
            t.Rows[0].Cells[1].Width = document.PageWidth * 0.5;
            t.Rows[0].Cells[2].Width = document.PageWidth * 0.4;

            // For each load case add one row
            foreach (CLoadCase lc in data.Model.m_arrLoadCases)
            {
                Row row = t.InsertRow();
                row.Cells[0].Paragraphs[0].InsertText(lc.ID.ToString());
                row.Cells[1].Paragraphs[0].InsertText(lc.Name);
                row.Cells[2].Paragraphs[0].InsertText(lc.Type.GetFriendlyName());
            }
            SetFontSizeForTable(t);
            par.InsertTableBeforeSelf(t);
        }

        private static void DrawLoadCombinations(DocX document, CModelData data)
        {
            Paragraph parULS = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[LoadCombinationsULS]"));
            Paragraph parSLS = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[LoadCombinationsSLS]"));
            parULS.RemoveText(0);
            parSLS.RemoveText(0);


            var t = parULS.FollowingTable;
            // For each load case add one row
            foreach (CLoadCombination lc in data.Model.m_arrLoadCombs)
            {
                t = (lc.eLComType == ELSType.eLS_ULS ? parULS.FollowingTable : parSLS.FollowingTable);
                t.AutoFit = AutoFit.Fixed;

                Row row = t.InsertRow();

                //row.Cells[0].Paragraphs[0].InsertText(lc.ID.ToString());
                row.Cells[0].Paragraphs[0].InsertText(lc.Name);
                row.Cells[1].Paragraphs[0].InsertText(lc.eLComType == ELSType.eLS_ULS ? "ULS" : "SLS");

                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < lc.LoadCasesList.Count; j++)
                {
                    sb.AppendFormat("{0:F2} * LC{1}", Math.Round(lc.LoadCasesFactorsList[j], 2), lc.LoadCasesList[j].ID);
                    if (j < lc.LoadCasesList.Count - 1) sb.Append(" + ");
                }
                row.Cells[2].Paragraphs[0].InsertText(sb.ToString());
                row.Cells[3].Paragraphs[0].InsertText(lc.CombinationKey);
                row.Cells[4].Paragraphs[0].InsertText(lc.Formula);

                row.Cells[0].Width = t.Rows[0].Cells[0].Width;
                row.Cells[1].Width = t.Rows[0].Cells[1].Width;
                row.Cells[2].Width = t.Rows[0].Cells[2].Width;
                row.Cells[3].Width = t.Rows[0].Cells[3].Width;
                row.Cells[4].Width = t.Rows[0].Cells[4].Width;
            }
            parULS.Remove(false);
            parSLS.Remove(false);

            SetFontSizeForTable(parULS.FollowingTable);
            SetFontSizeForTable(parSLS.FollowingTable);
        }

        private static void SetFontSizeForTable(Table table, double? fontSize = null)
        {
            if (table == null) return;

            foreach (Paragraph p in table.Paragraphs)
            {
                if(fontSize != null) p.FontSize(fontSize.Value);
                else p.FontSize(fontSizeInTable);
            }
        }

        private static void DrawMemberDesign(DocX document, CModelData data)
        {
            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[MemberDesign]"));
            par.RemoveText(0);

            foreach (CComponentInfo cInfo in data.ComponentList)
            {
                if (!cInfo.Design) continue;

                //CMember governingMember = data.sDesignResults_ULSandSLS.DesignResults[cInfo.MemberTypePosition].MemberWithMaximumDesignRatio;
                //if (governingMember != null)  par = par.InsertParagraphAfterSelf($"Governing member ID: {governingMember.ID}");
                //CLoadCombination governingLComb = data.sDesignResults_ULSandSLS.DesignResults[cInfo.MemberTypePosition].GoverningLoadCombination;
                //if (governingLComb != null) par = par.InsertParagraphAfterSelf($"Governing load combination ID: {governingLComb.ID}");

                ///////////////////////////////////
                // POVODNY KOD
                ///////////////////////////////////

                /*
                CMember governingMemberULS = data.sDesignResults_ULS.DesignResults[cInfo.MemberTypePosition].MemberWithMaximumDesignRatio;                
                CLoadCombination governingLCombULS = data.sDesignResults_ULS.DesignResults[cInfo.MemberTypePosition].GoverningLoadCombination;
                CMember governingMemberSLS = data.sDesignResults_SLS.DesignResults[cInfo.MemberTypePosition].MemberWithMaximumDesignRatio;                
                CLoadCombination governingLCombSLS = data.sDesignResults_SLS.DesignResults[cInfo.MemberTypePosition].GoverningLoadCombination;

                if (governingLCombSLS == null || governingLCombULS == null || governingMemberULS == null || governingMemberSLS == null) continue;

                par = par.InsertParagraphAfterSelf("Member type: " + cInfo.ComponentName);
                par.StyleName = "Heading2";
                if (governingMemberULS != null) par = par.InsertParagraphAfterSelf($"Governing member ID (ULS): {governingMemberULS.ID}");
                if (governingLCombULS != null) par = par.InsertParagraphAfterSelf($"Governing load combination ID (ULS): {governingLCombULS.ID}");
                if (governingMemberSLS != null) par = par.InsertParagraphAfterSelf($"Governing member ID (SLS): {governingMemberSLS.ID}");
                if (governingLCombSLS != null) par = par.InsertParagraphAfterSelf($"Governing load combination ID (SLS): {governingLCombSLS.ID}");
                

                if (governingLCombULS.ID == governingLCombSLS.ID && governingMemberULS.ID == governingMemberSLS.ID)
                {
                    par = par.InsertParagraphAfterSelf("Member internal forces");
                    par.StyleName = "Heading3";
                    par = par.InsertParagraphAfterSelf("");
                    par = AppendIFCanvases(document, par, data, governingLCombULS, governingMemberULS);

                    if (cInfo.IsFrameMember())
                    {
                        par = par.InsertParagraphAfterSelf("Frame internal forces");
                        par.StyleName = "Heading3";
                        par = par.InsertParagraphAfterSelf("");
                        par = AppendFrameIFCanvases(document, par, data, governingLCombULS, governingMemberSLS);
                    }
                }
                else
                {
                    par = par.InsertParagraphAfterSelf("Member internal forces ULS");
                    par.StyleName = "Heading3";
                    par = par.InsertParagraphAfterSelf("");
                    par = AppendIFCanvases(document, par, data, governingLCombULS, governingMemberULS);

                    par = par.InsertParagraphAfterSelf("Member internal forces SLS");
                    par.StyleName = "Heading3";
                    par = par.InsertParagraphAfterSelf("");
                    par = AppendIFCanvases(document, par, data, governingLCombSLS, governingMemberSLS);

                    if (cInfo.IsFrameMember())
                    {
                        par = par.InsertParagraphAfterSelf("Frame internal forces ULS");
                        par.StyleName = "Heading3";
                        par = par.InsertParagraphAfterSelf("");
                        par = AppendFrameIFCanvases(document, par, data, governingLCombULS, governingMemberULS);

                        par = par.InsertParagraphAfterSelf("Frame internal forces SLS");
                        par.StyleName = "Heading3";
                        par = par.InsertParagraphAfterSelf("");
                        par = AppendFrameIFCanvases(document, par, data, governingLCombSLS, governingMemberSLS);
                    }
                }

                par = par.InsertParagraphAfterSelf("Member design details - ULS");
                par.StyleName = "Heading3";

                CCalculMember calcul = null;
                data.dictULSDesignResults.TryGetValue(cInfo.MemberTypePosition, out calcul);
                if (calcul != null)
                {
                    DataTable dt = DataGridHelper.GetDesignResultsInDataTable(calcul, ELSType.eLS_ULS);
                    Table t = GetTable(document, dt);
                    par = par.InsertParagraphAfterSelf("");
                    AddSimpleTableAfterParagraph(t, par);
                }

                par = par.InsertParagraphAfterSelf("Member deflections");
                par.StyleName = "Heading3";

                par = par.InsertParagraphAfterSelf("Member design details - SLS");
                par.StyleName = "Heading3";

                data.dictSLSDesignResults.TryGetValue(cInfo.MemberTypePosition, out calcul);
                if (calcul != null)
                {
                    DataTable dt = DataGridHelper.GetDesignResultsInDataTable(calcul, ELSType.eLS_SLS);
                    Table t = GetTable(document, dt);
                    par = par.InsertParagraphAfterSelf("");
                    AddSimpleTableAfterParagraph(t, par);
                }
                */



                ///////////////////////////////////
                // NOVY KOD
                ///////////////////////////////////

                // TO Ondrej - rozdelil som to na dva bloky ULS, SLS, mozno sa to da zabalit do nejakej funkcie
                // Chcel by som ULS a SLS zobrazovat uplne oddelene

                // Ultimate limit state - ULS

                CMember governingMemberULS = data.sDesignResults_ULS.DesignResults[cInfo.MemberTypePosition].MemberWithMaximumDesignRatio;
                CLoadCombination governingLCombULS = data.sDesignResults_ULS.DesignResults[cInfo.MemberTypePosition].GoverningLoadCombination;
                if (governingLCombULS == null || governingMemberULS == null) continue; // Toto by mal byt mozno assert, lebo nie je v poriadku ak sa component posudzuje a nic sa nenajde

                par = par.InsertParagraphAfterSelf("Member type: " + cInfo.ComponentName);
                par.StyleName = "Heading2";

                par = par.InsertParagraphAfterSelf("Member internal forces and design ULS");
                par.StyleName = "Heading3";

                if (governingMemberULS != null) par = par.InsertParagraphAfterSelf($"Governing member ID (ULS): {governingMemberULS.ID}");
                if (governingLCombULS != null) par = par.InsertParagraphAfterSelf($"Governing load combination ID (ULS): {governingLCombULS.ID}");

                par = par.InsertParagraphAfterSelf("Member internal forces ULS");
                par.StyleName = "Heading4";
                par = par.InsertParagraphAfterSelf("");
                par = AppendMemberResultsCanvases_ULS(document, par, data, governingLCombULS, governingMemberULS); 

                if (cInfo.IsFrameMember())
                {
                    par = par.InsertParagraphAfterSelf("Frame internal forces ULS");
                    par.StyleName = "Heading4";
                    par = par.InsertParagraphAfterSelf("");
                    par = AppendFrameResultsCanvases_ULS(document, par, data, governingLCombULS, governingMemberULS);
                }
                par = par.InsertParagraphAfterSelf("Member design details - ULS");
                par.StyleName = "Heading4";

                CCalculMember calcul = null;
                data.dictULSDesignResults.TryGetValue(cInfo.MemberTypePosition, out calcul);
                if (calcul != null)
                {
                    DataTable dt = DataGridHelper.GetDesignResultsInDataTable(calcul, ELSType.eLS_ULS);
                    Table t = GetTable(document, dt);
                    par = par.InsertParagraphAfterSelf("");
                    AddSimpleTableAfterParagraph(t, par);
                }

                // Serviceability limit state - SLS

                CMember governingMemberSLS = data.sDesignResults_SLS.DesignResults[cInfo.MemberTypePosition].MemberWithMaximumDesignRatio;
                CLoadCombination governingLCombSLS = data.sDesignResults_SLS.DesignResults[cInfo.MemberTypePosition].GoverningLoadCombination;
                if (governingLCombSLS == null || governingMemberSLS == null) continue; // Toto by mal byt mozno assert, lebo nie je v poriadku ak sa component posudzuje a nic sa nenajde

                par = par.InsertParagraphAfterSelf("Member deflection and design SLS");
                par.StyleName = "Heading3";

                if (governingMemberSLS != null) par = par.InsertParagraphAfterSelf($"Governing member ID (SLS): {governingMemberSLS.ID}");
                if (governingLCombSLS != null) par = par.InsertParagraphAfterSelf($"Governing load combination ID (SLS): {governingLCombSLS.ID}");

                par = par.InsertParagraphAfterSelf("Member deflections SLS");
                par.StyleName = "Heading4";
                par = par.InsertParagraphAfterSelf("");
                par = AppendMemberResultsCanvases_SLS(document, par, data, governingLCombSLS, governingMemberSLS); 

                if (cInfo.IsFrameMember())
                {
                    par = par.InsertParagraphAfterSelf("Frame deflections SLS");
                    par.StyleName = "Heading4";
                    par = par.InsertParagraphAfterSelf("");
                    par = AppendFrameResultsCanvases_SLS(document, par, data, governingLCombSLS, governingMemberSLS);  
                }

                par = par.InsertParagraphAfterSelf("Member design details - SLS");
                par.StyleName = "Heading4";

                data.dictSLSDesignResults.TryGetValue(cInfo.MemberTypePosition, out calcul);
                if (calcul != null)
                {
                    DataTable dt = DataGridHelper.GetDesignResultsInDataTable(calcul, ELSType.eLS_SLS);
                    Table t = GetTable(document, dt);
                    par = par.InsertParagraphAfterSelf("");
                    AddSimpleTableAfterParagraph(t, par);
                }
            }
        }
        
        // TO Ondrej - nove funkcie - len pre ULS alebo SLS / member alebo ram
        // Done
        private static Paragraph AppendMemberResultsCanvases_ULS(DocX document, Paragraph par, CModelData data, CLoadCombination lcomb, CMember member)
        {
            List<Canvas> canvases = ExportHelper.GetIFCanvases(true, data.UseCRSCGeometricalAxes, lcomb, member, data.MemberInternalForcesInLoadCombinations, data.MemberDeflectionsInLoadCombinations);
            foreach (Canvas canvas in canvases)
            {
                par = par.InsertParagraphAfterSelf(canvas.ToolTip.ToString());
                //par = par.InsertParagraphAfterSelf("");  //novy odsek aby nedavalo obrazok vedla textu, Ak sa natiahne obrazok na sirku...tak sa toto moze zmazat.
                AppendImageFromCanvas(document, canvas, par);
            }
            return par;
        }

        private static Paragraph AppendFrameResultsCanvases_ULS(DocX document, Paragraph par, CModelData data, CLoadCombination lcomb, CMember member)
        {
            List<Canvas> canvases = ExportHelper.GetFrameInternalForcesCanvases(true, data.frameModels, member, lcomb, data.MemberInternalForcesInLoadCombinations, data.MemberDeflectionsInLoadCombinations, data.UseCRSCGeometricalAxes);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.Name != "LocalDeflection_Delta_x" && canvas.Name != "LocalDeflection_Delta_y")
                {
                    par = par.InsertParagraphAfterSelf(canvas.ToolTip.ToString());
                    //par = par.InsertParagraphAfterSelf("");  //novy odsek aby nedavalo obrazok vedla textu, Ak sa natiahne obrazok na sirku...tak sa toto moze zmazat.
                    AppendImageFromCanvas(document, canvas, par);
                }
            }
            return par;
        }

        private static Paragraph AppendMemberResultsCanvases_SLS(DocX document, Paragraph par, CModelData data, CLoadCombination lcomb, CMember member)
        {
            List<Canvas> canvases = ExportHelper.GetIFCanvases(false, data.UseCRSCGeometricalAxes, lcomb, member, data.MemberInternalForcesInLoadCombinations, data.MemberDeflectionsInLoadCombinations);
            foreach (Canvas canvas in canvases)
            {
                par = par.InsertParagraphAfterSelf(canvas.ToolTip.ToString());
                //par = par.InsertParagraphAfterSelf("");  //novy odsek aby nedavalo obrazok vedla textu, Ak sa natiahne obrazok na sirku...tak sa toto moze zmazat.
                AppendImageFromCanvas(document, canvas, par);
            }
            return par;
        }

        private static Paragraph AppendFrameResultsCanvases_SLS(DocX document, Paragraph par, CModelData data, CLoadCombination lcomb, CMember member)
        {
            List<Canvas> canvases = ExportHelper.GetFrameInternalForcesCanvases(false, data.frameModels, member, lcomb, data.MemberInternalForcesInLoadCombinations, data.MemberDeflectionsInLoadCombinations, data.UseCRSCGeometricalAxes);
            foreach (Canvas canvas in canvases)
            {
                par = par.InsertParagraphAfterSelf(canvas.ToolTip.ToString());
                //par = par.InsertParagraphAfterSelf("");  //novy odsek aby nedavalo obrazok vedla textu, Ak sa natiahne obrazok na sirku...tak sa toto moze zmazat.
                AppendImageFromCanvas(document, canvas, par);

            }
            return par;
        }

        private static void AppendImageFromCanvas(DocX document, Canvas canvas, Paragraph par)
        {
            using (Stream stream = ExportHelper.GetCanvasStream(canvas))
            {
                // Add a simple image from disk.
                var image = document.AddImage(stream);
                // Set Picture Height and Width.
                var picture = image.CreatePicture((int)canvas.ActualHeight, (int)canvas.ActualWidth);
                // Insert Picture in paragraph.             
                par.AppendPicture(picture);
            }
        }
        private static void AppendImageFromViewPort(DocX document, Viewport3D viewPort, Paragraph par)
        {
            using (Stream stream = ExportHelper.GetViewPortStream(viewPort))
            {
                // Add a simple image from disk.
                var image = document.AddImage(stream);
                // Set Picture Height and Width.
                var picture = image.CreatePicture((int)viewPort.ActualHeight, (int)viewPort.ActualWidth);
                // Insert Picture in paragraph.
                par.AppendPicture(picture);
            }
        }

        private static void DrawJointDesign(DocX document, CModelData data)
        {
            // Refaktorovat s FootingDesign
            DisplayOptions sDisplayOptions = data.DisplayOptions;
            sDisplayOptions.bDisplaySolidModel = true;

            sDisplayOptions.bDisplayMembers = true;
            sDisplayOptions.bDisplayJoints = true;
            sDisplayOptions.bDisplayPlates = true;
            sDisplayOptions.bDisplayConnectors = true;

            sDisplayOptions.bDisplayNodes = false;
            sDisplayOptions.bDisplayNodesDescription = false;

            sDisplayOptions.bUseOrtographicCamera = false;
            sDisplayOptions.bDisplayGlobalAxis = false;
            sDisplayOptions.bDisplayMemberDescription = false;

            // Do dokumentu exporujeme aj s wireframe
            sDisplayOptions.bDisplayWireFrameModel = true;
            sDisplayOptions.fWireFrameLineThickness = 0.002f;
            sDisplayOptions.bTransformScreenLines3DToCylinders3D = true;

            sDisplayOptions.bDisplayMembersWireFrame = true;
            sDisplayOptions.bDisplayJointsWireFrame = true;
            sDisplayOptions.bDisplayPlatesWireFrame = true;
            sDisplayOptions.bDisplayConnectorsWireFrame = false;

            sDisplayOptions.wireFrameColor = System.Windows.Media.Colors.Black; // Farba linii pre export, moze sa urobit nastavitelna samostatne pre 3D preview a export

            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[JointDesign]"));
            par.RemoveText(0);

            foreach (CComponentInfo cInfo in data.ComponentList)
            {
                if (!cInfo.Design) continue;

                bool bStartJointDesignDetailsExist = false;

                // Start Joint
                CCalculJoint calcul = null;
                data.dictStartJointResults.TryGetValue(cInfo.MemberTypePosition, out calcul);
                if (calcul != null)
                {
                    bStartJointDesignDetailsExist = true;

                    // Display Member type heading
                    par = par.InsertParagraphAfterSelf("Member type: " + cInfo.ComponentName);
                    par.StyleName = "Heading2";

                    par = par.InsertParagraphAfterSelf("Design Details - Member Start Joint");
                    par.StyleName = "Heading3";

                    par = par.InsertParagraphAfterSelf("");

                    Trackport3D trackport = null;
                    Viewport3D viewPort = ExportHelper.GetJointViewPort(calcul.joint, sDisplayOptions, data.Model, out trackport);
                    viewPort.UpdateLayout();
                    AppendImageFromViewPort(document, viewPort, par);
                    viewPort.Dispose();
                    trackport.Dispose();

                    DataTable dt = DataGridHelper.GetDesignResultsInDataTable(calcul);
                    Table t = GetTable(document, dt);
                    par = par.InsertParagraphAfterSelf("");
                    AddSimpleTableAfterParagraph(t, par);
                }

                // End Joint
                calcul = null;
                data.dictEndJointResults.TryGetValue(cInfo.MemberTypePosition, out calcul);
                if (calcul != null)
                {
                    if (!bStartJointDesignDetailsExist)
                    {
                        // Display Member type heading in case that it wasn't displayed in the start joint block
                        par = par.InsertParagraphAfterSelf("Member type: " + cInfo.ComponentName);
                        par.StyleName = "Heading2";
                    }

                    par = par.InsertParagraphAfterSelf("Design Details - Member End Joint");
                    par.StyleName = "Heading3";

                    par = par.InsertParagraphAfterSelf("");
                    Trackport3D trackport = null;
                    Viewport3D viewPort = ExportHelper.GetJointViewPort(calcul.joint, sDisplayOptions, data.Model, out trackport);
                    viewPort.UpdateLayout();
                    AppendImageFromViewPort(document, viewPort, par);
                    viewPort.Dispose();
                    trackport.Dispose();

                    DataTable dt = DataGridHelper.GetDesignResultsInDataTable(calcul);
                    Table t = GetTable(document, dt);
                    par = par.InsertParagraphAfterSelf("");
                    AddSimpleTableAfterParagraph(t, par);
                }
            }
        }

        private static void DrawFootingDesign(DocX document, CModelData data)
        {
            // Refaktorovat s JointDesign
            DisplayOptions sDisplayOptions = data.DisplayOptions;
            sDisplayOptions.bDisplaySolidModel = true;

            sDisplayOptions.bDisplayMembers = true;
            sDisplayOptions.bDisplayJoints = true;
            sDisplayOptions.bDisplayPlates = true;
            sDisplayOptions.bDisplayConnectors = true;

            sDisplayOptions.bDisplayNodes = false;
            sDisplayOptions.bDisplayNodesDescription = false;

            sDisplayOptions.bUseOrtographicCamera = false;
            sDisplayOptions.bDisplayGlobalAxis = false;
            sDisplayOptions.bDisplayMemberDescription = false;

            // Do dokumentu exporujeme aj s wireframe
            sDisplayOptions.bDisplayWireFrameModel = true;
            sDisplayOptions.fWireFrameLineThickness = 0.003f; // TODO - zavisi od zoomu !!!
            sDisplayOptions.bTransformScreenLines3DToCylinders3D = true;

            sDisplayOptions.bDisplayMembersWireFrame = true;
            sDisplayOptions.bDisplayJointsWireFrame = true;
            sDisplayOptions.bDisplayPlatesWireFrame = true;
            sDisplayOptions.bDisplayConnectorsWireFrame = false;

            sDisplayOptions.wireFrameColor = System.Windows.Media.Colors.Black; // Farba linii pre export, moze sa urobit nastavitelna samostatne pre 3D preview a export

            // Foundations
            sDisplayOptions.bDisplayFoundations = true;
            sDisplayOptions.bDisplayReinforcementBars = true;

            sDisplayOptions.bDisplayFoundationsWireFrame = true;
            sDisplayOptions.bDisplayReinforcementBarsWireFrame = true;

            sDisplayOptions.RotateModelX = -80;
            sDisplayOptions.RotateModelY = 45;
            sDisplayOptions.RotateModelZ = 5;

            Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[FootingDesign]"));
            par.RemoveText(0);

            foreach (CComponentInfo cInfo in data.ComponentList)
            {
                if(cInfo.MemberTypePosition != EMemberType_FS_Position.MainColumn &&
                    cInfo.MemberTypePosition != EMemberType_FS_Position.EdgeColumn &&
                    cInfo.MemberTypePosition != EMemberType_FS_Position.ColumnFrontSide &&
                    cInfo.MemberTypePosition != EMemberType_FS_Position.ColumnBackSide) continue;
                if (!cInfo.Design) continue;
                
                // Start Joint
                CCalculJoint calcul = null;
                data.dictStartJointResults.TryGetValue(cInfo.MemberTypePosition, out calcul);

                // End Joint
                CCalculJoint calculEnd = null;
                data.dictEndJointResults.TryGetValue(cInfo.MemberTypePosition, out calculEnd);

                if (calculEnd != null && calculEnd.footing != null)
                {
                    if (calcul == null) calcul = calculEnd;
                    else if (calcul.fEta_max_Footing < calculEnd.fEta_max_Footing) calcul = calculEnd;
                }
                
                if (calcul != null)
                {   
                    // Display Member type heading
                    par = par.InsertParagraphAfterSelf("Member type: " + cInfo.ComponentName);
                    par.StyleName = "Heading2";

                    par = par.InsertParagraphAfterSelf("Design Details");
                    par.StyleName = "Heading3";

                    par = par.InsertParagraphAfterSelf("");
                    Trackport3D trackport = null;
                    Viewport3D viewPort = ExportHelper.GetFootingViewPort(calcul.joint, calcul.footing, sDisplayOptions, out trackport);
                    viewPort.UpdateLayout();
                    AppendImageFromViewPort(document, viewPort, par);
                    viewPort.Dispose();
                    trackport.Dispose();

                    DataTable dt = DataGridHelper.GetFootingDesignResultsInDataTable(calcul);
                    Table t = GetTable(document, dt);
                    par = par.InsertParagraphAfterSelf("");
                    AddSimpleTableAfterParagraph(t, par);
                }
            }
        }

        private static void AddSimpleTableAfterParagraph(Table t, Paragraph p, bool autofit = false)
        {
            t.Design = TableDesign.TableGrid;
            t.Alignment = Alignment.left;
            
            if(autofit) t.AutoFit = AutoFit.Window;

            p.InsertTableBeforeSelf(t);
        }

        private static Table GetTable(DocX document, DataTable dt)
        {
            var t = document.AddTable(dt.Rows.Count + 1, dt.Columns.Count);
            t.AutoFit = AutoFit.Contents;
            //header
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                t.Rows[0].Cells[j].Paragraphs[0].InsertText(dt.Columns[j].Caption);
                t.Rows[0].Cells[j].Paragraphs[0].Bold();
                t.Rows[0].Cells[j].Width = document.PageWidth / dt.Columns.Count;
            }

            // For each load case add one row
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    t.Rows[i + 1].Cells[j].Paragraphs[0].InsertText(dt.Rows[i][j].ToString());
                }
            }

            SetFontSizeForTable(t);

            return t;
        }

        private static Table GetTableFromDataTable(DocX document, DataTable dt)
        {
            int columnCount = dt.Columns.Count;

            bool withoutPrices = true;
            List<int> priceColumnsIndexes = new List<int>();
            if (withoutPrices)
            {
                columnCount = 0;
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (dt.Columns[j].Caption.Contains("Price"))
                    {
                        priceColumnsIndexes.Add(j);
                    }
                    else
                    {
                        columnCount++;
                    }
                }
            }

            var t = document.AddTable(dt.Rows.Count + 1, columnCount);
            t.Design = TableDesign.TableGrid;
            t.Alignment = Alignment.left;

            List<float> columnsWidths = new List<float>();

            int colorColumn = -1;
            int columnSkip = 0;
            //header
            for (int j = 0; j < columnCount; j++)
            {
                if (priceColumnsIndexes.Contains(j)) columnSkip++;
                t.Rows[0].Cells[j].Paragraphs[0].InsertText(dt.Columns[j + columnSkip].Caption);
                t.Rows[0].Cells[j].Paragraphs[0].Bold();
                if (dt.Columns[j + columnSkip].ExtendedProperties["Unit"] != null)
                {
                    t.Rows[0].Cells[j].InsertParagraph(dt.Columns[j + columnSkip].ExtendedProperties["Unit"].ToString()).Bold();
                    SetAlignment(dt.Columns[j + columnSkip], t.Rows[0].Cells[j].Paragraphs[1]);
                }
                
                if (dt.Columns[j + columnSkip].ExtendedProperties["Width"] != null)
                {
                    columnsWidths.Add((float)dt.Columns[j + columnSkip].ExtendedProperties["Width"]);
                }
                else columnsWidths.Add(100f / columnCount);

                SetAlignment(dt.Columns[j + columnSkip], t.Rows[0].Cells[j].Paragraphs[0]);

                if (dt.Columns[j + columnSkip].ColumnName == "Color") colorColumn = j;
            }

            // For each load case add one row
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                columnSkip = 0;
                for (int j = 0; j < columnCount; j++)
                {
                    if (priceColumnsIndexes.Contains(j)) columnSkip++;

                    if (colorColumn != -1 && colorColumn == j)
                    {
                        if(!string.IsNullOrEmpty(dt.Rows[i][j + columnSkip].ToString()))
                            t.Rows[i + 1].Cells[j].FillColor = System.Drawing.ColorTranslator.FromHtml(dt.Rows[i][j + columnSkip].ToString());
                        continue;
                    }

                    t.Rows[i + 1].Cells[j].Paragraphs[0].InsertText(dt.Rows[i][j + columnSkip].ToString());
                    SetAlignment(dt.Columns[j + columnSkip], t.Rows[i + 1].Cells[j].Paragraphs[0]);
                }
            }

            SetFontSizeForTable(t, 6.5);

            t.AutoFit = AutoFit.ColumnWidth;
            t.SetWidthsPercentage(columnsWidths.ToArray(), quotationTableWidth);

            return t;
        }

        private static void SetAlignment(DataColumn col, Paragraph p)
        {
            if (col.ExtendedProperties["Align"] != null)
            {
                if ((AlignmentX)col.ExtendedProperties["Align"] == AlignmentX.Left)
                {
                    p.Alignment = Alignment.left;
                }
                else if ((AlignmentX)col.ExtendedProperties["Align"] == AlignmentX.Right)
                {
                    p.Alignment = Alignment.right;
                }
                else if ((AlignmentX)col.ExtendedProperties["Align"] == AlignmentX.Center)
                {
                    p.Alignment = Alignment.center;
                }
            }
        }

        private static void CreateTOC(DocX document)
        {
            Paragraph parTOC = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("[TOC]"));
            parTOC.RemoveText(0);

            // Add the Table of Content prior to the referenced paragraph

            int iContentMaxLevel = 2; // Maximalna uroven nadpisu "heading", ktora ma byt pridana do obsahu - TO Ondrej - nejako mi to nefunguje :(
            var toc = document.InsertTableOfContents(parTOC, "Table of Contents", TableOfContentsSwitches.H, null, iContentMaxLevel, 0);

            // Add a page break prior to the referenced paragraph so it starts on a fresh page after the Table of Content
            parTOC.InsertPageBreakBeforeSelf();

            //TableOfContents toc = document.InsertTableOfContents("Programatically generated TOC", TableOfContentsSwitches.H);
            //Paragraph par = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("Programatically generated TOC"));
            //if (par != null)
            //{
            //    int index = document.Paragraphs.IndexOf(par);
            //    document.Paragraphs[index].Remove();
            //    par.Remove(false);

            //} 
        }

        private static void CreateChapterWithBuletedList(DocX document)
        {
            // Add a paragraph at the end of the template.
            var p1 = document.InsertParagraph("TECHNICAL SOLUTION");
            p1.StyleName = "Heading1";

            var p2 = document.InsertParagraph("Picture");
            p2.StyleName = "No spacing";

            var p3 = document.InsertParagraph("DESCRIPTION OF FUNCTIONALITIES");
            p3.StyleName = "Heading2";

            var p4 = document.InsertParagraph("text of description of functionalities");
            p4.StyleName = "Normal";

            var p5 = document.InsertParagraph("PARTS INCLUDED IN THE DELIVERY");
            p5.StyleName = "Heading2";

            var p6 = document.InsertParagraph("text for PARTS INCLUDED IN THE DELIVERY");
            p6.StyleName = "Normal";

            var p7 = document.InsertParagraph("PARTS EXCLUDED FROM THE DELIVERY");
            p7.StyleName = "Heading2";

            var bulletedList = document.AddList("Mast / pole", 0, ListItemType.Bulleted);
            document.AddListItem(bulletedList, "Batteries");
            document.AddListItem(bulletedList, "Aerials and coaxial cables");
            // Add an item (level 0)
            document.AddListItem(bulletedList, "Radio - stations");
            document.AddListItem(bulletedList, "Radio station 1", 1);
            document.AddListItem(bulletedList, "Radio station 2", 1);
            document.AddListItem(bulletedList, "Push buttons");
            document.AddListItem(bulletedList, "Olflex cables");
            document.AddListItem(bulletedList, "CAN cables");
            document.AddListItem(bulletedList, "Solar panels and accessories");

            document.InsertList(bulletedList);
        }

        //Indentations
        //// Add the first paragraph.
        //var p = document.InsertParagraph("This is the first paragraph. It doesn't contain any indentation.");
        //p.SpacingAfter(30);
        //        // Add the second paragraph.
        //        var p2 = document.InsertParagraph("This is the second paragraph. It contains an indentation on the first line.");
        //// Indent only the first line of the Paragraph.
        //p2.IndentationFirstLine = -1.0f;
        //        p2.SpacingAfter(30);
        //        // Add the third paragraph.
        //        var p3 = document.InsertParagraph("This is the third paragraph. It contains an indentation on all the lines except the first one.");
        //// Indent all the lines of the Paragraph, except the first.
        //p3.IndentationHanging = 1.0f;




        //private static void DrawBasicGeometry(DocX document, CModelData data)
        //{            
        //    Paragraph p = document.InsertParagraph("Basic Geometry - Building");
        //    p.StyleName = "Heading1";

        //    p = p.InsertParagraphAfterSelf("Width\t\tB = \t\t10.00 m").Append("2").Script(Script.superscript);
        //    p = p.InsertParagraphAfterSelf("Length\t\tL = \t\t22.85 m").Append("2").Script(Script.superscript);
        //    p = p.InsertParagraphAfterSelf("Height\t\tH").Append("1").Script(Script.subscript).Append(" = \t\t5.00 m").Append("2").Script(Script.superscript);
        //    p = p.InsertParagraphAfterSelf("Height\t\tH").Append("2").Script(Script.subscript).Append(" = \t\t4.48 m").Append("2").Script(Script.superscript);
        //    p = p.InsertParagraphAfterSelf("Pitch\t\tα = \t\t3.0 deg");

        //    p.SpacingAfter(40d);
        //    p = p.InsertParagraphAfterSelf("3D structural model");
        //    p.StyleName = "Heading1";

        //    p = p.InsertParagraphAfterSelf("Theoretical dimensions");
        //    p = p.InsertParagraphAfterSelf("Width\t\tB = \t\t9.41 m");
        //    p = p.InsertParagraphAfterSelf("Length\t\tL = \t\t22.85 m");
        //    p = p.InsertParagraphAfterSelf("Height\t\tH").Append("1").Script(Script.subscript).Append(" = \t\t5.00 m");
        //    p = p.InsertParagraphAfterSelf("Height\t\tH").Append("2").Script(Script.subscript).Append(" = \t\t4.48 m");

        //    p = p.InsertParagraphAfterSelf("Rafter length \t\t9.423 m");
        //    p = p.InsertParagraphAfterSelf("Purlin spacing \t\t1.88 m \t- not used for purlin design");
        //    p = p.InsertParagraphAfterSelf("Girt spacing \t\t2.00 m \t- not used for girt design");
        //}

        //private static void DrawLogoAndProjectInfoTable(DocX document)
        //{
        //    // Add a Table of 5 rows and 2 columns into the document and sets its values.
        //    var t = document.AddTable(1, 3);
        //    t.Design = TableDesign.None;
        //    t.Alignment = Alignment.left;

        //    var image = document.AddImage(ConfigurationManager.AppSettings["logoForPDF"]);
        //    // Set Picture Height and Width.
        //    var picture = image.CreatePicture(150, 300);

        //    t.Rows[0].Cells[0].Paragraphs[0].AppendPicture(picture);
        //    CProjectInfo pInfo = GetProjectInfo();
        //    Paragraph p = t.Rows[0].Cells[1].Paragraphs[0].InsertParagraphAfterSelf("Project Name: ");
        //    p = p.InsertParagraphAfterSelf(pInfo.ProjectName).Bold();

        //    p = p.InsertParagraphAfterSelf("Site: ");
        //    p = p.InsertParagraphAfterSelf(pInfo.Site).Bold();
        //    p.SpacingAfter(20d);

        //    p = p.InsertParagraphAfterSelf("Project Number: ");
        //    p = p.InsertParagraphAfterSelf(pInfo.ProjectNumber).Bold();

        //    p = t.Rows[0].Cells[2].Paragraphs[0].InsertParagraphAfterSelf("Project Part: ");
        //    p = p.InsertParagraphAfterSelf(pInfo.ProjectPart).Bold();
        //    p.SpacingAfter(50d);

        //    p = p.InsertParagraphAfterSelf("Date: ");
        //    p = p.InsertParagraphAfterSelf(pInfo.Date.ToShortDateString()).Bold();


        //    document.InsertTable(t);
        //}

        //private static void DrawLogo(DocX document)
        //{
        //    // Add a simple image from disk.
        //    var image = document.AddImage(ConfigurationManager.AppSettings["logoForPDF"]);
        //    // Set Picture Height and Width.
        //    var picture = image.CreatePicture(300, 150);

        //    // Insert Picture in paragraph.
        //    var p = document.InsertParagraph();
        //    p.AppendPicture(picture);
        //}
    }
}

